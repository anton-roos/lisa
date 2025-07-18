using Lisa.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities
{
    public class LeaveEarly
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid LeaveEarlyId { get; set; }

        public Guid? AttendenceRecordId { get; set; }
        public AttendanceRecord? AttendanceRecord { get; set; }

        public Guid? LearnerId { get; set; }
        public Learner? Learner { get; set; }

        public Guid? SchoolGradeId { get; set; }
        public SchoolGrade? SchoolGrade { get; set; }

        public DateTime Date { get; set; }
        public TimeOnly? SignOutTime { get; set; }

        public PermissionType PermissionType { get; set; }
        public string? TelephonicNotes { get; set; }

        public PickUpType PickUpType { get; set; }

        public string? PickupFamilyMemberIDNo { get; set; }
        public string? PickupFamilyMemberFirstname { get; set; }
        public string? PickupFamilyMemberSurname { get; set; }

        public string? PickupUberTransportIDNo { get; set; }
        public string? PickupUberTransportRegNo { get; set; }
    }
}
