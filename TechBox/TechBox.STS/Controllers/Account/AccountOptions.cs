

namespace TechBox.STS.Controllers.Account
{
    public class AccountOptions
    {
	

        public static bool AllowLocalLogin = true;

        public static bool AllowRememberLogin = true;

        public static bool AutomaticRedirectAfterSignOut = false;

        public static bool IncludeWindowsGroups = false;

        public static bool ShowLogoutPrompt = false;

        public static string InvalidCredentialsErrorMessage = "Invalid username or password";

        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

       
    }
}
