using System.Collections.Concurrent;
using LanguageImmersionCompanion.Api.Domain.Conversations;

namespace LanguageImmersionCompanion.Api.Features.Conversations;

public sealed class InMemoryConversationStore
{
    private const string LearnerId = "learner";
    private readonly ConcurrentDictionary<Guid, ConversationSession> _sessions = new();
    private readonly DeterministicFriendResponseGenerator _responseGenerator;

    public InMemoryConversationStore(DeterministicFriendResponseGenerator responseGenerator)
    {
        _responseGenerator = responseGenerator;
    }

    public ConversationSession Create(string? learnerName)
    {
        var displayName = string.IsNullOrWhiteSpace(learnerName) ? "You" : learnerName.Trim();
        var conversation = Conversation.Create(
        [
            new ConversationParticipant(LearnerId, displayName, ParticipantRole.Learner),
            new ConversationParticipant("thandi", "Thandi", ParticipantRole.Friend),
            new ConversationParticipant("sipho", "Sipho", ParticipantRole.Friend)
        ]);
        var session = new ConversationSession(Guid.NewGuid(), conversation);
        _sessions[session.Id] = session;
        return session;
    }

    public bool TryGet(Guid id, out ConversationSession? session) =>
        _sessions.TryGetValue(id, out session);

    public bool TryAddTurn(Guid id, string text, out ConversationSession? session)
    {
        if (!_sessions.TryGetValue(id, out session))
        {
            return false;
        }

        lock (session.SyncRoot)
        {
            var response = _responseGenerator.Generate(session.Conversation.RequiredResponder);
            session.Conversation.AddLearnerTurn(LearnerId, text, [response]);
        }

        return true;
    }
}