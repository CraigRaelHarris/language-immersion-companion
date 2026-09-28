namespace LanguageImmersionCompanion.Api.Domain.Conversations;

public sealed record FriendResponse
{
    public FriendResponse(string participantId, string text)
    {
        if (string.IsNullOrWhiteSpace(participantId))
        {
            throw new DomainRuleViolationException("A friend response participant ID is required.");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainRuleViolationException("Friend response text is required.");
        }

        ParticipantId = participantId.Trim();
        Text = text.Trim();
    }

    public string ParticipantId { get; }

    public string Text { get; }
}