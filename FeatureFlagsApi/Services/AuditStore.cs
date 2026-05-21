using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Services;

public static class AuditStore
{
    public static List<AuditLog> Logs { get; } = [];
    private static int _nextId = 1;

    public static void Log(string action, string resource, string? resourceKey = null, string? details = null)
    {
        Logs.Add(new AuditLog
        {
            Id = _nextId++,
            Action = action,
            Resource = resource,
            ResourceKey = resourceKey,
            Details = details
        });
    }
}