using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options) : IdentityDbContext(options)
{
}
