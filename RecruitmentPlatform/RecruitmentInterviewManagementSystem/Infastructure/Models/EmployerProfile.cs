using System;
using System.Collections.Generic;

namespace RecruitmentInterviewManagementSystem.Models;

public partial class EmployerProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string? AvatarUrl { get; set; }

    public Guid CompanyId { get; set; }

    public string? Position { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}
