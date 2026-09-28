using System.Collections.ObjectModel;

namespace LanguageImmersionCompanion.Api.Domain.Conversations;

public sealed class Conversation
{
    private readonly IReadOnlyList<ConversationParticipant> _friends;
    private readonly List<ConversationTurn> _turns = [];
    private readonly FriendParticipationPolicy _participationPolicy;

    private Conversation(
        IReadOnlyList<ConversationParticipant> participants,
        ConversationParticipant learner,
        IReadOnlyList<ConversationParticipant> friends,
        FriendParticipationPolicy participationPolicy)
    {
        Participants = participants;
        Learner = learner;
        _friends = friends;
        _participationPolicy = participationPolicy;
        Turns = _turns.AsReadOnly();
    }

    public IReadOnlyList<ConversationParticipant> Participants { get; }

    public ConversationParticipant Learner { get; }

    public IReadOnlyList<ConversationTurn> Turns { get; }

    public ConversationParticipant RequiredResponder =>
        _participationPolicy.GetRequiredResponder(_friends, _turns.Count);

    public static Conversation Create(
        IEnumerable<ConversationParticipant> participants,
        FriendParticipationPolicy? participationPolicy = null)
    {
        ArgumentNullException.ThrowIfNull(participants);

        var participantList = participants.ToArray();
        if (participantList.Length != 3)
        {
            throw new DomainRuleViolationException(
                "A conversation must have exactly three participants.");
        }

        if (participantList.DistinctBy(participant => participant.Id, StringComparer.Ordinal).Count() != 3)
        {
            throw new DomainRuleViolationException("Participant IDs must be unique.");
        }

        var learners = participantList
            .Where(participant => participant.Role == ParticipantRole.Learner)
            .ToArray();
        var friends = participantList
            .Where(participant => participant.Role == ParticipantRole.Friend)
            .ToArray();

        if (learners.Length != 1 || friends.Length != 2)
        {
            throw new DomainRuleViolationException(
                "A conversation must have exactly one learner and two friends.");
        }

        return new Conversation(
            new ReadOnlyCollection<ConversationParticipant>(participantList),
            learners[0],
            new ReadOnlyCollection<ConversationParticipant>(friends),
            participationPolicy ?? new FriendParticipationPolicy());
    }

    public ConversationTurn AddLearnerTurn(
        string learnerParticipantId,
        string learnerText,
        IEnumerable<FriendResponse> friendResponses)
    {
        if (string.IsNullOrWhiteSpace(learnerParticipantId)
            || !string.Equals(learnerParticipantId.Trim(), Learner.Id, StringComparison.Ordinal))
        {
            throw new DomainRuleViolationException(
                "Only the conversation learner can author a learner turn.");
        }

        if (string.IsNullOrWhiteSpace(learnerText))
        {
            throw new DomainRuleViolationException("Learner message text is required.");
        }

        ArgumentNullException.ThrowIfNull(friendResponses);
        var responses = friendResponses.ToArray();

        if (responses.Length == 0)
        {
            throw new DomainRuleViolationException(
                "Every learner turn requires at least one friend response.");
        }

        if (responses
            .GroupBy(response => response.ParticipantId, StringComparer.Ordinal)
            .Any(group => group.Count() > 1))
        {
            throw new DomainRuleViolationException(
                "A friend can respond at most once per learner turn.");
        }

        var friendIds = _friends.Select(friend => friend.Id).ToHashSet(StringComparer.Ordinal);
        if (responses.Any(response => !friendIds.Contains(response.ParticipantId)))
        {
            throw new DomainRuleViolationException(
                "Only registered friends can provide friend responses.");
        }

        var requiredResponder = RequiredResponder;
        if (!responses.Any(response =>
                string.Equals(
                    response.ParticipantId,
                    requiredResponder.Id,
                    StringComparison.Ordinal)))
        {
            throw new DomainRuleViolationException(
                $"Friend '{requiredResponder.Id}' is required to respond to this turn.");
        }

        var turn = new ConversationTurn(
            _turns.Count + 1,
            Learner.Id,
            learnerText.Trim(),
            responses);

        _turns.Add(turn);
        return turn;
    }
}