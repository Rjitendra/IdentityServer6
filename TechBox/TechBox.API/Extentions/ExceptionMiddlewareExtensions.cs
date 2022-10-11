

namespace TechBox.API.Extentions
{
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.WebUtilities;
    using System.Net;
    public static class ExceptionMiddlewareExtensions
    {

        /// <summary>
        /// Configure general error response for the API when any unhandled exception is thrown.  This is a catch-all.
        /// </summary>
        /// <param name="app"></param>
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        await context.Response.WriteAsync(new ErrorDetails()
                        {
                            Status = 500,
                            Message = "Internal Server Error."
                        }.ToString());
                    }
                });
            });
        }

        /// <summary>
        /// Configure status code responses as a JSON response.
        /// </summary>
        /// <param name="app"></param>
        public static void ConfigureRedundantStatusCodePages(this IApplicationBuilder app)
        {
            // This is redundant but placed here to make the payload to say what the HTTP response is to make it easier to read.
            app.UseStatusCodePages(async context =>
            {
                context.HttpContext.Response.ContentType = "application/json";

                var responsePayload = new ErrorDetails()
                {
                    Message = ReasonPhrases.GetReasonPhrase(context.HttpContext.Response.StatusCode)
                };

                await context.HttpContext.Response.WriteAsync(responsePayload.ToString());
            });
        }
    }
}
