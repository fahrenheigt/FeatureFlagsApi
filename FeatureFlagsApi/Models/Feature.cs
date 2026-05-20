namespace FeatureFlagsApi.Models;

public class Feature
{
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
    public Dictionary<string, EnvironmentConfig> Environments { get; set; } = [];
}

public class EnvironmentConfig
{
    public bool Enabled { get; set; } = false;
    public int Rollout { get; set; } = 0;
    public List<string> AllowedGroups { get; set; } = [];
    public List<int> AllowedUsers { get; set; } = [];
}