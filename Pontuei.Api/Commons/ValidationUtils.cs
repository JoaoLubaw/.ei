using System.Security.Cryptography;
using System.Text;
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

    [GeneratedRegex(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled)]
    private static partial Regex PhoneNumberRegex();

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

    /// <summary>
    /// Validates a phone number in E.164 format (e.g., +1234567890).
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <returns><c>true</c> if the phone number is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return false;

        return PhoneNumberRegex().IsMatch(phoneNumber);
    }

    /// <summary>
    /// Generates a random 6-digit numeric code as a string.
    /// </summary>
    /// <returns></returns>
    public static string Generate6DigitCode()
    {
        int number = RandomNumberGenerator.GetInt32(0, 1000000);
        return number.ToString("D6");
    }

    /// <summary>
    /// Hashes a token using SHA256 and returns the Base64-encoded string.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static string HashToken(string token)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hash = sha256.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }
}