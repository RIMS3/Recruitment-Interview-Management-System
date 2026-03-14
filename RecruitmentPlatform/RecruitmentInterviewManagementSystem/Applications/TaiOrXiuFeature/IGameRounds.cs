using RecruitmentInterviewManagementSystem.Infastructure.Models;

namespace RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature
{
    public interface IGameRounds
    {
        Task<GameRounds?> GetCurrentRound();
        Task<GameRounds> CreateRound();

        Task CloseRound(Guid roundId);

        (int, int, int) RollDice();

        Task SetResult(Guid roundId);

        Task PayReward(Guid roundId);
    }
}
