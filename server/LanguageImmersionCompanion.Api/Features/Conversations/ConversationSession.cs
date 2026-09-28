using LanguageImmersionCompanion.Api.Domain.Conversations;

namespace LanguageImmersionCompanion.Api.Features.Conversations;

public sealed class ConversationSession
{
    public ConversationSession(Guid id, Conversation conversation)
    {
        Id = id;
        Conversation = conversation;
    }

    public Guid Id { get; }

    public Conversation Conversation { get; }

    public object SyncRoot { get; } = new();
}