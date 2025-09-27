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
        if (!Enum.IsDefined(typeof(OwnershipRole), role))
        {
            throw new DomainRuleException("Ownership role is invalid");
        }

        if (ownershipRecordId == Guid.Empty)
        {
            throw new DomainRuleException("Ownership record identifier is required");
        }

        Role = role;
        OwnershipRecordId = ownershipRecordId;
    }

    public OwnershipRole Role { get; }

    public Guid OwnershipRecordId { get; }
}
