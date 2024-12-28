using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lisa.Services;

public class SchoolService
{
    private readonly NavigationManager _navigationManager;


    public SchoolService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }


    public string GetSchoolName()
    {
        string currentUrl = _navigationManager.Uri; // Get the current URL
        string relativeUrl = currentUrl.Replace(_navigationManager.BaseUri, ""); // Remove base URL

        return relativeUrl switch
        {
            "broadlands" => "Broadlands High",
            "greenacres" => "Greenacres Academy",
            "destiny" => "Destiny College",
            "impact" => "Impact School",
            "distance" => "Distance Learning Center",
            _ => "DCEG Portal" // Default fallback
        };
    }

    public School GetCurrentSchool()
    {
        string currentUrl = _navigationManager.Uri; // Get the current URL
        string relativeUrl = currentUrl.Replace(_navigationManager.BaseUri, "");
        return schools.Find(s => s.Url.Contains(relativeUrl)) ?? new();
    }

    public async Task<School> GetSchoolAsync(int id)
    {
        await Task.Delay(1000); // Simulate a delay
        return await Task.Run(() => schools.Single(s => s.Id == id));
    }


    public List<School> GetSchools() => schools;

    private readonly List<School> schools = new()
    {
        new()
        {
            Id = 1,
            Title = "Impact Independent",
            Description = "Impact Independent High School",
            Image = "https://dcegroup.co.za/wp-content/uploads/2021/01/Impact-10.jpg",
            Url = "/impact",
            Color = "#C00000"
        },
        new()
        {
            Id = 2,
            Title = "Destiny Independent",
            Description = "Destiny Independent School Kempton Park",
            Image = "https://dcegroup.co.za/wp-content/uploads/2021/01/Destiny-Gen-25.jpg",
            Url = "/destiny",
            Color = "#640E27"
        },
        new()
        {
            Id = 3,
            Title = "Broadlands",
            Description = "Broadlands Private School",
            Image ="https://dcegroup.co.za/wp-content/uploads/2024/04/WhatsApp-Image-2024-02-27-at-14.23.24.jpeg" ,
            Url = "/broadlands",
            Color = "#10315E"
        },
        new()
        {
            Id = 4,
            Title = "Greenacres",
            Description = "Greenacres Private College",
            Image = "https://dcegroup.co.za/wp-content/uploads/2024/04/WhatsApp-Image-2024-02-27-at-14.23.18-2.jpeg",
            Url = "/greenacres",
            Color = "#24763D"
        },
        new()
        {
            Id = 5,
            Title = "Dream Distance",
            Description = "Dream Distance Education",
            Image
            ="https://dcegroup.co.za/wp-content/uploads/elementor/thumbs/Home-School-8-p6m2fsewq8zjst76dnf8o3y3ievg9tozcf4qgpca6s.jpg",
            Url = "/distance",
            Color = "#21BDA1"
        }
    };
}


public class School
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? Url { get; set; }
    public string? Color { get; set; }
    public void OnGet(int id)
    {
        Id = id;  // This binds the route parameter to the property
    }
}