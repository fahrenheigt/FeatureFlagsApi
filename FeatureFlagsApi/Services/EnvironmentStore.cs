using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Services;

public static class EnvironmentStore
{
    public static List<FeatureEnvironment> Environments { get; } = [];
}