# StateCore

[![NuGet Stats](https://img.shields.io/nuget/v/StateCore.svg)](https://www.nuget.org/packages/StateCore)
![Build](https://github.com/Chitova263/StateCore/workflows/main/badge.svg)
[![NuGet Downloads](https://img.shields.io/nuget/dt/StateCore.svg)](https://www.nuget.org/packages/StateCore)

A flexible and type-safe finite state machine library for C#.

## Features

- 🎯 **Type-Safe**: Strongly typed states and triggers using enums or custom classes
- 🔄 **Flexible Transitions**: Define complex state transitions with granular control
- 🎬 **Per-Transition Lifecycle Hooks**: Execute specific actions for individual transitions
- 🔗 **Fluent API**: Intuitive builder pattern for readable state machine definitions
- ⚡ **Multiple Actions**: Chain multiple entry/exit actions per transition
- 🧩 **Extensible**: Works with enums or custom IEquatable types

## Installation

```bash
dotnet add package StateCore
```

Or via Package Manager:
```powershell
Install-Package StateCore
```

## Quick Start

```csharp
using StateCore;

// Define your states and triggers
public enum State { Paused, Playing, Stopped }
public enum Trigger { Play, Pause, Stop }

// Create the state machine
var stateMachine = StateMachine<State, Trigger>
    .WithInitialState(State.Paused)
    .State(State.Paused, cfg =>
    {
        // Transition: Paused → Playing
        cfg
            .OnExit(() => Console.WriteLine("Leaving Paused state"))
            .OnEnter(() => Console.WriteLine("Entering Playing state"))
            .On(Trigger.Play)
            .GoTo(State.Playing);
        
        // Transition: Paused → Stopped
        cfg
            .OnExit(() => Console.WriteLine("Leaving Paused state"))
            .OnEnter(() => Console.WriteLine("Entering Stopped state"))
            .On(Trigger.Stop)
            .GoTo(State.Stopped);
    })
    .State(State.Playing, cfg => cfg
        .OnExit(() => Console.WriteLine("Leaving Playing state"))
        .OnEnter(() => Console.WriteLine("Entering Paused state"))
        .On(Trigger.Pause)
        .GoTo(State.Paused))
    .State(State.Stopped, cfg => cfg
        .OnEnter(() => Console.WriteLine("Entering Playing state from Stopped"))
        .On(Trigger.Play)
        .GoTo(State.Playing))
    .Build();

// Trigger state transitions
stateMachine.Trigger(Trigger.Play);
// Output:
// Leaving Paused state
// Entering Playing state

Console.WriteLine($"Current state: {stateMachine.CurrentState}"); // Playing
```

## Understanding Transitions

**Important**: In StateCore, the lifecycle hooks work as follows:
- **`OnExit()`** - Executes when leaving the **current state** (the state being configured)
- **`OnEnter()`** - Executes when entering the **target state** (the state in `GoTo()`)

```csharp
.State(State.Paused, cfg =>
{
    cfg
        .OnExit(() => Console.WriteLine("Exiting Paused"))        // Runs when leaving Paused
        .OnEnter(() => Console.WriteLine("Entering Playing"))     // Runs when entering Playing
        .On(Trigger.Play)
        .GoTo(State.Playing);  // ← OnEnter refers to THIS state
})
```

Each configuration chain represents a **specific transition path** with its own unique behavior.

## Advanced Usage

### Multiple Actions Per Transition

Chain multiple actions that execute in order when entering the target state:

```csharp
.State(State.Stopped, cfg => cfg
    .OnExit(() => Console.WriteLine("Leaving Stopped state"))
    .OnEnter(() => Console.WriteLine("1. Initializing audio system"))
    .OnEnter(() => Console.WriteLine("2. Loading media file"))
    .OnEnter(() => Console.WriteLine("3. Starting playback"))
    .OnEnter(() => Console.WriteLine("4. Updating UI to Playing"))
    .On(Trigger.Play)
    .GoTo(State.Playing))
```

When `Trigger.Play` is fired:
```
Output:
Leaving Stopped state
1. Initializing audio system
2. Loading media file
3. Starting playback
4. Updating UI to Playing
```

### Different Entry Behavior for Same Target State

You can enter the same state from different sources with different behavior:

```csharp
// From Paused
.State(State.Paused, cfg => cfg
    .OnExit(() => Console.WriteLine("Resuming from pause"))
    .OnEnter(() => Console.WriteLine("Continuing playback"))
    .On(Trigger.Play)
    .GoTo(State.Playing))

// From Stopped
.State(State.Stopped, cfg => cfg
    .OnExit(() => Console.WriteLine("Starting fresh"))
    .OnEnter(() => Console.WriteLine("Beginning new playback"))
    .On(Trigger.Play)
    .GoTo(State.Playing))
```

Both transitions go to `State.Playing`, but with different `OnEnter` actions!

### Multiple Transitions from Same State

Define different behavior for each outgoing transition:

```csharp
.State(State.Playing, cfg =>
{
    // Transition: Playing → Paused
    cfg
        .OnExit(() => Console.WriteLine("Pausing playback"))
        .OnEnter(() => Console.WriteLine("Now paused"))
        .OnEnter(() => SavePlaybackPosition())
        .On(Trigger.Pause)
        .GoTo(State.Paused);
    
    // Transition: Playing → Stopped
    cfg
        .OnExit(() => Console.WriteLine("Stopping playback"))
        .OnExit(() => ReleaseResources())
        .OnEnter(() => Console.WriteLine("Fully stopped"))
        .OnEnter(() => ResetPlaybackPosition())
        .On(Trigger.Stop)
        .GoTo(State.Stopped);
})
```

### Using Custom Classes as States

Instead of enums, use custom classes that implement `IEquatable<T>`:

```csharp
public class State : IEquatable<State>
{
    public int Id { get; }
    public string Name { get; }

    public State(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public bool Equals(State? other)
    {
        if (other is null) return false;
        return Id == other.Id && Name == other.Name;
    }

    public override bool Equals(object? obj) => Equals(obj as State);

    public override int GetHashCode() => HashCode.Combine(Id, Name);

    public static bool operator ==(State? left, State? right) => Equals(left, right);
    public static bool operator !=(State? left, State? right) => !Equals(left, right);
}

// Define state instances
var paused = new State(1, "Paused");
var playing = new State(2, "Playing");

// Use in state machine
var stateMachine = StateMachine<State, Trigger>
    .WithInitialState(paused)
    .State(paused, cfg => cfg
        .OnExit(() => Console.WriteLine($"Leaving {paused.Name}"))
        .OnEnter(() => Console.WriteLine($"Entering {playing.Name}"))
        .On(Trigger.Play)
        .GoTo(playing))
    .Build();
```

## Complete Example: Media Player

```csharp
public enum State { Stopped, Playing, Paused, Buffering }
public enum Trigger { Play, Pause, Stop, Buffer, Resume }

var mediaPlayer = StateMachine<State, Trigger>
    .WithInitialState(State.Stopped)
    .State(State.Stopped, cfg =>
    {
        // Stopped → Playing
        cfg
            .OnExit(() => Console.WriteLine("Starting player"))
            .OnEnter(() => Console.WriteLine("Loading media"))
            .OnEnter(() => Console.WriteLine("Playback started"))
            .On(Trigger.Play)
            .GoTo(State.Playing);
    })
    .State(State.Playing, cfg =>
    {
        // Playing → Paused
        cfg
            .OnExit(() => Console.WriteLine("Pausing playback"))
            .OnEnter(() => Console.WriteLine("Playback paused"))
            .OnEnter(() => SavePosition())
            .On(Trigger.Pause)
            .GoTo(State.Paused);
        
        // Playing → Stopped
        cfg
            .OnExit(() => Console.WriteLine("Stopping playback"))
            .OnExit(() => ReleaseResources())
            .OnEnter(() => Console.WriteLine("Player stopped"))
            .On(Trigger.Stop)
            .GoTo(State.Stopped);
        
        // Playing → Buffering
        cfg
            .OnExit(() => Console.WriteLine("Connection slow"))
            .OnEnter(() => Console.WriteLine("Buffering content..."))
            .OnEnter(() => ShowSpinner())
            .On(Trigger.Buffer)
            .GoTo(State.Buffering);
    })
    .State(State.Paused, cfg =>
    {
        // Paused → Playing
        cfg
            .OnExit(() => Console.WriteLine("Resuming playback"))
            .OnEnter(() => Console.WriteLine("Playback resumed"))
            .On(Trigger.Play)
            .GoTo(State.Playing);
        
        // Paused → Stopped
        cfg
            .OnExit(() => Console.WriteLine("Stopping from pause"))
            .OnEnter(() => Console.WriteLine("Player stopped"))
            .On(Trigger.Stop)
            .GoTo(State.Stopped);
    })
    .State(State.Buffering, cfg =>
    {
        // Buffering → Playing
        cfg
            .OnExit(() => Console.WriteLine("Buffer filled"))
            .OnExit(() => HideSpinner())
            .OnEnter(() => Console.WriteLine("Resuming playback"))
            .On(Trigger.Resume)
            .GoTo(State.Playing);
    })
    .Build();

// Use the state machine
mediaPlayer.Trigger(Trigger.Play);
// Output:
// Starting player
// Loading media
// Playback started

mediaPlayer.Trigger(Trigger.Pause);
// Output:
// Pausing playback
// Playback paused
// (SavePosition called)
```

## API Reference

### StateMachine<TState, TTrigger>

#### Builder Methods

- **`WithInitialState(TState state)`** - Sets the starting state
- **`State(TState state, Action<StateConfiguration> configure)`** - Configures transitions originating from a state
- **`Build()`** - Creates the state machine instance

#### Instance Methods

- **`Trigger(TTrigger trigger)`** - Fires a trigger to execute a state transition
- **`CurrentState`** - Gets the current state (property)

### StateConfiguration (Transition Chain)

Each configuration chain represents **one specific transition** from the current state to a target state.

#### Methods

- **`OnExit(Action action)`** - Executes when leaving the **current state** (the state being configured)
- **`OnEnter(Action action)`** - Executes when entering the **target state** (the state specified in `GoTo()`)
- **`On(TTrigger trigger)`** - Defines which trigger activates this transition
- **`GoTo(TState nextState)`** - Specifies the destination state for this transition

#### Execution Order

When a trigger is fired:
1. All `OnExit` actions for the transition (leaving current state)
2. All `OnEnter` actions for the transition (entering target state)
3. State changes to the target state

## Common Use Cases

- **Game State Management**: Menu → Playing → Paused → GameOver
- **Workflow Engines**: Draft → Review → Approved → Published
- **Connection Management**: Disconnected → Connecting → Connected → Error
- **Media Players**: Stopped → Playing → Paused → Buffering
- **Document Lifecycle**: New → InProgress → Review → Completed
- **Order Processing**: Pending → Processing → Shipped → Delivered

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Links

- [NuGet Package](https://www.nuget.org/packages/StateCore)
- [GitHub Repository](https://github.com/Chitova263/StateCore)
- [Report Issues](https://github.com/Chitova263/StateCore/issues)