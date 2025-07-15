using Lisa.Data;
using Lisa.Enums;
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

                LeaveEarly newLeave = new LeaveEarly();
                newLeave.AttendenceRecordId = leaveEarly.AttendenceRecordId;
                newLeave.LearnerId = leaveEarly.LearnerId;
                newLeave.SchoolGradeId = leaveEarly.SchoolGradeId;
                newLeave.Date = leaveEarly.Date;
                newLeave.SignOutTime = leaveEarly.SignOutTime;
                newLeave.PermissionType = leaveEarly.PermissionType;
                newLeave.TelephonicNotes = leaveEarly.TelephonicNotes;
                newLeave.PickUpType = leaveEarly.PickUpType;
                newLeave.PickupFamilyMemberIDNo = leaveEarly.PickupFamilyMemberIDNo;
                newLeave.PickupFamilyMemberFirstname = leaveEarly.PickupFamilyMemberFirstname;
                newLeave.PickupFamilyMemberSurname = leaveEarly.PickupFamilyMemberSurname;
                newLeave.PickupUberTransportIDNo = leaveEarly.PickupUberTransportIDNo;
                newLeave.PickupUberTransportRegNo = leaveEarly.PickupUberTransportRegNo;

                await context.LeaveEarlies.AddAsync(newLeave);
                await context.SaveChangesAsync();

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
