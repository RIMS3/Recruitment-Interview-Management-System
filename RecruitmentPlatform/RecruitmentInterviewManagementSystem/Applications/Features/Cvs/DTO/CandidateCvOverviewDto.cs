namespace RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO
{
    public class CandidateCvOverviewDto
    {
        public bool IsCvPro { get; set; }
        public bool CanCreateNew { get; set; }
        public int CurrentCvCount { get; set; }
        public IEnumerable<CvSummaryDto> Cvs { get; set; } = new List<CvSummaryDto>();
    }
}
