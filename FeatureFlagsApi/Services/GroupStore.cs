using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Services;

public static class GroupStore
{
    public static List<Group> Groups { get; } = [];
    private static int _nextId = 1;

    public static int NextId() => _nextId++;
}