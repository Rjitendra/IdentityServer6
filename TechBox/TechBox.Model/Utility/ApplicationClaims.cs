

namespace TechBox.Model.Utility
{
    /// <summary>
    /// Defines claim types for this application.
    /// </summary>
    public static class ApplicationClaims
    {
        /// <summary>
        /// Maps to <see cref="Enums.ApplicationUserRole"/>.
        /// </summary>
        public const string RoleId = "roleId";

        /// <summary>
        /// Maps <see cref="Enums.ApplicationUserRole"/> to friendly name.
        /// </summary>
        public const string RoleName = "roleName";
    }
}
