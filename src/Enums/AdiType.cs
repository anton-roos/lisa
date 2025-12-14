namespace Lisa.Enums;

/// <summary>
/// Defines the type of Academic Development Intervention (ADI) class.
/// </summary>
public enum AdiType
{
    /// <summary>
    /// Support ADI - Focused on a specific grade, learners must be from the selected grade.
    /// </summary>
    Support = 0,

    /// <summary>
    /// Break ADI - Multi-grade intervention, can include learners from any grade.
    /// Requires a reason when adding learners.
    /// </summary>
    Break = 1
}
