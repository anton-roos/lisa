using Lisa.Models.Entities;

namespace Lisa.Interfaces;

public interface ISchoolService
{
    Task<School?> SetCurrentSchoolAsync(Guid? schoolId);
    Task<School?> GetSchoolAsync(Guid id);
    Task<School?> GetCurrentSchoolAsync();
    Task<List<School>> GetAllSchoolsAsync();
    Task<List<SchoolType>> GetSchoolTypesAsync();
    Task<List<SchoolCurriculum>> GetSchoolCurriculumsAsync();
    Task<bool> AddSchoolAsync(School school);
    Task<bool> UpdateAsync(School school);
    Task<bool> DeleteSchoolAsync(School school);
}
