using System.Net.Http.Headers;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using NamespaceRoot.ProductName.Common.Infrastructure.Configuration;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.Security.Filters;

public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string _user;
    private readonly string _password;

    public HangfireAdminAuthorizationFilter(IHangfireSettings settings)
    {
        _user = settings.DashboardUser;
        _password = settings.DashboardPassword;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var header = httpContext.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic "))
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(header);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? string.Empty);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

            if (credentials.Length == 2 && credentials[0] == _user && credentials[1] == _password)
            {
                return true;
            }
        }
        catch
        {
            // Invalid encoding or format
        }

        SetChallengeResponse(httpContext);
        return false;
    }

    private static void SetChallengeResponse(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
    }
}