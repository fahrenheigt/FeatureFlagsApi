using FeatureFlagsApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapHealthEndpoints();
app.MapUserEndpoints();
app.MapGroupEndpoints();


app.Run();

public partial class Program { }