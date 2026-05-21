using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Tests;

public class EnvironmentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EnvironmentTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateEnvironment_Returns201()
    {
        var env = new FeatureEnvironment { Name = "prod", Description = "Production" };
        var response = await _client.PostAsJsonAsync("/api/environments", env);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetEnvironments_Returns200()
    {
        var response = await _client.GetAsync("/api/environments");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEnvironment_Returns200()
    {
        var env = new FeatureEnvironment { Name = "get-env", Description = "Test" };
        await _client.PostAsJsonAsync("/api/environments", env);
        var response = await _client.GetAsync("/api/environments/get-env");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEnvironment_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/environments/inexistant");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateEnvironment_Duplicate_Returns409()
    {
        var env = new FeatureEnvironment { Name = "staging", Description = "Staging" };
        await _client.PostAsJsonAsync("/api/environments", env);
        var response = await _client.PostAsJsonAsync("/api/environments", env);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEnvironment_Returns200()
    {
        var env = new FeatureEnvironment { Name = "dev", Description = "Development" };
        await _client.PostAsJsonAsync("/api/environments", env);
        var updated = new FeatureEnvironment { Name = "dev", Description = "Updated" };
        var response = await _client.PatchAsJsonAsync("/api/environments/dev", updated);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEnvironment_NotFound_Returns404()
    {
        var env = new FeatureEnvironment { Name = "x", Description = "x" };
        var response = await _client.PatchAsJsonAsync("/api/environments/inexistant", env);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEnvironment_ReturnsUpdatedDescription()
    {
        var env = new FeatureEnvironment { Name = "update-desc-env", Description = "Original" };
        await _client.PostAsJsonAsync("/api/environments", env);
        var updated = new FeatureEnvironment { Name = "update-desc-env", Description = "Updated" };
        var response = await _client.PatchAsJsonAsync("/api/environments/update-desc-env", updated);
        var result = await response.Content.ReadFromJsonAsync<FeatureEnvironment>();
        Assert.Equal("Updated", result!.Description);
    }

    [Fact]
    public async Task DeleteEnvironment_Returns204()
    {
        var env = new FeatureEnvironment { Name = "test", Description = "Test" };
        await _client.PostAsJsonAsync("/api/environments", env);
        var response = await _client.DeleteAsync("/api/environments/test");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEnvironment_NotFound_Returns404()
    {
        var response = await _client.DeleteAsync("/api/environments/inexistant");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEnvironment_ThenGet_Returns404()
    {
        var env = new FeatureEnvironment { Name = "deleted-env", Description = "Test" };
        await _client.PostAsJsonAsync("/api/environments", env);
        await _client.DeleteAsync("/api/environments/deleted-env");
        var response = await _client.GetAsync("/api/environments/deleted-env");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}