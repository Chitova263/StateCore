# StateCore

[![NuGet](https://img.shields.io/nuget/v/StateCore.svg)](https://www.nuget.org/packages/StateCore)
[![NuGet Downloads](https://img.shields.io/nuget/dt/StateCore.svg)](https://www.nuget.org/packages/StateCore)
[![Build](https://github.com/Chitova263/StateCore/workflows/main/badge.svg)](https://github.com/Chitova263/StateCore/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A lightweight, type-safe finite state machine library for .NET applications.

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Transition Lifecycle](#transition-lifecycle)
- [Advanced Usage](#advanced-usage)
- [API Reference](#api-reference)
- [Use Cases](#use-cases)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Type-Safe** — Strongly typed states and triggers using enums or custom types
- **Fluent API** — Intuitive builder pattern for readable state machine definitions
- **Per-Transition Hooks** — Execute specific actions for individual transitions
- **Multiple Actions** — Chain multiple entry/exit actions per transition
- **Extensible** — Works with enums or any type implementing `IEquatable<T>`
- **Zero Dependencies** — No external dependencies required

## Requirements

- .NET 9.0 or later

## Installation

### .NET CLI

```bash
dotnet add package StateCore
```

### Package Manager

```powershell
Install-Package StateCore
```

## Quick Start

```csharp
using StateCore;

// Define states and triggers
public enum State { Paused, Playing, Stopped }
public enum Trigger { Play, Pause, Stop }

// Build the state machine
var stateMachine = StateMachine<State, Trigger>
    .WithInitialState(State.Paused)
    .State(State.Paused, cfg =>
    {
        cfg.OnExit(() => Console.WriteLine("Leaving Paused"))
           .OnEnter(() => Console.WriteLine("Entering Playing"))
           .On(Trigger.Play)
           .GoTo(State.Playing);

        cfg.OnExit(() => Console.WriteLine("Leaving Paused"))
           .OnEnter(() => Console.WriteLine("Entering Stopped"))
           .On(Trigger.Stop)
           .GoTo(State.Stopped);
    })
    .State(State.Playing, cfg => cfg
        .OnExit(() => Console.WriteLine("Leaving Playing"))
        .OnEnter(() => Console.WriteLine("Entering Paused"))
        .On(Trigger.Pause)
        .GoTo(State.Paused))
    .State(State.Stopped, cfg => cfg
        .OnEnter(() => Console.WriteLine("Entering Playing"))
        .On(Trigger.Play)
        .GoTo(State.Playing))
    .Build();

// Fire a trigger
stateMachine.Trigger(Trigger.Play);
// Output:
// Leaving Paused
// Entering Playing

Console.WriteLine(stateMachine.CurrentState); // Playing
```

## Transition Lifecycle

In StateCore, lifecycle hooks are bound to specific transitions:

- **`OnExit()`** — Executes when leaving the current state (the state being configured)
- **`OnEnter()`** — Executes when entering the target state (the state specified in `GoTo()`)

```csharp
.State(State.Paused, cfg =>
{
    cfg.OnExit(() => Console.WriteLine("Exiting Paused"))   // Runs when leaving Paused
       .OnEnter(() => Console.WriteLine("Entering Playing")) // Runs when entering Playing
       .On(Trigger.Play)
       .GoTo(State.Playing);
})
```

Each configuration chain represents a specific transition path with its own behavior.

### Execution Order

When a trigger fires:

1. All `OnExit` actions execute (leaving the current state)
2. All `OnEnter` actions execute (entering the target state)
3. State updates to the target state

## Advanced Usage

### Chaining Multiple Actions

Execute multiple actions in sequence during a transition:

```csharp
.State(State.Stopped, cfg => cfg
    .OnExit(() => Console.WriteLine("Leaving Stopped"))
    .OnEnter(() => InitializeAudioSystem())
    .OnEnter(() => LoadMediaFile())
    .OnEnter(() => StartPlayback())
    .OnEnter(() => UpdateUI())
    .On(Trigger.Play)
    .GoTo(State.Playing))
```

### Context-Specific Entry Behavior

Define different behavior when entering the same state from different sources:

```csharp
// Entering Playing from Paused
.State(State.Paused, cfg => cfg
    .OnExit(() => Console.WriteLine("Resuming from pause"))
    .OnEnter(() => Console.WriteLine("Continuing playback"))
    .On(Trigger.Play)
    .GoTo(State.Playing))

// Entering Playing from Stopped
.State(State.Stopped, cfg => cfg
    .OnExit(() => Console.WriteLine("Starting fresh"))
    .OnEnter(() => Console.WriteLine("Beginning new playback"))
    .On(Trigger.Play)
    .GoTo(State.Playing))
```

### Multiple Transitions from One State

Define distinct behavior for each outgoing transition:

```csharp
.State(State.Playing, cfg =>
{
    cfg.OnExit(() => Console.WriteLine("Pausing"))
       .OnEnter(() => SavePlaybackPosition())
       .On(Trigger.Pause)
       .GoTo(State.Paused);

    cfg.OnExit(() => ReleaseResources())
       .OnEnter(() => ResetPlaybackPosition())
       .On(Trigger.Stop)
       .GoTo(State.Stopped);
})
```

### Custom State Types

Use custom classes instead of enums by implementing `IEquatable<T>`:

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

var paused = new State(1, "Paused");
var playing = new State(2, "Playing");

var stateMachine = StateMachine<State, Trigger>
    .WithInitialState(paused)
    .State(paused, cfg => cfg
        .OnEnter(() => Console.WriteLine($"Entering {playing.Name}"))
        .On(Trigger.Play)
        .GoTo(playing))
    .Build();
```

## API Reference

### StateMachine&lt;TState, TTrigger&gt;

#### Static Methods

| Method | Description |
|--------|-------------|
| `WithInitialState(TState state)` | Creates a builder with the specified initial state |

#### Builder Methods

| Method | Description |
|--------|-------------|
| `State(TState state, Action<StateConfiguration> configure)` | Configures transitions from the specified state |
| `Build()` | Creates the state machine instance |

#### Instance Members

| Member | Description |
|--------|-------------|
| `CurrentState` | Gets the current state |
| `Trigger(TTrigger trigger)` | Fires a trigger to execute a transition |

### StateConfiguration

Represents a transition chain from the current state to a target state.

| Method | Description |
|--------|-------------|
| `OnExit(Action action)` | Registers an action to execute when leaving the current state |
| `OnEnter(Action action)` | Registers an action to execute when entering the target state |
| `On(TTrigger trigger)` | Specifies the trigger that activates this transition |
| `GoTo(TState nextState)` | Specifies the destination state |

## Use Cases

| Domain | Example States |
|--------|----------------|
| Game Development | Menu → Playing → Paused → GameOver |
| Workflow Engines | Draft → Review → Approved → Published |
| Connection Management | Disconnected → Connecting → Connected → Error |
| Media Players | Stopped → Playing → Paused → Buffering |
| Document Lifecycle | New → InProgress → Review → Completed |
| Order Processing | Pending → Processing → Shipped → Delivered |

## Contributing

Contributions are welcome. Please open an issue to discuss proposed changes before submitting a pull request.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m 'Add your feature'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Open a pull request

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

[NuGet Package](https://www.nuget.org/packages/StateCore) · [GitHub Repository](https://github.com/Chitova263/StateCore) · [Report an Issue](https://github.com/Chitova263/StateCore/issues)
