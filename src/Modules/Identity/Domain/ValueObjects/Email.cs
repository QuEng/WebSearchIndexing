using System.Text.RegularExpressions;

namespace WebSearchIndexing.Modules.Identity.Domain.ValueObjects;

public record Email
{
    private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    private static readonly Regex EmailRegex = new(EmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; init; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or empty", nameof(value));

        var normalizedEmail = value.Trim().ToLowerInvariant();
        
        if (!EmailRegex.IsMatch(normalizedEmail))
            throw new ArgumentException("Invalid email format", nameof(value));

        if (normalizedEmail.Length > 254) // RFC 5321 limit
            throw new ArgumentException("Email is too long", nameof(value));

        Value = normalizedEmail;
    }

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);

    public string GetDomain()
    {
        var atIndex = Value.IndexOf('@');
        return atIndex >= 0 ? Value[(atIndex + 1)..] : string.Empty;
    }

    public string GetLocalPart()
    {
        var atIndex = Value.IndexOf('@');
        return atIndex >= 0 ? Value[..atIndex] : Value;
    }

    public override string ToString() => Value;
}
