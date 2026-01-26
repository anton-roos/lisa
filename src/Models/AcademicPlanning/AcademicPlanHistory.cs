using System;
using System.Text.Json;

namespace Lisa.Models.AcademicPlanning
{
    public class AcademicPlanHistory
    {
        public Guid Id { get; set; }
        public Guid AcademicPlanId { get; set; }
        public int VersionNumber { get; set; }
        public int Status { get; set; }
        public required string SnapshotJson { get; set; }
        public Guid ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; } // For rejection reasons and other notes
    }
}
