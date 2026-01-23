using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class LearnerService
(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    SchoolService schoolService,
    ILogger<LearnerService> logger
)
{
    public async Task<int> GetCountAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners.CountAsync();
    }

    public async Task<int> GetCountAsync(Guid schoolId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners.Where(x => x.SchoolId == schoolId).CountAsync();
    }

    public async Task<Learner?> GetByIdAsync(Guid id, bool activeOnly = false)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var query = context.Learners
            .AsNoTracking()
            .Include(l => l.RegisterClass)
            .ThenInclude(rc => rc!.SchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.PreviousSchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.PreviousRegisterClass)
            .ThenInclude(rc => rc!.SchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.Combination)
            .ThenInclude(c => c!.Subjects)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.CareGroup)
            .Include(l => l.Parents!)
            .Include(l => l.School)
            .Include(l => l.Results!)
            .ThenInclude(r => r.ResultSet)
            .ThenInclude(rs => rs!.Subject)
            .AsQueryable();

        if (activeOnly)
        {
            query = query.Where(l => l.Status == Lisa.Enums.LearnerStatus.Active);
        }

        return await query.FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Learner>> GetBySubjectIdAsync(int subjectId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.LearnerSubjects
            .Where(ls => ls.SubjectId == subjectId)
            .Select(ls => ls.Learner)
            .ToListAsync();
    }

    public async Task<bool> AddLearnerAsync(LearnerViewModel model, List<ParentViewModel> parents, Guid schoolId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();

            var newLearnerId = Guid.NewGuid();
            
            // Get current academic year for the school
            var currentAcademicYearId = await schoolService.GetCurrentAcademicYearIdAsync(schoolId);

            var learnerSubjects = new List<LearnerSubject>();

            foreach (var sid in model.SubjectIds)
            {
                var combId = FindCombinationId(model.CombinationSelections, sid);

                learnerSubjects.Add(new LearnerSubject
                {
                    LearnerId = newLearnerId,
                    SubjectId = sid,
                    CombinationId = combId,
                    AcademicYearId = currentAcademicYearId,
                    LearnerSubjectType = LearnerSubjectType.Combination
                });
            }

            foreach (var extraSubjectId in model.ExtraSubjectIds)
            {
                learnerSubjects.Add(new LearnerSubject
                {
                    LearnerId = newLearnerId,
                    SubjectId = extraSubjectId,
                    AcademicYearId = currentAcademicYearId,
                    LearnerSubjectType = LearnerSubjectType.Additional
                });
            }

            var learner = new Learner
            {
                Id = newLearnerId,
                Status = model.Status,
                CareGroupId = model.CareGroupId,
                CellNumber = model.CellNumber,
                Code = model.Code,
                Email = model.Email,
                Surname = model.Surname,
                IdNumber = model.IdNumber,
                Name = model.Name,
                RegisterClassId = model.RegisterClassId,
                SchoolId = schoolId,
                LearnerSubjects = learnerSubjects,
                MedicalAidName = model.MedicalAidName,
                MedicalAidNumber = model.MedicalAidNumber,
                MedicalAidPlan = model.MedicalAidPlan,
                Allergies = model.Allergies,
                MedicalAilments = model.MedicalAilments,
                MedicalInstructions = model.MedicalInstructions,
                DietaryRequirements = model.DietaryRequirements,
                MedicalTransport = model.MedicalTransport,
                Gender = model.Gender
            };

            await context.Learners.AddAsync(learner);

            var parentEntities = parents.Select(pm => new Parent
            {
                Id = Guid.NewGuid(),
                LearnerId = newLearnerId,
                Surname = pm.Surname,
                Name = pm.Name,
                Relationship = pm.Relationship,
                PrimaryEmail = pm.PrimaryEmail,
                SecondaryEmail = pm.SecondaryEmail,
                PrimaryCellNumber = pm.PrimaryCellNumber,
                SecondaryCellNumber = pm.SecondaryCellNumber,
                WhatsAppNumber = pm.WhatsAppNumber
            }).ToList();

            await context.Parents.AddRangeAsync(parentEntities);

            await context.SaveChangesAsync();

            logger.LogInformation("Added learner {LearnerId} successfully.", newLearnerId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding learner.");
            return false;
        }
    }

    public async Task<List<Learner>> GetByRegisterClassAsync(Guid registerClassId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Where(l => l.RegisterClassId == registerClassId ||
                       (l.PreviousRegisterClassId == registerClassId && 
                        (l.Status == Lisa.Enums.LearnerStatus.Promoted || 
                         l.Status == Lisa.Enums.LearnerStatus.Retained)))
            .Include(l => l.RegisterClass)
            .ThenInclude(rc => rc!.SchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.PreviousRegisterClass)
            .ThenInclude(rc => rc!.SchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.PreviousSchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.Combination)
            .ThenInclude(c => c!.Subjects)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.CareGroup)
            .Include(l => l.Parents!)
            .Include(l => l.School)
            .ToListAsync();
    }

    public async Task<List<Learner>> GetByCareGroupAsync(Guid careGroupId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Where(l => l.CareGroupId == careGroupId)
            .Include(l => l.RegisterClass)
            .ThenInclude(rc => rc!.SchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.Combination)
            .ThenInclude(c => c!.Subjects)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.CareGroup)
            .Include(l => l.Parents!)
            .Include(l => l.School)
            .ToListAsync();
    }

    public async Task<List<Learner>> GetByCombinationAndSubjectAsync(Guid combinationId, int subjectId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.RegisterClass!)
            .ThenInclude(rc => rc.CompulsorySubjects!)
            .Include(l => l.Combination!)
            .ThenInclude(c => c.Subjects!)
            .Where(l => l.LearnerSubjects!.Any(ls => ls.CombinationId == combinationId
                                                     && ls.SubjectId == subjectId))
            .ToListAsync();
    }

    public async Task<List<Learner>> GetLearnersWithTheirSubjectsByGradeAsync(Guid gradeId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Where(l => l.RegisterClass != null && l.RegisterClass.SchoolGradeId == gradeId)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.RegisterClass!)
            .ThenInclude(rc => rc.CompulsorySubjects!)
            .Include(l => l.Combination!)
            .ThenInclude(c => c.Subjects!)
            .ToListAsync();
    }

    public async Task<bool> UpdateLearnerAsync(LearnerViewModel model, List<ParentViewModel> parents)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();

            var learner = await GetLearnerWithParentsAsync(context, model.Id);

            if (learner == null)
            {
                logger.LogWarning("Learner {LearnerId} not found for update.", model.Id);
                return false;
            }

            // Get current academic year for the school
            var currentAcademicYearId = await schoolService.GetCurrentAcademicYearIdAsync(learner.SchoolId);

            UpdateLearnerProperties(learner, model);
            await UpdateParentsAsync(context, learner, parents);
            UpdateLearnerSubjects(context, learner, model, currentAcademicYearId);

            await context.SaveChangesAsync();
            logger.LogInformation("Updated learner {LearnerId} successfully.", model.Id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating learner.");
            return false;
        }
    }

    public async Task<Learner?> GetLearnerWithParentsAsync(Guid? learnerId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();

            return await context.Learners
                .Include(l => l.Parents)
                .Include(l => l.LearnerSubjects)
                .FirstOrDefaultAsync(l => l.Id == learnerId);
        }
        catch
        {
            logger.LogError("Error retrieving learner with parents for ID {LearnerId}.", learnerId);
            return null;
        }
    }

    private static async Task<Learner?> GetLearnerWithParentsAsync(LisaDbContext context, Guid? learnerId)
    {
        return await context.Learners
            .Include(l => l.Parents)
            .Include(l => l.LearnerSubjects)
            .FirstOrDefaultAsync(l => l.Id == learnerId);
    }

    private static void UpdateLearnerProperties(Learner learner, LearnerViewModel model)
    {
        learner.Status = model.Status;
        learner.CareGroupId = model.CareGroupId;
        learner.CellNumber = model.CellNumber;
        learner.Code = model.Code;
        learner.Email = model.Email;
        learner.Surname = model.Surname;
        learner.IdNumber = model.IdNumber;
        learner.Name = model.Name;
        learner.RegisterClassId = model.RegisterClassId;
        learner.MedicalAidName = model.MedicalAidName;
        learner.MedicalAidNumber = model.MedicalAidNumber;
        learner.MedicalAidPlan = model.MedicalAidPlan;
        learner.Allergies = model.Allergies;
        learner.MedicalAilments = model.MedicalAilments;
        learner.MedicalInstructions = model.MedicalInstructions;
        learner.DietaryRequirements = model.DietaryRequirements;
        learner.MedicalTransport = model.MedicalTransport;
        learner.Gender = model.Gender;
    }

    private static async Task UpdateParentsAsync(LisaDbContext context, Learner learner, List<ParentViewModel> parents)
    {
        var existingParentIds = learner.Parents?.Select(p => p.Id).ToList() ?? [];
        var updatedParentIds = parents.Where(p => p.Id.HasValue)
            .Select(p => p.Id!.Value)
            .ToList();

        var parentsToRemove = learner.Parents?.Where(p => !updatedParentIds.Contains(p.Id)).ToList() ?? [];
        context.Parents.RemoveRange(parentsToRemove);

        foreach (var pm in parents)
        {
            if (pm.Id.HasValue && existingParentIds.Contains(pm.Id.Value))
            {
                var parent = learner.Parents?.FirstOrDefault(x => x.Id == pm.Id.Value);
                if (parent != null)
                {
                    UpdateParentProperties(parent, pm);
                }
            }
            else
            {
                var newParent = CreateNewParent(learner.Id, pm);
                await context.Parents.AddAsync(newParent);
            }
        }
    }

    private static void UpdateParentProperties(Parent parent, ParentViewModel pm)
    {
        parent.Surname = pm.Surname;
        parent.Name = pm.Name;
        parent.Relationship = pm.Relationship;
        parent.PrimaryEmail = pm.PrimaryEmail;
        parent.SecondaryEmail = pm.SecondaryEmail;
        parent.PrimaryCellNumber = pm.PrimaryCellNumber;
        parent.SecondaryCellNumber = pm.SecondaryCellNumber;
        parent.WhatsAppNumber = pm.WhatsAppNumber;
    }

    private static Parent CreateNewParent(Guid learnerId, ParentViewModel pm)
    {
        return new Parent
        {
            Id = Guid.NewGuid(),
            LearnerId = learnerId,
            Surname = pm.Surname,
            Name = pm.Name,
            Relationship = pm.Relationship,
            PrimaryEmail = pm.PrimaryEmail,
            SecondaryEmail = pm.SecondaryEmail,
            PrimaryCellNumber = pm.PrimaryCellNumber,
            SecondaryCellNumber = pm.SecondaryCellNumber,
            WhatsAppNumber = pm.WhatsAppNumber
        };
    }

    private static void UpdateLearnerSubjects(LisaDbContext context, Learner learner, LearnerViewModel model, Guid? academicYearId)
    {
        if (learner.LearnerSubjects is not null && learner.LearnerSubjects.Count > 0)
        {
            context.LearnerSubjects.RemoveRange(learner.LearnerSubjects);
        }

        var newSubjects = model.SubjectIds.Select(sid =>
        {
            var combinationId = FindCombinationId(model.CombinationSelections, sid);
            return new LearnerSubject
            {
                LearnerId = learner.Id,
                SubjectId = sid,
                CombinationId = combinationId,
                AcademicYearId = academicYearId,
                LearnerSubjectType = LearnerSubjectType.Combination
            };
        }).ToList();

        var extraSubjects = model.ExtraSubjectIds.Select(extraSid => new LearnerSubject
        {
            LearnerId = learner.Id,
            SubjectId = extraSid,
            AcademicYearId = academicYearId,
            LearnerSubjectType = LearnerSubjectType.Additional
        }).ToList();

        newSubjects.AddRange(extraSubjects);

        context.LearnerSubjects.AddRange(newSubjects);
    }

    public async Task<List<Learner>> GetBySchoolAsync(Guid schoolId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await context.Learners
            .AsSplitQuery()
            .Include(l => l.RegisterClass!)
            .ThenInclude(rc => rc.SchoolGrade!)
            .ThenInclude(sg => sg.SystemGrade)
            .Include(l => l.PreviousSchoolGrade!)
            .ThenInclude(sg => sg.SystemGrade)
            .Include(l => l.PreviousRegisterClass!)
            .ThenInclude(rc => rc.SchoolGrade!)
            .ThenInclude(sg => sg.SystemGrade)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.CareGroup!)
            .Include(l => l.Parents!)
            .Where(l => l.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<List<Learner>> GetLearnersBySchoolWithParentsAsync(Guid schoolId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Include(l => l.Parents)
            .Where(l => l.SchoolId == schoolId && l.Parents!.Any())
            .ToListAsync();
    }

    public async Task<List<Learner>> GetByGradeAsync(Guid gradeId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var learners = await context.Learners
            .Where(l => (l.RegisterClass != null && l.RegisterClass.SchoolGradeId == gradeId) ||
                       (l.PreviousSchoolGradeId == gradeId && 
                        (l.Status == Lisa.Enums.LearnerStatus.Promoted || 
                         l.Status == Lisa.Enums.LearnerStatus.Retained)))
            .Include(l => l.RegisterClass!)
            .ThenInclude(r => r.SchoolGrade!)
            .ThenInclude(sg => sg.SystemGrade)
            .Include(l => l.PreviousSchoolGrade!)
            .ThenInclude(sg => sg.SystemGrade)
            .Include(l => l.PreviousRegisterClass!)
            .ThenInclude(rc => rc.SchoolGrade!)
            .ThenInclude(sg => sg.SystemGrade)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.Parents)
            .ToListAsync();
        return learners;
    }

    public async Task<bool> DisableLearnerAsync(Guid learnerId, string reason)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var learner = await context.Learners
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == learnerId);

            if (learner == null)
            {
                logger.LogWarning("Attempted to disable non-existent learner with ID: {LearnerId}", learnerId);
                return false;
            }

            if (learner.Status == Lisa.Enums.LearnerStatus.Disabled)
            {
                logger.LogWarning("Attempted to disable already disabled learner with ID: {LearnerId}", learnerId);
                return false;
            }

            learner.Status = Lisa.Enums.LearnerStatus.Disabled;
            learner.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            logger.LogInformation("Successfully disabled learner with ID: {LearnerId}, Reason: {Reason}", learnerId, reason);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disabling learner with ID: {LearnerId}", learnerId);
            return false;
        }
    }

    public async Task<bool> EnableLearnerAsync(Guid learnerId)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var learner = await context.Learners
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == learnerId);

            if (learner == null)
            {
                logger.LogWarning("Attempted to enable non-existent learner with ID: {LearnerId}", learnerId);
                return false;
            }

            if (learner.Status != Lisa.Enums.LearnerStatus.Disabled)
            {
                logger.LogWarning("Attempted to enable already active learner with ID: {LearnerId}", learnerId);
                return false;
            }

            learner.Status = Lisa.Enums.LearnerStatus.Active;
            learner.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            logger.LogInformation("Successfully enabled learner with ID: {LearnerId}", learnerId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enabling learner with ID: {LearnerId}", learnerId);
            return false;
        }
    }

    public async Task<IEnumerable<Learner>> GetDisabledLearnersAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .IgnoreQueryFilters()
            .Where(l => l.Status == Lisa.Enums.LearnerStatus.Disabled)
            .Include(l => l.RegisterClass)
            .ThenInclude(rc => rc!.SchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.School)
            .ToListAsync();
    }

    public async Task<IEnumerable<Learner>> GetAllLearnersIncludingDisabledAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .IgnoreQueryFilters()
            .Include(l => l.RegisterClass)
            .ThenInclude(rc => rc!.SchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.CareGroup)
            .Include(l => l.Parents!)
            .Include(l => l.School)
            .ToListAsync();
    }

    public async Task<Learner?> GetByIdIncludingDisabledAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .IgnoreQueryFilters()
            .Include(l => l.RegisterClass)
            .ThenInclude(rc => rc!.SchoolGrade)
            .ThenInclude(sg => sg!.SystemGrade)
            .Include(l => l.Combination)
            .ThenInclude(c => c!.Subjects)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.CareGroup)
            .Include(l => l.Parents!)
            .Include(l => l.School)
            .Include(l => l.Results!)
            .ThenInclude(r => r.ResultSet)
            .ThenInclude(rs => rs!.Subject)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    private static Guid? FindCombinationId(Dictionary<Guid, int> combinationSelections, int subjectId)
    {
        var match = combinationSelections
            .FirstOrDefault(kvp => kvp.Value == subjectId);

        if (match.Key == Guid.Empty)
        {
            return null;
        }

        return match.Key;
    }

    public async Task<List<Learner>> GetByGradeAndSubjectAsync(Guid gradeId, int subjectId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Where(l => l.RegisterClass != null
                        && l.RegisterClass.SchoolGradeId == gradeId
                        && l.RegisterClass.AcademicYear != null
                        && l.RegisterClass.AcademicYear.IsCurrent
                        && l.LearnerSubjects!.Any(ls => ls.SubjectId == subjectId
                            && ls.AcademicYear != null
                            && ls.AcademicYear.IsCurrent))
            .Include(l => l.RegisterClass!)
            .ThenInclude(rc => rc.SchoolGrade!)
            .ThenInclude(sg => sg.SystemGrade)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.Parents)
            .ToListAsync();
    }

    public async Task<List<Learner>> SearchLearnersAsync(Guid schoolId, string searchTerm)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Learners
            .Where(l => l.SchoolId == schoolId
                        && (l.Name!.Contains(searchTerm)
                            || l.Surname!.Contains(searchTerm)))
            .Include(l => l.RegisterClass!)
            .ThenInclude(rc => rc.SchoolGrade!)
            .ThenInclude(sg => sg.SystemGrade)
            .Include(l => l.LearnerSubjects!)
            .ThenInclude(ls => ls.Subject)
            .Include(l => l.Parents)
            .ToListAsync();
    }
}