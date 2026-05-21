using System.ComponentModel.DataAnnotations;

namespace FeatureFlagsApi.Models;

public class FeatureEnvironment
{
    [Required(ErrorMessage = "Le nom est obligatoire")]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Le nom ne peut contenir que des minuscules, chiffres et tirets")]
    public required string Name { get; set; }

    public string Description { get; set; } = string.Empty;
}