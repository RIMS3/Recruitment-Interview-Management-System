using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Infastructure.Models;

namespace RecruitmentInterviewManagementSystem.Models;

public partial class FakeTopcvContext : DbContext
{
    public FakeTopcvContext()
    {
    }

    public FakeTopcvContext(DbContextOptions<FakeTopcvContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<CandidateProfile> CandidateProfiles { get; set; }

    public virtual DbSet<CandidateSkill> CandidateSkills { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<CompanySubscription> CompanySubscriptions { get; set; }

    public virtual DbSet<Cv> Cvs { get; set; }

    public virtual DbSet<CvCertificate> CvCertificates { get; set; }

    public virtual DbSet<CvEducation> CvEducations { get; set; }

    public virtual DbSet<CvExperience> CvExperiences { get; set; }

    public virtual DbSet<CvProject> CvProjects { get; set; }

    public virtual DbSet<CvSkill> CvSkills { get; set; }

    public virtual DbSet<EmployerProfile> EmployerProfiles { get; set; }

    public virtual DbSet<JobPost> JobPosts { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<SavedJob> SavedJobs { get; set; }

    public virtual DbSet<ServicePackage> ServicePackages { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<User> Users { get; set; }


    public virtual DbSet<Interviews> Interview { get; set; }

    public virtual DbSet<InterviewsSlots> InterviewsSlots { get; set; }
    public virtual DbSet<Banner> Banners { get; set; }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Applicat__3214EC07582676B4");

            entity.HasIndex(e => new { e.JobId, e.CandidateId }, "UQ_Application").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AppliedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Cvid).HasColumnName("CVId");

            entity.HasOne(d => d.Candidate).WithMany(p => p.Applications)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__Candi__123EB7A3");

            entity.HasOne(d => d.Cv).WithMany(p => p.Applications)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicatio__CVId__1332DBDC");

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__JobId__114A936A");
        });

        modelBuilder.Entity<CandidateProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Candidat__3214EC071E63B6B1");

            entity.HasIndex(e => e.UserId, "UQ__Candidat__1788CC4DB5D0361F").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CurrentSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DesiredSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.JobLevel).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.CandidateProfile)
                .HasForeignKey<CandidateProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Candidate_User");
        });

        modelBuilder.Entity<CandidateSkill>(entity =>
        {
            entity.HasKey(e => new { e.CandidateId, e.SkillId }).HasName("PK__Candidat__B2A99284D1268521");

            entity.HasOne(d => d.Candidate).WithMany(p => p.CandidateSkills)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Candidate__Candi__72C60C4A");

            entity.HasOne(d => d.Skill).WithMany(p => p.CandidateSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Candidate__Skill__73BA3083");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Companie__3214EC07BB89D494");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.TaxCode).HasMaxLength(50);
            entity.Property(e => e.Website).HasMaxLength(255);
        });

        modelBuilder.Entity<CompanySubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CompanyS__3214EC074F42971F");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Company).WithMany(p => p.CompanySubscriptions)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CompanySu__Compa__2FCF1A8A");

            entity.HasOne(d => d.ServicePackage).WithMany(p => p.CompanySubscriptions)
                .HasForeignKey(d => d.ServicePackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CompanySu__Servi__30C33EC3");
        });

        modelBuilder.Entity<Cv>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CVs__3214EC074E6E5E70");

            entity.ToTable("CVs");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CurrentSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EducationSummary).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Field).HasMaxLength(255);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.Property(e => e.Nationality).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Candidate).WithMany(p => p.Cvs)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CVs__CandidateId__797309D9");
        });

        modelBuilder.Entity<CvCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CV_Certi__3214EC075FC69D71");

            entity.ToTable("CV_Certificates");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CertificateName).HasMaxLength(255);
            entity.Property(e => e.Cvid).HasColumnName("CVId");
            entity.Property(e => e.Organization).HasMaxLength(255);

            entity.HasOne(d => d.Cv).WithMany(p => p.CvCertificates)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CV_Certifi__CVId__0B91BA14");
        });

        modelBuilder.Entity<CvEducation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CV_Educa__3214EC070F7A288C");

            entity.ToTable("CV_Educations");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Cvid).HasColumnName("CVId");
            entity.Property(e => e.Major).HasMaxLength(255);
            entity.Property(e => e.SchoolName).HasMaxLength(255);

            entity.HasOne(d => d.Cv).WithMany(p => p.CvEducations)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CV_Educati__CVId__01142BA1");
        });

        modelBuilder.Entity<CvExperience>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CV_Exper__3214EC078322F34E");

            entity.ToTable("CV_Experiences");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.Cvid).HasColumnName("CVId");
            entity.Property(e => e.Position).HasMaxLength(255);

            entity.HasOne(d => d.Cv).WithMany(p => p.CvExperiences)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CV_Experie__CVId__7D439ABD");
        });

        modelBuilder.Entity<CvProject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CV_Proje__3214EC07D91F7C40");

            entity.ToTable("CV_Projects");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Cvid).HasColumnName("CVId");
            entity.Property(e => e.ProjectName).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(255);

            entity.HasOne(d => d.Cv).WithMany(p => p.CvProjects)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CV_Project__CVId__04E4BC85");
        });

        modelBuilder.Entity<CvSkill>(entity =>
        {
            entity.HasKey(e => new { e.Cvid, e.SkillName }).HasName("PK__CV_Skill__BB2F39F444CE8160");

            entity.ToTable("CV_Skills");

            entity.Property(e => e.Cvid).HasColumnName("CVId");
            entity.Property(e => e.SkillName).HasMaxLength(255);

            entity.HasOne(d => d.Cv).WithMany(p => p.CvSkills)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CV_Skills__CVId__07C12930");
        });

        modelBuilder.Entity<EmployerProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employer__3214EC07541EB4AD");

            entity.HasIndex(e => e.UserId, "UQ__Employer__1788CC4D61E45335").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Position).HasMaxLength(255);

            entity.HasOne(d => d.Company).WithMany(p => p.EmployerProfiles)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employer_Company");

            entity.HasOne(d => d.User).WithOne(p => p.EmployerProfile)
                .HasForeignKey<EmployerProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employer_User");
        });

        modelBuilder.Entity<JobPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JobPosts__3214EC07FC730E1F");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.SalaryMax).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SalaryMin).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.ViewCount).HasDefaultValue(0);

            entity.HasOne(d => d.Company).WithMany(p => p.JobPosts)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Job_Company");

            entity.HasMany(d => d.Skills).WithMany(p => p.Jobs)
                .UsingEntity<Dictionary<string, object>>(
                    "JobSkill",
                    r => r.HasOne<Skill>().WithMany()
                        .HasForeignKey("SkillId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__JobSkills__Skill__6FE99F9F"),
                    l => l.HasOne<JobPost>().WithMany()
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__JobSkills__JobId__6EF57B66"),
                    j =>
                    {
                        j.HasKey("JobId", "SkillId").HasName("PK__JobSkill__689C99DA801B13C1");
                        j.ToTable("JobSkills");
                    });
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC07ABC2DDDE");

            entity.HasIndex(e => e.OrderCode, "UQ__Orders__999B5229983DD7F5").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderCode).HasMaxLength(50);
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Employer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.EmployerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__Employer__22751F6C");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderIte__3214EC0752E81A21");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Order__2645B050");

            entity.HasOne(d => d.ServicePackage).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ServicePackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Servi__2739D489");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC07E13485CE");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.TransactionCode).HasMaxLength(255);

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__OrderI__2B0A656D");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07A8E2136C");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedByIp).HasMaxLength(100);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);
            entity.Property(e => e.IsUsed).HasDefaultValue(false);
            entity.Property(e => e.JwtId).HasMaxLength(255);
            entity.Property(e => e.RevokedAt).HasColumnType("datetime");
            entity.Property(e => e.RevokedByIp).HasMaxLength(100);
            entity.Property(e => e.Token).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshToken_User");
        });

        modelBuilder.Entity<SavedJob>(entity =>
        {
            entity.HasKey(e => new { e.CandidateId, e.JobId }).HasName("PK__SavedJob__EF05F290E9B5F635");

            entity.Property(e => e.SavedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Candidate).WithMany(p => p.SavedJobs)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SavedJobs__Candi__17036CC0");

            entity.HasOne(d => d.Job).WithMany(p => p.SavedJobs)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SavedJobs__JobId__17F790F9");
        });

        modelBuilder.Entity<ServicePackage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceP__3214EC07AFF571BD");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Skills__3214EC0767E0594D");

            entity.HasIndex(e => e.Name, "UQ__Skills__737584F6343A08C9").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC074C9A6D4F");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105348C8C58BF").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Salt)
                .HasMaxLength(255)
                .HasDefaultValue("default_salt_value");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
