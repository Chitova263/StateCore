using FiniteStateMachine;

namespace FiniteStateMachineTest;

public class StateMachineTest
{
    #region Initial State Tests

    [Fact]
    public void WithInitialState_SetsCurrentStateToInitialState()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Stopped)
            .State(State.Stopped, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .Build();

        Assert.Equal(State.Stopped, stateMachine.CurrentState);
    }

    [Fact]
    public void WithInitialState_DifferentInitialStates_SetsCorrectly()
    {
        var pausedMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .Build();

        var playingMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Playing)
            .State(State.Playing, cfg => cfg.On(Trigger.Pause).GoTo(State.Paused))
            .Build();

        Assert.Equal(State.Paused, pausedMachine.CurrentState);
        Assert.Equal(State.Playing, playingMachine.CurrentState);
    }

    #endregion

    #region Trigger Tests

    [Fact]
    public void Trigger_ValidTransition_ReturnsTrue()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .Build();

        var result = stateMachine.Trigger(Trigger.Play);

        Assert.True(result);
    }

    [Fact]
    public void Trigger_ValidTransition_ChangesState()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Play);

        Assert.Equal(State.Playing, stateMachine.CurrentState);
    }

    [Fact]
    public void Trigger_InvalidTransition_ReturnsFalse()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .Build();

        var result = stateMachine.Trigger(Trigger.Stop);

        Assert.False(result);
    }

    [Fact]
    public void Trigger_InvalidTransition_DoesNotChangeState()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Stop);

        Assert.Equal(State.Paused, stateMachine.CurrentState);
    }

    [Fact]
    public void Trigger_NoRuleForCurrentState_ReturnsFalse()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Playing, cfg => cfg.On(Trigger.Pause).GoTo(State.Paused))
            .Build();

        var result = stateMachine.Trigger(Trigger.Play);

        Assert.False(result);
    }

    [Fact]
    public void Trigger_SelfTransition_ReturnsTrue()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Playing)
            .State(State.Playing, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .Build();

        var result = stateMachine.Trigger(Trigger.Play);

        Assert.True(result);
        Assert.Equal(State.Playing, stateMachine.CurrentState);
    }

    [Fact]
    public void Trigger_MultipleTransitionsInSequence_UpdatesStateCorrectly()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Stopped)
            .State(State.Stopped, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .State(State.Playing, cfg => cfg.On(Trigger.Pause).GoTo(State.Paused))
            .State(State.Paused, cfg => cfg.On(Trigger.Stop).GoTo(State.Stopped))
            .Build();

        Assert.Equal(State.Stopped, stateMachine.CurrentState);

        stateMachine.Trigger(Trigger.Play);
        Assert.Equal(State.Playing, stateMachine.CurrentState);

        stateMachine.Trigger(Trigger.Pause);
        Assert.Equal(State.Paused, stateMachine.CurrentState);

        stateMachine.Trigger(Trigger.Stop);
        Assert.Equal(State.Stopped, stateMachine.CurrentState);
    }

    #endregion

    #region OnEnter Action Tests

    [Fact]
    public void Trigger_WithOnEnterAction_ExecutesAction()
    {
        var actionExecuted = false;
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnEnter(() => actionExecuted = true)
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Play);

        Assert.True(actionExecuted);
    }

    [Fact]
    public void Trigger_WithMultipleOnEnterActions_ExecutesAllInOrder()
    {
        var executionOrder = new List<int>();
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnEnter(() => executionOrder.Add(1))
                .OnEnter(() => executionOrder.Add(2))
                .OnEnter(() => executionOrder.Add(3))
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Play);

        Assert.Equal([1, 2, 3], executionOrder);
    }

    [Fact]
    public void Trigger_InvalidTransition_DoesNotExecuteOnEnterAction()
    {
        var actionExecuted = false;
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnEnter(() => actionExecuted = true)
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Stop);

        Assert.False(actionExecuted);
    }

    #endregion

    #region OnExit Action Tests

    [Fact]
    public void Trigger_WithOnExitAction_ExecutesAction()
    {
        var actionExecuted = false;
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnExit(() => actionExecuted = true)
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Play);

        Assert.True(actionExecuted);
    }

    [Fact]
    public void Trigger_WithMultipleOnExitActions_ExecutesAllInOrder()
    {
        var executionOrder = new List<int>();
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnExit(() => executionOrder.Add(1))
                .OnExit(() => executionOrder.Add(2))
                .OnExit(() => executionOrder.Add(3))
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Play);

        Assert.Equal([1, 2, 3], executionOrder);
    }

    [Fact]
    public void Trigger_InvalidTransition_DoesNotExecuteOnExitAction()
    {
        var actionExecuted = false;
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnExit(() => actionExecuted = true)
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Stop);

        Assert.False(actionExecuted);
    }

    #endregion

    #region Action Execution Order Tests

    [Fact]
    public void Trigger_WithOnExitAndOnEnter_ExecutesExitBeforeEnter()
    {
        var executionOrder = new List<string>();
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnExit(() => executionOrder.Add("exit"))
                .OnEnter(() => executionOrder.Add("enter"))
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Play);

        Assert.Equal(["exit", "enter"], executionOrder);
    }

    [Fact]
    public void Trigger_WithMultipleExitAndEnterActions_ExecutesInCorrectOrder()
    {
        var executionOrder = new List<string>();
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnExit(() => executionOrder.Add("exit1"))
                .OnExit(() => executionOrder.Add("exit2"))
                .OnEnter(() => executionOrder.Add("enter1"))
                .OnEnter(() => executionOrder.Add("enter2"))
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Play);

        Assert.Equal(["exit1", "exit2", "enter1", "enter2"], executionOrder);
    }

    #endregion

    #region Multiple Transitions from Same State Tests

    [Fact]
    public void State_WithMultipleTransitions_AllTransitionsWork()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Playing)
            .State(State.Playing, cfg =>
            {
                cfg.On(Trigger.Pause).GoTo(State.Paused);
                cfg.On(Trigger.Stop).GoTo(State.Stopped);
            })
            .Build();

        Assert.Equal(2, stateMachine.Rules.Count);

        var machine1 = StateMachine<State, Trigger>
            .WithInitialState(State.Playing)
            .State(State.Playing, cfg =>
            {
                cfg.On(Trigger.Pause).GoTo(State.Paused);
                cfg.On(Trigger.Stop).GoTo(State.Stopped);
            })
            .Build();

        machine1.Trigger(Trigger.Pause);
        Assert.Equal(State.Paused, machine1.CurrentState);

        var machine2 = StateMachine<State, Trigger>
            .WithInitialState(State.Playing)
            .State(State.Playing, cfg =>
            {
                cfg.On(Trigger.Pause).GoTo(State.Paused);
                cfg.On(Trigger.Stop).GoTo(State.Stopped);
            })
            .Build();

        machine2.Trigger(Trigger.Stop);
        Assert.Equal(State.Stopped, machine2.CurrentState);
    }

    [Fact]
    public void State_DifferentTransitionsHaveDifferentActions()
    {
        var pauseActions = new List<string>();
        var stopActions = new List<string>();

        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Playing)
            .State(State.Playing, cfg =>
            {
                cfg.OnExit(() => pauseActions.Add("pause-exit"))
                   .OnEnter(() => pauseActions.Add("pause-enter"))
                   .On(Trigger.Pause)
                   .GoTo(State.Paused);

                cfg.OnExit(() => stopActions.Add("stop-exit"))
                   .OnEnter(() => stopActions.Add("stop-enter"))
                   .On(Trigger.Stop)
                   .GoTo(State.Stopped);
            })
            .Build();

        stateMachine.Trigger(Trigger.Pause);

        Assert.Equal(["pause-exit", "pause-enter"], pauseActions);
        Assert.Empty(stopActions);
    }

    #endregion

    #region Context-Specific Entry Behavior Tests

    [Fact]
    public void Trigger_SameTargetFromDifferentSources_ExecutesDifferentActions()
    {
        var fromPausedActions = new List<string>();
        var fromStoppedActions = new List<string>();

        var machine1 = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnEnter(() => fromPausedActions.Add("from-paused"))
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        var machine2 = StateMachine<State, Trigger>
            .WithInitialState(State.Stopped)
            .State(State.Stopped, cfg => cfg
                .OnEnter(() => fromStoppedActions.Add("from-stopped"))
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        machine1.Trigger(Trigger.Play);
        machine2.Trigger(Trigger.Play);

        Assert.Equal(["from-paused"], fromPausedActions);
        Assert.Equal(["from-stopped"], fromStoppedActions);
    }

    #endregion

    #region Rules Dictionary Tests

    [Fact]
    public void Build_CreatesCorrectNumberOfRules()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg =>
            {
                cfg.On(Trigger.Play).GoTo(State.Playing);
                cfg.On(Trigger.Stop).GoTo(State.Stopped);
            })
            .State(State.Playing, cfg => cfg.On(Trigger.Pause).GoTo(State.Paused))
            .Build();

        Assert.Equal(3, stateMachine.Rules.Count);
    }

    [Fact]
    public void Build_RulesContainCorrectFromAndToStates()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .Build();

        var ruleKey = new RuleKey<State, Trigger>
        {
            From = State.Paused,
            Trigger = Trigger.Play
        };

        Assert.True(stateMachine.Rules.ContainsKey(ruleKey));
        Assert.Equal(State.Paused, stateMachine.Rules[ruleKey].From);
        Assert.Equal(State.Playing, stateMachine.Rules[ruleKey].To);
        Assert.Equal(Trigger.Play, stateMachine.Rules[ruleKey].Trigger);
    }

    #endregion

    #region Custom State Type Tests

    [Fact]
    public void StateMachine_WithCustomClassState_WorksCorrectly()
    {
        var paused = new ClassState { Name = "Paused" };
        var playing = new ClassState { Name = "Playing" };

        var stateMachine = StateMachine<ClassState, Trigger>
            .WithInitialState(paused)
            .State(paused, cfg => cfg.On(Trigger.Play).GoTo(playing))
            .State(playing, cfg => cfg.On(Trigger.Pause).GoTo(paused))
            .Build();

        Assert.Equal(paused, stateMachine.CurrentState);

        stateMachine.Trigger(Trigger.Play);
        Assert.Equal(playing, stateMachine.CurrentState);

        stateMachine.Trigger(Trigger.Pause);
        Assert.Equal(paused, stateMachine.CurrentState);
    }

    [Fact]
    public void StateMachine_WithCustomClassState_ExecutesActions()
    {
        var paused = new ClassState { Name = "Paused" };
        var playing = new ClassState { Name = "Playing" };
        var actionExecuted = false;

        var stateMachine = StateMachine<ClassState, Trigger>
            .WithInitialState(paused)
            .State(paused, cfg => cfg
                .OnEnter(() => actionExecuted = true)
                .On(Trigger.Play)
                .GoTo(playing))
            .Build();

        stateMachine.Trigger(Trigger.Play);

        Assert.True(actionExecuted);
    }

    [Fact]
    public void StateMachine_WithEqualCustomClassStates_MatchesCorrectly()
    {
        var paused1 = new ClassState { Name = "Paused" };
        var paused2 = new ClassState { Name = "Paused" };
        var playing = new ClassState { Name = "Playing" };

        var stateMachine = StateMachine<ClassState, Trigger>
            .WithInitialState(paused1)
            .State(paused2, cfg => cfg.On(Trigger.Play).GoTo(playing))
            .Build();

        var result = stateMachine.Trigger(Trigger.Play);

        Assert.True(result);
        Assert.Equal(playing, stateMachine.CurrentState);
    }

    #endregion

    #region Fluent API Tests

    [Fact]
    public void FluentApi_ChainingMultipleStates_Works()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Stopped)
            .State(State.Stopped, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .State(State.Playing, cfg => cfg.On(Trigger.Pause).GoTo(State.Paused))
            .State(State.Paused, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .Build();

        Assert.Equal(3, stateMachine.Rules.Count);
    }

    [Fact]
    public void FluentApi_ChainingActionsAndTransitions_Works()
    {
        var actions = new List<string>();

        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnExit(() => actions.Add("exit-paused"))
                .OnEnter(() => actions.Add("enter-playing"))
                .On(Trigger.Play)
                .GoTo(State.Playing)
                .OnExit(() => actions.Add("exit-paused-2"))
                .OnEnter(() => actions.Add("enter-stopped"))
                .On(Trigger.Stop)
                .GoTo(State.Stopped))
            .Build();

        stateMachine.Trigger(Trigger.Play);
        Assert.Equal(["exit-paused", "enter-playing"], actions);

        actions.Clear();
        var machine2 = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnExit(() => actions.Add("exit-paused"))
                .OnEnter(() => actions.Add("enter-playing"))
                .On(Trigger.Play)
                .GoTo(State.Playing)
                .OnExit(() => actions.Add("exit-paused-2"))
                .OnEnter(() => actions.Add("enter-stopped"))
                .On(Trigger.Stop)
                .GoTo(State.Stopped))
            .Build();

        machine2.Trigger(Trigger.Stop);
        Assert.Equal(["exit-paused-2", "enter-stopped"], actions);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Trigger_WithNoActions_TransitionsSuccessfully()
    {
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg.On(Trigger.Play).GoTo(State.Playing))
            .Build();

        var result = stateMachine.Trigger(Trigger.Play);

        Assert.True(result);
        Assert.Equal(State.Playing, stateMachine.CurrentState);
    }

    [Fact]
    public void SelfTransition_ExecutesActions()
    {
        var executionCount = 0;
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Playing)
            .State(State.Playing, cfg => cfg
                .OnExit(() => executionCount++)
                .OnEnter(() => executionCount++)
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .Build();

        stateMachine.Trigger(Trigger.Play);

        Assert.Equal(2, executionCount);
        Assert.Equal(State.Playing, stateMachine.CurrentState);
    }

    [Fact]
    public void MultipleTriggerCalls_ExecuteActionsEachTime()
    {
        var executionCount = 0;
        var stateMachine = StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg => cfg
                .OnEnter(() => executionCount++)
                .On(Trigger.Play)
                .GoTo(State.Playing))
            .State(State.Playing, cfg => cfg
                .OnEnter(() => executionCount++)
                .On(Trigger.Pause)
                .GoTo(State.Paused))
            .Build();

        stateMachine.Trigger(Trigger.Play);
        stateMachine.Trigger(Trigger.Pause);
        stateMachine.Trigger(Trigger.Play);

        Assert.Equal(3, executionCount);
    }

    #endregion
}
