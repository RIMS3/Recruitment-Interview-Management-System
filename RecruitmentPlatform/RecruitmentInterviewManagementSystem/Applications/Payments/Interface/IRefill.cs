using RecruitmentInterviewManagementSystem.Applications.Payments.DTO;

namespace RecruitmentInterviewManagementSystem.Applications.Payments.Interface
{
    public interface IRefill
    {
        Task<ResponseRefillDTO> Execute(RefillDTO request);

        Task<decimal> Execute(Guid idUser);
    }
}
