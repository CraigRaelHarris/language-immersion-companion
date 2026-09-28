using LanguageImmersionCompanion.Api.Domain.Conversations;

namespace LanguageImmersionCompanion.Api.Features.Conversations;

public sealed record CreateConversationRequest(string? LearnerName);

public sealed record AddTurnRequest(string? Text);

public sealed record ConversationResponse(
    Guid Id,
    IReadOnlyList<ParticipantResponse> Participants,
    IReadOnlyList<TurnResponse> Turns)
{
    public static ConversationResponse FromSession(ConversationSession session) => new(
        session.Id,
        session.Conversation.Participants
            .Select(participant => new ParticipantResponse(
                participant.Id,
                participant.DisplayName,
                participant.Role == ParticipantRole.Learner ? "learner" : "friend"))
            .ToArray(),
        session.Conversation.Turns
            .Select(turn => new TurnResponse(
                turn.SequenceNumber,
                turn.LearnerParticipantId,
                turn.LearnerText,
                turn.FriendResponses
                    .Select(response => new FriendResponseDto(response.ParticipantId, response.Text))
                    .ToArray()))
            .ToArray());
}

public sealed record ParticipantResponse(string Id, string DisplayName, string Role);

public sealed record TurnResponse(
    int SequenceNumber,
    string LearnerParticipantId,
    string LearnerText,
    IReadOnlyList<FriendResponseDto> FriendResponses);

public sealed record FriendResponseDto(string ParticipantId, string Text);