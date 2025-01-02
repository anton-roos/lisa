using Lisa.Interfaces;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Data;

public class LisaDbContext(DbContextOptions<LisaDbContext> options, ILogger<LisaDbContext> logger) 
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    private readonly ILogger<LisaDbContext> _logger = logger;
    public new DbSet<User> Users { get; set; } = null!;
    public DbSet<SystemAdministrator> SystemAdministrators { get; set; } = null!;
    public DbSet<Principal> Principals { get; set; } = null!;
    public DbSet<SchoolManagement> SchoolManagements { get; set; } = null!;
    public DbSet<Administrator> Administrators { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<School> Schools { get; set; } = null!;
    public DbSet<SchoolType> SchoolTypes { get; set; } = null!;
    public DbSet<SchoolCurriculum> SchoolCurriculums { get; set; } = null!;
    public DbSet<Grade> Grades { get; set; } = null!;
    public DbSet<Learner> Learners { get; set; } = null!;
    public DbSet<LearnerParent> LearnerParents { get; set; } = null!;
    public DbSet<RegisterClass> RegisterClasses { get; set; } = null!;
    public DbSet<SubjectCombination> SubjectCombinations { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<SubjectCombinationSubject> SubjectCombinationSubjects { get; set; } = null!;
    public DbSet<Period> Periods { get; set; } = null!;
    public DbSet<Result> Results { get; set; } = null!;
    public DbSet<CareGroup> CareGroups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users and Roles
        modelBuilder.Entity<User>().ToTable("AspNetUsers");
        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("UserType")
            .HasValue<User>("User")
            .HasValue<SystemAdministrator>("SystemAdministrator")
            .HasValue<Principal>("Principal")
            .HasValue<SchoolManagement>("SchoolManagement")
            .HasValue<Administrator>("Administrator")
            .HasValue<Teacher>("Teacher");

        modelBuilder.Entity<Principal>()
            .HasBaseType<User>();
        modelBuilder.Entity<Principal>()
            .HasOne(p => p.School)
            .WithMany(s => s.Principals)
            .HasForeignKey(p => p.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Administrator>()
            .HasBaseType<User>();
        modelBuilder.Entity<Administrator>()
            .HasOne(a => a.School)
            .WithMany(s => s.Administrators)
            .HasForeignKey(a => a.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SchoolManagement>()
            .HasBaseType<User>();
        modelBuilder.Entity<SchoolManagement>()
            .HasOne(sm => sm.School)
            .WithMany(s => s.SchoolManagements)
            .HasForeignKey(sm => sm.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SystemAdministrator>()
            .HasBaseType<User>();

        modelBuilder.Entity<Teacher>()
            .HasBaseType<User>();
        modelBuilder.Entity<Teacher>()
            .Property(t => t.FirstName)
            .HasMaxLength(50);
        modelBuilder.Entity<Teacher>()
            .HasOne(t => t.School)
            .WithMany(s => s.Teachers)
            .HasForeignKey(t => t.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        // Schools and Associated Entities
        modelBuilder.Entity<School>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<School>()
            .OwnsOne(s => s.SmtpDetails);
        modelBuilder.Entity<School>()
            .HasIndex(s => s.ShortName)
            .IsUnique();
        modelBuilder.Entity<School>()
            .Property(s => s.ShortName)
            .HasMaxLength(20);
        modelBuilder.Entity<School>()
            .HasMany(cg => cg.Learners)
            .WithOne(s => s.School)
            .HasForeignKey(l => l.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CareGroup>()
            .HasKey(cg => cg.Id);
        modelBuilder.Entity<CareGroup>()
            .HasIndex(cg => cg.SchoolId);
        modelBuilder.Entity<CareGroup>()
            .HasOne(cg => cg.School)
            .WithMany()
            .HasForeignKey(cg => cg.SchoolId);
        modelBuilder.Entity<CareGroup>()
            .HasIndex(cg => cg.Name)
            .IsUnique();
        modelBuilder.Entity<CareGroup>()
            .HasMany(cg => cg.CareGroupMembers)
            .WithOne()
            .HasForeignKey(l => l.CareGroupId);

        modelBuilder.Entity<Grade>()
            .HasKey(g => g.Id);
        modelBuilder.Entity<Grade>()
            .HasIndex(g => g.SchoolId);

        // Learners and Parents
        modelBuilder.Entity<Learner>()
            .HasKey(l => l.Id);
        modelBuilder.Entity<Learner>()
            .HasIndex(l => l.RegisterClassId);
        modelBuilder.Entity<Learner>()
            .HasIndex(l => l.Code)
            .IsUnique();
        modelBuilder.Entity<Learner>()
            .Property(l => l.Code)
            .HasMaxLength(20);
        modelBuilder.Entity<Learner>()
            .HasOne(r => r.RegisterClass)
            .WithMany(l => l.Learners)
            .HasForeignKey(l => l.RegisterClassId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Learner>()
            .HasOne(l => l.CareGroup)
            .WithMany(cg => cg.CareGroupMembers)
            .HasForeignKey(l => l.CareGroupId)
            .IsRequired(false);
        modelBuilder.Entity<Learner>()
            .HasQueryFilter(l => l.Active);
        modelBuilder.Entity<Learner>()
            .HasOne(l => l.SubjectCombination)
            .WithMany()
            .HasForeignKey(l => l.SubjectCombinationId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Learner>()
            .HasOne(l => l.School)
            .WithMany(s => s.Learners)
            .HasForeignKey(l => l.SchoolId);

        modelBuilder.Entity<LearnerParent>()
            .HasKey(lp => lp.Id);
        modelBuilder.Entity<LearnerParent>()
            .HasIndex(lp => lp.LearnerId);
        modelBuilder.Entity<LearnerParent>()
            .HasIndex(lp => lp.PrimaryEmail);
        modelBuilder.Entity<LearnerParent>()
            .HasIndex(lp => lp.SecondaryEmail);
        modelBuilder.Entity<LearnerParent>()
            .HasOne(lp => lp.Learner)
            .WithMany(l => l.LearnerParents)
            .HasForeignKey(lp => lp.LearnerId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LearnerParent>()
            .HasQueryFilter(lp => lp.Learner!.Active);

        // Classes and Subjects
        modelBuilder.Entity<RegisterClass>()
            .HasKey(rc => rc.Id);
        modelBuilder.Entity<RegisterClass>()
            .HasIndex(rc => rc.GradeId);
        modelBuilder.Entity<RegisterClass>()
            .HasOne(rc => rc.Grade)
            .WithMany(g => g.RegisterClasses)
            .HasForeignKey(rc => rc.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<RegisterClass>()
            .HasOne(rc => rc.Teacher)
            .WithMany()
            .HasForeignKey(rc => rc.TeacherId);
        modelBuilder.Entity<RegisterClass>()
            .HasIndex(rc => rc.Name)
            .IsUnique();
        modelBuilder.Entity<RegisterClass>()
            .HasMany(rc => rc.CompulsorySubjects)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>( 
                "RegisterClassSubject",
                j => j.HasOne<Subject>()
                    .WithMany()
                    .HasForeignKey("SubjectId")
                    .OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<RegisterClass>()
                    .WithMany()
                    .HasForeignKey("RegisterClassId")
                    .OnDelete(DeleteBehavior.Cascade));

        modelBuilder.Entity<SubjectCombination>()
            .HasKey(sc => sc.Id);
        modelBuilder.Entity<SubjectCombination>()
            .HasOne(sc => sc.Grade) // Navigation property in SubjectCombination
            .WithMany(g => g.SubjectCombinations) // Collection in Grade
            .HasForeignKey(sc => sc.GradeId) // Foreign key in SubjectCombination
            .OnDelete(DeleteBehavior.Restrict); // Do not cascade delete

        modelBuilder.Entity<Subject>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<Subject>()
            .HasIndex(s => s.Name)
            .IsUnique();
        modelBuilder.Entity<Subject>()
            .HasIndex(s => s.Code)
            .IsUnique();
        modelBuilder.Entity<Subject>()
            .Property(s => s.Code)
            .HasMaxLength(20);

        modelBuilder.Entity<SubjectCombinationSubject>()
            .HasKey(sc => new { sc.SubjectCombinationId, sc.SubjectId });

        // Periods and Schedules
        modelBuilder.Entity<Period>()
            .HasOne(p => p.Teacher)
            .WithMany()
            .HasForeignKey(p => p.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Period>()
            .HasOne(p => p.Subject)
            .WithMany()
            .HasForeignKey(p => p.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Period>()
            .HasOne(p => p.Grade)
            .WithMany()
            .HasForeignKey(p => p.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Period>()
            .Property(p => p.PeriodStartTime)
            .IsRequired();
        modelBuilder.Entity<Period>()
            .Property(p => p.PeriodEndTime)
            .IsRequired();
        modelBuilder.Entity<Period>()
            .HasIndex(p => new { p.TeacherId, p.PeriodStartTime, p.PeriodEndTime })
            .IsUnique();
        modelBuilder.Entity<Period>()
            .Property(p => p.Status)
            .HasConversion<int>()
            .HasDefaultValue(PeriodStatus.Scheduled);
        modelBuilder.Entity<Period>()
            .ToTable(t => t.HasCheckConstraint("CK_Period_StartTime_EndTime", "\"PeriodStartTime\" < \"PeriodEndTime\""));

        // Results
        modelBuilder.Entity<Result>()
            .HasKey(r => r.Id);
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.LearnerId);
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.SubjectId);
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Learner)
            .WithMany(r => r.Results)
            .HasForeignKey(r => r.LearnerId);
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Subject)
            .WithMany()
            .HasForeignKey(r => r.SubjectId);
        modelBuilder.Entity<Result>()
            .Property(r => r.Score)
            .HasColumnType("decimal(5, 2)");
        modelBuilder.Entity<Result>()
            .HasQueryFilter(r => r.Learner!.Active);
    }


    public override int SaveChanges()
    {
        ValidateEntities();
        SetAuditProperties();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateEntities();
        SetAuditProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditProperties()
    {
        foreach (var entry in ChangeTracker.Entries<Learner>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    private void ValidateEntities()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (entry.Entity is IValidatable validatable)
                {
                    try
                    {
                        validatable.Validate();
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Validation failed for entity of type {EntityType} with values: {CurrentValues}.", entry.Entity.GetType().Name,  entry.CurrentValues);
                        throw new InvalidOperationException($"Validation failed for entity of type {entry.Entity.GetType().Name}.", exception);
                    }
                }
            }
        }
    }
}