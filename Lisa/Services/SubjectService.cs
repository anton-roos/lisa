using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SubjectService
{
    private readonly LisaDbContext _dbContext;

    public SubjectService(LisaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Subject>> GetAllAsync()
    {
        return await _dbContext.Subjects.ToListAsync();
    }

    public async Task CreateAsync(Subject subject)
    {
        _dbContext.Subjects.Add(subject);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Subject?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Subjects.FindAsync(id);
    }

    public async Task UpdateAsync(Subject subject)
    {
        var existing = await _dbContext.Subjects.FindAsync(subject.Id);
        if (existing == null) return;

        existing.Name = subject.Name;
        existing.Code = subject.Code;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await _dbContext.Subjects.FindAsync(id);
        if (existing == null) return;

        _dbContext.Subjects.Remove(existing);
        await _dbContext.SaveChangesAsync();
    }
}