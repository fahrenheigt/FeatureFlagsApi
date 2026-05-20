using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FeatureFlagsApi.Models;

namespace FeatureFlagsApi.Tests;

public class UserTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUser_Returns201()
    {
        var user = new User { Email = "alice@example.com", Name = "Alice", Role = "user" };
        var response = await _client.PostAsJsonAsync("/api/users", user);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_Returns200()
    {
        var response = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUser_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/users/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_Duplicate_Returns409()
    {
        var user = new User { Email = "bob@example.com", Name = "Bob", Role = "user" };
        await _client.PostAsJsonAsync("/api/users", user);
        var response = await _client.PostAsJsonAsync("/api/users", user);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_Returns200()
    {
        var user = new User { Email = "charlie@example.com", Name = "Charlie", Role = "user" };
        var created = await _client.PostAsJsonAsync("/api/users", user);
        var createdUser = await created.Content.ReadFromJsonAsync<User>();

        var updated = new User { Name = "Charlie Updated", Email = "charlie@example.com", Role = "user" };
        var response = await _client.PatchAsJsonAsync($"/api/users/{createdUser!.Id}", updated);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_Returns204()
    {
        var user = new User { Email = "dave@example.com", Name = "Dave", Role = "user" };
        var created = await _client.PostAsJsonAsync("/api/users", user);
        var createdUser = await created.Content.ReadFromJsonAsync<User>();

        var response = await _client.DeleteAsync($"/api/users/{createdUser!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}