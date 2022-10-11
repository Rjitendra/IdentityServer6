

namespace TechBox.Model.Utility
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public static class HttpContextHelperExtensions
    {
        public static IApplicationBuilder UseHttpContextHelper(this IApplicationBuilder app)
        {
            HttpContextHelper.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            return app;
        }
    }
}