using FeatureFlagsApi.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, token) =>
    {
        var version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "dev";
        document.Info.Title = "Feature Flags API";
        document.Info.Version = version;
        document.Info.Description = "API de gestion de feature flags";
        return Task.CompletedTask;
    });
});
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference("/api/docs");

app.MapHealthEndpoints();
app.MapUserEndpoints();
app.MapGroupEndpoints();
app.MapEnvironmentEndpoints();
app.MapFeatureEndpoints();
app.MapAuditEndpoints();

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}
