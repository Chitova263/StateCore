namespace FiniteStateMachine;
/// <summary>
/// Represents a finite state machine for managing states and transitions.
/// </summary>
/// <typeparam name="TState">The type representing states in the state machine.</typeparam>
/// <typeparam name="TTrigger">The type representing triggers used to transition between states.</typeparam>
public sealed class StateMachine<TState, TTrigger>
    where TTrigger : Enum
    where TState : notnull
{
    /// <summary>
    /// A dictionary containing rules for state transitions.
    /// </summary>
    private readonly Dictionary<RuleKey<TState, TTrigger>, Rule<TState, TTrigger>> _rules;
    
    /// <summary>
    /// Gets the rules for state transitions as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<RuleKey<TState, TTrigger>, Rule<TState, TTrigger>> Rules => _rules;
    
    /// <summary>
    /// Gets the current state of the state machine.
    /// </summary>
    public TState CurrentState { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="StateMachine{TState, TTrigger}"/> class.
    /// </summary>
    /// <param name="initialState">The initial state of the state machine.</param>
    /// <param name="rules">The dictionary containing state transition rules.</param>
    internal StateMachine(TState initialState, Dictionary<RuleKey<TState, TTrigger>, Rule<TState, TTrigger>> rules)
    {
        _rules = rules;
        CurrentState = initialState;
    }

    /// <summary>
    /// Creates a builder for configuring a state machine with an initial state.
    /// </summary>
    /// <param name="initialState">The initial state for the state machine.</param>
    /// <returns>A builder for creating and configuring a finite state machine.</returns>
    public static FiniteStateMachineBuilder<TState, TTrigger> WithInitialState(TState initialState)
    {
        return new FiniteStateMachineBuilder<TState, TTrigger>(initialState);
    }
    
    /// <summary>
    /// Triggers a transition based on the specified trigger, updating the current state if a rule is matched.
    /// </summary>
    /// <param name="trigger">The trigger to cause a state transition.</param>
    /// <returns><c>true</c> if the trigger caused a state transition; otherwise, <c>false</c>.</returns>
    public bool Trigger(TTrigger trigger)
    {
        var key = new RuleKey<TState, TTrigger>
        {
            From = CurrentState,
            Trigger = trigger
        };
        if (!_rules.TryGetValue(key, out var rule)) return false;
        
        rule.ExitActions.ForEach(action =>  action());
        
        CurrentState = rule.To;
        
        rule.EntryActions.ForEach(action => action());
        
        return true;
    }
}