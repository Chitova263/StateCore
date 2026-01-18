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
    private readonly Dictionary<TState, Dictionary<TTrigger, TransitionOption<TTrigger, TState>>> _transitions = new();

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
    /// <exception cref="InvalidOperationException">Thrown when the state has already been configured.</exception>
    public FiniteStateMachineBuilder<TState, TTrigger> State(
        TState state,
        Action<StateConfiguration<TState, TTrigger>> configure)
    {
        if (_transitions.ContainsKey(state))
        {
            throw new InvalidOperationException($"State '{state}' has already been configured. Each state can only be configured once.");
        }

        var cfg = new StateConfiguration<TState, TTrigger>(state);
        configure(cfg);
        _transitions.Add(state, cfg.Transitions);
        return this;
    }
        
    /// <summary>
    /// Builds and returns the configured state machine.
    /// </summary>
    /// <returns>A new state machine configured with specified states and transitions.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the initial state has no transitions configured.</exception>
    public StateMachine<TState, TTrigger> Build()
    {
        if (!_transitions.ContainsKey(_initialState))
        {
            throw new InvalidOperationException(
                $"Initial state '{_initialState}' has no transitions configured. " +
                "The initial state must have at least one transition defined.");
        }

        var rules = GetTransitionRules();
        return new StateMachine<TState, TTrigger>(_initialState, rules);
    }

    private Dictionary<RuleKey<TState, TTrigger>, Rule<TState, TTrigger>> GetTransitionRules()
    {
        var rules = new Dictionary<RuleKey<TState, TTrigger>, Rule<TState, TTrigger>>();
        foreach (var (fromState, transitions) in _transitions)
        {
            foreach (var (trigger, transitionOption) in transitions)
            {
                var ruleKey = new RuleKey<TState, TTrigger>
                {
                    From = fromState,
                    Trigger = trigger
                };
                var rule = new Rule<TState, TTrigger>
                {
                    From = fromState,
                    To = transitionOption.TargetState,
                    Trigger = trigger,
                    EntryActions = transitionOption.EntryActions,
                    ExitActions = transitionOption.ExitActions,
                };
                rules.Add(ruleKey, rule);
            }
        }
        return rules;
    }
}