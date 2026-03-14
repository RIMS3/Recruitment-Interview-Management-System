namespace RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature
{
    public class BetRequest
    {
        public Guid  UserId { get; set; }
        public string BetType { get; set; }

        public decimal Amount { get; set; }
    }
}
