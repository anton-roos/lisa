using Microsoft.AspNetCore.Components;

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

    public List<School> GetSchools() => schools;

    private readonly List<School> schools = new()
    {
        new()
        {
            Title = "Impact Independent",
            Description = "Impact Independent High School",
            Image = "https://dcegroup.co.za/wp-content/uploads/2021/01/Impact-10.jpg",
            Url = "/impact",
            Color = "#C00000"
        },
        new()
        {
            Title = "Destiny Independent",
            Description = "Destiny Independent School Kempton Park",
            Image = "https://dcegroup.co.za/wp-content/uploads/2021/01/Destiny-Gen-25.jpg",
            Url = "/destiny",
            Color = "#640E27"
        },
        new()
        {
            Title = "Broadlands",
            Description = "Broadlands Private School",
            Image ="https://dcegroup.co.za/wp-content/uploads/2024/04/WhatsApp-Image-2024-02-27-at-14.23.24.jpeg" ,
            Url = "/broadlands",
            Color = "#10315E"
        },
        new()
        {
            Title = "Greenacres",
            Description = "Greenacres Private College",
            Image = "https://dcegroup.co.za/wp-content/uploads/2024/04/WhatsApp-Image-2024-02-27-at-14.23.18-2.jpeg",
            Url = "/greenacres",
            Color = "#24763D"
        },
        new()
        {
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
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? Url { get; set; }
    public string? Color { get; set; }
}