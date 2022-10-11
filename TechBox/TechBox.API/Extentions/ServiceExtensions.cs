namespace TechBox.API.Extentions
{
    public static class ServiceExtensions
    {
        public static void ConfigureDIServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
        }
    }
}