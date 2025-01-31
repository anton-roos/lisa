using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lisa.Services;

public class RegisterClassService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<RegisterClassService> logger)
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
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .AsNoTracking()
                .Include(rc => rc.Grade!)
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
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .Where(rc => rc.Grade != null && rc.Grade.SchoolId == schoolId)
                .Include(rc => rc.Grade!)
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
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.RegisterClasses
                .AsNoTracking()
                .Include(rc => rc.Grade!)
                    .ThenInclude(g => g.School!)
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
            await using var context = await _dbContextFactory.CreateDbContextAsync();
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
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.RegisterClasses.FindAsync(registerClass.Id);

            if (existing == null)
            {
                _logger.LogWarning("Attempted to update non-existent RegisterClass {RegisterClassId}.", registerClass.Id);
                return false;
            }

            existing.Name = registerClass.Name;
            existing.TeacherId = registerClass.TeacherId;
            existing.GradeId = registerClass.GradeId;
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
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var registerClass = await context.RegisterClasses.FindAsync(registerClassId);

            if (registerClass == null)
            {
                _logger.LogWarning("Attempted to delete non-existent RegisterClass {RegisterClassId}.", registerClassId);
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
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var existingCombination = await context.Combinations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.GradeId == combination.GradeId && c.Name == combination.Name);

            if (existingCombination != null)
            {
                _logger.LogInformation("Combination {CombinationName} already exists.", combination.Name);
                return existingCombination;
            }

            var newCombination = new Combination
            {
                Id = Guid.NewGuid(),
                Name = combination.Name,
                GradeId = combination.GradeId,
                Grade = combination.Grade,
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
}
