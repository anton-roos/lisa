using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class RegisterClassService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    public async Task<RegisterClass?> GetByIdAsync(Guid registerClassId)
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.RegisterClasses
            .Include(rc => rc.Grade)
            .Include(rc => rc.Teacher)
            .Include(rc => rc.CompulsorySubjects)
            .Include(rc => rc.Learners)
            .SingleOrDefaultAsync(rc => rc.Id == registerClassId);
    }

    public async Task<List<RegisterClass>> GetAllAsync()
    {
        try
        {
            var _context = _dbContextFactory.CreateDbContext();

            // Load only required fields to avoid excessive eager loading
            return await _context.RegisterClasses
                .Include(rc => rc.Grade)
                    .ThenInclude(g => g.School)
                .Include(rc => rc.Teacher)
                .Include(rc => rc.CompulsorySubjects) // Use carefully for large datasets
                .Include(rc => rc.Learners)          // Ensure Learners is necessary
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log detailed error
            Console.WriteLine($"Error fetching register classes: {ex.Message}");
            throw;
        }
    }

    public async Task<Combination> RegisterClassAsync(Combination combination)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var existingCombination = await _context.Combinations
            .Include(c => c.Grade)
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.GradeId == combination.GradeId && c.Name == combination.Name);

        if (existingCombination != null)
        {
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

        _context.Combinations.Add(newCombination);
        await _context.SaveChangesAsync();

        return newCombination;
    }

    public Task DeleteAsync(RegisterClass registerClass)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.RegisterClasses.Remove(registerClass);
        return _context.SaveChangesAsync();
    }

    public async Task<RegisterClass> UpdateAsync(RegisterClass registerClass)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.RegisterClasses.Update(registerClass);
        await _context.SaveChangesAsync();
        return registerClass;
    }

    public async Task<RegisterClass> CreateAsync(RegisterClass registerClass)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.RegisterClasses.Add(registerClass);
        await _context.SaveChangesAsync();
        return registerClass;
    }
}
