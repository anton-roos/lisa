using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

public class TeacherService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    public async Task<List<Teacher>> GetAllAsync()
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Teachers.ToListAsync();
    }

    public async Task CreateAsync(Teacher teacher)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
    }

    public async Task<Teacher?> GetByIdAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Teachers
            .Include(t => t.School)
            .Include(t => t.Subjects)
            .Include(t => t.RegisterClasses).ThenInclude(rc => rc.Grade)
            .Include(t => t.Periods)
            .FirstOrDefaultAsync(t => t.Id == id); ;
    }

    public async Task UpdateAsync(Teacher teacher)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var existing = await _context.Teachers.FindAsync(teacher.Id);
        if (existing == null) return;

        existing.FirstName = teacher.FirstName;
        existing.LastName = teacher.LastName;
        existing.Email = teacher.Email;
        existing.PhoneNumber = teacher.PhoneNumber;
        existing.SchoolId = teacher.SchoolId;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var existing = await _context.Teachers.FindAsync(id);
        if (existing == null) return;

        _context.Teachers.Remove(existing);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Teacher>> GetTeachersForSchoolAsync(Guid schoolId)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var teachers = await _context.Teachers
            .Where(t => t.SchoolId == schoolId)
            .ToListAsync();
        return teachers;
    }
}