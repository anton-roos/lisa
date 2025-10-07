#nullable disable

using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lisa.Areas.Identity.Pages.Account;

public class LogoutModel(SignInManager<User> signInManager, ILogger<LogoutModel> logger) : PageModel
{
    public async Task<IActionResult> OnGet(string returnUrl = null)
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out.");
        
        if (!string.IsNullOrEmpty(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }
        
        return LocalRedirect("~/");
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out.");
        
        if (!string.IsNullOrEmpty(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return LocalRedirect("~/");
    }
}