using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

public class TeacherService(LisaDbContext dbContext)
{
    private readonly LisaDbContext _dbContext = dbContext;

    public async Task<List<Teacher>> GetAllAsync()
    {
        return await _dbContext.Teachers.ToListAsync();
    }

    public async Task CreateAsync(Teacher teacher)
    {
        _dbContext.Teachers.Add(teacher);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Teacher?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Teachers
            .Include(t => t.School)
            .Include(t => t.Subjects)
            .Include(t => t.RegisterClasses).ThenInclude(rc => rc.Grade)
            .Include(t => t.Periods)
            .FirstOrDefaultAsync(t => t.Id == id); ;
    }

    public async Task UpdateAsync(Teacher teacher)
    {
        var existing = await _dbContext.Teachers.FindAsync(teacher.Id);
        if (existing == null) return;

        existing.FirstName = teacher.FirstName;
        existing.LastName = teacher.LastName;
        existing.Email = teacher.Email;
        existing.PhoneNumber = teacher.PhoneNumber;
        existing.SchoolId = teacher.SchoolId;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await _dbContext.Teachers.FindAsync(id);
        if (existing == null) return;

        _dbContext.Teachers.Remove(existing);
        await _dbContext.SaveChangesAsync();
    }
}