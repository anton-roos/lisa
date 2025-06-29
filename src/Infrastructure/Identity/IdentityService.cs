using Lisa.Application.Common.Interfaces;
using Lisa.Application.Common.Models;
using Lisa.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Result = Lisa.Application.Common.Models.Result;

namespace Lisa.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<User> userManager,
        IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user?.UserName;
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(Guid userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(User user)
    {
        var identityResult = await _userManager.DeleteAsync(user);

        return identityResult.ToApplicationResult();
    }

    public async Task<(Application.Common.Models.Result Result, Guid UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new User
        {
            UserName = userName,
            Email = userName,
        };

        var identityResult = await _userManager.CreateAsync(user, password);

        return (identityResult.ToApplicationResult(), user.Id);
    }

    public async Task<Application.Common.Models.Result> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }
}
