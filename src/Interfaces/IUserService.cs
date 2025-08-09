using Lisa.Models.Entities;
using Lisa.Models.ViewModels;

namespace Lisa.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllByRoleAndSchoolAsync(string[] roles, Guid? schoolId);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetTeacherForGradeAndSubjectAsync(Guid? schoolId, Guid gradeId, int subjectId);
    Task UpdateUserSelectedSchool(User user);
    Task<bool> UpdateAsync(UserViewModel user, string? newPassword);
    Task<bool> DeleteAsync(Guid id);
    Task<List<User>> GetBySchoolAsync(Guid schoolId);
    Task<bool> HasRegisterClassesAsync(Guid userId);
    Task<List<User>> GetAvailableTeachersAsync(Guid userId);
    Task<bool> TransferRegisterClassesAsync(Guid oldUserId, Guid newUserId);
    Task<List<User>?> GetLearnerPrincipal(Guid learnerId);
}
