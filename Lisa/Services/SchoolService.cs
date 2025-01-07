using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class SchoolService(LisaDbContext context, IServiceProvider serviceProvider, UserManager<User> userManager)
{
    private School? _selectedSchool;
    private List<School>? _schools;
    private readonly LisaDbContext _context = context;
    public event Action? SchoolsUpdated;
    public event Action? SchoolSelected;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly UserManager<User> _userManager = userManager;

    public School? SelectedSchool
    {
        get
        {
            if (_selectedSchool == null)
            {
                InitializeSelectedSchool();
            }
            return _selectedSchool;
        }
        set
        {
            _selectedSchool = value;
            SchoolSelected?.Invoke();
        }
    }

    public List<School>? Schools
    {
        get
        {
            if (_schools == null)
            {
                IntiliazeSchools();
            }
            return _schools;
        }
    }

    public void SetCurrentSchool(Guid schoolId)
    {
        _selectedSchool = _context.Schools.Single(s => s.Id == schoolId);
        SchoolSelected?.Invoke();
    }

    public async Task<School?> GetSchoolAsync(Guid id) => await _context.Schools.FindAsync(id);

    public async Task<List<SchoolType>> GetSchoolTypesAsync() => await _context.SchoolTypes.ToListAsync();

    public async Task<List<SchoolCurriculum>> GetSchoolCurriculumsAsync() => await _context.SchoolCurriculums.ToListAsync();

    public async Task DeleteSchoolAsync(School school)
    {
        _context.Schools.Remove(school);
        await _context.SaveChangesAsync();
        SchoolsUpdated?.Invoke();
    }

    public async Task AddSchoolAsync(School school)
    {
        _context.Schools.Add(school);
        await _context.SaveChangesAsync();
        SchoolsUpdated?.Invoke();
    }

    public async Task UpdateSchoolAsync(School school)
    {
        _context.Schools.Update(school);
        await _context.SaveChangesAsync();
        SchoolsUpdated?.Invoke();
    }

    private void InitializeSelectedSchool()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();

        _selectedSchool = dbContext.Schools.FirstOrDefault();
        SchoolSelected?.Invoke();
        scope.Dispose();
    }

    public void IntiliazeSchools()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        _schools = [.. dbContext.Schools];
        if (_schools != null)
        {
            SchoolsUpdated?.Invoke();
        }
        else
        {
            _schools = [new School { ShortName = "No Schools in DB" }];
        }
        scope.Dispose();
    }

    public async Task<List<Grade>> GetGradesForSchool(Guid schoolId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        var grades = await dbContext.Grades
            .Where(s => s.SchoolId == schoolId)
            .ToListAsync();
        return grades;
    }

    public async Task<Grade?> GetGradeByIdAsync(Guid gradeId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        return await dbContext.Grades.SingleOrDefaultAsync(s => s.Id == gradeId);
    }

    public Task AddGradeToSchool(Grade newGrade)
    {
        _context.Grades.Add(newGrade);
        return _context.SaveChangesAsync();
    }

    public Task UpdateGrade(Grade grade)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        dbContext.Grades.Update(grade);
        return dbContext.SaveChangesAsync();
    }

    public async Task<List<Subject>> GetSubjects()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        var subjects = await dbContext.Subjects.ToListAsync();
        return subjects;
    }

    public async Task<List<User>> GetUsersByRoleAndSchoolAsync(string roleName, Guid schoolId)
    {
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // 1. Get all users in the given role (as base User)
        var allUsers = await userManager.GetUsersInRoleAsync(roleName);

        // 2. Now switch on the role name, cast to the proper derived type, filter by school, then return
        switch (roleName)
        {
            case Roles.Principal:
                {
                    // .OfType<Principal>() returns only those that really are Principals
                    var principals = allUsers
                        .OfType<Principal>()                 // cast to derived type
                        .Where(p => p.SchoolId == schoolId)  // filter by school
                        .ToList();

                    // Return them as a List<User> if your method signature requires that
                    return principals.Cast<User>().ToList();
                }

            case Roles.Administrator:
                {
                    var admins = allUsers
                        .OfType<Administrator>()
                        .Where(a => a.SchoolId == schoolId)
                        .ToList();

                    return admins.Cast<User>().ToList();
                }

            case Roles.SystemAdministrator:
                {
                    var sysAdmins = allUsers
                        .OfType<SystemAdministrator>()
                        .ToList();

                    return sysAdmins.Cast<User>().ToList();
                }

            case Roles.SchoolManagement:
                {
                    var managementUsers = allUsers
                        .OfType<SchoolManagement>()
                        .Where(m => m.SchoolId == schoolId)
                        .ToList();

                    return managementUsers.Cast<User>().ToList();
                }

            default:
                {
                    // If the role name is unrecognized, or you want a fallback
                    // Possibly no casting or no filtering if your base User doesn't store SchoolId
                    // (or if the base user does have a SchoolId, you can do that filter here).
                    // For example:
                    var baseUsers = allUsers
                        .ToList();
                    return baseUsers.Cast<User>().ToList();
                }
        }
    }
    public async Task<List<RegisterClass>> GetRegisterClassesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        var registerClasses = await dbContext.RegisterClasses
            .Include(rc => rc.Grade)
            .Include(rc => rc.Teacher)
            .Include(rc => rc.CompulsorySubjects)
            .Include(rc => rc.Learners)
            .ToListAsync();
        return registerClasses;
    }

    public async Task<RegisterClass?> GetRegisterClassByIdAsync(Guid registerClassId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        return await dbContext.RegisterClasses
            .Include(rc => rc.Grade)
            .Include(rc => rc.Teacher)
            .Include(rc => rc.CompulsorySubjects)
            .Include(rc => rc.Learners)
            .SingleOrDefaultAsync(rc => rc.Id == registerClassId);
    }

    public Task DeleteRegisterClassAsync(RegisterClass registerClass)
    {
        _context.RegisterClasses.Remove(registerClass);
        return _context.SaveChangesAsync();
    }

    public async Task<List<Teacher>> GetTeachersForSchoolAsync(Guid schoolId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        var teachers = await dbContext.Teachers
            .Where(t => t.SchoolId == schoolId)
            .ToListAsync();
        return teachers;
    }

    public async Task UpdateRegisterClassAsync(RegisterClass registerClass)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LisaDbContext>();
        dbContext.RegisterClasses.Update(registerClass);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddRegisterClassAsync(RegisterClass registerClass)
    {
        _context.RegisterClasses.Add(registerClass);
        await _context.SaveChangesAsync();
    }
}