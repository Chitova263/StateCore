namespace FiniteStateMachine;

public sealed class TransitionConfiguration<TState, TTrigger> where TTrigger : Enum
{
    private readonly StateConfiguration<TState, TTrigger> _stateConfiguration;
    private readonly TTrigger _trigger;

    internal TransitionConfiguration(StateConfiguration<TState, TTrigger> stateConfiguration, TTrigger trigger)
    {
        _stateConfiguration = stateConfiguration;
        _trigger = trigger;
    }

    /// <summary>
    /// Specifies the target state for this transition.
    /// </summary>
    /// <param name="target">The state to transition to.</param>
    /// <returns>The state configuration for continued fluent configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this trigger has already been configured for the current state.</exception>
    public StateConfiguration<TState, TTrigger> GoTo(TState target)
    {
        if (_stateConfiguration.Transitions.ContainsKey(_trigger))
        {
            throw new InvalidOperationException(
                $"Trigger '{_trigger}' has already been configured for state '{_stateConfiguration.StateName}'. Each trigger can only be configured once per state.");
        }

        _stateConfiguration.Transitions[_trigger] = new TransitionOption<TTrigger, TState>
        {
            Trigger = _trigger,
            TargetState = target,
            EntryActions = _stateConfiguration.EntryActions.ToList(),
            ExitActions = _stateConfiguration.ExitActions.ToList()
        };

        _stateConfiguration.ClearActions();

        return _stateConfiguration;
    }
}
