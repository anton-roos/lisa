using Microsoft.EntityFrameworkCore;

namespace Lisa.Data;

public class LisaDbContext : DbContext
{
    public LisaDbContext(DbContextOptions<LisaDbContext> options)
        : base(options) { }

    public DbSet<School> Schools { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Learner> Learners { get; set; }
    public DbSet<LearnerParent> LearnerParents { get; set; }
    public DbSet<RegisterClass> RegisterClasses { get; set; }
    public DbSet<SubjectCombination> SubjectCombinations { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<SubjectCombinationSubject> SubjectCombinationSubjects { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Principal> Principals { get; set; }
    public DbSet<Administrator> Administrators { get; set; }
    public DbSet<SchoolManagement> SchoolManagements { get; set; }
    public DbSet<SystemAdministrator> SystemAdministrators { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<TeacherSubject> TeacherSubjects { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<SessionSubject> SessionSubjects { get; set; }
    public DbSet<Result> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // School
        modelBuilder.Entity<School>()
            .HasKey(s => s.Id);

        // Grade
        modelBuilder.Entity<Grade>()
            .HasKey(g => g.Id);
        modelBuilder.Entity<Grade>()
            .HasIndex(g => g.SchoolId);

        // Learner
        modelBuilder.Entity<Learner>()
            .HasKey(l => l.Id);
        modelBuilder.Entity<Learner>()
            .HasIndex(l => l.RegisterClassId);
        modelBuilder.Entity<Learner>()
            .HasIndex(l => l.LearnerCode);

        // LearnerParent
        modelBuilder.Entity<LearnerParent>()
            .HasKey(lp => lp.Id);
        modelBuilder.Entity<LearnerParent>()
            .HasIndex(lp => lp.LearnerId);
        modelBuilder.Entity<LearnerParent>()
            .HasIndex(lp => lp.Email);

        // RegisterClass
        modelBuilder.Entity<RegisterClass>()
            .HasKey(rc => rc.Id);
        modelBuilder.Entity<RegisterClass>()
            .HasIndex(rc => rc.GradeId);
        modelBuilder.Entity<RegisterClass>()
            .HasIndex(rc => rc.SubjectCombinationId);

        // SubjectCombination
        modelBuilder.Entity<SubjectCombination>()
            .HasKey(sc => sc.Id);

        // Subject
        modelBuilder.Entity<Subject>()
            .HasKey(s => s.Id);

        // SubjectCombinationSubject
        modelBuilder.Entity<SubjectCombinationSubject>()
            .HasKey(sc => new { sc.SubjectCombinationId, sc.SubjectId });
        modelBuilder.Entity<SubjectCombinationSubject>()
            .HasIndex(sc => new { sc.SubjectCombinationId, sc.SubjectId });

        // User
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        modelBuilder.Entity<User>()
            .HasIndex(u => u.LastName);

        // Principal
        modelBuilder.Entity<Principal>()
            .HasBaseType<User>();
        modelBuilder.Entity<Principal>()
            .HasOne(p => p.School)
            .WithMany()
            .HasForeignKey(p => p.SchoolId);

        // Administrator
        modelBuilder.Entity<Administrator>()
            .HasBaseType<User>();
        modelBuilder.Entity<Administrator>()
            .HasOne(a => a.School)
            .WithMany()
            .HasForeignKey(a => a.SchoolId);

        // SchoolManagement
        modelBuilder.Entity<SchoolManagement>()
            .HasBaseType<User>();
        modelBuilder.Entity<SchoolManagement>()
            .HasOne(sm => sm.School)
            .WithMany()
            .HasForeignKey(sm => sm.SchoolId);

        // SystemAdministrator
        modelBuilder.Entity<SystemAdministrator>()
            .HasBaseType<User>();

        // Teacher
        modelBuilder.Entity<Teacher>()
            .HasBaseType<User>();
        modelBuilder.Entity<Teacher>()
            .HasOne(t => t.School)
            .WithMany()
            .HasForeignKey(t => t.SchoolId);

        // TeacherSubject
        modelBuilder.Entity<TeacherSubject>()
            .HasKey(ts => new { ts.TeacherId, ts.SubjectId });

        // Session
        modelBuilder.Entity<Session>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<Session>()
            .HasIndex(s => s.TeacherId);
        modelBuilder.Entity<Session>()
            .HasIndex(s => new { s.TeacherId, s.SessionStartTime, s.SessionEndTime })
            .IsUnique();

        // SessionSubject
        modelBuilder.Entity<SessionSubject>()
            .HasKey(ss => new { ss.SessionId, ss.SubjectId, ss.TeacherId });
        modelBuilder.Entity<SessionSubject>()
            .HasIndex(ss => new { ss.SessionId, ss.SubjectId });

        // Result
        modelBuilder.Entity<Result>()
            .HasKey(r => r.Id);
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.LearnerId);
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.SubjectId);
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.Score);

        // Relationships
        modelBuilder.Entity<Learner>()
            .HasOne(l => l.RegisterClass)
            .WithMany()
            .HasForeignKey(l => l.RegisterClassId);

        modelBuilder.Entity<LearnerParent>()
            .HasOne(lp => lp.Learner)
            .WithMany()
            .HasForeignKey(lp => lp.LearnerId);

        modelBuilder.Entity<RegisterClass>()
            .HasOne(rc => rc.Grade)
            .WithMany()
            .HasForeignKey(rc => rc.GradeId);

        modelBuilder.Entity<RegisterClass>()
            .HasOne(rc => rc.SubjectCombination)
            .WithMany()
            .HasForeignKey(rc => rc.SubjectCombinationId);

        modelBuilder.Entity<Teacher>()
            .HasOne(t => t.School)
            .WithMany(s => s.Teachers)
            .HasForeignKey(t => t.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Teacher>()
            .HasMany(t => t.TeacherSubjects)
            .WithOne(ts => ts.Teacher)
            .HasForeignKey(ts => ts.TeacherId);

        modelBuilder.Entity<Session>()
            .HasOne(s => s.Teacher)
            .WithMany()
            .HasForeignKey(s => s.TeacherId);

        modelBuilder.Entity<SessionSubject>()
            .HasOne(ss => ss.Session)
            .WithMany()
            .HasForeignKey(ss => ss.SessionId);

        modelBuilder.Entity<SessionSubject>()
            .HasOne(ss => ss.Subject)
            .WithMany()
            .HasForeignKey(ss => ss.SubjectId);

        modelBuilder.Entity<SessionSubject>()
            .HasOne(ss => ss.Teacher)
            .WithMany()
            .HasForeignKey(ss => ss.TeacherId);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Learner)
            .WithMany()
            .HasForeignKey(r => r.LearnerId);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Subject)
            .WithMany()
            .HasForeignKey(r => r.SubjectId);
    }
}

public class SystemAdministrator : User
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public School School { get; set; }
}

public class SchoolManagement : User
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public School School { get; set; }

}

public class Administrator : User
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public School School { get; set; }

}


public class Principal : User
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public School School { get; set; }
}

public class School
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }

    public ICollection<Grade> Grades { get; set; }
    public ICollection<RegisterClass> RegisterClasses { get; set; }
    public ICollection<Teacher> Teachers { get; set; }
}

public class Grade
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int SequenceNumber { get; set; }
    public int SchoolId { get; set; }

    public School School { get; set; }
    public ICollection<RegisterClass> RegisterClasses { get; set; }
}

public class Learner
{
    public int Id { get; set; }
    public string LearnerCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RegisterClassId { get; set; }

    public RegisterClass RegisterClass { get; set; }
    public ICollection<LearnerParent> LearnerParents { get; set; }
    public ICollection<Result> Results { get; set; }
}

public class LearnerParent
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CellNumber { get; set; }
    public int LearnerId { get; set; }

    public Learner Learner { get; set; }
}

public class RegisterClass
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int GradeId { get; set; }
    public int SubjectCombinationId { get; set; }

    public Grade Grade { get; set; }
    public SubjectCombination SubjectCombination { get; set; }
    public ICollection<Learner> Learners { get; set; }
}

public class SubjectCombination
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<SubjectCombinationSubject> SubjectCombinationSubjects { get; set; }
}

public class SubjectCombinationSubject
{
    public int SubjectCombinationId { get; set; }
    public int SubjectId { get; set; }

    public SubjectCombination SubjectCombination { get; set; }
    public Subject Subject { get; set; }
}

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
}

public class Teacher : User
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public School School { get; set; }
    public ICollection<TeacherSubject> TeacherSubjects { get; set; }
}

public class TeacherSubject
{
    public int TeacherId { get; set; }
    public int SubjectId { get; set; }

    public Teacher Teacher { get; set; }
    public Subject Subject { get; set; }
}

public class Session
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public DateTime SessionStartTime { get; set; }
    public DateTime SessionEndTime { get; set; }
    public string Description { get; set; }
    public SessionStatus Status { get; set; }

    public Teacher Teacher { get; set; }
    public ICollection<SessionSubject> SessionSubjects { get; set; }
}

public enum SessionStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public class SessionSubject
{
    public int SessionId { get; set; }
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }

    public Session Session { get; set; }
    public Subject Subject { get; set; }
    public Teacher Teacher { get; set; }
}

public class Result
{
    public int Id { get; set; }
    public int LearnerId { get; set; }
    public int SubjectId { get; set; }
    public decimal Score { get; set; }
    public DateTime ResultDate { get; set; }

    public Learner Learner { get; set; }
    public Subject Subject { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
