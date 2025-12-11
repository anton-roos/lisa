using Lisa.Enums;
using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class LeaveEarly : AcademicEntity
{
    public Guid? AttendenceRecordId { get; set; }
    public AttendanceRecord? AttendanceRecord { get; set; }

    public Guid? LearnerId { get; set; }
    public Learner? Learner { get; set; }

    public Guid? SchoolGradeId { get; set; }
    public SchoolGrade? SchoolGrade { get; set; }

    public DateTime Date { get; set; }
    public TimeOnly? SignOutTime { get; set; }

    public PermissionType PermissionType { get; set; }
    
    [MaxLength(512)]
    public string? TelephonicNotes { get; set; }

    public PickUpType PickUpType { get; set; }

    [MaxLength(64)]
    public string? PickupFamilyMemberIdNo { get; set; }
    [MaxLength(64)]
    public string? PickupFamilyMemberFirstname { get; set; }
    [MaxLength(64)]
    public string? PickupFamilyMemberSurname { get; set; }

    [MaxLength(64)]
    public string? PickupUberTransportIdNo { get; set; }
    [MaxLength(64)]
    public string? PickupUberTransportRegNo { get; set; }
}
