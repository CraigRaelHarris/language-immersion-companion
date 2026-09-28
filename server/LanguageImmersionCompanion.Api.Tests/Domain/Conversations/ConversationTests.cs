using LanguageImmersionCompanion.Api.Domain.Conversations;

namespace LanguageImmersionCompanion.Api.Tests.Domain.Conversations;

public sealed class ConversationTests
{
    [Fact]
    public void CreateBuildsConversationWithOneLearnerAndTwoFriends()
    {
        var conversation = CreateConversation();

        Assert.Equal(3, conversation.Participants.Count);
        Assert.Equal(LearnerId, conversation.Learner.Id);
        Assert.Equal(FriendAId, conversation.RequiredResponder.Id);
        Assert.Empty(conversation.Turns);
    }

    [Fact]
    public void ParticipantsCannotBeModifiedExternally()
    {
        var conversation = CreateConversation();
        var participants = Assert.IsAssignableFrom<IList<ConversationParticipant>>(
            conversation.Participants);

        Assert.Throws<NotSupportedException>(() => participants.Add(Friend("friend-c", "Zola")));
        Assert.Equal(3, conversation.Participants.Count);
    }

    [Theory]
    [InlineData("", "Learner")]
    [InlineData("   ", "Learner")]
    [InlineData("learner", "")]
    [InlineData("learner", "   ")]
    public void ParticipantRequiresIdAndDisplayName(string id, string displayName)
    {
        Assert.Throws<DomainRuleViolationException>(
            () => new ConversationParticipant(id, displayName, ParticipantRole.Learner));
    }

    [Fact]
    public void CreateRejectsDuplicateParticipantIds()
    {
        var participants = new[]
        {
            Learner(LearnerId, "Learner"),
            Friend(FriendAId, "Thandi"),
            Friend(FriendAId, "Sipho")
        };

        var error = Assert.Throws<DomainRuleViolationException>(
            () => Conversation.Create(participants));

        Assert.Equal("Participant IDs must be unique.", error.Message);
    }

    [Fact]
    public void CreateRejectsAnythingOtherThanThreeParticipants()
    {
        var participants = new[]
        {
            Learner(LearnerId, "Learner"),
            Friend(FriendAId, "Thandi")
        };

        Assert.Throws<DomainRuleViolationException>(() => Conversation.Create(participants));
    }

    [Fact]
    public void CreateRejectsIncorrectRoleCounts()
    {
        var participants = new[]
        {
            Learner(LearnerId, "Learner"),
            Learner("learner-2", "Other learner"),
            Friend(FriendAId, "Thandi")
        };

        var error = Assert.Throws<DomainRuleViolationException>(
            () => Conversation.Create(participants));

        Assert.Equal(
            "A conversation must have exactly one learner and two friends.",
            error.Message);
    }

    [Fact]
    public void AddLearnerTurnRejectsAnUnknownAuthor()
    {
        var conversation = CreateConversation();

        Assert.Throws<DomainRuleViolationException>(() => conversation.AddLearnerTurn(
            "someone-else",
            "Sawubona",
            [Response(FriendAId)]));
        Assert.Empty(conversation.Turns);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddLearnerTurnRejectsBlankLearnerText(string text)
    {
        var conversation = CreateConversation();

        Assert.Throws<DomainRuleViolationException>(() => conversation.AddLearnerTurn(
            LearnerId,
            text,
            [Response(FriendAId)]));
        Assert.Empty(conversation.Turns);
    }

    [Fact]
    public void AddLearnerTurnRequiresAtLeastOneFriendResponse()
    {
        var conversation = CreateConversation();

        Assert.Throws<DomainRuleViolationException>(() => conversation.AddLearnerTurn(
            LearnerId,
            "Sawubona",
            []));
        Assert.Empty(conversation.Turns);
    }

    [Fact]
    public void RequiredResponderAlternatesForEveryAcceptedTurn()
    {
        var conversation = CreateConversation();

        Assert.Equal(FriendAId, conversation.RequiredResponder.Id);
        var first = conversation.AddLearnerTurn(
            LearnerId,
            "Sawubona",
            [Response(FriendAId)]);

        Assert.Equal(1, first.SequenceNumber);
        Assert.Equal(FriendBId, conversation.RequiredResponder.Id);
        var second = conversation.AddLearnerTurn(
            LearnerId,
            "Ngiyaphila",
            [Response(FriendBId)]);

        Assert.Equal(2, second.SequenceNumber);
        Assert.Equal(FriendAId, conversation.RequiredResponder.Id);
        var third = conversation.AddLearnerTurn(
            LearnerId,
            "Wena unjani?",
            [Response(FriendAId)]);

        Assert.Equal(3, third.SequenceNumber);
        Assert.Equal(FriendBId, conversation.RequiredResponder.Id);
    }

    [Fact]
    public void AddLearnerTurnAllowsBothFriendsToRespond()
    {
        var conversation = CreateConversation();

        var turn = conversation.AddLearnerTurn(
            LearnerId,
            "Sawubona",
            [Response(FriendAId), Response(FriendBId)]);

        Assert.Equal(2, turn.FriendResponses.Count);
        Assert.Single(conversation.Turns);
    }

    [Fact]
    public void AddLearnerTurnRejectsOnlyTheOptionalFriend()
    {
        var conversation = CreateConversation();

        var error = Assert.Throws<DomainRuleViolationException>(
            () => conversation.AddLearnerTurn(
                LearnerId,
                "Sawubona",
                [Response(FriendBId)]));

        Assert.Contains(FriendAId, error.Message);
        Assert.Empty(conversation.Turns);
        Assert.Equal(FriendAId, conversation.RequiredResponder.Id);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData(LearnerId)]
    public void AddLearnerTurnRejectsResponsesFromNonFriends(string responderId)
    {
        var conversation = CreateConversation();

        Assert.Throws<DomainRuleViolationException>(() => conversation.AddLearnerTurn(
            LearnerId,
            "Sawubona",
            [Response(responderId), Response(FriendAId)]));
        Assert.Empty(conversation.Turns);
    }

    [Fact]
    public void AddLearnerTurnRejectsDuplicateResponsesFromOneFriend()
    {
        var conversation = CreateConversation();

        Assert.Throws<DomainRuleViolationException>(() => conversation.AddLearnerTurn(
            LearnerId,
            "Sawubona",
            [Response(FriendAId), Response(FriendAId)]));
        Assert.Empty(conversation.Turns);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FriendResponseRequiresText(string text)
    {
        Assert.Throws<DomainRuleViolationException>(() => new FriendResponse(FriendAId, text));
    }

    [Fact]
    public void AcceptedTurnTrimsTextAndExposesReadOnlyResponses()
    {
        var conversation = CreateConversation();

        var turn = conversation.AddLearnerTurn(
            $" {LearnerId} ",
            "  Sawubona  ",
            [new FriendResponse(FriendAId, "  Yebo  ")]);
        var responses = Assert.IsAssignableFrom<IList<FriendResponse>>(turn.FriendResponses);

        Assert.Equal("Sawubona", turn.LearnerText);
        Assert.Equal("Yebo", turn.FriendResponses[0].Text);
        Assert.Throws<NotSupportedException>(() => responses.Add(Response(FriendBId)));
    }

    private const string LearnerId = "learner";
    private const string FriendAId = "friend-a";
    private const string FriendBId = "friend-b";

    private static Conversation CreateConversation() => Conversation.Create(
        [
            Learner(LearnerId, "Learner"),
            Friend(FriendAId, "Thandi"),
            Friend(FriendBId, "Sipho")
        ]);

    private static ConversationParticipant Learner(string id, string displayName) =>
        new(id, displayName, ParticipantRole.Learner);

    private static ConversationParticipant Friend(string id, string displayName) =>
        new(id, displayName, ParticipantRole.Friend);

    private static FriendResponse Response(string participantId) =>
        new(participantId, "Ngiyaphila.");
}