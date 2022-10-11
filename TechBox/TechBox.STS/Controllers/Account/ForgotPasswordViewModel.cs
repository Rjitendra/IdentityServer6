using System.ComponentModel.DataAnnotations;

namespace TechBox.STS.Controllers.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
