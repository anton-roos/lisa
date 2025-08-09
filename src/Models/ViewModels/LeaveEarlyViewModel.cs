using Lisa.Enums;
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

        [Required(ErrorMessage = "Permission type is required.")]
        [EnumDataType(typeof(PermissionType), ErrorMessage = "Invalid permission type.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid permission type.")]
        public PermissionType PermissionType { get; set; }
        public string? TelephonicNotes { get; set; }
        [Required(ErrorMessage = "Pick-up type is required.")]
        [EnumDataType(typeof(PickUpType), ErrorMessage = "Invalid pick-up type.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid pick-up type.")]
        public PickUpType PickUpType { get; set; }
        public string? PickupFamilyMemberIdNo { get; set; }
        public string? PickupFamilyMemberFirstname { get; set; }
        public string? PickupFamilyMemberSurname { get; set; }
        public string? PickupUberTransportIdNo { get; set; }
        public string? PickupUberTransportRegNo { get; set; }
    }
}
