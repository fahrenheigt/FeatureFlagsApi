namespace FeatureFlagsApi.Models;

public class FeatureEnvironment
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}