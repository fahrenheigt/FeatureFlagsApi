using FeatureFlagsApi.Endpoints;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.Use(async (context, next) =>
{
    await next();
});

app.MapHealthEndpoints();
app.MapUserEndpoints();
app.MapGroupEndpoints();
app.MapEnvironmentEndpoints();
app.MapFeatureEndpoints();

app.Run();

public partial class Program { }