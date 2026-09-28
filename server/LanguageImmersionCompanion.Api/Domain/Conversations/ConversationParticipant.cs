namespace LanguageImmersionCompanion.Api.Domain.Conversations;

public sealed record ConversationParticipant
{
    public ConversationParticipant(string id, string displayName, ParticipantRole role)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainRuleViolationException("A participant ID is required.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainRuleViolationException("A participant display name is required.");
        }

        Id = id.Trim();
        DisplayName = displayName.Trim();
        Role = role;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public ParticipantRole Role { get; }
}