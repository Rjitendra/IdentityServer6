

namespace TechBox.API
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using System.Reflection;
    using TechBox.API.Config;
    using TechBox.API.Extentions;
    using TechBox.Model.Config;
    using TechBox.Model.Contexts;
    using TechBox.Model.Utility;
    public static class HostingExtensions
    {
        private const string CorsPolicy = "_MyAllowSubdomainPolicy";
        private const string ClientApplicationSettings = "ClientApplicationSettings";


        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            var assembly = typeof(ApiContext).GetTypeInfo().Assembly.GetName().Name;
            builder.Services.AddDbContext<ApiContext>(builder => builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(assembly)));
            builder.Services.Configure<IdentityServerSettings>(builder.Configuration.GetSection("IdentityServerSettings")); // Adding IentityServer Setting into the 
            builder.Services.AddHttpContextAccessor(); // Needed for HttpContextHelper
            builder.Services.ConfigureDIServices();
            // Add MVC support
            builder.Services.AddMvc(options => { options.EnableEndpointRouting = false; });

            var identityServerSettings = builder.Configuration.GetSection("IdentityServerSettings").Get<IdentityServerSettings>();
            builder.Services.AddCors(options =>
                options.AddPolicy(CorsPolicy, builder => builder.WithOrigins(identityServerSettings.AllowedOrigins).AllowAnyHeader().AllowAnyMethod()));

            // Add Authorization Policies
            //services.ConfigureAuthorizationPolicies();

            // configure authentication to use IdentityServer4 (e.g. STS)
            builder.Services.AddAuthentication("Bearer").AddIdentityServerAuthentication(identityServerSettings.GetOptions());
         // configure Global Application Settings
            PopulateGlobalSettings(builder.Services);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers();
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

            }
            app.UseRouting();
            app.UseCors(CorsPolicy);
            app.UseAuthentication();
            app.UseAuthorization();
            app.ConfigureRedundantStatusCodePages(); // Provide JSON responses for standard response codes such as HTTP 401.

            app.UseHttpContextHelper(); // Helper to get Base URL anywhere in application
            app.MapControllers();
            // app.MapControllers().RequireAuthorization("ApiScope");

            return app;
        }
        /// <summary>
        /// Hits the database for all of the global setting fields and populates the GlobalAppSetting Singleton to be used throughout the application.
        /// </summary>
        /// <param name="services"></param>
        private static void PopulateGlobalSettings(IServiceCollection services)
        {
            services.AddSingleton<GlobalAppSettings>(new GlobalAppSettings()
            {
                DefaultMinutesPerTimeSlot = 30, // Hard coded for now, till WI#22005 makes this data driven and populated from the populate method.
                SlotsToDisplayPerStore = 5 // will probably always stay hard coded but this can be moved to a global setting that's data driven.
            });
        }
    }
}
