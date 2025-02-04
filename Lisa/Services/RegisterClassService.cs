using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;

namespace Lisa.Services;

public class RegisterClassService(
    IDbContextFactory<LisaDbContext> dbContextFactory,
    ILogger<RegisterClassService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<RegisterClassService> _logger = logger;

    /// <summary>
    /// Retrieves a RegisterClass by ID.
    /// </summary>
    public async Task<RegisterClass?> GetByIdAsync(Guid registerClassId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .AsNoTracking()
                .Include(rc => rc.SchoolGrade!)
                .Include(rc => rc.Teacher!)
                .Include(rc => rc.CompulsorySubjects!)
                .Include(rc => rc.Learners!)
                .FirstOrDefaultAsync(rc => rc.Id == registerClassId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching RegisterClass with ID: {RegisterClassId}", registerClassId);
            return null;
        }
    }

    /// <summary>
    /// Retrieves a RegisterClass by ID.
    /// </summary>
    public async Task<List<RegisterClass>>? GetBySchoolIdAsync(Guid schoolId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .Where(rc => rc.SchoolGrade != null && rc.SchoolGrade.SchoolId == schoolId)
                .Include(rc => rc.SchoolGrade!)
                .ThenInclude(sg => sg.SystemGrade)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching RegisterClass with ID: {schoolId}", schoolId);
            return [];
        }
    }

    /// <summary>
    /// Retrieves all RegisterClasses.
    /// </summary>
    public async Task<List<RegisterClass>> GetAllAsync()
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .AsNoTracking()
                .Include(rc => rc.SchoolGrade!)
                .ThenInclude(g => g.School!)
                .Include(rc => rc.SchoolGrade!)
                .ThenInclude(g => g.SystemGrade!)
                .Include(rc => rc.Teacher!)
                .Include(rc => rc.CompulsorySubjects!)
                .Include(rc => rc.Learners!)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all RegisterClasses.");
            return new List<RegisterClass>();
        }
    }

    /// <summary>
    /// Creates a new RegisterClass.
    /// </summary>
    public async Task<RegisterClass?> CreateAsync(RegisterClass registerClass)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            await context.RegisterClasses.AddAsync(registerClass);
            await context.SaveChangesAsync();
            _logger.LogInformation("Created new RegisterClass: {RegisterClassId}", registerClass.Id);
            return registerClass;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating RegisterClass.");
            return null;
        }
    }

    /// <summary>
    /// Updates an existing RegisterClass.
    /// </summary>
    public async Task<bool> UpdateAsync(RegisterClass registerClass)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.RegisterClasses.FindAsync(registerClass.Id);

            if (existing == null)
            {
                _logger.LogWarning("Attempted to update non-existent RegisterClass {RegisterClassId}.",
                    registerClass.Id);
                return false;
            }

            existing.Name = registerClass.Name;
            existing.TeacherId = registerClass.TeacherId;
            existing.SchoolGradeId = registerClass.SchoolGradeId;
            existing.CompulsorySubjects = registerClass.CompulsorySubjects;

            context.Entry(existing).State = EntityState.Modified;
            await context.SaveChangesAsync();
            _logger.LogInformation("Updated RegisterClass: {RegisterClassId}", registerClass.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating RegisterClass: {RegisterClassId}", registerClass.Id);
            return false;
        }
    }

    /// <summary>
    /// Deletes a RegisterClass.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid registerClassId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var registerClass = await context.RegisterClasses.FindAsync(registerClassId);

            if (registerClass == null)
            {
                _logger.LogWarning("Attempted to delete non-existent RegisterClass {RegisterClassId}.",
                    registerClassId);
                return false;
            }

            context.RegisterClasses.Remove(registerClass);
            await context.SaveChangesAsync();
            _logger.LogInformation("Deleted RegisterClass: {RegisterClassId}", registerClassId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting RegisterClass: {RegisterClassId}", registerClassId);
            return false;
        }
    }

    /// <summary>
    /// Registers a new class combination.
    /// </summary>
    public async Task<Combination?> RegisterClassAsync(Combination combination)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var existingCombination = await context.Combinations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.SchoolGradeId == combination.SchoolGradeId && c.Name == combination.Name);

            if (existingCombination != null)
            {
                _logger.LogInformation("Combination {CombinationName} already exists.", combination.Name);
                return existingCombination;
            }

            var newCombination = new Combination
            {
                Id = Guid.NewGuid(),
                Name = combination.Name,
                SchoolGradeId = combination.SchoolGradeId,
                SchoolGrade = combination.SchoolGrade,
                Subjects = combination.Subjects
            };

            await context.Combinations.AddAsync(newCombination);
            await context.SaveChangesAsync();
            _logger.LogInformation("Created new Combination: {CombinationId}", newCombination.Id);

            return newCombination;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering Combination {CombinationName}.", combination.Name);
            return null;
        }
    }

    public async Task<bool> SaveRegisterClassAsync(RegisterClassViewModel model, Guid? registerClassId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var selectedSubjects = await context.Subjects
                .Where(s => model.SubjectIds.Contains(s.Id))
                .ToListAsync();

            RegisterClass registerClass;

            if (registerClassId.HasValue)
            {
                // EDIT Mode
                registerClass = await context.RegisterClasses
                    .Include(rc => rc.CompulsorySubjects)
                    .FirstOrDefaultAsync(rc => rc.Id == registerClassId.Value);

                if (registerClass == null)
                {
                    _logger.LogWarning("Attempted to update non-existent RegisterClass {RegisterClassId}.", registerClassId.Value);
                    return false;
                }

                registerClass.Name = model.Name;
                registerClass.SchoolGradeId = model.GradeId;
                registerClass.TeacherId = model.TeacherId;

                registerClass.CompulsorySubjects.Clear();
                registerClass.CompulsorySubjects.AddRange(selectedSubjects);
            }
            else
            {
                // ADD Mode
                registerClass = new RegisterClass
                {
                    Name = model.Name,
                    SchoolGradeId = model.GradeId,
                    CompulsorySubjects = selectedSubjects,
                    TeacherId = model.TeacherId,
                };

                await context.RegisterClasses.AddAsync(registerClass);
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Register Class {Action} successfully: {RegisterClassId}", registerClassId.HasValue ? "updated" : "created", registerClass.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving RegisterClass.");
            return false;
        }
    }
}
