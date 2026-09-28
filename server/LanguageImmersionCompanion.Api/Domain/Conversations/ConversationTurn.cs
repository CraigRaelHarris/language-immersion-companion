using System.Collections.ObjectModel;

namespace LanguageImmersionCompanion.Api.Domain.Conversations;

public sealed class ConversationTurn
{
    internal ConversationTurn(
        int sequenceNumber,
        string learnerParticipantId,
        string learnerText,
        IEnumerable<FriendResponse> friendResponses)
    {
        SequenceNumber = sequenceNumber;
        LearnerParticipantId = learnerParticipantId;
        LearnerText = learnerText;
        FriendResponses = new ReadOnlyCollection<FriendResponse>(friendResponses.ToArray());
    }

    public int SequenceNumber { get; }

    public string LearnerParticipantId { get; }

    public string LearnerText { get; }

    public IReadOnlyList<FriendResponse> FriendResponses { get; }
}