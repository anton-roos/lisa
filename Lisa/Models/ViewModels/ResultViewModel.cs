using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.ViewModels;

public class ResultViewModel
{
    
    [Required(ErrorMessage = "Please enter a mark.")]
    public decimal Score { get; set; }
    public bool Absent { get; set; } = false;
    public string? AbsentReason { get; set; }
}