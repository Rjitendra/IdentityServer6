using Duende.IdentityServer.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TechBox.STS.Controllers.Account
{
    public class ResetPasswordViewModel
    {
        public string Code { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(
           100,
           ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
           MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}