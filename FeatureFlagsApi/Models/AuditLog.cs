namespace FeatureFlagsApi.Models;

public class AuditLog
{
    public int Id { get; set; }
    public required string Action { get; set; }
    public required string Resource { get; set; }
    public string? ResourceKey { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}