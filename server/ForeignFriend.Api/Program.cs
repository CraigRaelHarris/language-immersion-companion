var builder = WebApplication.CreateBuilder(args);

const string developmentCorsPolicy = "DevelopmentClient";

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
}

app.MapGet("/api/health", () => Results.Ok(new HealthResponse("healthy", "ForeignFriend.Api")))
    .WithName("GetHealth");

app.Run();

public partial class Program;

public sealed record HealthResponse(string Status, string Service);
