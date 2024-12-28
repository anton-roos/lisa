namespace Lisa.Services;

public class UserService
{
    public List<string> GetUserPermissions()
    {
        return new List<string> { "view_home", "view_counter", "view_fetchdata", "view_learners", "view_schools", "view_reports", "view_dashboard" };
    }
}