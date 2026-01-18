namespace FiniteStateMachine;

/// <summary>
/// Configuration class for defining transitions from a specific state.
/// </summary>
/// <typeparam name="TState">Type representing the states.</typeparam>
/// <typeparam name="TTrigger">Enum type representing triggers.</typeparam>
public sealed class StateConfiguration<TState, TTrigger> where TTrigger : Enum
{
    private readonly List<Action> _entryActions;
    private readonly List<Action> _exitActions;

    internal IReadOnlyList<Action> EntryActions => _entryActions;
    internal IReadOnlyList<Action> ExitActions => _exitActions;

    /// <summary>
    /// Gets the name of the state being configured (for error messages).
    /// </summary>
    internal string StateName { get; }

    /// <summary>
    /// Gets the transitions defined for the current state.
    /// </summary>
    internal Dictionary<TTrigger, TransitionOption<TTrigger, TState>> Transitions { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StateConfiguration{TState, TTrigger}"/> class.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    internal StateConfiguration(TState state)
    {
        StateName = state?.ToString() ?? "null";
        _entryActions = new List<Action>();
        _exitActions = new List<Action>();
    }

    internal void ClearActions()
    {
        _entryActions.Clear();
        _exitActions.Clear();
    }

    /// <summary>
    /// Starts the configuration of a transition for a given trigger.
    /// </summary>
    /// <param name="trigger">The trigger that causes a transition.</param>
    /// <returns>A configuration object to setup the transition.</returns>
    public TransitionConfiguration<TState, TTrigger> On(TTrigger trigger)
    {
        return new TransitionConfiguration<TState, TTrigger>(this, trigger);
    }

    /// <summary>
    /// Registers an action to execute when entering the target state.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The current configuration for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public StateConfiguration<TState, TTrigger> OnEnter(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _entryActions.Add(action);
        return this;
    }

    /// <summary>
    /// Registers an action to execute when exiting the current state.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The current configuration for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public StateConfiguration<TState, TTrigger> OnExit(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _exitActions.Add(action);
        return this;
    }
}
