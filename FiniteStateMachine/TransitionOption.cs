namespace FiniteStateMachine;

internal sealed record TransitionOption<TTrigger, TState>
{
    public required TTrigger Trigger { get; init; }
    public required TState TargetState { get; init; }
    public required List<Action> EntryActions { get; init; }
    public required List<Action> ExitActions { get; init; }
}