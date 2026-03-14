using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature;
using RecruitmentInterviewManagementSystem.Infastructure.Models;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.TaiOrXiuImplement
{
    public class GameRound : IGameRounds
    {
        private readonly FakeTopcvContext _db;

        public GameRound(FakeTopcvContext fakeTopcvContext)
        {
            _db = fakeTopcvContext;
        }

        // đóng cược
        public async Task CloseRound(Guid roundId)
        {
            var round = await _db.GameRounds.FirstOrDefaultAsync(x => x.IdGame == roundId);
            if (round == null) return;
            round.IsClosed = true;
            await _db.SaveChangesAsync();
        }

        // tạo vòng cược mới
        public async Task<GameRounds> CreateRound()
        {
            var round = new GameRounds
            {
                IdGame = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                IsClosed = false
            };
            _db.GameRounds.Add(round);
            await _db.SaveChangesAsync();
            return round;
        }

        // lấy vòng cược hiện tại
        public async Task<GameRounds?> GetCurrentRound()
        {
            return await _db.GameRounds.Where(x => !x.IsClosed)
                .OrderByDescending(x => x.StartTime)
                .FirstOrDefaultAsync();
        }

        // trả thưởng
        public async Task PayReward(Guid roundId)
        {
            var round = await _db.GameRounds .FirstOrDefaultAsync(x => x.IdGame == roundId);
      
            if (round == null) return;

            var bets = await _db.Bets.Where(x => x.IdGame == roundId).ToListAsync();
                
              
            foreach (var bet in bets)
            {
                if (bet.BetType == round.Result)
                {
                    decimal win = bet.Amount * 1.95m;

                    bet.WinAmount = win;

                    var user = await _db.Users
                        .FirstOrDefaultAsync(x => x.Id == bet.IdUser);

                    if (user != null)
                    {
                        user.Coin += win;
                    }
                }
            }

            await _db.SaveChangesAsync();
        }

        // tung xúc sắc
        public (int, int, int) RollDice()
        {
            Random random = new Random();

            int dice1 = random.Next(1, 7);
            int dice2 = random.Next(1, 7);
            int dice3 = random.Next(1, 7);

            return (dice1, dice2, dice3);
        }

        public async Task SetResult(Guid roundId)
        {
            var round = await _db.GameRounds .FirstOrDefaultAsync(x => x.IdGame == roundId);
      
            if (round == null) return;

            var dice = RollDice();

            round.Dice1 = dice.Item1;
            round.Dice2 = dice.Item2;
            round.Dice3 = dice.Item3;

            int sum = round.Dice1 + round.Dice2 + round.Dice3;

            round.Result = sum >= 11 ? "Tai" : "Xiu";

            round.EndTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
