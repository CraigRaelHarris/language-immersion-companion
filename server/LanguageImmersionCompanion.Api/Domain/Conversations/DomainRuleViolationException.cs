namespace LanguageImmersionCompanion.Api.Domain.Conversations;

public sealed class DomainRuleViolationException : Exception
{
    public DomainRuleViolationException(string message)
        : base(message)
    {
    }
}