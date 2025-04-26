using Lisa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lisa.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(AttendanceService attendanceService, ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    [HttpPost("record")]
    public async Task<IActionResult> RecordAttendance([FromBody] AttendanceRecordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? recordedByUserId = userId != null ? Guid.Parse(userId) : null;

            var attendance = await _attendanceService.RecordAttendanceAsync(
                request.LearnerId,
                request.SchoolId,
                request.RegisterClassId,
                request.IsPresent,
                request.Notes,
                recordedByUserId
            );

            return Ok(attendance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording attendance");
            return StatusCode(500, "An error occurred while recording attendance");
        }
    }

    [HttpPost("signout/{attendanceId}")]
    public async Task<IActionResult> SignOut(Guid attendanceId)
    {
        try
        {
            var result = await _attendanceService.RecordSignOutAsync(attendanceId);
            if (!result)
            {
                return NotFound($"Attendance record with ID {attendanceId} not found");
            }

            return Ok("Sign-out recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording sign-out for attendance {AttendanceId}", attendanceId);
            return StatusCode(500, "An error occurred while recording sign-out");
        }
    }
}

public class AttendanceRecordRequest
{
    public Guid LearnerId { get; set; }
    public Guid SchoolId { get; set; }
    public Guid RegisterClassId { get; set; }
    public bool IsPresent { get; set; }
    public string? Notes { get; set; }
}