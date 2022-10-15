
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TechBox.Model.Contexts;
using TechBox.Model.Dto;
using TechBox.Model.Entity;
using TechBox.Service.Interfaces;
using TechBox.STS.Config;

namespace TechBox.STS.Controllers.Account
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private ApiContext ApiContext { get; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationUserIdentityRole> _roleManager;
        private readonly IIdentityServerInteractionService _interaction;
        private IPersistedGrantStore _persistedGrantStore { get; }
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private IOptions<ClientApplicationSettings> _clientSettings { get; }
        private readonly IMailService _mailService;


        public AccountController(
            ApiContext apiContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationUserIdentityRole> roleManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IPersistedGrantStore persistedGrantStore,
            IEventService events,
            IOptions<ClientApplicationSettings> clientSettings,
            IMailService mailService
            )
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            ApiContext = apiContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _persistedGrantStore = persistedGrantStore;
            _events = events;
            _clientSettings = clientSettings;
            _mailService = mailService;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {

            string[] roleNames = { "None", "Admin" };

            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationUserIdentityRole();
                    role.Name = roleName;
                    await _roleManager.CreateAsync(role);
                }

            }


            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context == null)
                return Redirect(this._clientSettings.Value.AngularBaseUrl);

            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client.ClientId));

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                        {
                            // The client is native, so this change in how to
                            // return the response is for better UX for the end user.
                            return this.LoadingPage("Redirect", model.ReturnUrl);
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect(model.ReturnUrl);
                    }

                    // request for a local page
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception("invalid return URL");
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client.ClientId));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }
            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }


        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
             var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {

                // logout of the MVC app
                await this._signInManager.SignOutAsync();
                await this.HttpContext.SignOutAsync();

                // revoke all user grants effectively logs the user out of all clients
                var subjectId = this.User.Claims.Single(x => x.Type == "sub").Value;
                var c = new PersistedGrantFilter { SubjectId = subjectId };
                var grants = await this._persistedGrantStore.GetAllAsync(c);

                foreach (var grant in grants)
                {
                    await this._persistedGrantStore.RemoveAsync(grant.Key);
                }
                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return this.View(this.BuildRegistrationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration(RegistrationViewModel model)
        {
            // validate the model
            if (!this.ModelState.IsValid)
            {
                return this.View(this.BuildRegistrationViewModel(model));
            }
            var existUser = this.ApiContext.Users.Where(x => x.Email == model.Email && x.IsExternalUser == false).SingleOrDefault();


            // ensure unique email address
            if (existUser != null)
            {
                this.ModelState.AddModelError(string.Empty, "That email address is already registered.");

                return this.View(this.BuildRegistrationViewModel(model));
            }

            var newUser = new ApplicationUser(model.Email);
            // need to remove below line code,once implement pw related functionality

            newUser.IsEnabled = true;
            // create the new user
            var result = await this._userManager.CreateAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                var user = this.ApiContext.Users.Where(x => x.Email == model.Email && x.IsExternalUser == false).SingleOrDefault();
                // load the newly created user
                //var user = await this._userManager.FindByEmailAsync(model.Email);

                // add user claims
                var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Name, $"{model.FirstName} {model.LastName}"),
                        new Claim(JwtClaimTypes.GivenName, model.FirstName),
                        new Claim(JwtClaimTypes.FamilyName, model.LastName),
                        new Claim(JwtClaimTypes.Email, model.Email),
                };

                await this._userManager.AddClaimsAsync(user, claims);
                await _userManager.AddToRoleAsync(user, "None");

                // send confirmation email
                var code = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = this.Url.EmailConfirmationLink(user.Id, code, this.Request.Scheme);
                var mailObj = new MailRequestDto()
                {
                    ToEmail = user.Email,
                    Subject = "Email Confirm",
                    Body = callbackUrl,
                    Attachments = null
                };
                await this._mailService.SendEmailAsync(mailObj);

                return this.RedirectToAction(nameof(RegistrationComplete));

            }

            // if user creation failed, display errors and re-render the view
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }

            return this.View(this.BuildRegistrationViewModel(model));
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return this.RedirectToAction(nameof(Login));
            }

            var user = await this._userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return this.RedirectToAction(nameof(Login));
            }

            var result = await this._userManager.ConfirmEmailAsync(user, code);

            return this.View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   
                    var existUser = this.ApiContext.Users.Where(x => x.Email == model.Email && x.IsExternalUser == false).SingleOrDefault();
                    ViewBag.errorMessage = "<p>An email has been sent to your registered email.</p> Please check your email and follow the instructions.";

                    if (existUser == null || !existUser.EmailConfirmed)
                    { // reveal that the user does not exist or is not confirmed

                        ViewBag.errorMessage = "User Not Found";
                        return View("ForgotPasswordConfirmation");
                    }

                    var code = await _userManager.GeneratePasswordResetTokenAsync(existUser);
                    var callbackUrl = this.Url.ResetPasswordCallbackLink(existUser.Id, code, this.Request.Scheme);
                    var mailObj = new MailRequestDto()
                    {
                        ToEmail = existUser.Email,
                        Subject = "Rest Password",
                        Body = callbackUrl,
                        Attachments = null
                    };
                    await this._mailService.SendEmailAsync(mailObj);
                    return View("ForgotPasswordConfirmation");
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("[controller]/registration/complete")]
        public IActionResult RegistrationComplete()
        {
            return this.View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }

            var model = new ResetPasswordViewModel { Code = code };
            return this.View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }
            var user = this.ApiContext.Users.Where(x => x.Email == model.Email && x.IsExternalUser == false).SingleOrDefault();

            // var user = await this.UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // don't reveal that the user does not exist
                return this.RedirectToAction(nameof(this.ResetPasswordConfirmation));
            }

            var result = await this._userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return this.RedirectToAction(nameof(this.ResetPasswordConfirmation));
            }

            // on failure, add errors to model
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }

            return this.View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return this.View();
        }

        [HttpGet]
        [AllowAnonymous]

        public IActionResult Lockout()
        {
            return this.View();
        }

        [HttpGet]
        // 
        // [Authorize(Roles = UserRole.Admin)]
        public IActionResult Search()
        {
            return this.View();
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,

                    AllowRememberLogin = AccountOptions.AllowRememberLogin, // added later

                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = this._clientSettings.Value.AngularBaseUrl,   // logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
        private RegistrationViewModel BuildRegistrationViewModel(RegistrationViewModel model = null)
        {
            // set the model
            model = model ?? new RegistrationViewModel();

            return model;
        }
    }
}
