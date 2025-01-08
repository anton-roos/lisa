using Lisa.Data;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class ParentService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
}