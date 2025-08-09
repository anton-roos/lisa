namespace Lisa.Models.ViewModels
{
    public class PeriodViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public ICollection<PeriodSubjectViewModel>? PeriodSubjects { get; set; }
    }
}