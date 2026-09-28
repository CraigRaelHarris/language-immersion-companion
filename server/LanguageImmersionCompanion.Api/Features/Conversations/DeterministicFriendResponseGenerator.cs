using LanguageImmersionCompanion.Api.Domain.Conversations;

namespace LanguageImmersionCompanion.Api.Features.Conversations;

public sealed class DeterministicFriendResponseGenerator
{
    public FriendResponse Generate(ConversationParticipant responder)
    {
        var text = responder.Id switch
        {
            "thandi" => "Sawubona! Ngiyajabula ukukhuluma nawe. (Hello! I am happy to speak with you.)",
            "sipho" => "Kuhle! Uqhubeka kahle. (Good! You are doing well.)",
            _ => throw new InvalidOperationException($"No response is configured for '{responder.Id}'.")
        };

        return new FriendResponse(responder.Id, text);
    }
}