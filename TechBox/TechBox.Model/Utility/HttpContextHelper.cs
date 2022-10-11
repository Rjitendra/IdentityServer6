

namespace TechBox.Model.Utility
{
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Helper to get the base URL path
    /// </summary>
    public static class HttpContextHelper
    {
        private static IHttpContextAccessor? m_httpContextAccessor;

        public static HttpContext Current => m_httpContextAccessor.HttpContext;

        /// <summary>
        /// Returns base path of application
        /// </summary>
        public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}/";

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            m_httpContextAccessor = contextAccessor;
        }
    }
}
