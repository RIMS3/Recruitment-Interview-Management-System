using System.ComponentModel.DataAnnotations;

namespace RecruitmentInterviewManagementSystem.DTOs;

public class CRUDCreateJobPostRequest
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Requirement { get; set; }

    public string? Benefit { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Lương tối thiểu không được âm")]
    public decimal? SalaryMin { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Lương tối đa không được âm")]
    public decimal? SalaryMax { get; set; }

    public string? Location { get; set; }

    public int? JobType { get; set; }

    [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
    public DateTime? ExpireAt { get; set; }

    public int Experience { get; set; }
}