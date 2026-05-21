using FeatureFlagsApi.Services;
using System.ComponentModel.DataAnnotations;

namespace FeatureFlagsApi.Tests;

public class ValidationHelperTests
{
    [Fact]
    public void Validate_ValidObject_ReturnsValid()
    {
        var obj = new TestModel { Name = "Valid" };
        var (isValid, errors) = ValidationHelper.Validate(obj);
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_InvalidObject_ReturnsErrors()
    {
        var obj = new TestModel { Name = "" };
        var (isValid, errors) = ValidationHelper.Validate(obj);
        Assert.False(isValid);
        Assert.NotEmpty(errors);
    }

    private class TestModel
    {
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [MinLength(1)]
        public string Name { get; set; } = string.Empty;
    }
}