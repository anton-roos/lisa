using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Services;
using Microsoft.EntityFrameworkCore;

public class TeacherService(IDbContextFactory<LisaDbContext> dbContextFactory, IUiEventService uiEventService)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly IUiEventService _uiEventService = uiEventService;

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
        await _context.DisposeAsync();
        await _uiEventService.PublishAsync(UiEvents.TeachersUpdated);
    }

    public async Task<Teacher?> GetByIdAsync(Guid id)
    {
        await using var _context = _dbContextFactory.CreateDbContext();
        return await _context.Teachers
            .Include(t => t.School)
            .Include(t => t.Subjects)
            .Include(t => t.RegisterClasses!)
                .ThenInclude(rc => rc.Grade)
            .Include(t => t.Periods)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task UpdateAsync(Teacher teacher)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var existing = await _context.Teachers.FindAsync(teacher.Id);
        if (existing == null) return;

        existing.Surname = teacher.Surname;
        existing.Name = teacher.Name;
        existing.Email = teacher.Email;
        existing.PhoneNumber = teacher.PhoneNumber;
        existing.SchoolId = teacher.SchoolId;

        await _context.SaveChangesAsync();
        await _context.DisposeAsync();
        await _uiEventService.PublishAsync(UiEvents.TeachersUpdated);
    }

    public async Task<bool> HasRegisterClassesAsync(Guid TeacherId)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        return await context.RegisterClasses!.AnyAsync(rc => rc.TeacherId == TeacherId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var existing = await _context.Teachers.FindAsync(id);
        if (existing == null) return;

        _context.Teachers.Remove(existing);
        await _context.SaveChangesAsync();
        await _context.DisposeAsync();
        await _uiEventService.PublishAsync(UiEvents.TeachersUpdated);
    }

    public async Task<List<Teacher>> GetTeachersForSchoolAsync(Guid schoolId)
    {
        var _context = _dbContextFactory.CreateDbContext();
        var teachers = await _context.Teachers
            .Where(t => t.SchoolId == schoolId)
            .ToListAsync();
        await _context.DisposeAsync();
        return teachers;
    }

    public async Task<List<Teacher>> GetAvailableTeachersAsync(Guid TeacherId)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        var teacher = context.Teachers.Find(TeacherId);
        if (teacher == null) return new List<Teacher>();
        var availableTeachers = await context.Teachers.Where(t => t.SchoolId == teacher.SchoolId
        && t.Id != TeacherId).ToListAsync();
        return availableTeachers;
    }

    public async Task<bool> TransferRegisterClassesAsync(Guid oldTeacherId, Guid newTeacherId)
    {
        await using var context = _dbContextFactory.CreateDbContext();

        var oldTeacher = await context.Teachers.FindAsync(oldTeacherId);
        if (oldTeacher == null) return false;

        var newTeacher = await context.Teachers.FindAsync(newTeacherId);
        if (newTeacher == null) return false;

        var registerClasses = context.RegisterClasses.Where(rc => rc.TeacherId == oldTeacherId);

        await registerClasses.ForEachAsync(rc => rc.TeacherId = newTeacherId);

        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteTeacherAsync(Guid teacherId)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        var teacher = context.Teachers.Find(teacherId);
        if (teacher == null) return false;
        context.Teachers.Remove(teacher);
        await context.SaveChangesAsync();
        return true;
    }
}
