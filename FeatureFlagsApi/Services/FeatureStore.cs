using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Services;

public static class FeatureStore
{
    public static List<Feature> Features { get; } = [];
}