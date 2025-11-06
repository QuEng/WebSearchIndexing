using WebSearchIndexing.Modules.Identity.Application.Abstractions;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Hashes a password using BCrypt with salt rounds of 12
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>BCrypt hashed password</returns>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hashedPassword">BCrypt hashed password to verify against</param>
    /// <returns>True if password matches the hash, false otherwise</returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
        
        if (string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch (Exception)
        {
            // If hash is invalid format, return false
            return false;
        }
    }

    /// <summary>
    /// Determines if a password hash needs to be rehashed due to updated security settings
    /// </summary>
    /// <param name="hash">The hash to check</param>
    /// <returns>True if the hash should be updated, false otherwise</returns>
    public bool NeedsRehash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return true;

        try
        {
            // For BCrypt, we can check if the hash starts with the expected format
            // BCrypt hashes start with $2a$, $2b$, or $2y$ followed by cost
            // Our target cost is 12, so we check if the hash uses a lower cost
            if (hash.StartsWith("$2"))
            {
                var parts = hash.Split('$');
                if (parts.Length >= 3 && int.TryParse(parts[2], out var cost))
                {
                    return cost < 12;
                }
            }
            
            // If we can't determine the cost, assume it needs rehashing
            return true;
        }
        catch (Exception)
        {
            // If we can't parse the hash, it should be rehashed
            return true;
        }
    }
}
