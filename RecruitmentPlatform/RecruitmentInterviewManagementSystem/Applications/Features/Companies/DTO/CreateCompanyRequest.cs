namespace RecruitmentInterviewManagementSystem.Applications.Features.Companies.DTO
{
    public class CreateCompanyRequest
    {
        public string Name { get; set; } = null!;
        public string? TaxCode { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Position { get; set; } // vị trí của recruiter trong công ty
    }
}