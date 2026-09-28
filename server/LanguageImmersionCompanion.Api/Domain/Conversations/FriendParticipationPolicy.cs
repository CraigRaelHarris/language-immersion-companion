namespace LanguageImmersionCompanion.Api.Domain.Conversations;

public sealed class FriendParticipationPolicy
{
    public ConversationParticipant GetRequiredResponder(
        IReadOnlyList<ConversationParticipant> friends,
        int completedTurnCount)
    {
        if (friends.Count != 2)
        {
            throw new DomainRuleViolationException(
                "The participation policy requires exactly two friends.");
        }

        if (completedTurnCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedTurnCount),
                "The completed turn count cannot be negative.");
        }

        return friends[completedTurnCount % friends.Count];
    }
}