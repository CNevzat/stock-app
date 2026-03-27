using Hangfire.Dashboard;

namespace StockApp.Infrastructure;

/// <summary>
/// Hangfire Dashboard erişim kuralları:
/// - Localhost'tan gelen istekler doğrudan kabul edilir (development kolaylığı).
/// - Dışarıdan gelen isteklerde JWT tabanlı "Admin" rolü aranır.
///   Not: Tarayıcı JWT'yi Bearer header'da göndermez; bu nedenle production'da
///   bu dashboard'u reverse proxy (nginx basic auth vb.) ile korumak önerilir.
/// </summary>
public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Localhost'tan gelen istekler her zaman kabul edilir
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            var isLocal = remoteIp.Equals(System.Net.IPAddress.Loopback)
                       || remoteIp.Equals(System.Net.IPAddress.IPv6Loopback)
                       || remoteIp.ToString() == "::1"
                       || remoteIp.ToString() == "127.0.0.1";

            if (isLocal)
                return true;
        }

        // Uzaktan erişimde Admin rolü zorunlu
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            return false;

        return httpContext.User.IsInRole("Admin");
    }
}
