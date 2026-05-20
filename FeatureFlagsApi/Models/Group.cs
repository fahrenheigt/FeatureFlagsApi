using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Models;

public class Group
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<int> UserIds { get; set; } = [];
}