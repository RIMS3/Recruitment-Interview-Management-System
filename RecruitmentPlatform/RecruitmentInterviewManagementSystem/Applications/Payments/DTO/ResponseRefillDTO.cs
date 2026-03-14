namespace RecruitmentInterviewManagementSystem.Applications.Payments.DTO
{
    public class ResponseRefillDTO
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public string QRCode { get; set; }  = string.Empty;
    }
}
