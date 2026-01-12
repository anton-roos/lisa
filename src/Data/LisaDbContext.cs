using Lisa.Interfaces;
using Lisa.Models.Entities;
using Lisa.Models.AcademicPlanning; 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Lisa.Enums;

namespace Lisa.Data;

public class LisaDbContext
(
    DbContextOptions<LisaDbContext> options
) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<School> Schools { get; set; } = null!;
    public DbSet<SchoolType> SchoolTypes { get; set; } = null!;
    public DbSet<SchoolCurriculum> SchoolCurriculums { get; set; } = null!;
    public DbSet<SchoolGrade> SchoolGrades { get; set; } = null!;
    public DbSet<SystemGrade> SystemGrades { get; set; } = null!;
    public DbSet<Learner> Learners { get; set; } = null!;
    public DbSet<Parent> Parents { get; set; } = null!;
    public DbSet<RegisterClass> RegisterClasses { get; set; } = null!;
    public DbSet<Combination> Combinations { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Period> Periods { get; set; } = null!;
    public DbSet<Result> Results { get; set; } = null!;
    public DbSet<CareGroup> CareGroups { get; set; } = null!;
    public DbSet<EmailTemplate> EmailTemplates { get; set; } = null!;
    public DbSet<EventLog> EventLogs { get; set; } = null!;
    public DbSet<LearnerSubject> LearnerSubjects { get; set; } = null!;
    public DbSet<EmailCampaign> EmailCampaigns { get; set; } = null!;
    public DbSet<EmailRecipient> EmailRecipients { get; set; } = null!;
    public DbSet<TeacherSubject> TeacherSubjects { get; set; } = null!;
    public DbSet<ResultSet> ResultSets { get; set; } = null!;
    public DbSet<AssessmentType> AssessmentTypes { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<LeaveEarly> LeaveEarlies { get; set; } = null!;
    public DbSet<AcademicDevelopmentClass> AcademicDevelopmentClasses { get; set; } = null!;
    public DbSet<AdiLearner> AdiLearners { get; set; } = null!;
    public DbSet<AdiSubject> AdiSubjects { get; set; } = null!;
    public DbSet<AdiTeacher> AdiTeachers { get; set; } = null!;
    public DbSet<AcademicPlan> AcademicPlans { get; set; } = null!;
    public DbSet<AcademicPlanWeek> AcademicPlanWeeks {get; set; } = null!;
    public DbSet<AcademicPlanPeriod> AcademicPlanPeriods { get; set; } = null!;
    public DbSet<TeachingPlan> TeachingPlans { get; set; } = null!;
    public DbSet<AcademicYear> AcademicYears { get; set; } = null!;
    public DbSet<LearnerAcademicRecord> LearnerAcademicRecords { get; set; } = null!;
    public DbSet<AcademicPlanHistory> AcademicPlanHistories { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("AspNetUsers");
        modelBuilder.Entity<User>()
        .HasIndex(u => u.Abbreviation);
        modelBuilder.Entity<User>()
            .Property(t => t.Surname)
            .HasMaxLength(50);
        modelBuilder.Entity<User>()
            .Property(t => t.Abbreviation)
            .HasMaxLength(4);
        modelBuilder.Entity<User>()
            .HasOne(t => t.School)
            .WithMany(s => s.Staff)
            .HasForeignKey(t => t.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeacherSubject>()
            .HasKey(st => st.Id);
        modelBuilder.Entity<TeacherSubject>()
            .HasOne(st => st.User)
            .WithMany(t => t.Subjects)
            .HasForeignKey(st => st.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Schools and Associated Entities
        modelBuilder.Entity<School>()
            .HasKey(s => s.Id);
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
            .WithMany(s => s.CareGroups)
            .HasForeignKey(cg => cg.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CareGroup>()
            .HasIndex(cg => cg.Name)
            .IsUnique();
        modelBuilder.Entity<CareGroup>()
            .HasMany(cg => cg.CareGroupMembers)
            .WithOne()
            .HasForeignKey(l => l.CareGroupId);
        modelBuilder.Entity<CareGroup>()
            .HasMany(c => c.Users)
            .WithMany(u => u.CareGroups)
            .UsingEntity<Dictionary<string, object>>(
                "CareGroupUser",
                j => j
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UserId"),
                j => j
                    .HasOne<CareGroup>()
                    .WithMany()
                    .HasForeignKey("CareGroupId")
            );

        modelBuilder.Entity<SystemGrade>()
            .HasKey(sg => sg.Id);

        modelBuilder.Entity<SystemGrade>()
            .HasIndex(sg => sg.Name)
            .IsUnique();

        modelBuilder.Entity<SystemGrade>()
            .Property(sg => sg.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<SchoolGrade>()
            .HasKey(sg => sg.Id);

        modelBuilder.Entity<SchoolGrade>()
            .HasOne(sg => sg.SystemGrade)
            .WithMany()
            .HasForeignKey(sg => sg.SystemGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SchoolGrade>()
            .HasOne(sg => sg.School)
            .WithMany(s => s.SchoolGrades)
            .HasForeignKey(sg => sg.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SchoolGrade>()
            .HasIndex(sg => new { sg.SchoolId, sg.SystemGradeId })
            .IsUnique();

        modelBuilder.Entity<SchoolGrade>()
            .Navigation(sg => sg.SystemGrade)
            .AutoInclude();

        // Learners and Parents
        modelBuilder.Entity<Learner>()
            .HasKey(l => l.Id);
        modelBuilder.Entity<Learner>()
            .HasIndex(l => l.RegisterClassId);
        modelBuilder.Entity<Learner>()
            .HasIndex(l => l.Code);
        modelBuilder.Entity<Learner>()
            .Property(l => l.Code)
            .HasMaxLength(20);
        modelBuilder.Entity<Learner>()
            .HasQueryFilter(l => !l.IsDisabled); // Add query filter to exclude disabled learners
        modelBuilder.Entity<Learner>()
            .HasOne(r => r.RegisterClass)
            .WithMany(l => l.Learners)
            .HasForeignKey(l => l.RegisterClassId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Learner>()
            .HasOne(l => l.CareGroup)
            .WithMany(cg => cg.CareGroupMembers)
            .HasForeignKey(l => l.CareGroupId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Learner>()
            .HasOne(l => l.Combination)
            .WithMany()
            .HasForeignKey(l => l.CombinationId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Learner>()
            .HasOne(l => l.School)
            .WithMany(s => s.Learners)
            .HasForeignKey(l => l.SchoolId);

        modelBuilder.Entity<Parent>()
            .HasKey(lp => lp.Id);
        modelBuilder.Entity<Parent>()
            .HasIndex(lp => lp.LearnerId);
        modelBuilder.Entity<Parent>()
            .HasIndex(lp => lp.PrimaryEmail);
        modelBuilder.Entity<Parent>()
            .HasIndex(lp => lp.SecondaryEmail);
        modelBuilder.Entity<Parent>()
            .HasOne(lp => lp.Learner)
            .WithMany(l => l.Parents)
            .HasForeignKey(lp => lp.LearnerId)
            .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

        // Classes and Subjects
        modelBuilder.Entity<LearnerSubject>()
            .HasKey(ls => new { ls.LearnerId, ls.SubjectId });
        modelBuilder.Entity<LearnerSubject>()
            .HasOne(ls => ls.Learner)
            .WithMany(l => l.LearnerSubjects)
            .HasForeignKey(ls => ls.LearnerId)
            .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict
        modelBuilder.Entity<LearnerSubject>()
            .HasOne(ls => ls.Combination)
            .WithMany()
            .HasForeignKey(ls => ls.CombinationId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LearnerSubject>()
            .Property(ls => ls.LearnerSubjectType)
            .HasConversion<int>();

        modelBuilder.Entity<RegisterClass>()
            .HasKey(rc => rc.Id);
        modelBuilder.Entity<RegisterClass>()
            .HasIndex(rc => rc.SchoolGradeId);
        modelBuilder.Entity<RegisterClass>()
            .HasOne(rc => rc.SchoolGrade)
            .WithMany(g => g.RegisterClasses)
            .HasForeignKey(rc => rc.SchoolGradeId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RegisterClass>()
            .HasOne(rc => rc.User)
            .WithMany(t => t.RegisterClasses)
            .HasForeignKey(rc => rc.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<RegisterClass>()
            .HasIndex(rc => rc.Name);
        modelBuilder.Entity<RegisterClass>()
            .HasMany(rc => rc.CompulsorySubjects)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "RegisterClassCompulsorySubject",
                j => j.HasOne<Subject>()
                    .WithMany()
                    .HasForeignKey("SubjectId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<RegisterClass>()
                    .WithMany()
                    .HasForeignKey("RegisterClassId")
                    .OnDelete(DeleteBehavior.Cascade));

        modelBuilder.Entity<Combination>()
            .HasKey(sc => sc.Id);
        modelBuilder.Entity<Combination>()
            .HasOne(sc => sc.SchoolGrade)
            .WithMany(g => g.Combinations)
            .HasForeignKey(sc => sc.SchoolGradeId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Combination>()
            .HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Combination>()
            .HasMany(c => c.Subjects)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "CombinationSubject",
                j => j.HasOne<Subject>()
                    .WithMany()
                    .HasForeignKey("SubjectId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Combination>()
                    .WithMany()
                    .HasForeignKey("CombinationId")
                    .OnDelete(DeleteBehavior.Cascade));

        modelBuilder.Entity<Subject>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<Subject>()
            .Property(s => s.Id)
            .UseIdentityColumn();
        modelBuilder.Entity<Subject>()
            .HasIndex(s => s.Name);
        modelBuilder.Entity<Subject>()
            .HasIndex(s => s.Code);
        modelBuilder.Entity<Subject>()
            .Property(s => s.Code)
            .HasMaxLength(20);

        // Periods and Schedules
        modelBuilder.Entity<Period>()
            .HasOne(p => p.School)
            .WithMany(s => s.Periods)
            .HasForeignKey(p => p.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Period>()
            .HasOne(p => p.Teacher)
            .WithMany(t => t.Periods)
            .HasForeignKey(p => p.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Period>()
            .HasOne(p => p.Subject)
            .WithMany()
            .HasForeignKey(p => p.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Period>()
            .HasOne(p => p.SchoolGrade)
            .WithMany()
            .HasForeignKey(p => p.SchoolGradeId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Period>()
            .Property(p => p.StartTime)
            .IsRequired();
        modelBuilder.Entity<Period>()
            .Property(p => p.EndTime)
            .IsRequired();
        modelBuilder.Entity<Period>()
            .HasIndex(p => new { p.TeacherId, p.StartTime, p.EndTime })
            .IsUnique();
        modelBuilder.Entity<Period>()
            .Property(p => p.Status)
            .HasConversion<int>()
            .HasDefaultValue(PeriodStatus.Scheduled);
        modelBuilder.Entity<Period>()
            .ToTable(t => t.HasCheckConstraint("CK_Period_StartTime_EndTime", "\"StartTime\" < \"EndTime\""));

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.LearnerId);
            entity.HasIndex(r => r.ResultSetId);
            entity.HasOne(r => r.Learner)
                  .WithMany(l => l.Results)
                  .HasForeignKey(r => r.LearnerId)
                  .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict
            entity.HasOne(r => r.ResultSet)
                  .WithMany(rs => rs.Results)
                  .HasForeignKey(r => r.ResultSetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ResultSet>(entity =>
        {
            entity.HasKey(rs => rs.Id);
            entity.HasOne(rs => rs.Subject)
                  .WithMany()
                  .HasForeignKey(rs => rs.SubjectId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(rs => rs.CapturedByUser)
                  .WithMany()
                  .HasForeignKey(rs => rs.CapturedById)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(rs => rs.Teacher)
                  .WithMany()
                  .HasForeignKey(rs => rs.TeacherId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ResultSet>()
        .Property(rs => rs.AssessmentTypeId)
        .HasDefaultValue(0);

        modelBuilder.Entity<ResultSet>()
        .HasOne(rs => rs.AssessmentType)
        .WithMany()
        .HasForeignKey(rs => rs.AssessmentTypeId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmailCampaign>(entity =>
        {
            entity.ToTable("EmailCampaigns");
            entity.HasKey(ec => ec.Id);
            entity.Property(ec => ec.Id)
                  .ValueGeneratedNever();
            entity.Property(ec => ec.Name)
                  .HasMaxLength(200)
                  .IsRequired(false);
            entity.Property(ec => ec.Description)
                  .HasMaxLength(2000)
                  .IsRequired(false);
            entity.Property(ec => ec.SubjectLine)
                  .HasMaxLength(200)
                  .IsRequired(false);
            entity.Property(ec => ec.Status)
                  .HasConversion<string>()
                  .HasMaxLength(50);
            entity.HasMany(ec => ec.EmailRecipients)
                  .WithOne(r => r.EmailCampaign)
                  .HasForeignKey(r => r.EmailCampaignId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(r => r.CreatedAt)
                  .IsRequired();
            entity.Property(ec => ec.UpdatedAt)
                  .IsRequired();
        });

        modelBuilder.Entity<EmailRecipient>(entity =>
        {
            entity.ToTable("EmailRecipients");

            entity.HasKey(r => r.Id);

            entity.Property(r => r.Id)
                .ValueGeneratedNever();

            entity.Property(r => r.EmailAddress)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasOne(r => r.EmailCampaign)
                .WithMany(ec => ec.EmailRecipients)
                .HasForeignKey(r => r.EmailCampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Parent)
                .WithMany(p => p.EmailReceipts)
                .HasForeignKey(r => r.ParentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(r => r.Learner)
                .WithMany(l => l.EmailReceipts)
                .HasForeignKey(r => r.LearnerId)
                .OnDelete(DeleteBehavior.Restrict); // Changed from SetNull to Restrict

            entity.HasOne(r => r.User)
                .WithMany(u => u.EmailReceipts)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(r => r.CreatedAt)
                .IsRequired();

            entity.Property(r => r.UpdatedAt)
                .IsRequired();
        });

        // Configure AttendanceSession entity
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.SchoolId, s.Start });
            entity.HasIndex(s => s.AcademicDevelopmentClassId);

            entity.HasOne(s => s.School)
                  .WithMany()
                  .HasForeignKey(s => s.SchoolId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.AcademicDevelopmentClass)
                .WithMany()
                .HasForeignKey(s => s.AcademicDevelopmentClassId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(s => s.Start)
                .IsRequired();
        });

        // Configure Attendance entity with relation to AttendanceSession
        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => new { a.LearnerId, a.Start });

            entity.HasOne(a => a.Learner)
                  .WithMany()
                  .HasForeignKey(a => a.LearnerId)
                  .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

            entity.HasOne(a => a.Attendance)
                  .WithMany(a => a.AttendanceRecords)
                  .HasForeignKey(a => a.AttendanceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeaveEarly>(entity =>
        {
           entity.HasOne(le => le.AttendanceRecord)
                 .WithMany()
                 .HasForeignKey(le => le.AttendenceRecordId)
                 .OnDelete(DeleteBehavior.Restrict);

           entity.HasOne(le => le.Learner)
                 .WithMany()
                 .HasForeignKey(le => le.LearnerId)
                 .OnDelete(DeleteBehavior.Restrict);

           entity.HasOne(le => le.SchoolGrade)
                 .WithMany()
                 .HasForeignKey(le => le.SchoolGradeId)
                 .OnDelete(DeleteBehavior.Restrict);
        });

        // Academic Development Classes
        modelBuilder.Entity<AcademicDevelopmentClass>(entity =>
        {
            entity.HasKey(adc => adc.Id);

            entity.Property(adc => adc.DateTime)
                .IsRequired()
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            entity.Property(adc => adc.CreatedAt)
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            entity.Property(adc => adc.UpdatedAt)
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            entity.Property(adc => adc.AdiType)
                .HasConversion<int>()
                .HasDefaultValue(Enums.AdiType.Support);

            entity.HasOne(adc => adc.SchoolGrade)
                .WithMany()
                .HasForeignKey(adc => adc.SchoolGradeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasOne(adc => adc.Subject)
                .WithMany()
                .HasForeignKey(adc => adc.SubjectId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasOne(adc => adc.Teacher)
                .WithMany()
                .HasForeignKey(adc => adc.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(adc => adc.School)
                .WithMany()
                .HasForeignKey(adc => adc.SchoolId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.HasIndex(adc => new { adc.SchoolId, adc.DateTime });

            entity.HasMany(adc => adc.AdiLearners)
                .WithOne(al => al.AcademicDevelopmentClass)
                .HasForeignKey(al => al.AcademicDevelopmentClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(adc => adc.AdiSubjects)
                .WithOne(asub => asub.AcademicDevelopmentClass)
                .HasForeignKey(asub => asub.AcademicDevelopmentClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(adc => adc.AdiTeachers)
                .WithOne(at => at.AcademicDevelopmentClass)
                .HasForeignKey(at => at.AcademicDevelopmentClassId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ADI Subjects (join table for Break ADIs with multiple subjects)
        modelBuilder.Entity<AdiSubject>(entity =>
        {
            entity.HasKey(asub => new { asub.AcademicDevelopmentClassId, asub.SubjectId });

            entity.HasOne(asub => asub.AcademicDevelopmentClass)
                .WithMany(adc => adc.AdiSubjects)
                .HasForeignKey(asub => asub.AcademicDevelopmentClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(asub => asub.Subject)
                .WithMany()
                .HasForeignKey(asub => asub.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ADI Teachers (join table for Break ADIs with multiple teachers)
        modelBuilder.Entity<AdiTeacher>(entity =>
        {
            entity.HasKey(at => new { at.AcademicDevelopmentClassId, at.TeacherId });

            entity.HasOne(at => at.AcademicDevelopmentClass)
                .WithMany(adc => adc.AdiTeachers)
                .HasForeignKey(at => at.AcademicDevelopmentClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(at => at.Teacher)
                .WithMany()
                .HasForeignKey(at => at.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ADI Learners (join table for learners assigned to ADI classes)
        modelBuilder.Entity<AdiLearner>(entity =>
        {
            entity.HasKey(al => al.Id);

            entity.Property(al => al.BreakReason)
                .HasMaxLength(500);

            entity.HasOne(al => al.AcademicDevelopmentClass)
                .WithMany(adc => adc.AdiLearners)
                .HasForeignKey(al => al.AcademicDevelopmentClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(al => al.Learner)
                .WithMany()
                .HasForeignKey(al => al.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(al => new { al.AcademicDevelopmentClassId, al.LearnerId })
                .IsUnique();
        });

        // Configure the TeachingPlan entity to map to the "TeachingPlans" table
        //Changed the entity name from "AcademicPlan" to "TeachingPlan" due to there's already another entity with the same name.
        modelBuilder.Entity<TeachingPlan>(entity =>
        {
            entity.ToTable("TeachingPlans");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.SchoolId, e.SchoolGradeId, e.SubjectId, e.TeacherId })
                .IsUnique()
                .HasDatabaseName("UQ_TeachingPlan_School_Grade_Subject_Teacher");

            entity.HasMany(e => e.Weeks)
                .WithOne(w => w.AcademicPlan)
                .HasForeignKey(w => w.AcademicPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure the AcademicPlanWeek entity to map to the "AcademicPlanWeeks" table
        modelBuilder.Entity<AcademicPlanWeek>(entity =>
        {
            entity.ToTable("AcademicPlanWeeks");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.AcademicPlanId)
                .HasDatabaseName("idx_apweeks_plan");

            entity.HasMany(e => e.Periods)
                .WithOne(p => p.AcademicPlanWeek)
                .HasForeignKey(p => p.AcademicPlanWeekId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure the AcademicPlanPeriod entity to map to the "AcademicPlanPeriods" table
        modelBuilder.Entity<AcademicPlanPeriod>(entity =>
        {
            entity.ToTable("AcademicPlanPeriods");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.AcademicPlanWeekId)
                .HasDatabaseName("idx_apperiods_week");
        });
        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasIndex(a => new { a.SchoolId, a.Year })
                .IsUnique();

            entity.HasIndex(a => new { a.SchoolId, a.IsCurrent });

            entity.HasOne(a => a.School)
                .WithMany()
                .HasForeignKey(a => a.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<LearnerAcademicRecord>(entity =>
        {
            entity.HasKey(lr => lr.Id);

            entity.HasOne(lr => lr.Learner)
                .WithMany()
                .HasForeignKey(lr => lr.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lr => lr.SchoolGrade)
                .WithMany()
                .HasForeignKey(lr => lr.SchoolGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lr => lr.RegisterClass)
                .WithMany()
                .HasForeignKey(lr => lr.RegisterClassId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<TeachingPlan>(entity =>
        {
            entity.Property(e => e.Status)
                .HasConversion<int>();

            entity.Property(e => e.IsLocked)
                .HasDefaultValue(false);

            entity.Property(e => e.CurrentVersion)
                .HasDefaultValue(1);
        });

        modelBuilder.Entity<AcademicPlanHistory>(entity =>
        {
            entity.ToTable("AcademicPlanHistories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.SnapshotJson).IsRequired();
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.AcademicPlanId)
                .HasDatabaseName("idx_academicplanhistory_plan");
        });
        }

    public override int SaveChanges()
    {
        SetAuditProperties();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditProperties()
    {
        foreach (var entry in ChangeTracker.Entries<Learner>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<AcademicDevelopmentClass>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
