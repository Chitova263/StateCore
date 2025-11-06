using FiniteStateMachine;
using Xunit.Abstractions;

namespace FiniteStateMachineTest;

public class StateMachineTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public StateMachineTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ShouldCreateFiniteStateMachine()
    {
        var stateMachine = BuildStateMachine();
        var stateMachineCurrentState = stateMachine.CurrentState;
        Assert.Equal(9, stateMachine.Rules.Count);
        Assert.Equal(State.Paused, stateMachineCurrentState);
    }
    
    [Fact]
    public void ShouldTransitionToNextState()
    {
        var stateMachine = BuildStateMachine();
        var result = stateMachine.Trigger(Trigger.Play);
        Assert.True(result);
        Assert.Equal(State.Playing, stateMachine.CurrentState);
        
    }
    
    private static StateMachine<State, Trigger> BuildStateMachine()
    {
         return StateMachine<State, Trigger>
            .WithInitialState(State.Paused)
            .State(State.Paused, cfg =>
            {
                cfg
                    .OnExit(() => Console.WriteLine("Exiting Paused"))
                    .OnEnter(() => Console.WriteLine("Entering Playing First Action"))
                    .OnEnter(() => Console.WriteLine("Entering Playing Second Action"))
                    .On(Trigger.Play)
                    .GoTo(State.Playing);
                
                cfg
                    .OnEnter(() => Console.WriteLine("Entering Stopped State"))
                    .OnEnter(() => Console.WriteLine("Executing Second Action"))
                    .OnExit(() => Console.WriteLine("Exiting Paused State"))
                    .On(Trigger.Pause).GoTo(State.Paused)
                    .On(Trigger.Stop).GoTo(State.Stopped);
            })
            .State(State.Stopped, cfg =>
                {
                    cfg
                        .On(Trigger.Play).GoTo(State.Playing)
                        .On(Trigger.Pause).GoTo(State.Stopped)
                        .On(Trigger.Stop).GoTo(State.Stopped);
                }
            )
            .State(State.Playing, cfg =>
                {
                    cfg
                        .On(Trigger.Play).GoTo(State.Playing)
                        .On(Trigger.Pause).GoTo(State.Paused);
                    
                    cfg
                        .OnEnter(() => Console.WriteLine("Entering Stopped"))
                        .OnExit(() => Console.WriteLine("Exiting Playing"))
                        .On(Trigger.Stop)
                        .GoTo(State.Stopped);

                }
            )
            .Build();
    }
}