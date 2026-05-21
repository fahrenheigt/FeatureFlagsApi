namespace FeatureFlagsApi.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
        app.MapGet("/api/version", () => Results.Ok(new { version = "0.7.7" }));
    }
}