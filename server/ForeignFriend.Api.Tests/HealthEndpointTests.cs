using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ForeignFriend.Api.Tests;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealthReturnsHealthyService()
    {
        var response = await _client.GetAsync("/api/health");
        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(health);
        Assert.Equal("healthy", health.Status);
        Assert.Equal("ForeignFriend.Api", health.Service);
    }

    [Fact]
    public async Task DevelopmentCorsAllowsLoopbackFlutterOrigin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/health");
        request.Headers.Add("Origin", "http://localhost:54321");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(
            "http://localhost:54321",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task OpenApiDocumentDescribesHealthEndpoint()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(
            document.RootElement
                .GetProperty("paths")
                .TryGetProperty("/api/health", out var healthPath));
        Assert.Equal(
            "Check API health",
            healthPath.GetProperty("get").GetProperty("summary").GetString());
    }

    [Fact]
    public async Task DevelopmentRootRedirectsToOpenApiDocument()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("/openapi/v1.json", response.RequestMessage?.RequestUri?.AbsolutePath);
    }
}