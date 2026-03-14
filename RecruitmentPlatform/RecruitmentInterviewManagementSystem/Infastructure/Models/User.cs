using RecruitmentInterviewManagementSystem.Infastructure.Models;
using System;
using System.Collections.Generic;

namespace RecruitmentInterviewManagementSystem.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public int? Role { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Salt { get; set; } = null!;

    public decimal Coin { get; set; } = 0;

    public string BeginnerCode { get; set; } = "2HONDAICODON";

    public bool  IsActivedCodeBeginer { get; set; } = false;

    public ICollection<Order> orders { get; set; } = new List<Order>();

    public virtual CandidateProfile? CandidateProfile { get; set; }

    public virtual EmployerProfile? EmployerProfile { get; set; }


    public virtual ICollection<Bets> Bets { get; set; } = new List<Bets>();


    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
