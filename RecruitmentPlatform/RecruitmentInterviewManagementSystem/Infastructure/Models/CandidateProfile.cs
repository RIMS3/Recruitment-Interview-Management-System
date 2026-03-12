using RecruitmentInterviewManagementSystem.Infastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentInterviewManagementSystem.Models;

public partial class CandidateProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public int? Gender { get; set; }
 
    public string? AvatarUrl { get; set; }

    public string? Address { get; set; }

    public int? ExperienceYears { get; set; }

    public decimal? CurrentSalary { get; set; }

    public decimal? DesiredSalary { get; set; }

    public string? JobLevel { get; set; }

    public string? Summary { get; set; }

    public bool IsCvPro { get; set; } = false;
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();

    public virtual ICollection<Cv> Cvs { get; set; } = new List<Cv>();

    public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<BookingLink> BookingLinks { get; set; } = new List<BookingLink>();
}
