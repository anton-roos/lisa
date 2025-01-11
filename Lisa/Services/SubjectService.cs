using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SubjectService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;

    public async Task<List<Subject>> GetAllAsync()
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Subjects.ToListAsync();
    }

    public async Task CreateAsync(Subject subject)
    {
        var _context = _dbContextFactory.CreateDbContext();
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
    }

    public async Task<Subject?> GetByIdAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        return await _context.Subjects.FindAsync(id);
    }

    public async Task UpdateAsync(Subject subject)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var existing = await _context.Subjects.FindAsync(subject.Id);
        if (existing == null) return;

        existing.Name = subject.Name;
        existing.Code = subject.Code;
        existing.Description = subject.Description;

        await _context.SaveChangesAsync();
    }

    public Task<List<Subject>> GetSubjectsByIdsAsync(IEnumerable<Guid> ids)
    {
        var _context = _dbContextFactory.CreateDbContext();
        return _context.Subjects
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();

        var existing = await _context.Subjects.FindAsync(id);
        if (existing == null) return;

        _context.Subjects.Remove(existing);
        await _context.SaveChangesAsync();
    }
}