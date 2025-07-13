using Lisa.Enums;
using Lisa.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels
{
    public class LeaveEarlyViewModel
    {
        public Guid? AttendenceRecordId { get; set; }
        public Guid? LearnerId { get; set; }
        public Guid? SchoolGradeId { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly? SignOutTime { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Permission is required")]
        public PermissionType PermissionType { get; set; }
        public string? TelephonicNotes { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "PickUp Type is required")]
        public PickUpType PickUpType { get; set; }
        public string? PickupFamilyMemberIDNo { get; set; }
        public string? PickupFamilyMemberFirstname { get; set; }
        public string? PickupFamilyMemberSurname { get; set; }
        public string? PickupUberTransportIDNo { get; set; }
        public string? PickupUberTransportRegNo { get; set; }
    }
}
