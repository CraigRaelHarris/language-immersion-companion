namespace LanguageImmersionCompanion.Api.Features.Conversations;

public static class ConversationEndpoints
{
    public static IEndpointRouteBuilder MapConversationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var conversations = endpoints.MapGroup("/api/conversations")
            .WithTags("Conversations");

        conversations.MapPost("/", CreateConversation)
            .WithName("CreateConversation")
            .WithSummary("Start a text conversation")
            .Produces<ConversationResponse>(StatusCodes.Status201Created);

        conversations.MapGet("/{conversationId:guid}", GetConversation)
            .WithName("GetConversation")
            .WithSummary("Get a conversation")
            .Produces<ConversationResponse>()
            .Produces(StatusCodes.Status404NotFound);

        conversations.MapPost("/{conversationId:guid}/turns", AddTurn)
            .WithName("AddConversationTurn")
            .WithSummary("Send a learner message")
            .Produces<ConversationResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static IResult CreateConversation(
        CreateConversationRequest? request,
        InMemoryConversationStore store)
    {
        var session = store.Create(request?.LearnerName);
        return Results.Created(
            $"/api/conversations/{session.Id}",
            ConversationResponse.FromSession(session));
    }

    private static IResult GetConversation(Guid conversationId, InMemoryConversationStore store)
    {
        if (!store.TryGet(conversationId, out var session) || session is null)
        {
            return Results.NotFound();
        }

        lock (session.SyncRoot)
        {
            return Results.Ok(ConversationResponse.FromSession(session));
        }
    }

    private static IResult AddTurn(
        Guid conversationId,
        AddTurnRequest? request,
        InMemoryConversationStore store)
    {
        if (string.IsNullOrWhiteSpace(request?.Text))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["text"] = ["Message text is required."]
            });
        }

        if (!store.TryAddTurn(conversationId, request.Text.Trim(), out var session)
            || session is null)
        {
            return Results.NotFound();
        }

        lock (session.SyncRoot)
        {
            return Results.Ok(ConversationResponse.FromSession(session));
        }
    }
}