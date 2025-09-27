namespace AsyncScapeIA.Domain;

using System.Text.RegularExpressions;

public enum LifecycleState
{
    Proposed,
    Active,
    Deprecated,
    Retired
}

public enum BusinessCriticality
{
    Low,
    Medium,
    High
}

public sealed class ArchitectureComponent
{
    private static readonly Regex SlugPattern = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);
    private readonly List<OwnershipAssignment> _ownershipAssignments = [];

    private ArchitectureComponent(
        Guid id,
        string slug,
        string name,
        string domain,
        LifecycleState lifecycleState,
        BusinessCriticality businessCriticality,
        string? description,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        Id = id;
        Slug = slug;
        Name = name;
        Domain = domain;
        LifecycleState = lifecycleState;
        BusinessCriticality = businessCriticality;
        Description = description;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; }

    public string Slug { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public string Domain { get; private set; }

    public LifecycleState LifecycleState { get; private set; }

    public BusinessCriticality BusinessCriticality { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<OwnershipAssignment> OwnershipAssignments => _ownershipAssignments.AsReadOnly();

    public static ArchitectureComponent Create(
        string name,
        string domain,
        LifecycleState lifecycleState,
        string? slug = null,
        BusinessCriticality businessCriticality = BusinessCriticality.Medium,
        string? description = null,
        DateTimeOffset? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleException("Component name is required");
        }

        if (name.Length > 200)
        {
            throw new DomainRuleException("Component name must be 200 characters or fewer");
        }

        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new DomainRuleException("Component domain is required");
        }

        var resolvedSlug = slug ?? GenerateSlug(name);
        if (!SlugPattern.IsMatch(resolvedSlug))
        {
            throw new DomainRuleException("Component slug must be lowercase alphanumeric with hyphen separators");
        }

        var timestamp = createdAtUtc ?? DateTimeOffset.UtcNow;

        return new ArchitectureComponent(
            Guid.NewGuid(),
            resolvedSlug,
            name.Trim(),
            domain.Trim(),
            lifecycleState,
            businessCriticality,
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            timestamp,
            timestamp);
    }

    public void UpdateDetails(string name, string? description, string domain, BusinessCriticality businessCriticality)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleException("Component name is required");
        }

        if (name.Length > 200)
        {
            throw new DomainRuleException("Component name must be 200 characters or fewer");
        }

        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new DomainRuleException("Component domain is required");
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Domain = domain.Trim();
        BusinessCriticality = businessCriticality;
        Touch();
    }

    public void SetSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainRuleException("Component slug is required");
        }

        if (!SlugPattern.IsMatch(slug))
        {
            throw new DomainRuleException("Component slug must be lowercase alphanumeric with hyphen separators");
        }

        Slug = slug;
        Touch();
    }

    public void SetLifecycleState(LifecycleState lifecycleState)
    {
        LifecycleState = lifecycleState;
        Touch();
    }

    public void AssignOwnership(OwnershipAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        if (assignment.OwnershipRecordId == Guid.Empty)
        {
            throw new DomainRuleException("Ownership assignment must reference a record");
        }

        if (assignment.Role == OwnershipRole.Primary && _ownershipAssignments.Any(o => o.Role == OwnershipRole.Primary))
        {
            throw new DomainRuleException("Component already has a primary owner");
        }

        if (_ownershipAssignments.Any(o => o.Role == assignment.Role && o.OwnershipRecordId == assignment.OwnershipRecordId))
        {
            return; // idempotent assignment
        }

        _ownershipAssignments.Add(assignment);
        Touch();
    }

    public void RemoveOwnership(Guid ownershipRecordId)
    {
        _ownershipAssignments.RemoveAll(o => o.OwnershipRecordId == ownershipRecordId);
        Touch();
    }

    public void ValidateInvariants()
    {
        if (!_ownershipAssignments.Any(o => o.Role == OwnershipRole.Primary))
        {
            throw new DomainRuleException("Architecture component must have a primary owner");
        }

        if (!SlugPattern.IsMatch(Slug))
        {
            throw new DomainRuleException("Component slug is invalid");
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new DomainRuleException("Component name cannot be empty");
        }
    }

    private static string GenerateSlug(string input)
    {
        var trimmed = input.Trim().ToLowerInvariant();
        var normalized = Regex.Replace(trimmed, "[^a-z0-9]+", "-");
        normalized = Regex.Replace(normalized, "-+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? "component" : normalized;
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
