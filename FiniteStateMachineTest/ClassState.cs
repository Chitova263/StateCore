namespace FiniteStateMachineTest;

public class ClassState : IEquatable<ClassState>
{
    public required string Name { get; set; }

    public bool Equals(ClassState? other)
    {
        return Name == other?.Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ClassState state)
        {
            return false;
        }

        return state.Name == Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}