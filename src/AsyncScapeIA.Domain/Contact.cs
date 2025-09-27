namespace AsyncScapeIA.Domain;

using System.Text.RegularExpressions;

public sealed record Contact
{
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PhonePattern = new(@"^[+0-9()#*\-\s]{7,50}$", RegexOptions.Compiled);

    public Contact(string name, string email, string? phone, string? timeZone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleException("Contact name is required");
        }

        if (string.IsNullOrWhiteSpace(email) || !EmailPattern.IsMatch(email.Trim()))
        {
            throw new DomainRuleException("Contact email is invalid");
        }

        if (!string.IsNullOrWhiteSpace(phone) && !PhonePattern.IsMatch(phone.Trim()))
        {
            throw new DomainRuleException("Contact phone is invalid");
        }

        Name = name.Trim();
        Email = email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        TimeZone = string.IsNullOrWhiteSpace(timeZone) ? null : timeZone.Trim();
    }

    public string Name { get; }

    public string Email { get; }

    public string? Phone { get; }

    public string? TimeZone { get; }
}
