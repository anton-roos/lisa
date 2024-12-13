using Lisa.Services;

public class NavigationService
{
    private readonly UserService _userService; // Service to get user info and permissions
    private readonly SchoolService _schoolService; // Service to get school info

    public NavigationService(UserService userService, SchoolService schoolService)
    {
        _userService = userService;
        _schoolService = schoolService;
    }

    public List<NavigationItem> GetNavigationItems()
    {
        // Get the current user's permissions and the school they're associated with
        var userPermissions = _userService.GetUserPermissions();
        var currentSchool = _schoolService.GetCurrentSchool();

        // Define the common navigation items
        var allNavigationItems = new List<NavigationItem>
        {
            new NavigationItem("Home", "/", "oi oi-home", false, "view_home"),
            new NavigationItem("Counter", "counter", "oi oi-plus", false, "view_counter"),
            new NavigationItem("Fetch data", "fetchdata", "oi oi-list-rich", false, "view_fetchdata"),
            new NavigationItem("Learners", "learners", "oi oi-people", false, "view_learners"),
            new NavigationItem("Schools", "schools", "oi oi-book", false, "view_schools")
        };

        // Add school-specific navigation items if a school is selected
        if (!string.IsNullOrEmpty(currentSchool.Url))
        {
            allNavigationItems.Add(new NavigationItem("School Dashboard", "dashboard", "oi oi-dashboard", true, "view_dashboard", new List<string> { currentSchool.Url }));
            allNavigationItems.Add(new NavigationItem("School Reports", "reports", "oi oi-document", true, "view_reports", new List<string> { currentSchool.Url }));
        }

        // Filter items based on user permissions and school-specific availability
        return allNavigationItems
            .Where(item =>
                (string.IsNullOrEmpty(item.Permission) || userPermissions.Contains(item.Permission)) &&
                (item.AllowedSchools.Count == 0 || item.AllowedSchools.Contains(currentSchool.Url)) &&
                (!item.IsSchoolSpecific || currentSchool != null)
            )
            .ToList();
    }
}

public class NavigationItem
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string IconClass { get; set; }
    public string Permission { get; set; }
    public List<string> AllowedSchools { get; set; } // Schools that can see this item

    // New property to handle school-specific items
    public bool IsSchoolSpecific { get; set; } // Whether this item is specific to a school

    public NavigationItem(string title, string url, string iconClass, bool isSchoolSpecific = false, string permission = null, List<string> allowedSchools = null)
    {
        Title = title;
        Url = url;
        IconClass = iconClass;
        IsSchoolSpecific = isSchoolSpecific;
        Permission = permission;
        AllowedSchools = allowedSchools ?? new List<string>();
    }
}