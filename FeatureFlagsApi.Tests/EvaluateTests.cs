using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Tests;

public class EvaluateTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EvaluateTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<User> CreateUser(string email, string name)
    {
        var user = new User { Email = email, Name = name, Role = "user" };
        var response = await _client.PostAsJsonAsync("/api/users", user);
        return (await response.Content.ReadFromJsonAsync<User>())!;
    }

    private async Task<Feature> CreateFeature(string key, bool enabled = true)
    {
        var feature = new Feature { Key = key, Name = key, Enabled = enabled };
        await _client.PostAsJsonAsync("/api/features", feature);
        if (enabled) await _client.PatchAsync($"/api/features/{key}/enable", null);
        return feature;
    }

    private async Task SetConfig(string key, string env, EnvironmentConfig config)
    {
        await _client.PutAsJsonAsync($"/api/features/{key}/environments/{env}/config", config);
    }

    [Fact]
    public async Task Evaluate_FeatureNotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/features/inexistant/evaluate?userId=1&env=prod");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_EnvironmentNotFound_Returns404()
    {
        await CreateFeature("eval-no-env");
        var user = await CreateUser("eval-no-env@test.com", "Test");
        var response = await _client.GetAsync($"/api/features/eval-no-env/evaluate?userId={user.Id}&env=inexistant");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_UserNotFound_Returns404()
    {
        await CreateFeature("eval-no-user");
        await SetConfig("eval-no-user", "prod", new EnvironmentConfig { Enabled = true });
        var response = await _client.GetAsync("/api/features/eval-no-user/evaluate?userId=9999&env=prod");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_FeatureDisabled_ReturnsFalse()
    {
        await CreateFeature("eval-disabled", false);
        var user = await CreateUser("eval-disabled@test.com", "Test");
        await SetConfig("eval-disabled", "prod", new EnvironmentConfig { Enabled = true });
        var response = await _client.GetAsync($"/api/features/eval-disabled/evaluate?userId={user.Id}&env=prod");
        var result = await response.Content.ReadFromJsonAsync<EvaluationResult>();
        Assert.False(result!.Enabled);
    }

    [Fact]
    public async Task Evaluate_UserExplicitlyAllowed_ReturnsTrue()
    {
        await CreateFeature("eval-explicit");
        var user = await CreateUser("eval-explicit@test.com", "Test");
        await SetConfig("eval-explicit", "prod", new EnvironmentConfig
        {
            Enabled = true,
            AllowedUsers = [user.Id]
        });
        var response = await _client.GetAsync($"/api/features/eval-explicit/evaluate?userId={user.Id}&env=prod");
        var result = await response.Content.ReadFromJsonAsync<EvaluationResult>();
        Assert.True(result!.Enabled);
        Assert.Equal("user explicitly allowed", result.Reason);
    }

    [Fact]
    public async Task Evaluate_UserInAllowedGroup_ReturnsTrue()
    {
        await CreateFeature("eval-group");
        var user = await CreateUser("eval-group@test.com", "Test");

        var group = new Group { Name = "eval-beta-testers", Description = "Test" };
        var createdGroup = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        await _client.PostAsync($"/api/groups/{createdGroup!.Id}/users/{user.Id}", null);

        await SetConfig("eval-group", "prod", new EnvironmentConfig
        {
            Enabled = true,
            AllowedGroups = ["eval-beta-testers"]
        });

        var response = await _client.GetAsync($"/api/features/eval-group/evaluate?userId={user.Id}&env=prod");
        var result = await response.Content.ReadFromJsonAsync<EvaluationResult>();
        Assert.True(result!.Enabled);
        Assert.Contains("eval-beta-testers", result.Reason);
    }

    [Fact]
    public async Task Evaluate_UserInRollout_ReturnsTrue()
    {
        await CreateFeature("eval-rollout");
        var user = await CreateUser("eval-rollout@test.com", "Test");
        await SetConfig("eval-rollout", "prod", new EnvironmentConfig
        {
            Enabled = true,
            Rollout = 100
        });
        var response = await _client.GetAsync($"/api/features/eval-rollout/evaluate?userId={user.Id}&env=prod");
        var result = await response.Content.ReadFromJsonAsync<EvaluationResult>();
        Assert.True(result!.Enabled);
    }
}