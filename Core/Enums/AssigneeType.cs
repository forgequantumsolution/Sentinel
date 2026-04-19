namespace Core.Enums
{
    /// <summary>
    /// Defines who the feature-permission is assigned to.
    /// </summary>
    public enum AssigneeType
    {
        /// <summary>
        /// Permission is assigned to an entire organization.
        /// This acts as the "ceiling" — users within the org
        /// can only be granted permissions that the org itself has.
        /// </summary>
        Organization = 0,

        /// <summary>
        /// Permission is assigned to a specific user within an organization.
        /// The user can only be granted this if the org already has it.
        /// </summary>
        User = 1,

        /// <summary>
        /// Permission is assigned to a UserGroup.
        /// All users in the group inherit the permission.
        /// The group's organization must already have the permission.
        /// </summary>
        Group = 2
    }
}
