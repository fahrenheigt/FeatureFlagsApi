using FeatureFlagsApi.Models;
using FeatureFlagsApi.Services;

namespace FeatureFlagsApi.Tests;

public class FeatureServiceTests
{
    private readonly Feature _feature = new()
    {
        Key = "new-dashboard",
        Name = "Nouveau dashboard",
        Description = "Test",
        Enabled = true
    };

    private readonly User _user = new()
    {
        Id = 1,
        Email = "alice@example.com",
        Name = "Alice",
        Role = "user"
    };

    private readonly EnvironmentConfig _config = new()
    {
        Enabled = true,
        Rollout = 0,
        AllowedGroups = [],
        AllowedUsers = []
    };

    [Fact]
    public void FeatureDisabled_ReturnsFalse()
    {
        var feature = new Feature { Key = "test", Name = "Test", Enabled = false };
        var result = FeatureService.EvaluateFeatureAccess(feature, _config, _user, []);
        Assert.False(result.Enabled);
        Assert.Equal("feature is disabled", result.Reason);
    }

    [Fact]
    public void EnvironmentDisabled_ReturnsFalse()
    {
        var config = new EnvironmentConfig { Enabled = false };
        var result = FeatureService.EvaluateFeatureAccess(_feature, config, _user, []);
        Assert.False(result.Enabled);
        Assert.Equal("feature is disabled in this environment", result.Reason);
    }

    [Fact]
    public void UserExplicitlyAllowed_ReturnsTrue()
    {
        var config = new EnvironmentConfig { Enabled = true, AllowedUsers = [1] };
        var result = FeatureService.EvaluateFeatureAccess(_feature, config, _user, []);
        Assert.True(result.Enabled);
        Assert.Equal("user explicitly allowed", result.Reason);
    }

    [Fact]
    public void UserInAllowedGroup_ReturnsTrue()
    {
        var config = new EnvironmentConfig { Enabled = true, AllowedGroups = ["beta-testers"] };
        var result = FeatureService.EvaluateFeatureAccess(_feature, config, _user, ["beta-testers"]);
        Assert.True(result.Enabled);
        Assert.Contains("beta-testers", result.Reason);
    }

    [Fact]
    public void UserInRollout_ReturnsTrue()
    {
        var config = new EnvironmentConfig { Enabled = true, Rollout = 100 };
        var result = FeatureService.EvaluateFeatureAccess(_feature, config, _user, []);
        Assert.True(result.Enabled);
        Assert.Equal("user is in rollout percentage", result.Reason);
    }

    [Fact]
    public void UserNotInRollout_ReturnsFalse()
    {
        var config = new EnvironmentConfig { Enabled = true, Rollout = 0 };
        var result = FeatureService.EvaluateFeatureAccess(_feature, config, _user, []);
        Assert.False(result.Enabled);
        Assert.Equal("user is not allowed", result.Reason);
    }

    [Fact]
    public void RolloutIsStable_SameResultEveryTime()
    {
        var config = new EnvironmentConfig { Enabled = true, Rollout = 50 };
        var result1 = FeatureService.EvaluateFeatureAccess(_feature, config, _user, []);
        var result2 = FeatureService.EvaluateFeatureAccess(_feature, config, _user, []);
        Assert.Equal(result1.Enabled, result2.Enabled);
    }

    [Fact]
    public void UserNotAllowed_ReturnsFalse()
    {
        var result = FeatureService.EvaluateFeatureAccess(_feature, _config, _user, []);
        Assert.False(result.Enabled);
        Assert.Equal("user is not allowed", result.Reason);
    }
}