using System.Text.Json.Serialization;
using Pontuei.Api.Dtos.Objects;

using Pontuei.Api.Common;

namespace Pontuei.Api.Dtos.Requests;

/// <summary>
/// Payload for creating a new user account via e-mail + password.
/// Matches the "Nova conta" screen in the app prototype (name, e-mail, password,
/// confirm password, terms acceptance).
/// </summary>
public class CreateUserRequestDto
{
    /// <summary>
    /// User's full display name.
    /// Required. Maps to <c>user_name</c>.
    /// </summary>
    [JsonPropertyName("userName")]
    public required string UserName { get; set; }

    /// <summary>
    /// User's e-mail address used for login and notifications.
    /// Required. Maps to <c>user_email</c>.
    /// </summary>
    [JsonPropertyName("userEmail")]
    public required string UserEmail { get; set; }

    /// <summary>
    /// Plain-text password that will be hashed before persistence.
    /// Must satisfy the rules shown in the prototype:
    /// ≥ 8 characters, ≥ 1 uppercase, ≥ 1 lowercase, ≥ 1 digit, ≥ 1 special character.
    /// </summary>
    [JsonPropertyName("password")]
    public required string Password { get; set; }

    /// <summary>
    /// Password confirmation — must match <see cref="Password"/>.
    /// Validated at the DTO level before the service is invoked.
    /// </summary>
    [JsonPropertyName("confirmPassword")]
    public required string ConfirmPassword { get; set; }

    /// <summary>
    /// Optional phone number for future communication or 2FA purposes.
    /// Maps to <c>user_phone_number</c>.
    /// </summary>
    [JsonPropertyName("userPhoneNumber")]
    public string? UserPhoneNumber { get; set; }

    /// <summary>
    /// Whether push notifications should be enabled for this user.
    /// Optional. Maps to <c>user_push_notifications_enabled</c>.
    /// </summary>
    [JsonPropertyName("userPushNotificationsEnabled")]
    public required bool UserPushNotificationsEnabled { get; set; }

    /// <summary>
    /// Whether e-mail notifications should be enabled for this user.
    /// Optional. Maps to <c>user_email_notifications_enabled</c>.
    /// </summary>
    [JsonPropertyName("userEmailNotificationsEnabled")]
    public required bool UserEmailNotificationsEnabled { get; set; }

    /// <summary>
    /// Whether the user is an administrator.
    /// Optional. Maps to <c>user_is_admin</c>.
    /// </summary>
    [JsonPropertyName("userIsAdmin")]
    public bool? UserIsAdmin { get; set; }

    /// <summary>
    /// Whether the user has accepted the terms and conditions.
    /// Required. Maps to <c>user_accepted_terms</c>.
    /// </summary>
    [JsonPropertyName("userAcceptedTerms")]
    public required bool UserAcceptedTerms { get; set; }

    // ── Validation helpers ────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> when all required fields are non-empty.
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(UserName) &&
        !string.IsNullOrWhiteSpace(UserEmail) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) &&
        UserAcceptedTerms
        ;

    /// <summary>
    /// Returns <c>true</c> when Password and ConfirmPassword match.
    /// </summary>
    public bool PasswordsMatch() =>
        Password == ConfirmPassword;

    /// <summary>
    /// Validates the password against the rules shown in the prototype:
    /// ≥ 8 chars, ≥ 1 uppercase, ≥ 1 lowercase, ≥ 1 digit, ≥ 1 special character.
    /// </summary>
    public bool IsValidPassword() =>
        ValidationUtils.IsValidPassword(Password);

    /// <summary>
    /// Validates the e-mail format using a basic regex check.
    /// </summary>
    public bool IsValidEmail() =>
        ValidationUtils.IsValidEmail(UserEmail);
}

/// <summary>
/// Payload for updating mutable profile fields.
/// Matches the "Informações de conta" settings screen (name and e-mail editable inline).
/// All fields are optional — only non-null values are applied.
/// </summary>
public class UpdateUserRequestDto
{
    /// <summary>
    /// New display name.
    /// Optional. Maps to <c>user_name</c>.
    /// </summary>
    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    /// <summary>
    /// New e-mail address.  Triggers an e-mail verification flow when changed.
    /// Optional. Maps to <c>user_email</c>.
    /// </summary>
    [JsonPropertyName("userEmail")]
    public string? UserEmail { get; set; }

    /// <summary>
    /// New phone number.
    /// Optional. Maps to <c>user_phone_number</c>.
    /// </summary>
    [JsonPropertyName("userPhoneNumber")]
    public string? UserPhoneNumber { get; set; }

    /// <summary>
    /// Whether push notifications should be enabled for this user.
    /// Optional. Maps to <c>user_push_notifications_enabled</c>.
    /// </summary>
    [JsonPropertyName("userPushNotificationsEnabled")]
    public bool? UserPushNotificationsEnabled { get; set; }

    /// <summary>
    /// Whether e-mail notifications should be enabled for this user.
    /// Optional. Maps to <c>user_email_notifications_enabled</c>.
    /// </summary>
    [JsonPropertyName("userEmailNotificationsEnabled")]
    public bool? UserEmailNotificationsEnabled { get; set; }

    public bool IsValidEmail() =>
        ValidationUtils.IsValidEmail(UserEmail);

}

public class GetUsersRequestDto
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public UserFiltersDto? Filters { get; set; }
}

public class UserFiltersDto
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserPhoneNumber { get; set; }
    public bool? UserEmailVerified { get; set; }
    public bool? UserIsAdmin { get; set; }
}