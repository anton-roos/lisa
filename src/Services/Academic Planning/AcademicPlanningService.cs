using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Data;
using AcademicPlanStatusEnum = Lisa.Enums.AcademicPlanStatus;
using Lisa.Models.AcademicPlanning;
using Lisa.Services.AcademicPlanning.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services.AcademicPlanning
{
    public class AcademicPlanningService : IAcademicPlanningService
    {
        private readonly LisaDbContext _db;

        public AcademicPlanningService(LisaDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // Get single plan by combination (school, grade, subject, teacher)
        public async Task<AcademicPlanDto?> GetPlanAsync(Guid schoolId, Guid schoolGradeId, int subjectId, Guid teacherId, CancellationToken cancellationToken = default)
        {
            var plan = await _db.Set<TeachingPlan>()
                .AsNoTracking()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .Where(p =>
                    p.SchoolId == schoolId &&
                    p.SchoolGradeId == schoolGradeId &&
                    p.SubjectId == subjectId &&
                    p.TeacherId == teacherId)
                .FirstOrDefaultAsync(cancellationToken);

            if (plan == null) return null;
            return MapToDto(plan);
        }

        // Get all plans for a school
        public async Task<List<AcademicPlanDto>> GetPlansBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default)
        {
            var plans = await _db.Set<TeachingPlan>()
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

            // check duplicate: same school, grade, subject, teacher
            var exists = await _db.Set<TeachingPlan>().AnyAsync(p =>
                p.SchoolId == dto.SchoolId &&
                p.SchoolGradeId == dto.SchoolGradeId &&
                p.SubjectId == dto.SubjectId &&
                p.TeacherId == dto.TeacherId,
                cancellationToken);

            if (exists)
                throw new InvalidOperationException("An academic plan already exists for this School / Grade / Subject / Teacher combination.");

            var entity = new TeachingPlan
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                SchoolId = dto.SchoolId,
                SchoolGradeId = dto.SchoolGradeId,
                SubjectId = dto.SubjectId,
                TeacherId = dto.TeacherId,
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
                            Resources = p.Resources,
                            Assessment = p.Assessment,
                            Homework = p.Homework,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        weekEntity.Periods.Add(periodEntity);
                    }
                }

                entity.Weeks.Add(weekEntity);
            }

            await _db.Set<TeachingPlan>().AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        // Update plan
        public async Task<bool> UpdatePlanAsync(AcademicPlanDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var plan = await _db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);

            if (plan == null) return false;

            // uniqueness check if key fields changed
            if (plan.SchoolId != dto.SchoolId ||
                plan.SchoolGradeId != dto.SchoolGradeId ||
                plan.SubjectId != dto.SubjectId ||
                plan.TeacherId != dto.TeacherId)
            {
                var duplicate = await _db.Set<TeachingPlan>().AnyAsync(p =>
                    p.Id != dto.Id &&
                    p.SchoolId == dto.SchoolId &&
                    p.SchoolGradeId == dto.SchoolGradeId &&
                    p.SubjectId == dto.SubjectId &&
                    p.TeacherId == dto.TeacherId, cancellationToken);

                if (duplicate)
                    throw new InvalidOperationException("Another academic plan already exists for this School / Grade / Subject / Teacher combination.");
            }

            // update scalars
            plan.SchoolId = dto.SchoolId;
            plan.SchoolGradeId = dto.SchoolGradeId;
            plan.SubjectId = dto.SubjectId;
            plan.TeacherId = dto.TeacherId;
            plan.UpdatedAt = DateTime.UtcNow;

            // sync weeks & periods
            var incomingWeekIds = dto.Weeks.Select(w => w.Id).Where(id => id != Guid.Empty).ToHashSet();
            var weeksToRemove = plan.Weeks.Where(w => !incomingWeekIds.Contains(w.Id)).ToList();
            if (weeksToRemove.Any()) _db.Set<AcademicPlanWeek>().RemoveRange(weeksToRemove);

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
                                Resources = pDto.Resources,
                                Assessment = pDto.Assessment,
                                Homework = pDto.Homework,
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
                    if (periodsToRemove.Any()) _db.Set<AcademicPlanPeriod>().RemoveRange(periodsToRemove);

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
                            existPeriod.Resources = pDto.Resources;
                            existPeriod.Assessment = pDto.Assessment;
                            existPeriod.Homework = pDto.Homework;
                            existPeriod.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeletePlanAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            var plan = await _db.Set<TeachingPlan>().FindAsync(new object[] { planId }, cancellationToken);
            if (plan == null) return false;

            _db.Set<TeachingPlan>().Remove(plan);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        #region Lifecycle methods (using enum alias)
        public async Task SubmitForReviewAsync(Guid planId, Guid userId, CancellationToken cancellationToken = default)
        {
            var plan = await _db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null) throw new InvalidOperationException("Plan not found");
            if (plan.Status != AcademicPlanStatusEnum.Draft) throw new InvalidOperationException("Only drafts can be submitted");

            if (plan.TeacherId != userId)
                throw new UnauthorizedAccessException("Cannot submit another teacher's plan");

            plan.Status = AcademicPlanStatusEnum.PendingReview;
            plan.SubmittedAt = DateTime.UtcNow;
            plan.UpdatedAt = DateTime.UtcNow;
            plan.CurrentVersion++;

            await RecordHistoryAsync(plan, userId);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task ApprovePlanAsync(Guid planId, Guid approverUserId, CancellationToken cancellationToken = default)
        {
            var plan = await _db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null) throw new InvalidOperationException("Plan not found");
            if (plan.Status != AcademicPlanStatusEnum.PendingReview)
                throw new InvalidOperationException("Only plans pending review can be approved");

            plan.Status = AcademicPlanStatusEnum.Published;
            plan.ApprovedAt = DateTime.UtcNow;
            plan.ApprovedByUserId = approverUserId;
            plan.IsLocked = true;
            plan.UpdatedAt = DateTime.UtcNow;
            plan.CurrentVersion++;

            await RecordHistoryAsync(plan, approverUserId);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RejectPlanAsync(Guid planId, Guid approverUserId, string reason, CancellationToken cancellationToken = default)
        {
            var plan = await _db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null) throw new InvalidOperationException("Plan not found");
            if (plan.Status != AcademicPlanStatusEnum.PendingReview)
                throw new InvalidOperationException("Only plans pending review can be rejected");

            plan.Status = AcademicPlanStatusEnum.Draft;
            plan.UpdatedAt = DateTime.UtcNow;
            plan.CurrentVersion++;

            await RecordHistoryAsync(plan, approverUserId);
            await _db.SaveChangesAsync(cancellationToken);
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
                Resources = p.Resources,
                Assessment = p.Assessment,
                Homework = p.Homework
            };
        }

        private async Task RecordHistoryAsync(TeachingPlan plan, Guid changedByUserId)
        {
            var history = new AcademicPlanHistory
            {
                Id = Guid.NewGuid(),
                AcademicPlanId = plan.Id,
                VersionNumber = plan.CurrentVersion,
                Status = (int)plan.Status,
                SnapshotJson = JsonSerializer.Serialize(plan),
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTime.UtcNow
            };

            await _db.Set<AcademicPlanHistory>().AddAsync(history);
        }

        public async Task<TeachingPlan?> GetTeachingPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            return await _db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
        }
        public async Task<List<AcademicPlanHistoryDto>> GetPlanHistoryAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            var history = await _db.Set<AcademicPlanHistory>()
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
            var plan = await _db.Set<TeachingPlan>()
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
                plan.Weeks.Add(week);
            }

            week.Periods.Clear();
            
            foreach (var period in periods)
            {
                period.Id = period.Id == Guid.Empty ? Guid.NewGuid() : period.Id;
                period.AcademicPlanWeekId = week.Id;
                period.CreatedAt = DateTime.UtcNow;
                period.UpdatedAt = DateTime.UtcNow;
                week.Periods.Add(period);
            }

            plan.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            
            return true;
        }
        #endregion
    }
}
