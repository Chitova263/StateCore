namespace FiniteStateMachine;

public sealed record RuleKey<TState, TTrigger>
    where TTrigger : Enum
{
    public required TState From { get; init; }
    public required TTrigger Trigger { get; init; }
};