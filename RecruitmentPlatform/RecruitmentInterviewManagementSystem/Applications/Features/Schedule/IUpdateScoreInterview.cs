namespace RecruitmentInterviewManagementSystem.Applications.Features.Schedule
{
    public interface IUpdateScoreInterview
    {
        Task<bool> Execute(UpdateScoreInterivewDTo request);
    }

    public class UpdateScoreInterivewDTo
    {
        public Guid IdInterview { get; set; }
        public decimal Technical { get; set; }
        public decimal SoftSkill { get; set; }
    }
}
