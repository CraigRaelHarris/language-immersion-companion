using System.Net;
using System.Net.Http.Json;
using LanguageImmersionCompanion.Api.Features.Conversations;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LanguageImmersionCompanion.Api.Tests;

public sealed class ConversationEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ConversationEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateConversationReturnsLearnerAndTwoFriends()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/conversations",
            new CreateConversationRequest("Craig"));
        var conversation = await response.Content.ReadFromJsonAsync<ConversationResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(conversation);
        Assert.EndsWith($"/api/conversations/{conversation.Id}", response.Headers.Location?.ToString());
        Assert.Empty(conversation.Turns);
        Assert.Collection(
            conversation.Participants,
            learner =>
            {
                Assert.Equal("Craig", learner.DisplayName);
                Assert.Equal("learner", learner.Role);
            },
            thandi => Assert.Equal("Thandi", thandi.DisplayName),
            sipho => Assert.Equal("Sipho", sipho.DisplayName));
    }

    [Fact]
    public async Task SendingTurnsPersistsHistoryAndAlternatesFriends()
    {
        var conversation = await CreateConversation();

        var firstResponse = await _client.PostAsJsonAsync(
            $"/api/conversations/{conversation.Id}/turns",
            new AddTurnRequest("Sawubona"));
        var afterFirst = await firstResponse.Content.ReadFromJsonAsync<ConversationResponse>();

        var secondResponse = await _client.PostAsJsonAsync(
            $"/api/conversations/{conversation.Id}/turns",
            new AddTurnRequest("Ngiyaphila"));
        var afterSecond = await secondResponse.Content.ReadFromJsonAsync<ConversationResponse>();

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.NotNull(afterFirst);
        Assert.Equal("thandi", Assert.Single(afterFirst.Turns).FriendResponses.Single().ParticipantId);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.NotNull(afterSecond);
        Assert.Equal(2, afterSecond.Turns.Count);
        Assert.Equal("sipho", afterSecond.Turns[1].FriendResponses.Single().ParticipantId);

        var persisted = await _client.GetFromJsonAsync<ConversationResponse>(
            $"/api/conversations/{conversation.Id}");
        Assert.Equal(2, persisted?.Turns.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendingBlankTurnReturnsValidationProblem(string text)
    {
        var conversation = await CreateConversation();

        var response = await _client.PostAsJsonAsync(
            $"/api/conversations/{conversation.Id}/turns",
            new AddTurnRequest(text));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnknownConversationReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/conversations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<ConversationResponse> CreateConversation()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/conversations",
            new CreateConversationRequest(null));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ConversationResponse>())!;
    }
}