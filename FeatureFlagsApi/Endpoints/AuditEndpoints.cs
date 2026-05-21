using FeatureFlagsApi.Services;

namespace FeatureFlagsApi.Endpoints;

public static class AuditEndpoints
{
    public static void MapAuditEndpoints(this WebApplication app)
    {
        app.MapGet("/api/audit-logs", () =>
            Results.Ok(AuditStore.Logs));
    }
}