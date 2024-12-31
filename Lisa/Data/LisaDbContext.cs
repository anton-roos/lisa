using Microsoft.EntityFrameworkCore;

namespace Lisa.Data;

public class LisaDbContext : DbContext
{
    private readonly ILogger<LisaDbContext> _logger;

    public LisaDbContext(DbContextOptions<LisaDbContext> options, ILogger<LisaDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<SystemAdministrator> SystemAdministrators { get; set; }
    public DbSet<Principal> Principals { get; set; }
    public DbSet<SchoolManagement> SchoolManagements { get; set; }
    public DbSet<Administrator> Administrators { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<SchoolType> SchoolTypes { get; set; }
    public DbSet<SchoolCurriculum> SchoolCurriculums { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Learner> Learners { get; set; }
    public DbSet<LearnerParent> LearnerParents { get; set; }
    public DbSet<RegisterClass> RegisterClasses { get; set; }
    public DbSet<SubjectCombination> SubjectCombinations { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<SubjectCombinationSubject> SubjectCombinationSubjects { get; set; }    public ICollection<Learner>? Learners { get; set; }
    public DbSet<Period> Periods { get; set; }
    public DbSet<Result> Results { get; set; }

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
            .WithMany()
            .HasForeignKey(p => p.SchoolId);

        modelBuilder.Entity<Administrator>()
            .HasBaseType<User>();
        modelBuilder.Entity<Administrator>()
            .HasOne(a => a.School)
            .WithMany()
            .HasForeignKey(a => a.SchoolId);

        modelBuilder.Entity<SchoolManagement>()
            .HasBaseType<User>();
        modelBuilder.Entity<SchoolManagement>()
            .HasOne(sm => sm.School)
            .WithMany()
            .HasForeignKey(sm => sm.SchoolId);

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
            .HasOne(l => l.RegisterClass)
            .WithMany()
            .HasForeignKey(l => l.RegisterClassId)
            .IsRequired(false);
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
            .WithMany()
            .HasForeignKey(lp => lp.LearnerId);

        // Classes and Subjects
        modelBuilder.Entity<RegisterClass>()
            .HasKey(rc => rc.Id);
        modelBuilder.Entity<RegisterClass>()
            .HasIndex(rc => rc.GradeId);
        modelBuilder.Entity<RegisterClass>()
            .HasOne(rc => rc.Grade)
            .WithMany()
            .HasForeignKey(rc => rc.GradeId);
        modelBuilder.Entity<RegisterClass>()
            .HasOne(rc => rc.Teacher)
            .WithMany()
            .HasForeignKey(rc => rc.TeacherId);
        modelBuilder.Entity<RegisterClass>()
            .HasIndex(rc => rc.Name)
            .IsUnique();

        modelBuilder.Entity<SubjectCombination>()
            .HasKey(sc => sc.Id);

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
            .ToTable(t => t.HasCheckConstraint("CK_Period_StartTime_EndTime", "[PeriodStartTime] < [PeriodEndTime]"));

        // Results
        modelBuilder.Entity<Result>()
            .HasKey(r => r.Id);
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.LearnerId);
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.SubjectId);
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Learner)
            .WithMany()
            .HasForeignKey(r => r.LearnerId);
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Subject)
            .WithMany()
            .HasForeignKey(r => r.SubjectId);
        modelBuilder.Entity<Result>()
            .Property(r => r.Score)
            .HasColumnType("decimal(5, 2)");
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

public interface IValidatable
{
    void Validate();
}

public class User
{
    public Guid Id { get; set; }
}

public class SystemAdministrator : User
{
}

public class Principal : User
{
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
}

public class SchoolManagement : User
{
    public Guid SchoolId { get; set; }
    public School? School { get; set; }

}

public class Administrator : User
{
    public Guid SchoolId { get; set; }
    public School? School { get; set; }

}

public class Teacher : User
{
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public ICollection<Subject>? Subjects { get; set; }
}

public class School
{
    public Guid Id { get; set; }
    public string? ShortName { get; set; }
    public string? LongName { get; set; }
    public string? Color { get; set; }
    public Guid SchoolTypeId { get; set; }
    public Guid SchoolCurriculumId { get; set; }
    public SchoolType? SchoolType { get; set; }
    public SchoolCurriculum? Curriculum { get; set; }
    public SmtpDetails? SmtpDetails { get; set; }
    public ICollection<Grade>? Grades { get; set; }
    public ICollection<RegisterClass>? RegisterClasses { get; set; }
    public ICollection<Teacher>? Teachers { get; set; }
    public ICollection<Principal>? Principals { get; set; }
    public ICollection<Administrator>? Administrators { get; set; }
    public ICollection<SchoolManagement>? SchoolManagements { get; set; }
}

public class SmtpDetails
{
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class SchoolType
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}

public class SchoolCurriculum
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class Grade
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int SequenceNumber { get; set; }
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public ICollection<RegisterClass>? RegisterClasses { get; set; }
}

public class Learner
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? RegisterClassId { get; set; }
    public RegisterClass? RegisterClass { get; set; }
    public ICollection<LearnerParent>? LearnerParents { get; set; }
    public ICollection<Result>? Results { get; set; }
    public Guid? CareGroupId { get; set; }
    public CareGroup? CareGroup { get; set; }
    public Guid SubjectCombinationId { get; set; }
    public SubjectCombination? SubjectCombination { get; set; }
}

public class LearnerParent
{
    public Guid Id { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CellNumber { get; set; }
    public Guid LearnerId { get; set; }
    public Learner? Learner { get; set; }
}

public class RegisterClass
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid GradeId { get; set; }
    public Grade? Grade { get; set; }
    public ICollection<Learner>? Learners { get; set; }
    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}

public class SubjectCombination
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public ICollection<SubjectCombinationSubject>? SubjectCombinationSubjects { get; set; }
}

public class SubjectCombinationSubject
{
    public Guid SubjectCombinationId { get; set; }
    public Guid SubjectId { get; set; }
    public SubjectCombination? SubjectCombination { get; set; }
    public Subject? Subject { get; set; }
}

public class Subject
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
}

public class Period : IValidatable
{
    public Guid Id { get; set; }
    public DateTime PeriodStartTime { get; set; }
    public DateTime PeriodEndTime { get; set; }
    public PeriodStatus Status { get; set; }
    public string? Description { get; set; }
    public Guid GradeId { get; set; }
    public Grade? Grade { get; set; }
    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public Guid SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public void Validate()
    {
        if (PeriodStartTime >= PeriodEndTime)
        {
            throw new InvalidOperationException("Period start time must be before end time.");
        }
    }
}

public enum PeriodStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2
}

public class Result
{
    public Guid Id { get; set; }
    public Guid LearnerId { get; set; }
    public Guid SubjectId { get; set; }
    public decimal Score { get; set; }
    public DateTime ResultDate { get; set; }
    public Learner? Learner { get; set; }
    public Subject? Subject { get; set; }
}


public class CareGroup
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid SchoolId { get; set; }
    public School? School { get; set; }
    public ICollection<Learner>? CareGroupMembers { get; set; }
}