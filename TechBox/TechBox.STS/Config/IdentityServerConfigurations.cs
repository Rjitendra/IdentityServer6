


namespace TechBox.STS.Config
{
    using TechBox.Model;
    using TechBox.Model.Utility;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using Duende.IdentityServer.Models;
    using Duende.IdentityServer.Configuration;
    using Duende.IdentityServer.EntityFramework.Options;
    using Duende.IdentityServer;

    /// <summary>
    /// Class to hard code Identity Server settings.  This includes registered APIs, Clients, IdentityResources, certificate acquisition, etc..
    /// </summary>
    public static class IdentityServerConfigurations
    {
        public static IEnumerable<ApiScope> ApiScopes(ClientApplicationSettings settings)
        {
            return new List<ApiScope>
        {
            new ApiScope(name:IdentityServerConfigConstants.TechBoxApiScope, displayName:"TechBoxAPIScope")
        };

        }

        public static IEnumerable<ApiResource> GetApiResources(ClientApplicationSettings settings)
        {
            return new List<ApiResource>
            {
                new ApiResource(IdentityServerConfigConstants.TechBoxApi, "TechBoxAPI")
                {
                    Scopes = { IdentityServerConfigConstants.TechBoxApiScope },
                    ApiSecrets={ new Secret(settings.ApiSecret.Sha256()) },
                    UserClaims = new []
                    {
                        ClaimTypes.Name,
                        ClaimTypes.GivenName,
                        ClaimTypes.Surname,
                        ApplicationClaims.RoleId
                    }
                }
            };
        }


        public static IEnumerable<Client> GetClients(ClientApplicationSettings settings)
        {
            return new[]
            {
                new Client
                {
                    ClientId = "techbox-angular",
                    ClientName = "TechBox Access",
                    ClientUri = settings.AngularBaseUrl,
                    AllowedGrantTypes = GrantTypes.Code, // We are using Code with PKCE (Proof Key for Code Exchange). NOTE: This is recommended strategy over implicit.
                    RequirePkce = true, // Forcing requiring PKCE
                   AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                   AccessTokenType = AccessTokenType.Reference,
                    RedirectUris =
                    {
                        $"{settings.AngularBaseUrl}",
                        $"{settings.AngularBaseUrl}assets/silent-callback.html",
                        $"{settings.AngularBaseUrl}signin-callback"

                    },

                      PostLogoutRedirectUris = { settings.AngularBaseUrl },
                     
                       AllowedCorsOrigins = { settings.AngularBaseUrl.TrimEnd('/') },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConfigConstants.TechBoxApiScope,
                        IdentityServerConfigConstants.TechBoxProfile
                    },
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    IdentityTokenLifetime = 28800, // 8 hours
                    AccessTokenLifetime = 28800,  // 8 hours
                    RequireClientSecret = false


                },
                new Client
                {
                    ClientId = "jitu-client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret(settings.ApiSecret.Sha256()) },
                    AllowedScopes = { IdentityServerConfigConstants.TechBoxApiScope },
                    RequireConsent = false,
                    AccessTokenType = AccessTokenType.Reference
                } ,

            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource(IdentityServerConfigConstants.TechBoxProfile, new[] { ApplicationClaims.RoleId })
            };
        }

        internal static Action<IdentityOptions> GetConfigureIdentityOptions()
        {
            return options =>
            {
                // password requirements
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;

                // lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // user validation settings
                options.User.RequireUniqueEmail = false;

                // sign-in settings
                options.SignIn.RequireConfirmedEmail = true;
            };
        }

        /// <summary>
        /// Creates identity server configuration.
        /// </summary>
        /// <returns></returns>
        internal static Action<IdentityServerOptions> GetIdentityServerOptions()
        {
            return options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            };
        }

        /// <summary>
		/// Creates operational store configuration.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		internal static Action<OperationalStoreOptions> GetOperationalStoreOptions(string connectionString)
        {
            return options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString);
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 30;
            };
        }

        /// <summary>
		/// Gets signing certificate embedded resource from assembly and returns as X509 certificate.
		/// </summary>
		/// <returns>Signing certificate.</returns>
		internal static X509Certificate2 GetCertificate()
        {
            var assembly = typeof(IdentityServerConfigurations).GetTypeInfo().Assembly;
            var resourceName = $"{assembly.GetName().Name}.STSCert.pfx";
            var resource = assembly.GetManifestResourceStream(resourceName);

            try
            {
                using (var ms = new MemoryStream())
                {
                    resource?.CopyTo(ms);

                    return new X509Certificate2(ms.ToArray(), "lhI4Y3w94HUoCOVID19oB6HOWQjQZdLBZWKG");
                }
            }
            catch (CryptographicException ex)
            {
                Log.Logger.Error(
                    "Failure loading embedded signing certificate having resource name {0}. Available resources are as follows: {1}.",
                    resourceName,
                    string.Join(',', assembly.GetManifestResourceNames()));

                throw ex;
            }
        }

    }
}







