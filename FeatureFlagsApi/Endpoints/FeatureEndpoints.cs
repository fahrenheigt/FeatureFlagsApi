using FeatureFlagsApi.Models;
using FeatureFlagsApi.Services;

namespace FeatureFlagsApi.Endpoints;

public static class FeatureEndpoints
{
    public static void MapFeatureEndpoints(this WebApplication app)
    {
        app.MapGet("/api/features", GetAllFeatures);
        app.MapGet("/api/features/{key}", GetFeature);
        app.MapPost("/api/features", CreateFeature);
        app.MapPatch("/api/features/{key}", UpdateFeature);
        app.MapDelete("/api/features/{key}", DeleteFeature);
        app.MapPatch("/api/features/{key}/enable", EnableFeature);
        app.MapPatch("/api/features/{key}/disable", DisableFeature);
        app.MapPut("/api/features/{key}/environments/{env}/config", SetEnvironmentConfig);
        app.MapGet("/api/features/{key}/environments/{env}/config", GetEnvironmentConfig);
        app.MapDelete("/api/features/{key}/environments/{env}/config", DeleteEnvironmentConfig);
        app.MapGet("/api/features/{key}/evaluate", EvaluateFeature);
        app.MapGet("/api/features/{key}/audit-logs", GetAuditLogs);
    }

    private static IResult GetAllFeatures() =>
        Results.Ok(FeatureStore.Features);

    private static IResult GetFeature(string key)
    {
        var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
        return feature is null ? Results.NotFound() : Results.Ok(feature);
    }

    private static IResult CreateFeature(Feature feature)
    {
        var (isValid, errors) = ValidationHelper.Validate(feature);
        if (!isValid) return Results.UnprocessableEntity(errors);

        if (FeatureStore.Features.Any(f => f.Key == feature.Key))
            return Results.Conflict("Une feature avec cette clé existe déjà.");

        FeatureStore.Features.Add(feature);
        AuditStore.Log("created", AuditStore.ResourceFeature, feature.Key);
        return Results.Created($"/api/features/{feature.Key}", feature);
    }

    private static IResult UpdateFeature(string key, Feature updated)
    {
        var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
        if (feature is null) return Results.NotFound();

        var (isValid, errors) = ValidationHelper.Validate(updated);
        if (!isValid) return Results.UnprocessableEntity(errors);

        feature.Name = updated.Name;
        feature.Description = updated.Description;
        AuditStore.Log("updated", AuditStore.ResourceFeature, key);
        return Results.Ok(feature);
    }

    private static IResult DeleteFeature(string key)
    {
        var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
        if (feature is null) return Results.NotFound();

        FeatureStore.Features.Remove(feature);
        AuditStore.Log("deleted", AuditStore.ResourceFeature, key);
        return Results.NoContent();
    }

    private static IResult EnableFeature(string key)
    {
        var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
        if (feature is null) return Results.NotFound();

        feature.Enabled = true;
        AuditStore.Log("enabled", AuditStore.ResourceFeature, key);
        return Results.Ok(feature);
    }

    private static IResult DisableFeature(string key)
    {
        var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
        if (feature is null) return Results.NotFound();

        feature.Enabled = false;
        AuditStore.Log("disabled", AuditStore.ResourceFeature, key);
        return Results.Ok(feature);
    }

    private static IResult SetEnvironmentConfig(string key, string env, EnvironmentConfig config)
    {
        var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
        if (feature is null) return Results.NotFound();

        var (isValid, errors) = ValidationHelper.Validate(config);
        if (!isValid) return Results.UnprocessableEntity(errors);

        feature.Environments[env] = config;
        AuditStore.Log("config-updated", AuditStore.ResourceFeature, key, $"env={env}");
        return Results.Ok(feature);
    }

    private static IResult GetEnvironmentConfig(string key, string env)
    {
        var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
        if (feature is null) return Results.NotFound();

        if (!feature.Environments.TryGetValue(env, out var config))
            return Results.NotFound();

        return Results.Ok(config);
    }

    private static IResult DeleteEnvironmentConfig(string key, string env)
    {
        var feature = FeatureStore.Features.FirstOrDefault(f => f.Key == key);
        if (feature is null) return Results.NotFound();

        if (!feature.Environments.ContainsKey(env))
            return Results.NotFound();

        feature.Environments.Remove(env);
        AuditStore.Log("config-deleted", AuditStore.ResourceFeature, key, $"env={env}");
        return Results.NoContent();
    }

    private static IResult EvaluateFeature(string key, int userId, string env)
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
    }

    private static IResult GetAuditLogs(string key)
    {
        var logs = AuditStore.Logs
            .Where(l => l.Resource == AuditStore.ResourceFeature && l.ResourceKey == key)
            .ToList();
        return Results.Ok(logs);
    }
}