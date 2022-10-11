using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TechBox.Model.Contexts;
using TechBox.Model.Entity;
using TechBox.Model.Enum;
using TechBox.Model.Utility;

namespace TechBox.STS
{
    public class ProfileService : IProfileService
    {
        public ProfileService(UserManager<ApplicationUser> userManager, ApiContext db)
        {
            UserManager = userManager;
            Db = db;
        }

        protected ApiContext Db { get; }

        protected UserManager<ApplicationUser> UserManager { get; }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await UserManager.GetUserAsync(context.Subject);
            if (user == null)
            {
                return;
            }

            // issue all user claims
            var userClaims = await UserManager.GetClaimsAsync(user);
            context.IssuedClaims.AddRange(userClaims);

            //// Checking user has usersetting claims, if not adding default user setting to the claims table.
            //if (userClaims.Where(a => a.Type == "usersetting").SingleOrDefault() == null)
            //{
            //	await this.AddUserToDefaultuserSetting(user, context);
            //}

            // Add application roles to the Claims Identity Token
            await AddRoleClaims(context, user);

        }

        /// <summary>
        /// Adds roles to the user claims.  This is separate because Roles aren't stored in the dbo.AspNetUserClaims table, but instead stored in dbo.AspNetRoles.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task AddRoleClaims(ProfileDataRequestContext context, ApplicationUser user)
        {
            // Get the User's roles, these are only the role name
            var roleNames = await UserManager.GetRolesAsync(user);
            // Get the application's roles, these have the role and the id
            var applicationRoles = ApplicationUserRoleExtensions.GetApplicationUserRoles().ToList();

            // Join User's roles with the application's roles.  We do this to get the ID of the role
            var userAppRoles = roleNames.Join(applicationRoles,
                roleName => roleName,
                appRole => appRole.Name,
                (roleName, appRole) => new { appRole });

            // Create claims for the RoleId
            var roleIdClaims = userAppRoles.Select(r => new Claim(ApplicationClaims.RoleId, ((int)r.appRole.Id).ToString()));
            // Create claims for the RoleName
            var roleNameClaims = userAppRoles.Select(r => new Claim(ApplicationClaims.RoleName, r.appRole.Name));

            // Finally, add the claims for this user
            context.IssuedClaims.AddRange(roleIdClaims);
            context.IssuedClaims.AddRange(roleNameClaims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await UserManager.GetUserAsync(context.Subject);

            context.IsActive = (user != null) && user.EmailConfirmed;
        }


        ///// <summary>
        ///// Assigning user to the default usersetting claims, if they are not already assigned.
        ///// </summary>
        ///// <param name="user"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //private async Task AddUserToDefaultuserSetting(ApplicationUser user, ProfileDataRequestContext context)
        //{
        //	var setting = Newtonsoft.Json.JsonConvert.SerializeObject(new UserSettingDto());
        //	var userSettingClaim = new Claim("usersetting", setting);
        //	await this.UserManager.AddClaimAsync(user, userSettingClaim);
        //	context.IssuedClaims.Add(userSettingClaim);
        //}
    }
}