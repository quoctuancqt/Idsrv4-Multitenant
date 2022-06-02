using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Core.Security
{
    public static class TenantAuthenticationExtentions
    {
        public static IApplicationBuilder UseTenantAuthentication(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<TenantAuthorizationMiddleware>(Array.Empty<object>());
        }
    }

    public class TenantAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextAccessor _contextAccessor;

        public TenantAuthorizationMiddleware(RequestDelegate next, IHttpContextAccessor contextAccessor)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public IAuthenticationSchemeProvider Schemes { get; set; }

        public async Task Invoke(HttpContext context)
        {
            var userTenant = _contextAccessor.HttpContext.User.FindFirst(c => c.Type == CustomClaimTypes.TenantId)?.Value;
            
            var urlTenant = _contextAccessor.HttpContext?.Request?.Host.Host.Split(".")?[0];
            
            if (string.IsNullOrWhiteSpace(urlTenant))
            {
                await context.ForbidAsync();
            }

            if (urlTenant != userTenant.Replace(" ", "").ToLower())
            {
                await context.ForbidAsync();
            }

            await _next(context);
        }
    }
}