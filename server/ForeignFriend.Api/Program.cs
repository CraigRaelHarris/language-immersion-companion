var builder = WebApplication.CreateBuilder(args);

const string developmentCorsPolicy = "DevelopmentClient";

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy(developmentCorsPolicy, policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.IsLoopback && uri.Scheme is "http" or "https";
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(developmentCorsPolicy);
    app.MapOpenApi();
    app.MapGet("/", () => Results.Redirect("/openapi/v1.json"))
        .ExcludeFromDescription();
}

app.MapGet("/api/health", () => Results.Ok(new HealthResponse("healthy", "ForeignFriend.Api")))
    .WithName("GetHealth")
    .WithTags("System")
    .WithSummary("Check API health")
    .WithDescription("Confirms that the Foreign Friend API is running and reachable.")
    .Produces<HealthResponse>(StatusCodes.Status200OK);

app.Run();

public partial class Program;

public sealed record HealthResponse(string Status, string Service);
