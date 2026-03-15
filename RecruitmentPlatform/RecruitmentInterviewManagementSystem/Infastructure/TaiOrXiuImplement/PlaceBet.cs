using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature;
using RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature.DTO;
using RecruitmentInterviewManagementSystem.Infastructure.Models;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.TaiOrXiuImplement
{
    public class PlaceBet : IPlaceBet
    {
        private readonly FakeTopcvContext _db;
        private readonly ILogger<IPlaceBet> _logger;

        public PlaceBet(FakeTopcvContext fakeTopcvContext, ILogger<IPlaceBet> logger)
        {
            _db = fakeTopcvContext;
            _logger = logger;
        }

        public async Task<bool> Execute(Guid userId, string betType, decimal amount)
        {
            var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

                if (user == null) return false;

                if (user.Coin < amount) return false;

                var round = await _db.GameRounds
                    .Where(r => r.IsClosed == false)
                    .OrderByDescending(r => r.StartTime)
                    .FirstOrDefaultAsync();

                if (round == null) return false;

                // trừ tiền
                user.Coin -= amount;

                var bet = new Bets
                {
                    Id = Guid.NewGuid(),
                    IdUser = userId,
                    IdGame = round.IdGame,
                    BetType = betType,
                    Amount = amount,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Bets.Add(bet);

                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation($"User {userId} placed a bet of {amount} on {betType} for round {round.IdGame}");
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<BridgeDTO>> GetHistory()
        {
            return await _db.GameRounds.OrderByDescending(x => x.StartTime).Take(20)
                 .Select(s => new BridgeDTO
                 {
                     Result = s.Result,
                     Dice1 = s.Dice1,
                     Dice2 = s.Dice2,
                     Dice3 = s.Dice3,
                 })
                 .ToListAsync();
        }
    }


}
