using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using TechBox.Model.Contexts;
using TechBox.Model.Dto;
using TechBox.Model.Entity;
using TechBox.Service.Implementations;
using TechBox.Service.Interfaces;
using TechBox.STS.Config;

namespace TechBox.STS
{
    public static class HostingExtensions
    {
        private const string CorsPolicy = "DefaultCorsPolicy";
        private const string ClientApplicationSettings = "ClientApplicationSettings";

        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();



            // Add DB Contexts
            builder.Services.AddDbContext<ApiContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("TechBox.STS")));



            // configure reading the client application settings from appsettings.json file
            var clientSettings = builder.Configuration.GetSection(ClientApplicationSettings).Get<ClientApplicationSettings>();
            builder.Services.Configure<ClientApplicationSettings>(builder.Configuration.GetSection(ClientApplicationSettings));

            // Add ASP.NET Core Identity
            builder.Services.AddIdentity<ApplicationUser, ApplicationUserIdentityRole>(IdentityServerConfigurations.GetConfigureIdentityOptions())
                .AddEntityFrameworkStores<ApiContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
            var logFactory = (ILoggerFactory)new LoggerFactory();
            // We need to specify the IdentityServer CORS settings, which is separate from the Identity Server CORS
            var idsrvCors = new DefaultCorsPolicyService(logFactory.CreateLogger<DefaultCorsPolicyService>())
            {
                AllowedOrigins = { clientSettings.AngularBaseUrl.TrimEnd('/') }
            };
            // General MVC project services
            builder.Services.AddRazorPages();

            // configure cookies
            builder.Services.ConfigureApplicationCookie(
                options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });


            // add IdentityServer6

            var clientAppSettings = builder.Configuration.GetSection(ClientApplicationSettings).Get<ClientApplicationSettings>();
            builder.Services.AddIdentityServer(IdentityServerConfigurations.GetIdentityServerOptions())
                 .AddInMemoryApiResources(IdentityServerConfigurations.GetApiResources(clientAppSettings))
            .AddInMemoryApiScopes(IdentityServerConfigurations.ApiScopes(clientAppSettings))
            .AddInMemoryIdentityResources(IdentityServerConfigurations.GetIdentityResources())
                   .AddInMemoryClients(IdentityServerConfigurations.GetClients(clientAppSettings))
                   .AddSigningCredential(IdentityServerConfigurations.GetCertificate())
                // order matters very much here; AspNetIdentity has its own IProfileService implementation
                // by adding our own profile service after, we are overriding their implementation of IProfileService
                .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<ProfileService>();

            var googleKey = builder.Configuration.GetSection("GoogleKey");
            builder.Services.AddAuthentication()
             .AddGoogle("Google", options =>
             {
                 options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                 options.ClientId = googleKey["ClientId"];
                 options.ClientSecret = googleKey["ClientSecret"];
             });

            var githubKey = builder.Configuration.GetSection("GitHub");
            builder.Services.AddAuthentication()
             .AddGitHub("GitHub", options =>
             {
                 options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                 options.ClientId = githubKey["ClientId"];
                 options.ClientSecret = githubKey["ClientSecret"];
             });

            builder.Services.Configure<MailSettingsDto>(builder.Configuration.GetSection("MailSettings"));
            builder.Services.AddTransient<IMailService, MailService>();

            return builder.Build();
        }
        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
            });
            app.UseCors(CorsPolicy);
            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });


            return app;

        }
    }
}