using System.ComponentModel.DataAnnotations;

namespace FeatureFlagsApi.Models;

public class Feature
{
    [Required(ErrorMessage = "La clé est obligatoire")]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "La clé ne peut contenir que des minuscules, chiffres et tirets")]
    public required string Key { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire")]
    public required string Name { get; set; }

    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
    public Dictionary<string, EnvironmentConfig> Environments { get; set; } = [];
}

public class EnvironmentConfig
{
    [Range(0, 100, ErrorMessage = "Le rollout doit être entre 0 et 100")]
    public int Rollout { get; set; } = 0;

    public bool Enabled { get; set; } = false;
    public List<string> AllowedGroups { get; set; } = [];
    public List<int> AllowedUsers { get; set; } = [];
}