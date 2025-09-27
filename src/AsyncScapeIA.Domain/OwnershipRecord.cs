namespace AsyncScapeIA.Domain;

public sealed class OwnershipRecord
{
    private readonly List<Contact> _escalationContacts;

    private OwnershipRecord(
        Guid id,
        string externalGroupId,
        string displayName,
        Contact primaryContact,
        IEnumerable<Contact> escalationContacts,
        DateTimeOffset effectiveFromUtc,
        DateTimeOffset? effectiveToUtc)
    {
        Id = id;
        ExternalGroupId = externalGroupId;
        DisplayName = displayName;
        PrimaryContact = primaryContact;
        _escalationContacts = new List<Contact>(escalationContacts);
        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
    }

    public Guid Id { get; }

    public string ExternalGroupId { get; private set; }

    public string DisplayName { get; private set; }

    public Contact PrimaryContact { get; private set; }

    public IReadOnlyCollection<Contact> EscalationContacts => _escalationContacts.AsReadOnly();

    public DateTimeOffset EffectiveFromUtc { get; private set; }

    public DateTimeOffset? EffectiveToUtc { get; private set; }

    public static OwnershipRecord Create(
        string externalGroupId,
        string displayName,
        Contact primaryContact,
        IEnumerable<Contact>? escalationContacts = null,
        DateTimeOffset? effectiveFromUtc = null,
        DateTimeOffset? effectiveToUtc = null)
    {
        if (string.IsNullOrWhiteSpace(externalGroupId))
        {
            throw new DomainRuleException("External group id is required");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainRuleException("Display name is required");
        }

        ArgumentNullException.ThrowIfNull(primaryContact);

        var from = effectiveFromUtc ?? DateTimeOffset.UtcNow;
        if (effectiveToUtc.HasValue && effectiveToUtc.Value <= from)
        {
            throw new DomainRuleException("EffectiveTo must be greater than EffectiveFrom");
        }

        var escalation = escalationContacts?.ToList() ?? new List<Contact>();

        return new OwnershipRecord(
            Guid.NewGuid(),
            externalGroupId.Trim(),
            displayName.Trim(),
            primaryContact,
            escalation,
            from,
            effectiveToUtc);
    }

    public void UpdatePrimaryContact(Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        PrimaryContact = contact;
    }

    public void ReplaceEscalationContacts(IEnumerable<Contact> contacts)
    {
        ArgumentNullException.ThrowIfNull(contacts);
        _escalationContacts.Clear();
        _escalationContacts.AddRange(contacts);
    }

    public void UpdateMetadata(string externalGroupId, string displayName)
    {
        if (string.IsNullOrWhiteSpace(externalGroupId))
        {
            throw new DomainRuleException("External group id is required");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainRuleException("Display name is required");
        }

        ExternalGroupId = externalGroupId.Trim();
        DisplayName = displayName.Trim();
    }

    public void Expire(DateTimeOffset effectiveToUtc)
    {
        if (effectiveToUtc <= EffectiveFromUtc)
        {
            throw new DomainRuleException("EffectiveTo must be greater than EffectiveFrom");
        }

        EffectiveToUtc = effectiveToUtc;
    }
}
