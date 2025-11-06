using System.Text.RegularExpressions;

namespace WebSearchIndexing.Modules.Identity.Domain.ValueObjects;

public record Password
{
    private const int MinLength = 8;
    private const int MaxLength = 128;
    
    private static readonly Regex HasUpperCase = new(@"[A-Z]", RegexOptions.Compiled);
    private static readonly Regex HasLowerCase = new(@"[a-z]", RegexOptions.Compiled);
    private static readonly Regex HasDigit = new(@"\d", RegexOptions.Compiled);
    private static readonly Regex HasSpecialChar = new(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]", RegexOptions.Compiled);

    public string Value { get; init; }

    public Password(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Password cannot be null or empty", nameof(value));

        if (value.Length < MinLength)
            throw new ArgumentException($"Password must be at least {MinLength} characters long", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"Password cannot be longer than {MaxLength} characters", nameof(value));

        ValidateStrength(value);

        Value = value;
    }

    private static void ValidateStrength(string password)
    {
        var errors = new List<string>();

        if (!HasUpperCase.IsMatch(password))
            errors.Add("Password must contain at least one uppercase letter");

        if (!HasLowerCase.IsMatch(password))
            errors.Add("Password must contain at least one lowercase letter");

        if (!HasDigit.IsMatch(password))
            errors.Add("Password must contain at least one digit");

        if (!HasSpecialChar.IsMatch(password))
            errors.Add("Password must contain at least one special character");

        if (errors.Any())
            throw new ArgumentException($"Password does not meet strength requirements: {string.Join(", ", errors)}", nameof(password));
    }

    public static implicit operator string(Password password) => password.Value;

    public static bool IsValid(string value)
    {
        try
        {
            _ = new Password(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static (bool IsValid, IEnumerable<string> Errors) Validate(string value)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add("Password cannot be null or empty");
            return (false, errors);
        }

        if (value.Length < MinLength)
            errors.Add($"Password must be at least {MinLength} characters long");

        if (value.Length > MaxLength)
            errors.Add($"Password cannot be longer than {MaxLength} characters");

        if (!HasUpperCase.IsMatch(value))
            errors.Add("Password must contain at least one uppercase letter");

        if (!HasLowerCase.IsMatch(value))
            errors.Add("Password must contain at least one lowercase letter");

        if (!HasDigit.IsMatch(value))
            errors.Add("Password must contain at least one digit");

        if (!HasSpecialChar.IsMatch(value))
            errors.Add("Password must contain at least one special character");

        return (!errors.Any(), errors);
    }

    public override string ToString() => new('*', Math.Min(Value.Length, 8)); // Masked representation
}
