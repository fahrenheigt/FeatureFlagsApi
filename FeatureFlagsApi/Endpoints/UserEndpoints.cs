using FeatureFlagsApi.Models;
using FeatureFlagsApi.Services;

namespace FeatureFlagsApi.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/api/users", () =>
            Results.Ok(UserStore.Users));

        app.MapGet("/api/users/{id}", (int id) =>
        {
            var user = UserStore.Users.FirstOrDefault(u => u.Id == id);
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        app.MapPost("/api/users", (User user) =>
        {
            var (isValid, errors) = ValidationHelper.Validate(user);
            if (!isValid) return Results.UnprocessableEntity(errors);

            if (UserStore.Users.Any(u => u.Email == user.Email))
                return Results.Conflict("Un utilisateur avec cet email existe déjà.");

            user.Id = UserStore.NextId();
            UserStore.Users.Add(user);
            return Results.Created($"/api/users/{user.Id}", user);
        });

        app.MapPatch("/api/users/{id}", (int id, User updated) =>
        {
            var user = UserStore.Users.FirstOrDefault(u => u.Id == id);
            if (user is null) return Results.NotFound();

            var (isValid, errors) = ValidationHelper.Validate(updated);
            if (!isValid) return Results.UnprocessableEntity(errors);

            user.Name = updated.Name;
            user.Email = updated.Email;
            user.Role = updated.Role;
            return Results.Ok(user);
        });

        app.MapDelete("/api/users/{id}", (int id) =>
        {
            var user = UserStore.Users.FirstOrDefault(u => u.Id == id);
            if (user is null) return Results.NotFound();

            UserStore.Users.Remove(user);
            return Results.NoContent();
        });
    }
}