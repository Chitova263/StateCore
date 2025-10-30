namespace FiniteStateMachine;

/// <summary>
/// Builder class for creating a finite state machine.
/// </summary>
/// <typeparam name="TState">Type representing the states of the machine.</typeparam>
/// <typeparam name="TTrigger">Enum type representing triggers to cause transitions.</typeparam>
public sealed class FiniteStateMachineBuilder<TState, TTrigger>
    where TTrigger : Enum
    where TState : notnull
{
    private readonly TState _initialState;
    private readonly Dictionary<TState, Dictionary<TTrigger, TState>> _transitions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FiniteStateMachineBuilder{TState, TTrigger}"/> class.
    /// </summary>
    /// <param name="initialState">The initial state of the state machine.</param>
    internal FiniteStateMachineBuilder(TState initialState)
    {
        _initialState = initialState;
    }
        
    /// <summary>
    /// Defines a state and its possible transitions.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="configure">An action to configure the state transitions.</param>
    /// <returns>A reference to the current builder instance.</returns>
    public FiniteStateMachineBuilder<TState, TTrigger> State(
        TState state,
        Action<StateConfiguration<TState, TTrigger>> configure)
    {
        var cfg = new StateConfiguration<TState, TTrigger>(state);
        configure(cfg);
        _transitions.Add(state, cfg.Transitions);
        return this;
    }
        
    /// <summary>
    /// Builds and returns the configured state machine.
    /// </summary>
    /// <returns>A new state machine configured with specified states and transitions.</returns>
    public StateMachine<TState, TTrigger> Build()
    {
        var rules = GetTransitionRules();
        return new StateMachine<TState, TTrigger>(_initialState, rules);
    }

    private Dictionary<RuleKey<TState, TTrigger>, Rule<TState, TTrigger>> GetTransitionRules()
    {
        var rules = new Dictionary<RuleKey<TState, TTrigger>, Rule<TState, TTrigger>>();
        foreach (var (fromState, transitions) in _transitions)
        {
            foreach (var (trigger, targetState) in transitions)
            {
                var ruleKey = new RuleKey<TState, TTrigger>
                {
                    From = fromState,
                    Trigger = trigger
                };
                var rule = new Rule<TState, TTrigger>
                {
                    From = fromState,
                    To = targetState,
                    Trigger = trigger
                };
                rules.Add(ruleKey, rule);
            }
        }
        return rules;
    }
}