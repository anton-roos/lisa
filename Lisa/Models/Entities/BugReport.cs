using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities;

public class BugReport
{
    public Guid Id { get; set; }
    [MaxLength(1024)]
    public string? WhatTried { get; set; }
    [MaxLength(1024)]
    public string? WhatHappened { get; set; }
    public DateTime ReportedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    [MaxLength(30)]
    public string? ReportedBy { get; set; }
    public bool UserAuthenticated { get; set; }
    [MaxLength(128)]
    public string? PageUrl { get; set; }
    [MaxLength(10)]
    public string? Version { get; set; }
    public BugReportStatus Status { get; set; }
}
