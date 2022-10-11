

namespace TechBox.API.Config
{
    using IdentityServer4.AccessTokenValidation;
    public class IdentityServerSettings
    {
        public string[] AllowedOrigins { get; set; }

        public string ApiName { get; set; }

        public string ApiSecret { get; set; }

        public string BaseUrl { get; set; }

        public string ClientUrl { get; set; }

        internal Action<IdentityServerAuthenticationOptions> GetOptions()
        {
            return options =>
            {
                options.Authority = this.BaseUrl;
                options.RequireHttpsMetadata = false;
                options.ApiName = this.ApiName;
                options.ApiSecret = this.ApiSecret;

            };
        }
    }
}