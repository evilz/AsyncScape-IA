namespace AsyncScapeIA.Domain;

public enum OwnershipRole
{
    Primary,
    Secondary,
    Escalation
}

public sealed class OwnershipAssignment
{
    public OwnershipAssignment(OwnershipRole role, Guid ownershipRecordId)
    {
        Role = role;
        OwnershipRecordId = ownershipRecordId;
    }

    public OwnershipRole Role { get; }

    public Guid OwnershipRecordId { get; }
}