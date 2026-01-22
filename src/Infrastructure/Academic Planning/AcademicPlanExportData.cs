using Lisa.Models.AcademicPlanning;

namespace Lisa.Infrastructure.AcademicPlanning
{
    public class AcademicPlanExportData
    {
        public TeachingPlan Plan { get; set; } = null!;
        public string SchoolName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}
