using FeatureFlagsApi.Models;
using FeatureFlagsApi.Services;
using Group = FeatureFlagsApi.Models.Group;

namespace FeatureFlagsApi.Endpoints;

public static class GroupEndpoints
{
    public static void MapGroupEndpoints(this WebApplication app)
    {
        app.MapGet("/api/groups", () =>
            Results.Ok(GroupStore.Groups));

        app.MapGet("/api/groups/{id}", (int id) =>
        {
            var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
            return group is null ? Results.NotFound() : Results.Ok(group);
        });

        app.MapPost("/api/groups", (Group group) =>
        {
            var (isValid, errors) = ValidationHelper.Validate(group);
            if (!isValid) return Results.UnprocessableEntity(errors);

            if (GroupStore.Groups.Any(g => g.Name == group.Name))
                return Results.Conflict("Un groupe avec ce nom existe déjà.");

            group.Id = GroupStore.NextId();
            GroupStore.Groups.Add(group);
            return Results.Created($"/api/groups/{group.Id}", group);
        });

        app.MapPatch("/api/groups/{id}", (int id, Group updated) =>
        {
            var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
            if (group is null) return Results.NotFound();

            var (isValid, errors) = ValidationHelper.Validate(updated);
            if (!isValid) return Results.UnprocessableEntity(errors);

            group.Name = updated.Name ?? group.Name;
            group.Description = updated.Description ?? group.Description;
            return Results.Ok(group);
        });

        app.MapDelete("/api/groups/{id}", (int id) =>
        {
            var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
            if (group is null) return Results.NotFound();

            GroupStore.Groups.Remove(group);
            return Results.NoContent();
        });

        app.MapPost("/api/groups/{id}/users/{userId}", (int id, int userId) =>
        {
            var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
            if (group is null) return Results.NotFound("Groupe introuvable.");

            var user = UserStore.Users.FirstOrDefault(u => u.Id == userId);
            if (user is null) return Results.NotFound("Utilisateur introuvable.");

            if (group.UserIds.Contains(userId))
                return Results.Conflict("L'utilisateur est déjà dans ce groupe.");

            group.UserIds.Add(userId);
            return Results.Ok(group);
        });

        app.MapDelete("/api/groups/{id}/users/{userId}", (int id, int userId) =>
        {
            var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
            if (group is null) return Results.NotFound("Groupe introuvable.");

            if (!group.UserIds.Contains(userId))
                return Results.NotFound("Utilisateur introuvable dans ce groupe.");

            group.UserIds.Remove(userId);
            return Results.NoContent();
        });

        app.MapGet("/api/groups/{id}/users", (int id) =>
        {
            var group = GroupStore.Groups.FirstOrDefault(g => g.Id == id);
            if (group is null) return Results.NotFound();

            var users = UserStore.Users.Where(u => group.UserIds.Contains(u.Id));
            return Results.Ok(users);
        });
    }
}