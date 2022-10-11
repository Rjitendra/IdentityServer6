

namespace TechBox.API.Polices
{
    using TechBox.Model.Enum;
    using TechBox.Model.Utility;
    public static class Policies
    {
        public static void ConfigureAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // require that the user has a role claim and that it is not ApplicationUserRole.None
                options.AddPolicy(AuthPolicyNames.HasRole, policy =>
                {
                    policy.RequireAssertion(handler =>
                    {
                        // This application doesn't use claims to store roles on the database side but instead to use the dbo.AspNetUserRoles.  
                        // When the Claims are created for the user, they are added as claims even though roles aren't stored in dbo.AspNetUserClaims
                        var role = handler.User.Claims.Where(c => c.Type == ApplicationClaims.RoleId).ToList();

                        return role.Count(r => r.Value != ApplicationUserRole.None.ToString()) > 0;
                    });
                });
            });

        }
    }
}
