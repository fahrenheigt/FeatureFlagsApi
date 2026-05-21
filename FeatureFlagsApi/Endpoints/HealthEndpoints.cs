namespace FeatureFlagsApi.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
        app.MapGet("/api/version", () =>
        {
            var version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "dev";
            return Results.Ok(new { version });
        });
    }
}