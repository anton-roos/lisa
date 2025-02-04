using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lisa.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromForm] string username, [FromForm] string password)
    {
        // Validate credentials. (Replace this with your real authentication logic.)
        if (username != "admin@dcegroup.co.za" || password != "Lis@Adm!n7Dc3Gr0up")
        {
            return Unauthorized("Invalid credentials.");
        }

        // Create the claims and the JWT token.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username)
            // add more claims as needed
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        // **Option 1:** Store the JWT in a cookie so that your Blazor circuit and SignalR can use it.
        Response.Cookies.Append("jwt", jwtToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTimeOffset.Now.AddMinutes(30)
        });

        // **Option 2:** Alternatively, you could return the token to the client and have client-side code store it.
        // For this example we use cookies.

        // Tell htmx to redirect to "/" after a successful login.
        Response.Headers.Add("HX-Redirect", "/");
        return Ok();
    }
}
