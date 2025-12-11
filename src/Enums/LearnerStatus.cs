namespace Lisa.Enums;

public enum LearnerStatus
{
    Initial,          // Newly created, not yet enrolled
    Active,           // Currently enrolled and attending
    PendingPromotion, // Year-end mode: awaiting promotion decision
    Promoted,         // Promoted to next grade (temporary state during year-end processing)
    Retained,         // Repeated grade (temporary state during year-end processing)
    Transferred,      // Left to another school
    Graduated,        // Completed final grade
    Disabled          // Soft-deleted/deactivated
}
