using Lisa.Models.Entities;

namespace Lisa.Models.EmailModels;

public class ProgressReportModel
{
    public string? ChildName { get; set; }
    public List<Result?> Results { get; set; } = [];
}
