using RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature.DTO;

namespace RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature
{
    public interface IPlaceBet
    {
        Task<bool> Execute(Guid userId, string betType, decimal amount);

        Task<List<BridgeDTO>> GetHistory();
    }
}
