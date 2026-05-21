using FeatureFlagsApi.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, token) =>
    {
        document.Info.Title = "Feature Flags API";
        document.Info.Version = "0.7.7";
        document.Info.Description = "API de gestion de feature flags";
        return Task.CompletedTask;
    });
});
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapHealthEndpoints();
app.MapUserEndpoints();
app.MapGroupEndpoints();
app.MapEnvironmentEndpoints();
app.MapFeatureEndpoints();

app.Run();

public partial class Program { }