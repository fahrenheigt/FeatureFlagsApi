namespace FeatureFlagsApi.Endpoints;

public static class HealthEndpoints
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static string GetVersion() =>
    Environment.GetEnvironmentVariable("APP_VERSION") ?? "dev";

    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
        app.MapGet("/api/version", () =>
        {
            var version = GetVersion();
            return Results.Ok(new { version });
        });
    }
}