namespace Lisa.Models.Entities;

public class BugReport
{
    public Guid Id { get; set; }
    public string? WhatTried { get; set; }
    public string? WhatHappened { get; set; }
    public DateTime ReportedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ReportedBy { get; set; }
    public bool UserAuthenticated { get; set; }
    public string? PageUrl { get; set; }
    public string? Version { get; set; }
    public BugReportStatus Status { get; set; }
}