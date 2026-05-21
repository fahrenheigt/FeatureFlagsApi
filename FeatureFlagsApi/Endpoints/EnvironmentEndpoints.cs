using FeatureFlagsApi.Models;
using FeatureFlagsApi.Services;
using FeatureEnvironment = FeatureFlagsApi.Models.FeatureEnvironment;

namespace FeatureFlagsApi.Endpoints;

public static class EnvironmentEndpoints
{
    public static void MapEnvironmentEndpoints(this WebApplication app)
    {
        app.MapGet("/api/environments", () =>
            Results.Ok(EnvironmentStore.Environments));

        app.MapGet("/api/environments/{name}", (string name) =>
        {
            var env = EnvironmentStore.Environments.FirstOrDefault(e => e.Name == name);
            return env is null ? Results.NotFound() : Results.Ok(env);
        });

        app.MapPost("/api/environments", (FeatureEnvironment env) =>
        {
            var (isValid, errors) = ValidationHelper.Validate(env);
            if (!isValid) return Results.UnprocessableEntity(errors);

            if (EnvironmentStore.Environments.Any(e => e.Name == env.Name))
                return Results.Conflict("Un environnement avec ce nom existe déjà.");

            EnvironmentStore.Environments.Add(env);
            return Results.Created($"/api/environments/{env.Name}", env);
        });

        app.MapPatch("/api/environments/{name}", (string name, FeatureEnvironment updated) =>
        {
            var env = EnvironmentStore.Environments.FirstOrDefault(e => e.Name == name);
            if (env is null) return Results.NotFound();

            var (isValid, errors) = ValidationHelper.Validate(updated);
            if (!isValid) return Results.UnprocessableEntity(errors);

            env.Description = updated.Description ?? env.Description;
            return Results.Ok(env);
        });

        app.MapDelete("/api/environments/{name}", (string name) =>
        {
            var env = EnvironmentStore.Environments.FirstOrDefault(e => e.Name == name);
            if (env is null) return Results.NotFound();

            EnvironmentStore.Environments.Remove(env);
            return Results.NoContent();
        });
    }
}