using Lisa.Models.Entities;

namespace Lisa.Models.ViewModels;

public class TestEmailViewModel
{
    public Learner? Learner { get; set; }
    public List<User>? Principals { get; set; } = [];
    public School? School { get; set; }
}
