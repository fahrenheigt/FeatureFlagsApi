using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Services;

public static class UserStore
{
    public static List<User> Users { get; } = [];
    private static int _nextId = 1;

    public static int NextId() => _nextId++;
}