using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Tests;

public class AuditTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuditTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuditLogs_Returns200()
    {
        var response = await _client.GetAsync("/api/audit-logs");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateFeature_CreatesAuditLog()
    {
        var feature = new Feature { Key = "audit-create", Name = "Audit Create", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);

        var response = await _client.GetAsync("/api/features/audit-create/audit-logs");
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLog>>();

        Assert.Contains(logs!, l => l.Action == "created" && l.ResourceKey == "audit-create");
    }

    [Fact]
    public async Task EnableFeature_CreatesAuditLog()
    {
        var feature = new Feature { Key = "audit-enable", Name = "Audit Enable", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        await _client.PatchAsync("/api/features/audit-enable/enable", null);

        var response = await _client.GetAsync("/api/features/audit-enable/audit-logs");
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLog>>();

        Assert.Contains(logs!, l => l.Action == "enabled" && l.ResourceKey == "audit-enable");
    }

    [Fact]
    public async Task DisableFeature_CreatesAuditLog()
    {
        var feature = new Feature { Key = "audit-disable", Name = "Audit Disable", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        await _client.PatchAsync("/api/features/audit-disable/disable", null);

        var response = await _client.GetAsync("/api/features/audit-disable/audit-logs");
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLog>>();

        Assert.Contains(logs!, l => l.Action == "disabled" && l.ResourceKey == "audit-disable");
    }

    [Fact]
    public async Task UpdateFeature_CreatesAuditLog()
    {
        var feature = new Feature { Key = "audit-update", Name = "Audit Update", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var updated = new Feature { Key = "audit-update", Name = "Updated", Description = "Test" };
        await _client.PatchAsJsonAsync("/api/features/audit-update", updated);

        var response = await _client.GetAsync("/api/features/audit-update/audit-logs");
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLog>>();

        Assert.Contains(logs!, l => l.Action == "updated" && l.ResourceKey == "audit-update");
    }

    [Fact]
    public async Task DeleteFeature_CreatesAuditLog()
    {
        var feature = new Feature { Key = "audit-delete", Name = "Audit Delete", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        await _client.DeleteAsync("/api/features/audit-delete");

        var response = await _client.GetAsync("/api/audit-logs");
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLog>>();

        Assert.Contains(logs!, l => l.Action == "deleted" && l.ResourceKey == "audit-delete");
    }

    [Fact]
    public async Task UpdateConfig_CreatesAuditLog()
    {
        var feature = new Feature { Key = "audit-config", Name = "Audit Config", Description = "Test" };
        await _client.PostAsJsonAsync("/api/features", feature);
        var config = new EnvironmentConfig { Enabled = true, Rollout = 25 };
        await _client.PutAsJsonAsync("/api/features/audit-config/environments/prod/config", config);

        var response = await _client.GetAsync("/api/features/audit-config/audit-logs");
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLog>>();

        Assert.Contains(logs!, l => l.Action == "config-updated" && l.ResourceKey == "audit-config");
    }

    [Fact]
    public async Task AddUserToGroup_CreatesAuditLog()
    {
        var group = new Group { Name = "audit-group", Description = "Test" };
        var createdGroup = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();

        var user = new User { Email = "audit-group@test.com", Name = "Audit", Role = "user" };
        var createdUser = await (await _client.PostAsJsonAsync("/api/users", user))
            .Content.ReadFromJsonAsync<User>();

        await _client.PostAsync($"/api/groups/{createdGroup!.Id}/users/{createdUser!.Id}", null);

        var response = await _client.GetAsync("/api/audit-logs");
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLog>>();

        Assert.Contains(logs!, l => l.Action == "user-added" && l.Resource == "group");
    }

    [Fact]
    public async Task RemoveUserFromGroup_CreatesAuditLog()
    {
        var group = new Group { Name = "audit-remove-group", Description = "Test" };
        var createdGroup = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();

        var user = new User { Email = "audit-remove@test.com", Name = "Audit Remove", Role = "user" };
        var createdUser = await (await _client.PostAsJsonAsync("/api/users", user))
            .Content.ReadFromJsonAsync<User>();

        await _client.PostAsync($"/api/groups/{createdGroup!.Id}/users/{createdUser!.Id}", null);
        await _client.DeleteAsync($"/api/groups/{createdGroup!.Id}/users/{createdUser!.Id}");

        var response = await _client.GetAsync("/api/audit-logs");
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLog>>();

        Assert.Contains(logs!, l => l.Action == "user-removed" && l.Resource == "group");
    }
}