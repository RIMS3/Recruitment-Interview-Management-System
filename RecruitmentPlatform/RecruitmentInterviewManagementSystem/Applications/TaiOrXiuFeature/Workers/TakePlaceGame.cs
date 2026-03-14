using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature;
using RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature.HubResult;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature.Workers
{
    public class TakePlaceGame : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<TaiXiuHub> _hub;
        private readonly ILogger<TakePlaceGame> _logger;
        private const string ROOM = "taixiu-room";

        public TakePlaceGame(
            IServiceScopeFactory scopeFactory,
            IHubContext<TaiXiuHub> hub , ILogger<TakePlaceGame> logger)
        {
            _scopeFactory = scopeFactory;
            _hub = hub;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var gameService = scope.ServiceProvider
                    .GetRequiredService<IGameRounds>();

                try
                {
                    // 1️⃣ Tạo round
                    var round = await gameService.CreateRound();

                    Console.WriteLine($"Round started: {round.IdGame}");

                    // gửi realtime cho frontend
                    await _hub.Clients.Group(ROOM)
                        .SendAsync("RoundStart", round.IdGame);

                    // 2️⃣ countdown 20s
                    for (int i = 40; i >= 0; i--)
                    {
                        await _hub.Clients.Group(ROOM)
                            .SendAsync("Countdown", i);
                        //_logger.LogInformation($"Countdown: {i} seconds remaining");
                        await Task.Delay(1000, stoppingToken);
                    }

                    // 3️⃣ đóng cược
                    await gameService.CloseRound(round.IdGame);

                    Console.WriteLine($"Round closed: {round.IdGame}");

                    await _hub.Clients.Group(ROOM)
                        .SendAsync("RoundClosed");

                    // 4️⃣ tung xúc xắc + set result
                    await gameService.SetResult(round.IdGame);

                    var currentRound = await gameService.GetCurrentRound();

                    Console.WriteLine($"Dice rolled");

                    // lấy kết quả từ DB
                    using var scope2 = _scopeFactory.CreateScope();
                    var db = scope2.ServiceProvider.GetRequiredService<FakeTopcvContext>();

                    var resultRound = await db.GameRounds
                        .FindAsync(round.IdGame);

                    if (resultRound != null)
                    {
                        // gửi kết quả cho frontend
                        await _hub.Clients.Group(ROOM)
                            .SendAsync("Result", new
                            {
                                resultRound.Dice1,
                                resultRound.Dice2,
                                resultRound.Dice3,
                                resultRound.Result
                            });
                    }

                    // 5️⃣ trả thưởng
                    await gameService.PayReward(round.IdGame);

                    Console.WriteLine($"Reward paid");

                    // 6️⃣ nghỉ 15s trước round mới
                    for (int i = 15; i >= 0; i--)
                    {
                        await _hub.Clients.Group(ROOM)
                            .SendAsync("NextRoundCountdown", i);

                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Game loop error: {ex.Message}");
                }
            }
        }
    }
}