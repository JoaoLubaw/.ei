using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Pontuei.Api.Models;

/// <summary>
/// Represents a user in the system, including their personal information, authentication details, and notification preferences.
/// </summary>
[Table("user"), DataContract]
public class User : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    [Column("user_id"), DataMember]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the user, which is a required field and represents the user's full name or display name.
    /// </summary>
    [Column("user_name"), DataMember]
    public required string UserName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user, which is a required field and is used for communication and authentication purposes.
    /// </summary>
    [Column("user_email"), DataMember]
    public required string UserEmail { get; set; }

    /// <summary>
    /// Gets or sets the hashed password of the user, which is an optional field and is used for authentication purposes. 
    /// This field should be securely hashed and salted to protect user credentials.
    /// </summary>
    [Column("user_password_hash"), DataMember]
    public string? UserPasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the user's Google account, which is an optional field and can be used for authentication via Google OAuth.
    /// </summary>
    [Column("user_google_id"), DataMember]
    public string? UserGoogleId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's email address has been verified.
    /// </summary>
    [Column("user_email_verified"), DataMember]
    public bool UserEmailVerified { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user's email address was verified, which is an optional field and is null if the email has not been verified yet.
    /// </summary>
    [Column("user_email_verified_at"), DataMember]
    public DateTime? UserEmailVerifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the email verification code associated with the user, which is an optional field and can be used for email confirmation or password reset processes.
    /// </summary>
    [Column("user_email_verification_code"), DataMember]
    public string? UserEmailVerificationCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether push notifications are enabled for the user.
    /// </summary>
    [Column("user_push_notifications_enabled"), DataMember]
    public bool UserPushNotificationsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether email notifications are enabled for the user.
    /// </summary>
    [Column("user_email_notifications_enabled"), DataMember]
    public bool UserEmailNotificationsEnabled { get; set; }

}