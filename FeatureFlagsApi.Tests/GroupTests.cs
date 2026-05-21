using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Tests;

public class GroupTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GroupTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateGroup_Returns201()
    {
        var group = new Group { Name = "beta-testers", Description = "Utilisateurs beta" };
        var response = await _client.PostAsJsonAsync("/api/groups", group);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetGroups_Returns200()
    {
        var response = await _client.GetAsync("/api/groups");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetGroup_Returns200()
    {
        var group = new Group { Name = "get-group", Description = "Test" };
        var created = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        var response = await _client.GetAsync($"/api/groups/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetGroup_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/groups/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateGroup_Duplicate_Returns409()
    {
        var group = new Group { Name = "duplicate-group", Description = "Test" };
        await _client.PostAsJsonAsync("/api/groups", group);
        var response = await _client.PostAsJsonAsync("/api/groups", group);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGroup_Returns200()
    {
        var group = new Group { Name = "update-group", Description = "Test" };
        var created = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        var updated = new Group { Name = "update-group", Description = "Updated" };
        var response = await _client.PatchAsJsonAsync($"/api/groups/{created!.Id}", updated);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGroup_NotFound_Returns404()
    {
        var group = new Group { Name = "x", Description = "x" };
        var response = await _client.PatchAsJsonAsync("/api/groups/9999", group);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGroup_ReturnsUpdatedDescription()
    {
        var group = new Group { Name = "update-desc-group", Description = "Original" };
        var created = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        var updated = new Group { Name = "update-desc-group", Description = "Updated" };
        var response = await _client.PatchAsJsonAsync($"/api/groups/{created!.Id}", updated);
        var result = await response.Content.ReadFromJsonAsync<Group>();
        Assert.Equal("Updated", result!.Description);
    }

    [Fact]
    public async Task DeleteGroup_Returns204()
    {
        var group = new Group { Name = "delete-group", Description = "Test" };
        var created = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        var response = await _client.DeleteAsync($"/api/groups/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteGroup_NotFound_Returns404()
    {
        var response = await _client.DeleteAsync("/api/groups/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddUserToGroup_Returns200()
    {
        var group = new Group { Name = "group-with-user", Description = "Test" };
        var createdGroup = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        var user = new User { Email = "groupuser@example.com", Name = "GroupUser", Role = "user" };
        var createdUser = await (await _client.PostAsJsonAsync("/api/users", user))
            .Content.ReadFromJsonAsync<User>();
        var response = await _client.PostAsync($"/api/groups/{createdGroup!.Id}/users/{createdUser!.Id}", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddUserToGroup_GroupNotFound_Returns404()
    {
        var response = await _client.PostAsync("/api/groups/9999/users/1", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddUserToGroup_UserNotFound_Returns404()
    {
        var group = new Group { Name = "group-user-notfound", Description = "Test" };
        var createdGroup = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        var response = await _client.PostAsync($"/api/groups/{createdGroup!.Id}/users/9999", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddUserToGroup_Duplicate_Returns409()
    {
        var group = new Group { Name = "group-duplicate-user", Description = "Test" };
        var createdGroup = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        var user = new User { Email = "dupuser@example.com", Name = "DupUser", Role = "user" };
        var createdUser = await (await _client.PostAsJsonAsync("/api/users", user))
            .Content.ReadFromJsonAsync<User>();
        await _client.PostAsync($"/api/groups/{createdGroup!.Id}/users/{createdUser!.Id}", null);
        var response = await _client.PostAsync($"/api/groups/{createdGroup!.Id}/users/{createdUser!.Id}", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RemoveUserFromGroup_Returns204()
    {
        var group = new Group { Name = "group-remove-user", Description = "Test" };
        var createdGroup = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        var user = new User { Email = "removeuser@example.com", Name = "RemoveUser", Role = "user" };
        var createdUser = await (await _client.PostAsJsonAsync("/api/users", user))
            .Content.ReadFromJsonAsync<User>();
        await _client.PostAsync($"/api/groups/{createdGroup!.Id}/users/{createdUser!.Id}", null);
        var response = await _client.DeleteAsync($"/api/groups/{createdGroup!.Id}/users/{createdUser!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RemoveUserFromGroup_GroupNotFound_Returns404()
    {
        var response = await _client.DeleteAsync("/api/groups/9999/users/1");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveUserFromGroup_UserNotFound_Returns404()
    {
        var group = new Group { Name = "group-remove-notfound", Description = "Test" };
        var createdGroup = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        var response = await _client.DeleteAsync($"/api/groups/{createdGroup!.Id}/users/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetGroupUsers_Returns200()
    {
        var group = new Group { Name = "group-get-users", Description = "Test" };
        var createdGroup = await (await _client.PostAsJsonAsync("/api/groups", group))
            .Content.ReadFromJsonAsync<Group>();
        var response = await _client.GetAsync($"/api/groups/{createdGroup!.Id}/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetGroupUsers_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/groups/9999/users");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}