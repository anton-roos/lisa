using System.Reflection;
using Lisa.Application.Common.Interfaces;
using Lisa.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Infrastructure.Data;

public class LisaDbContext
(
    DbContextOptions<LisaDbContext> options
) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options), ILisaDbContext
{
    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
