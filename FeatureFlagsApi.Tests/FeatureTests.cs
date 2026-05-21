using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Tests;

public class FeatureTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FeatureTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateFeature_Returns201()
    {
        var feature = new Feature { Key = "new-dashboard", Name = "Nouveau dashboard", Description = "Test" };
        var response = await _client.PostAsJsonAsync("/api/features", feature);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetFeatures_Returns200()
    {
        var response = await _client.GetAsync("/api/features");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFeature_Returns200()
    {
        var feature = new Feature { Key = "get-feature", Name = "Get Feature", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var response = await _client.GetAsync("/api/features/get-feature");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFeature_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/features/inexistant");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateFeature_Duplicate_Returns409()
    {
        var feature = new Feature { Key = "duplicate-feature", Name = "Duplicate", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var response = await _client.PostAsJsonAsync("/api/features", feature);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFeature_Returns200()
    {
        var feature = new Feature { Key = "update-feature", Name = "Update Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var updated = new Feature { Key = "update-feature", Name = "Updated Name", Description = "Updated" };
        var response = await _client.PatchAsJsonAsync("/api/features/update-feature", updated);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFeature_NotFound_Returns404()
    {
        var feature = new Feature { Key = "x", Name = "x", Description = "x" };
        var response = await _client.PatchAsJsonAsync("/api/features/inexistant", feature);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFeature_ReturnsUpdatedName()
    {
        var feature = new Feature { Key = "update-name-feature", Name = "Original", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var updated = new Feature { Key = "update-name-feature", Name = "Updated", Description = "Test" };
        var response = await _client.PatchAsJsonAsync("/api/features/update-name-feature", updated);
        var result = await response.Content.ReadFromJsonAsync<Feature>();
        Assert.Equal("Updated", result!.Name);
    }

    [Fact]
    public async Task EnableFeature_Returns200()
    {
        var feature = new Feature { Key = "enable-feature", Name = "Enable Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var response = await _client.PatchAsync("/api/features/enable-feature/enable", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EnableFeature_NotFound_Returns404()
    {
        var response = await _client.PatchAsync("/api/features/inexistant/enable", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DisableFeature_Returns200()
    {
        var feature = new Feature { Key = "disable-feature", Name = "Disable Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var response = await _client.PatchAsync("/api/features/disable-feature/disable", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DisableFeature_NotFound_Returns404()
    {
        var response = await _client.PatchAsync("/api/features/inexistant/disable", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SetEnvironmentConfig_Returns200()
    {
        var feature = new Feature { Key = "config-feature", Name = "Config Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var config = new EnvironmentConfig { Enabled = true, Rollout = 25, AllowedGroups = ["beta-testers"], AllowedUsers = [1, 4, 8] };
        var response = await _client.PutAsJsonAsync("/api/features/config-feature/environments/prod/config", config);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SetEnvironmentConfig_FeatureNotFound_Returns404()
    {
        var config = new EnvironmentConfig { Enabled = true };
        var response = await _client.PutAsJsonAsync("/api/features/inexistant/environments/prod/config", config);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetEnvironmentConfig_Returns200()
    {
        var feature = new Feature { Key = "get-config-feature", Name = "Get Config Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var config = new EnvironmentConfig { Enabled = true, Rollout = 50 };
        await _client.PutAsJsonAsync("/api/features/get-config-feature/environments/prod/config", config);
        var response = await _client.GetAsync("/api/features/get-config-feature/environments/prod/config");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEnvironmentConfig_FeatureNotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/features/inexistant/environments/prod/config");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetEnvironmentConfig_EnvNotFound_Returns404()
    {
        var feature = new Feature { Key = "no-env-config-feature", Name = "Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var response = await _client.GetAsync("/api/features/no-env-config-feature/environments/inexistant/config");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEnvironmentConfig_Returns204()
    {
        var feature = new Feature { Key = "delete-config-feature", Name = "Delete Config Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var config = new EnvironmentConfig { Enabled = true, Rollout = 50 };
        await _client.PutAsJsonAsync("/api/features/delete-config-feature/environments/prod/config", config);
        var response = await _client.DeleteAsync("/api/features/delete-config-feature/environments/prod/config");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEnvironmentConfig_FeatureNotFound_Returns404()
    {
        var response = await _client.DeleteAsync("/api/features/inexistant/environments/prod/config");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEnvironmentConfig_EnvNotFound_Returns404()
    {
        var feature = new Feature { Key = "no-env-delete-feature", Name = "Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var response = await _client.DeleteAsync("/api/features/no-env-delete-feature/environments/inexistant/config");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFeature_Returns204()
    {
        var feature = new Feature { Key = "delete-feature", Name = "Delete Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var response = await _client.DeleteAsync("/api/features/delete-feature");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFeature_NotFound_Returns404()
    {
        var response = await _client.DeleteAsync("/api/features/inexistant");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateFeature_InvalidKey_Returns422()
    {
        var feature = new Feature { Key = "Invalid Key!", Name = "Test", Description = "Test" };
        var response = await _client.PostAsJsonAsync("/api/features", feature);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFeature_InvalidKey_Returns422()
    {
        var feature = new Feature { Key = "valid-feature-422", Name = "Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var updated = new Feature { Key = "Invalid Key!", Name = "Test", Description = "Test" };
        var response = await _client.PatchAsJsonAsync("/api/features/valid-feature-422", updated);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task SetEnvironmentConfig_InvalidRollout_Returns422()
    {
        var feature = new Feature { Key = "rollout-422", Name = "Test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var config = new EnvironmentConfig { Enabled = true, Rollout = 150 };
        var response = await _client.PutAsJsonAsync("/api/features/rollout-422/environments/prod/config", config);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}