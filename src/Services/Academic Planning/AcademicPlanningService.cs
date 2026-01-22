using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Data;
using AcademicPlanStatusEnum = Lisa.Enums.AcademicPlanStatus;
using Lisa.Models.AcademicPlanning;
using Lisa.Models.Entities;
using Lisa.Services.AcademicPlanning.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services.AcademicPlanning
{
    public class AcademicPlanningService : IAcademicPlanningService
    {
        private readonly IDbContextFactory<LisaDbContext> _dbFactory;

        public AcademicPlanningService(IDbContextFactory<LisaDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        // Get single plan by combination (school, grade, subject, teacher, year, term)
        public async Task<AcademicPlanDto?> GetPlanAsync(Guid schoolId, Guid schoolGradeId, int subjectId, Guid teacherId, Guid academicYearId, int term, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plan = await db.Set<TeachingPlan>()
                .AsNoTracking()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .Where(p =>
                    p.SchoolId == schoolId &&
                    p.SchoolGradeId == schoolGradeId &&
                    p.SubjectId == subjectId &&
                    p.TeacherId == teacherId &&
                    p.AcademicYearId == academicYearId &&
                    p.Term == term)
                .FirstOrDefaultAsync(cancellationToken);

            if (plan == null) return null;
            return MapToDto(plan);
        }

        // Get all plans for a school
        public async Task<List<AcademicPlanDto>> GetPlansBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plans = await db.Set<TeachingPlan>()
                .AsNoTracking()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .Where(p => p.SchoolId == schoolId)
                .ToListAsync(cancellationToken);

            return plans.Select(MapToDto).ToList();
        }

        // Create plan: returns new Plan Id
        public async Task<Guid> CreatePlanAsync(AcademicPlanDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            // check duplicate: same school, grade, subject, teacher, year, term
            var exists = await db.Set<TeachingPlan>().AnyAsync(p =>
                p.SchoolId == dto.SchoolId &&
                p.SchoolGradeId == dto.SchoolGradeId &&
                p.SubjectId == dto.SubjectId &&
                p.TeacherId == dto.TeacherId &&
                p.AcademicYearId == dto.AcademicYearId &&
                p.Term == dto.Term,
                cancellationToken);

            if (exists)
                throw new InvalidOperationException("An academic plan already exists for this School / Grade / Subject / Teacher / Year / Term combination.");

            var entity = new TeachingPlan
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                SchoolId = dto.SchoolId,
                SchoolGradeId = dto.SchoolGradeId,
                SubjectId = dto.SubjectId,
                TeacherId = dto.TeacherId,
                AcademicYearId = dto.AcademicYearId,
                Term = dto.Term,
                IsCatchUpPlan = dto.IsCatchUpPlan,
                OriginalPlanId = dto.OriginalPlanId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = AcademicPlanStatusEnum.Draft
            };

            // map weeks & periods
            foreach (var w in dto.Weeks ?? Enumerable.Empty<AcademicPlanWeekDto>())
            {
                var weekEntity = new AcademicPlanWeek
                {
                    Id = w.Id == Guid.Empty ? Guid.NewGuid() : w.Id,
                    AcademicPlanId = entity.Id,
                    WeekNumber = w.WeekNumber,
                    StartDate = w.StartDate ?? DateTime.MinValue,
                    EndDate = w.EndDate ?? DateTime.MaxValue,
                    Notes = w.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (w.Periods != null)
                {
                    foreach (var p in w.Periods)
                    {
                        var periodEntity = new AcademicPlanPeriod
                        {
                            Id = p.Id == Guid.Empty ? Guid.NewGuid() : p.Id,
                            AcademicPlanWeekId = weekEntity.Id,
                            PeriodNumber = p.PeriodNumber,
                            Topic = p.Topic,
                            SubTopic = p.SubTopic,
                            PercentagePlanned = p.PercentagePlanned,
                            DatePlanned = p.DatePlanned,
                            PercentageCompleted = p.PercentageCompleted,
                            DateCompleted = p.DateCompleted,
                            Resources = p.Resources,
                            LessonDetailDescription = p.LessonDetailDescription,
                            ClassWorkDescription = p.ClassWorkDescription,
                            Homework = p.Homework,
                            Assessment = p.Assessment,
                            AssessmentDescription = p.AssessmentDescription,
                            Notes = p.Notes,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        weekEntity.Periods.Add(periodEntity);
                    }
                }

                entity.Weeks.Add(weekEntity);
            }

            await db.Set<TeachingPlan>().AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        // Update plan
        public async Task<bool> UpdatePlanAsync(AcademicPlanDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plan = await db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);

            if (plan == null) return false;

            // uniqueness check if key fields changed
            if (plan.SchoolId != dto.SchoolId ||
                plan.SchoolGradeId != dto.SchoolGradeId ||
                plan.SubjectId != dto.SubjectId ||
                plan.TeacherId != dto.TeacherId ||
                plan.AcademicYearId != dto.AcademicYearId ||
                plan.Term != dto.Term)
            {
                var duplicate = await db.Set<TeachingPlan>().AnyAsync(p =>
                    p.Id != dto.Id &&
                    p.SchoolId == dto.SchoolId &&
                    p.SchoolGradeId == dto.SchoolGradeId &&
                    p.SubjectId == dto.SubjectId &&
                    p.TeacherId == dto.TeacherId &&
                    p.AcademicYearId == dto.AcademicYearId &&
                    p.Term == dto.Term, cancellationToken);

                if (duplicate)
                    throw new InvalidOperationException("Another academic plan already exists for this School / Grade / Subject / Teacher / Year / Term combination.");
            }

            // update scalars
            plan.SchoolId = dto.SchoolId;
            plan.SchoolGradeId = dto.SchoolGradeId;
            plan.SubjectId = dto.SubjectId;
            plan.TeacherId = dto.TeacherId;
            plan.AcademicYearId = dto.AcademicYearId;
            plan.Term = dto.Term;
            plan.IsCatchUpPlan = dto.IsCatchUpPlan;
            plan.OriginalPlanId = dto.OriginalPlanId;
            plan.UpdatedAt = DateTime.UtcNow;

            // sync weeks & periods
            var incomingWeekIds = dto.Weeks.Select(w => w.Id).Where(id => id != Guid.Empty).ToHashSet();
            var weeksToRemove = plan.Weeks.Where(w => !incomingWeekIds.Contains(w.Id)).ToList();
            if (weeksToRemove.Any()) db.Set<AcademicPlanWeek>().RemoveRange(weeksToRemove);

            foreach (var wDto in dto.Weeks)
            {
                var existingWeek = plan.Weeks.FirstOrDefault(w => w.Id == wDto.Id);
                if (existingWeek == null)
                {
                    // add new week
                    var newWeek = new AcademicPlanWeek
                    {
                        Id = wDto.Id == Guid.Empty ? Guid.NewGuid() : wDto.Id,
                        AcademicPlanId = plan.Id,
                        WeekNumber = wDto.WeekNumber,
                        StartDate = wDto.StartDate ?? DateTime.MinValue,
                        EndDate = wDto.EndDate ?? DateTime.MaxValue,
                        Notes = wDto.Notes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    if (wDto.Periods != null)
                    {
                        foreach (var pDto in wDto.Periods)
                        {
                            var newPeriod = new AcademicPlanPeriod
                            {
                                Id = pDto.Id == Guid.Empty ? Guid.NewGuid() : pDto.Id,
                                AcademicPlanWeekId = newWeek.Id,
                                PeriodNumber = pDto.PeriodNumber,
                                Topic = pDto.Topic,
                                SubTopic = pDto.SubTopic,
                                PercentagePlanned = pDto.PercentagePlanned,
                                DatePlanned = pDto.DatePlanned,
                                PercentageCompleted = pDto.PercentageCompleted,
                                DateCompleted = pDto.DateCompleted,
                                Resources = pDto.Resources,
                                LessonDetailDescription = pDto.LessonDetailDescription,
                                ClassWorkDescription = pDto.ClassWorkDescription,
                                Homework = pDto.Homework,
                                Assessment = pDto.Assessment,
                                AssessmentDescription = pDto.AssessmentDescription,
                                Notes = pDto.Notes,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            newWeek.Periods.Add(newPeriod);
                        }
                    }

                    plan.Weeks.Add(newWeek);
                }
                else
                {
                    // update existing week
                    existingWeek.WeekNumber = wDto.WeekNumber;
                    existingWeek.StartDate = wDto.StartDate ?? DateTime.MinValue;
                    existingWeek.EndDate = wDto.EndDate ?? DateTime.MaxValue;
                    existingWeek.Notes = wDto.Notes;
                    existingWeek.UpdatedAt = DateTime.UtcNow;

                    var incomingPeriodIds = (wDto.Periods ?? Enumerable.Empty<AcademicPlanPeriodDto>())
                        .Select(p => p.Id).Where(id => id != Guid.Empty).ToHashSet();
                    var periodsToRemove = existingWeek.Periods.Where(p => !incomingPeriodIds.Contains(p.Id)).ToList();
                    if (periodsToRemove.Any()) db.Set<AcademicPlanPeriod>().RemoveRange(periodsToRemove);

                    foreach (var pDto in wDto.Periods ?? Enumerable.Empty<AcademicPlanPeriodDto>())
                    {
                        var existPeriod = existingWeek.Periods.FirstOrDefault(p => p.Id == pDto.Id);
                        if (existPeriod == null)
                        {
                            var newPeriod = new AcademicPlanPeriod
                            {
                                Id = pDto.Id == Guid.Empty ? Guid.NewGuid() : pDto.Id,
                                AcademicPlanWeekId = existingWeek.Id,
                                PeriodNumber = pDto.PeriodNumber,
                                Topic = pDto.Topic,
                                Resources = pDto.Resources,
                                Assessment = pDto.Assessment,
                                Homework = pDto.Homework,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            existingWeek.Periods.Add(newPeriod);
                        }
                        else
                        {
                            existPeriod.PeriodNumber = pDto.PeriodNumber;
                            existPeriod.Topic = pDto.Topic;
                            existPeriod.SubTopic = pDto.SubTopic;
                            existPeriod.PercentagePlanned = pDto.PercentagePlanned;
                            existPeriod.DatePlanned = pDto.DatePlanned;
                            existPeriod.PercentageCompleted = pDto.PercentageCompleted;
                            existPeriod.DateCompleted = pDto.DateCompleted;
                            existPeriod.Resources = pDto.Resources;
                            existPeriod.LessonDetailDescription = pDto.LessonDetailDescription;
                            existPeriod.ClassWorkDescription = pDto.ClassWorkDescription;
                            existPeriod.Homework = pDto.Homework;
                            existPeriod.Assessment = pDto.Assessment;
                            existPeriod.AssessmentDescription = pDto.AssessmentDescription;
                            existPeriod.Notes = pDto.Notes;
                            existPeriod.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeletePlanAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plan = await db.Set<TeachingPlan>().FindAsync(new object[] { planId }, cancellationToken);
            if (plan == null) return false;

            db.Set<TeachingPlan>().Remove(plan);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        #region Lifecycle methods (using enum alias)
        public async Task SubmitForReviewAsync(Guid planId, Guid userId, bool isSystemAdministrator = false, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plan = await db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null) throw new InvalidOperationException("Plan not found");
            if (plan.Status != AcademicPlanStatusEnum.Draft) throw new InvalidOperationException("Only drafts can be submitted");

            // System Administrators can submit any plan, teachers can only submit their own
            if (!isSystemAdministrator && plan.TeacherId != userId)
                throw new UnauthorizedAccessException("Cannot submit another teacher's plan");

            plan.Status = AcademicPlanStatusEnum.PendingReview;
            plan.SubmittedAt = DateTime.UtcNow;
            plan.UpdatedAt = DateTime.UtcNow;
            plan.CurrentVersion++;

            await RecordHistoryAsync(db, plan, userId);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task ApprovePlanAsync(Guid planId, Guid approverUserId, bool isSystemAdministrator = false, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plan = await db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null) throw new InvalidOperationException("Plan not found");
            
            // System Administrators can approve Draft or PendingReview plans directly
            // Regular users can only approve PendingReview plans
            if (isSystemAdministrator)
            {
                if (plan.Status != AcademicPlanStatusEnum.Draft && plan.Status != AcademicPlanStatusEnum.PendingReview)
                    throw new InvalidOperationException("Only draft or pending review plans can be approved");
            }
            else
            {
                if (plan.Status != AcademicPlanStatusEnum.PendingReview)
                    throw new InvalidOperationException("Only plans pending review can be approved");
            }

            plan.Status = AcademicPlanStatusEnum.Approved;
            plan.ApprovedAt = DateTime.UtcNow;
            plan.ApprovedByUserId = approverUserId;
            plan.IsLocked = true;
            plan.UpdatedAt = DateTime.UtcNow;
            plan.CurrentVersion++;

            await RecordHistoryAsync(db, plan, approverUserId);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task RejectPlanAsync(Guid planId, Guid approverUserId, string reason, bool isSystemAdministrator = false, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plan = await db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null) throw new InvalidOperationException("Plan not found");
            
            // System Administrators can reject Draft or PendingReview plans
            // Regular users can only reject PendingReview plans
            if (isSystemAdministrator)
            {
                if (plan.Status != AcademicPlanStatusEnum.Draft && plan.Status != AcademicPlanStatusEnum.PendingReview)
                    throw new InvalidOperationException("Only draft or pending review plans can be rejected");
            }
            else
            {
                if (plan.Status != AcademicPlanStatusEnum.PendingReview)
                    throw new InvalidOperationException("Only plans pending review can be rejected");
            }

            plan.Status = AcademicPlanStatusEnum.Rejected;
            plan.UpdatedAt = DateTime.UtcNow;
            plan.CurrentVersion++;
            
            // Store rejection reason in history
            await RecordHistoryAsync(db, plan, approverUserId, $"Rejected: {reason}");
            await db.SaveChangesAsync(cancellationToken);
        }
        #endregion

        #region Mapping helpers
        private AcademicPlanDto MapToDto(TeachingPlan entity)
        {
            return new AcademicPlanDto
            {
                Id = entity.Id,
                SchoolId = entity.SchoolId,
                SchoolGradeId = entity.SchoolGradeId,
                SubjectId = entity.SubjectId,
                TeacherId = entity.TeacherId,
                AcademicYearId = entity.AcademicYearId,
                Term = entity.Term,
                IsCatchUpPlan = entity.IsCatchUpPlan,
                OriginalPlanId = entity.OriginalPlanId,
                Weeks = entity.Weeks?.OrderBy(w => w.WeekNumber).Select(MapWeekToDto).ToList() ?? new List<AcademicPlanWeekDto>()
            };
        }

        private AcademicPlanWeekDto MapWeekToDto(AcademicPlanWeek w)
        {
            return new AcademicPlanWeekDto
            {
                Id = w.Id,
                WeekNumber = w.WeekNumber,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                Notes = w.Notes,
                Periods = w.Periods?.OrderBy(p => p.PeriodNumber).Select(MapPeriodToDto).ToList() ?? new List<AcademicPlanPeriodDto>()
            };
        }

        private AcademicPlanPeriodDto MapPeriodToDto(AcademicPlanPeriod p)
        {
            return new AcademicPlanPeriodDto
            {
                Id = p.Id,
                PeriodNumber = p.PeriodNumber,
                Topic = p.Topic,
                SubTopic = p.SubTopic,
                PercentagePlanned = p.PercentagePlanned,
                DatePlanned = p.DatePlanned,
                PercentageCompleted = p.PercentageCompleted,
                DateCompleted = p.DateCompleted,
                Resources = p.Resources,
                LessonDetailDescription = p.LessonDetailDescription,
                ClassWorkDescription = p.ClassWorkDescription,
                Homework = p.Homework,
                Assessment = p.Assessment,
                AssessmentDescription = p.AssessmentDescription,
                Notes = p.Notes
            };
        }

        private async Task RecordHistoryAsync(LisaDbContext db, TeachingPlan plan, Guid changedByUserId, string? notes = null)
        {
            // Create a DTO to avoid circular reference issues when serializing
            var planSnapshot = new
            {
                Id = plan.Id,
                SchoolId = plan.SchoolId,
                SchoolGradeId = plan.SchoolGradeId,
                SubjectId = plan.SubjectId,
                TeacherId = plan.TeacherId,
                AcademicYearId = plan.AcademicYearId,
                Term = plan.Term,
                Status = plan.Status,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                SubmittedAt = plan.SubmittedAt,
                ApprovedAt = plan.ApprovedAt,
                ApprovedByUserId = plan.ApprovedByUserId,
                CurrentVersion = plan.CurrentVersion,
                IsLocked = plan.IsLocked,
                IsCatchUpPlan = plan.IsCatchUpPlan,
                OriginalPlanId = plan.OriginalPlanId,
                Weeks = plan.Weeks?.Select(w => new
                {
                    Id = w.Id,
                    WeekNumber = w.WeekNumber,
                    StartDate = w.StartDate,
                    EndDate = w.EndDate,
                    Notes = w.Notes,
                    Periods = w.Periods?.Select(p => new
                    {
                        Id = p.Id,
                        PeriodNumber = p.PeriodNumber,
                        Topic = p.Topic,
                        SubTopic = p.SubTopic,
                        PercentagePlanned = p.PercentagePlanned,
                        DatePlanned = p.DatePlanned,
                        PercentageCompleted = p.PercentageCompleted,
                        DateCompleted = p.DateCompleted,
                        LessonDetailDescription = p.LessonDetailDescription,
                        ClassWorkDescription = p.ClassWorkDescription,
                        Homework = p.Homework,
                        Assessment = p.Assessment,
                        AssessmentDescription = p.AssessmentDescription,
                        Notes = p.Notes,
                        Resources = p.Resources,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    }).ToList()
                }).ToList()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            var history = new AcademicPlanHistory
            {
                Id = Guid.NewGuid(),
                AcademicPlanId = plan.Id,
                VersionNumber = plan.CurrentVersion,
                Status = (int)plan.Status,
                SnapshotJson = JsonSerializer.Serialize(planSnapshot, options),
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTime.UtcNow,
                Notes = notes
            };

            await db.Set<AcademicPlanHistory>().AddAsync(history);
        }

        public async Task<TeachingPlan?> GetTeachingPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
        }
        public async Task<List<AcademicPlanHistoryDto>> GetPlanHistoryAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var history = await db.Set<AcademicPlanHistory>()
                .Where(h => h.AcademicPlanId == planId)
                .OrderBy(h => h.VersionNumber)
                .ToListAsync(cancellationToken);

            return history.Select(h => new AcademicPlanHistoryDto
            {
                Id = h.Id,
                VersionNumber = h.VersionNumber,
                Status = h.Status,
                SnapshotJson = h.SnapshotJson,
                ChangedByUserId = h.ChangedByUserId,
                ChangedAt = h.ChangedAt
            }).ToList();
        }
        public async Task<bool> SavePlanPeriodsAsync(Guid planId, List<AcademicPlanPeriod> periods, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var plan = await db.Set<TeachingPlan>()
                    .Include(p => p.Weeks)
                        .ThenInclude(w => w.Periods)
                    .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

                if (plan == null) return false;

                var week = plan.Weeks.FirstOrDefault();
                if (week == null)
                {
                    week = new AcademicPlanWeek
                    {
                        Id = Guid.NewGuid(),
                        AcademicPlanId = planId,
                        WeekNumber = 1,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(7),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    db.Set<AcademicPlanWeek>().Add(week);
                    await db.SaveChangesAsync(cancellationToken); // Save week first to get the ID
                }
                else
                {
                    // Ensure existing week dates are UTC
                    if (week.StartDate.HasValue && week.StartDate.Value.Kind != DateTimeKind.Utc)
                    {
                        week.StartDate = week.StartDate.Value.ToUniversalTime();
                    }
                    if (week.EndDate.HasValue && week.EndDate.Value.Kind != DateTimeKind.Utc)
                    {
                        week.EndDate = week.EndDate.Value.ToUniversalTime();
                    }
                }

                // Remove existing periods from database
                var existingPeriods = await db.Set<AcademicPlanPeriod>()
                    .Where(p => p.AcademicPlanWeekId == week.Id)
                    .ToListAsync(cancellationToken);
                
                if (existingPeriods.Any())
                {
                    db.Set<AcademicPlanPeriod>().RemoveRange(existingPeriods);
                    await db.SaveChangesAsync(cancellationToken);
                }
                
                // Add new periods
                foreach (var period in periods)
                {
                    // Ensure Topic has a value (required field) - database requires NOT NULL
                    var topicValue = !string.IsNullOrWhiteSpace(period.Topic) 
                        ? period.Topic.Trim() 
                        : $"Period {period.PeriodNumber}"; // Default topic - always non-null

                    // Ensure topic doesn't exceed max length
                    if (topicValue.Length > 200)
                    {
                        topicValue = topicValue.Substring(0, 200);
                    }

                // Ensure all DateTime values are UTC for PostgreSQL
                var createdAt = period.CreatedAt == default(DateTime) 
                    ? DateTime.UtcNow 
                    : period.CreatedAt.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(period.CreatedAt, DateTimeKind.Utc) 
                        : period.CreatedAt.ToUniversalTime();
                
                var datePlanned = period.DatePlanned.HasValue
                    ? (period.DatePlanned.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(period.DatePlanned.Value, DateTimeKind.Utc)
                        : period.DatePlanned.Value.ToUniversalTime())
                    : (DateTime?)null;
                
                var dateCompleted = period.DateCompleted.HasValue
                    ? (period.DateCompleted.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(period.DateCompleted.Value, DateTimeKind.Utc)
                        : period.DateCompleted.Value.ToUniversalTime())
                    : (DateTime?)null;

                var newPeriod = new AcademicPlanPeriod
                {
                    Id = period.Id == Guid.Empty ? Guid.NewGuid() : period.Id,
                    AcademicPlanWeekId = week.Id,
                    PeriodNumber = period.PeriodNumber,
                    Topic = topicValue, // Always non-null and non-empty
                    SubTopic = !string.IsNullOrWhiteSpace(period.SubTopic) ? period.SubTopic.Trim() : null,
                    PercentagePlanned = period.PercentagePlanned,
                    DatePlanned = datePlanned,
                    PercentageCompleted = period.PercentageCompleted,
                    DateCompleted = dateCompleted,
                    Resources = period.Resources,
                    LessonDetailDescription = period.LessonDetailDescription,
                    ClassWorkDescription = period.ClassWorkDescription,
                    Homework = period.Homework,
                    Assessment = period.Assessment,
                    AssessmentDescription = period.AssessmentDescription,
                    Notes = !string.IsNullOrWhiteSpace(period.Notes) && period.Notes.Length > 1000 
                        ? period.Notes.Substring(0, 1000) 
                        : period.Notes,
                    CreatedAt = createdAt,
                    UpdatedAt = DateTime.UtcNow
                };
                    
                    db.Set<AcademicPlanPeriod>().Add(newPeriod);
                }

                plan.UpdatedAt = DateTime.UtcNow;
                
                // Ensure plan dates are UTC
                if (plan.CreatedAt.Kind != DateTimeKind.Utc)
                {
                    plan.CreatedAt = plan.CreatedAt.ToUniversalTime();
                }
                
                await db.SaveChangesAsync(cancellationToken);
                
                return true;
            }
            catch (Exception ex)
            {
                // Build detailed error message with inner exception
                var errorMessage = $"Error saving plan periods: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\nInner Exception (2): {ex.InnerException.InnerException.Message}";
                    }
                }
                errorMessage += $"\nStack Trace: {ex.StackTrace}";
                
                // Throw a new exception with the detailed message
                throw new InvalidOperationException(errorMessage, ex);
            }
        }

        public async Task<List<AcademicPlanDisplayDto>> GetPlansForDisplayAsync(Guid? schoolId, Guid? teacherId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var query = db.Set<TeachingPlan>().AsNoTracking().AsQueryable();

            if (schoolId.HasValue)
            {
                query = query.Where(p => p.SchoolId == schoolId.Value);
            }

            if (teacherId.HasValue)
            {
                query = query.Where(p => p.TeacherId == teacherId.Value);
            }

            var plans = await query.ToListAsync(cancellationToken);

            // Load related data
            var schoolIds = plans.Select(p => p.SchoolId).Distinct().ToList();
            var schoolGradeIds = plans.Select(p => p.SchoolGradeId).Distinct().ToList();
            var subjectIds = plans.Select(p => p.SubjectId).Distinct().ToList();
            var teacherIds = plans.Select(p => p.TeacherId).Distinct().ToList();
            var academicYearIds = plans.Select(p => p.AcademicYearId).Distinct().ToList();

            var schools = await db.Set<School>().Where(s => schoolIds.Contains(s.Id)).ToListAsync(cancellationToken);
            var schoolGrades = await db.Set<SchoolGrade>()
                .Include(sg => sg.SystemGrade)
                .Where(sg => schoolGradeIds.Contains(sg.Id))
                .ToListAsync(cancellationToken);
            var subjects = await db.Set<Subject>().Where(s => subjectIds.Contains(s.Id)).ToListAsync(cancellationToken);
            var teachers = await db.Set<User>().Where(u => teacherIds.Contains(u.Id)).ToListAsync(cancellationToken);
            var academicYears = await db.Set<AcademicYear>().Where(ay => academicYearIds.Contains(ay.Id)).ToListAsync(cancellationToken);

            return plans.Select(p =>
            {
                var school = schools.FirstOrDefault(s => s.Id == p.SchoolId);
                var schoolGrade = schoolGrades.FirstOrDefault(sg => sg.Id == p.SchoolGradeId);
                var subject = subjects.FirstOrDefault(s => s.Id == p.SubjectId);
                var teacher = teachers.FirstOrDefault(u => u.Id == p.TeacherId);
                var academicYear = academicYears.FirstOrDefault(ay => ay.Id == p.AcademicYearId);

                return new AcademicPlanDisplayDto
                {
                    Id = p.Id,
                    SchoolName = school?.LongName ?? "Unknown",
                    GradeName = schoolGrade?.SystemGrade?.Name ?? "Unknown",
                    SubjectName = subject?.Name ?? "Unknown",
                    TeacherName = $"{teacher?.Name} {teacher?.Surname}".Trim(),
                    Year = academicYear?.Year ?? DateTime.UtcNow.Year,
                    Term = p.Term,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsCatchUpPlan = p.IsCatchUpPlan
                };
            }).ToList();
        }
        #endregion
    }
}
