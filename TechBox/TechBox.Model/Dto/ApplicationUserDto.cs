

namespace TechBox.Model.Dto
{
    using System.Collections.Generic;

    public class ApplicationUserDto
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ApplicationUserId { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// First name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Property for User is Enbaled or not
        /// </summary>
        public bool IsEnabled { get; set; }

        public string UserName { get; set; }

        public List<ApplicationUserRoleDto> UserRoles { get; set; }

        public string Password { get; set; }
    }
}
