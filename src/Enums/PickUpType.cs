namespace Lisa.Enums
{
    public enum PickUpType
    {
        None = 0,

        ParentOrGuardian = 1, // as per contract

        DesignatedFamilyMember = 2, // ID provided

        UberOrTransportDriver = 3, // with ID and Reg No

        LearnerWalkHome = 4 // walk home
    }
}
