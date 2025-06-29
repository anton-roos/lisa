using Lisa.Domain.Models.EmailModels;

namespace Lisa.Domain.Models.ViewModels;

public class ProgressFeedbackViewModel
{
    public ProgressFeedback? Feedback { get; set; }
    public List<User>? Principals { get; set; } = [];
}
