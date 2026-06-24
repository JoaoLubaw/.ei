using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using Pontuei.Api.Enums;

namespace Pontuei.Api.Models;

/// <summary>
/// Represents an email verification code associated with a user, including its type, expiration, and usage details.
/// </summary>
[Table("email_verification_code"), DataContract]
public class EmailVerificationCode
{
    [Column("email_verification_code_id"), DataMember]
    public int EmailVerificationCodeId { get; set; }

    /// <summary>
    /// Unique identifier for the user associated with this email verification code.
    /// </summary>
    [Column("user_id"), DataMember]
    public Guid UserId { get; set; }

    [ForeignKey("UserId"), IgnoreDataMember]
    public virtual User? User { get; set; }

    /// <summary>
    /// The actual code sent to the user's email for verification purposes.
    /// </summary>
    [Column("email_verification_code_code"), DataMember]
    public required string EmailVerificationCodeCode { get; set; }

    /// <summary>
    /// The type of email verification code, indicating whether it's for email confirmation or password reset.
    /// </summary>
    [Column("email_verification_code_type"), DataMember]
    public EmailVerificationCodeType EmailVerificationCodeType { get; set; }

    /// <summary>
    /// The expiration date and time of the email verification code, after which it becomes invalid.
    /// </summary>
    [Column("email_verification_code_expires_at"), DataMember]
    public DateTime EmailVerificationCodeExpiresAt { get; set; }

    /// <summary>
    /// The date and time when the email verification code was used, if applicable. This field is null if the code has not been used yet.
    /// </summary>
    [Column("email_verification_code_used_at"), DataMember]
    public DateTime? EmailVerificationCodeUsedAt { get; set; }
}