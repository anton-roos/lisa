using Lisa.Data;
using Lisa.Models.Entities;
using Lisa.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services
{
    public class LeaveEarlyService(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<LearnerService> logger)
    {
        public async Task<bool> SaveLeaveEarlyAsync(LeaveEarlyViewModel leaveEarly)
        {
            try
            {
                await using var context = await dbContextFactory.CreateDbContextAsync();

                var newLearnerId = Guid.NewGuid();

                

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding learner.");
                return false;
            }
        }
    }
}
