using FeatureFlagsApi.Models;
using FeatureFlagsApi.Services;

namespace FeatureFlagsApi.Endpoints;

public static class FeatureEndpoints
{
    public static void MapFeatureEndpoints(this WebApplication app)
    {
        app.MapGet("/api/features", () =>
            Results.Ok(FeatureStore.Features));

        app.MapGet("/api/features/{key}", (string key) =>
        {
            var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
            return feature is null ? Results.NotFound() : Results.Ok(feature);
        });

        app.MapPost("/api/features", (Feature feature) =>
        {
            if (FeatureStore.Features.Any(f => f.Key == feature.Key))
                return Results.Conflict("Une feature avec cette clé existe déjà.");

            FeatureStore.Features.Add(feature);
            return Results.Created($"/api/features/{feature.Key}", feature);
        });

        app.MapPatch("/api/features/{key}", (string key, Feature updated) =>
        {
            var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
            if (feature is null) return Results.NotFound();

            feature.Name = updated.Name ?? feature.Name;
            feature.Description = updated.Description ?? feature.Description;
            return Results.Ok(feature);
        });

        app.MapDelete("/api/features/{key}", (string key) =>
        {
            var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
            if (feature is null) return Results.NotFound();

            FeatureStore.Features.Remove(feature);
            return Results.NoContent();
        });

        app.MapPatch("/api/features/{key}/enable", (string key) =>
        {
            var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
            if (feature is null) return Results.NotFound();

            feature.Enabled = true;
            return Results.Ok(feature);
        });

        app.MapPatch("/api/features/{key}/disable", (string key) =>
        {
            var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
            if (feature is null) return Results.NotFound();

            feature.Enabled = false;
            return Results.Ok(feature);
        });

        // Config par environnement
        app.MapPut("/api/features/{key}/environments/{env}/config", (string key, string env, EnvironmentConfig config) =>
        {
            var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
            if (feature is null) return Results.NotFound();

            feature.Environments[env] = config;
            return Results.Ok(feature);
        });

        app.MapGet("/api/features/{key}/environments/{env}/config", (string key, string env) =>
        {
            var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
            if (feature is null) return Results.NotFound();

            if (!feature.Environments.TryGetValue(env, out var config))
                return Results.NotFound();

            return Results.Ok(config);
        });

        app.MapDelete("/api/features/{key}/environments/{env}/config", (string key, string env) =>
        {
            var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
            if (feature is null) return Results.NotFound();

            if (!feature.Environments.ContainsKey(env))
                return Results.NotFound();

            feature.Environments.Remove(env);
            return Results.NoContent();
        });

        app.MapGet("/api/features/{key}/evaluate", (string key, int userId, string env) =>
        {
            var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
            if (feature is null) return Results.NotFound("Feature introuvable.");

            if (!feature.Environments.TryGetValue(env, out var config))
                return Results.NotFound("Environnement introuvable.");

            var user = UserStore.Users.FirstOrDefault(u => u.Id == userId);
            if (user is null) return Results.NotFound("Utilisateur introuvable.");

            var userGroups = GroupStore.Groups
                .Where(g => g.UserIds.Contains(userId))
                .Select(g => g.Name)
                .ToList();

            var result = FeatureService.EvaluateFeatureAccess(feature, config, user, userGroups);
            return Results.Ok(result);
        });
    }
}