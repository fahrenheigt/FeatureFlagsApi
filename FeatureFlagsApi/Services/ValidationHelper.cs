using System.ComponentModel.DataAnnotations;

namespace FeatureFlagsApi.Services;

public static class ValidationHelper
{
    public static (bool IsValid, List<string> Errors) Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        var errors = results
            .Where(r => r.ErrorMessage != null)
            .Select(r => r.ErrorMessage!)
            .ToList();
        return (isValid, errors);
    }
}