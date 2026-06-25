using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Pontuei.Api.Common;

/// <summary>
/// Validation utilities.
/// </summary>
public static partial class ValidationUtils
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^(?=.*\d)(?=.*[A-Z])(?=.*[!@#$%^&*(),.?""':{}|<>])[A-Za-z\d!@#$%^&*(),.?""':{}|<>]{8,}$", RegexOptions.Compiled)]
    private static partial Regex PasswordRegex();

    // Validates an email address format
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }
        return EmailRegex().IsMatch(email);
    }

    // Validates a password with: 
    // - At least 8 characters,
    // - One uppercase letter, 
    // - One number, 
    // - And one special character
    public static bool IsValidPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return PasswordRegex().IsMatch(password);
    }

}