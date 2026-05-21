using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Services;

public static class FeatureService
{
    public static EvaluationResult EvaluateFeatureAccess(
        Feature feature,
        EnvironmentConfig config,
        User user,
        List<string> userGroups)
    {
        if (!feature.Enabled)
            return new EvaluationResult
            {
                Feature = feature.Key,
                Enabled = false,
                Reason = "feature is disabled"
            };

        if (!config.Enabled)
            return new EvaluationResult
            {
                Feature = feature.Key,
                Enabled = false,
                Reason = "feature is disabled in this environment"
            };

        if (config.AllowedUsers.Contains(user.Id))
            return new EvaluationResult
            {
                Feature = feature.Key,
                Enabled = true,
                Reason = "user explicitly allowed"
            };

        if (userGroups.Any(g => config.AllowedGroups.Contains(g)))
            return new EvaluationResult
            {
                Feature = feature.Key,
                Enabled = true,
                Reason = $"user belongs to allowed group {userGroups.First(g => config.AllowedGroups.Contains(g))}"
            };

        var hash = Math.Abs((user.Id.ToString() + feature.Key).GetHashCode()) % 100;
        if (hash < config.Rollout)
            return new EvaluationResult
            {
                Feature = feature.Key,
                Enabled = true,
                Reason = "user is in rollout percentage"
            };

        return new EvaluationResult
        {
            Feature = feature.Key,
            Enabled = false,
            Reason = "user is not allowed"
        };
    }
}