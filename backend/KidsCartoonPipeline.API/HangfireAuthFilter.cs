using Hangfire.Dashboard;

namespace KidsCartoonPipeline.API;

public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In development, allow all access.
        // For production, add proper authentication here.
        return true;
    }
}
