using System;

namespace Lisa.Services.AcademicPlanning.DTOs
{
    public class AcademicPlanHistoryDto
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public int Status { get; set; }
        public string SnapshotJson { get; set; } = string.Empty;
        public Guid ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
