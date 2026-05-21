using System.ComponentModel.DataAnnotations;

namespace FeatureFlagsApi.Models;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "L'email est obligatoire")]
    [EmailAddress(ErrorMessage = "L'email est invalide")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [MinLength(2, ErrorMessage = "Le nom doit contenir au moins 2 caractères")]
    public required string Name { get; set; }

    [Required]
    [RegularExpression("^(user|admin)$", ErrorMessage = "Le rôle doit être 'user' ou 'admin'")]
    public string Role { get; set; } = "user";
}