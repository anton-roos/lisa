using Lisa.Models.Entities;

namespace Lisa.Models.EmailModels;

public class ProgressFeedback
{
    public string LearnerName { get; set; } = string.Empty;
    public Dictionary<string, List<Result>> ResultsBySubject { get; set; } = [];
}

public class ProgressFeedbackListItem
{
    public Guid LearnerId { get; set; }
    public string Surname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
