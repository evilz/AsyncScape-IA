namespace AsyncScapeIA.Domain;

public enum LifecycleState
{
    Proposed,
    Active,
    Deprecated,
    Retired
}

public sealed class ArchitectureComponent
{
    private readonly List<OwnershipAssignment> _ownershipAssignments = [];

    private ArchitectureComponent(Guid id, string name, string domain, LifecycleState lifecycleState)
    {
        Id = id;
        Name = name;
        Domain = domain;
        LifecycleState = lifecycleState;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Domain { get; }

    public LifecycleState LifecycleState { get; }

    public IReadOnlyCollection<OwnershipAssignment> OwnershipAssignments => _ownershipAssignments.AsReadOnly();

    public static ArchitectureComponent Create(string name, string domain, LifecycleState lifecycleState)
    {
        return new ArchitectureComponent(Guid.NewGuid(), name, domain, lifecycleState);
    }

    public void AssignOwnership(OwnershipAssignment assignment)
    {
        _ownershipAssignments.Add(assignment);
    }

    public void ValidateInvariants()
    {
        throw new NotImplementedException();
    }
}