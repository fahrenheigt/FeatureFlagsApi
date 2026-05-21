using FeatureFlagsApi.Models;
using FeatureFlagsApi.Services;
using Group = FeatureFlagsApi.Models.Group;

namespace FeatureFlagsApi.Endpoints;

public static class GroupEndpoints
{
    public static void MapGroupEndpoints(this WebApplication app)
    {
        app.MapGet("/api/groups", GetAllGroups);
        app.MapGet("/api/groups/{id}", GetGroup);
        app.MapPost("/api/groups", CreateGroup);
        app.MapPatch("/api/groups/{id}", UpdateGroup);
        app.MapDelete("/api/groups/{id}", DeleteGroup);
        app.MapPost("/api/groups/{id}/users/{userId}", AddUserToGroup);
        app.MapDelete("/api/groups/{id}/users/{userId}", RemoveUserFromGroup);
        app.MapGet("/api/groups/{id}/users", GetGroupUsers);
    }

    private static IResult GetAllGroups() =>
        Results.Ok(GroupStore.Groups);

    private static IResult GetGroup(int id)
    {
        var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
        return group is null ? Results.NotFound() : Results.Ok(group);
    }

    private static IResult CreateGroup(Group group)
    {
        var (isValid, errors) = ValidationHelper.Validate(group);
        if (!isValid) return Results.UnprocessableEntity(errors);

        if (GroupStore.Groups.Any(g => g.Name == group.Name))
            return Results.Conflict("Un groupe avec ce nom existe déjà.");

        group.Id = GroupStore.NextId();
        GroupStore.Groups.Add(group);
        return Results.Created($"/api/groups/{group.Id}", group);
    }

    private static IResult UpdateGroup(int id, Group updated)
    {
        var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
        if (group is null) return Results.NotFound();

        var (isValid, errors) = ValidationHelper.Validate(updated);
        if (!isValid) return Results.UnprocessableEntity(errors);

        group.Name = updated.Name;
        group.Description = updated.Description;
        return Results.Ok(group);
    }

    private static IResult DeleteGroup(int id)
    {
        var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
        if (group is null) return Results.NotFound();

        GroupStore.Groups.Remove(group);
        return Results.NoContent();
    }

    private static IResult AddUserToGroup(int id, int userId)
    {
        var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
        if (group is null) return Results.NotFound("Groupe introuvable.");

        var user = UserStore.Users.FirstOrDefault(u => u.Id == userId);
        if (user is null) return Results.NotFound("Utilisateur introuvable.");

        if (group.UserIds.Contains(userId))
            return Results.Conflict("L'utilisateur est déjà dans ce groupe.");

        group.UserIds.Add(userId);
        AuditStore.Log("user-added", AuditStore.ResourceGroup, group.Name, $"userId={userId}");
        return Results.Ok(group);
    }

    private static IResult RemoveUserFromGroup(int id, int userId)
    {
        var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
        if (group is null) return Results.NotFound("Groupe introuvable.");

        if (!group.UserIds.Contains(userId))
            return Results.NotFound("Utilisateur introuvable dans ce groupe.");

        group.UserIds.Remove(userId);
        AuditStore.Log("user-removed", AuditStore.ResourceGroup, group.Name, $"userId={userId}");
        return Results.NoContent();
    }

    private static IResult GetGroupUsers(int id)
    {
        var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
        if (group is null) return Results.NotFound();

        var users = UserStore.Users.Where(u => group.UserIds.Contains(u.Id));
        return Results.Ok(users);
    }
}