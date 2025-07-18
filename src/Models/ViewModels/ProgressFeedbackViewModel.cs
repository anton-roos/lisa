using Lisa.Models.EmailModels;
using Lisa.Models.Entities;

namespace Lisa.Models.ViewModels;

public class ProgressFeedbackViewModel
{
    public ProgressFeedback? Feedback { get; set; }
    public List<User>? Principals { get; set; } = [];
}
