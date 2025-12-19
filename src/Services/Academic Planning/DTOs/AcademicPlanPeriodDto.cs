using System;

namespace Lisa.Services.AcademicPlanning.DTOs
{
    public class AcademicPlanPeriodDto
    {
        public Guid Id { get; set; }

        public int PeriodNumber { get; set; }

        // Added missing properties 
        public string? Topic { get; set; }
        public string? Resources { get; set; }
        public string? Assessment { get; set; }
        public string? Homework { get; set; }
    }
}
