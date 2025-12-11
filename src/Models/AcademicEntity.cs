using Lisa.Models.Entities;

namespace Lisa.Models;

/// <summary>
/// Base class for entities that are linked to an academic year.
/// Inherits from Entity to include standard audit fields.
/// Filtering by AcademicYearId where IsCurrent=true replaces the need for IsArchived flag.
/// </summary>
public class AcademicEntity : Entity
{
    public Guid? AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }
}
