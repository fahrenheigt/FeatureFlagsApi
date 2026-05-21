namespace FeatureFlagsApi.Models;

public class EvaluationResult
{
    public required string Feature { get; set; }
    public bool Enabled { get; set; }
    public required string Reason { get; set; }
}