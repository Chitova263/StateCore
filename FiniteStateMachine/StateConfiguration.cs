namespace FiniteStateMachine;

/// <summary>
/// Configuration class for defining transitions from a specific state.
/// </summary>
/// <typeparam name="TState">Type representing the states.</typeparam>
/// <typeparam name="TTrigger">Enum type representing triggers.</typeparam>
public sealed class StateConfiguration<TState, TTrigger> where TTrigger : Enum
{
    private readonly TState _state;
        
    /// <summary>
    /// Gets the transitions defined for the current state.
    /// </summary>
    internal Dictionary<TTrigger, TState> Transitions { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StateConfiguration{TState, TTrigger}"/> class.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    internal StateConfiguration(TState state)
    {
        _state = state;
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
}