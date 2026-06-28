using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using Pontuei.Shared.Enums;

namespace Pontuei.Api.Models;

/// <summary>
/// Represents an email verification code associated with a user, including its type, expiration, and usage details.
/// </summary>
[Table("verification_code"), DataContract]
public class VerificationCode : BaseEntity
{
    [Column("verification_code_id"), DataMember]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid VerificationCodeId { get; set; }


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
    [Column("verification_code_hash"), DataMember]
    public required string VerificationCodeHash { get; set; }

    /// <summary>
    /// The type of email verification code, indicating whether it's for email confirmation or password reset.
    /// </summary>
    [Column("verification_code_type"), DataMember]
    public VerificationCodeType VerificationCodeType { get; set; }


    /// <summary>
    /// The expiration date and time of the email verification code, after which it becomes invalid.
    /// </summary>
    [Column("verification_code_expires_at"), DataMember]
    public DateTime VerificationCodeExpiresAt { get; set; }

    /// <summary>
    /// The date and time when the email verification code was used, if applicable. This field is null if the code has not been used yet.
    /// </summary>
    [Column("verification_code_used_at"), DataMember]
    public DateTime? VerificationCodeUsedAt { get; set; }

    /// <summary>
    /// The payload associated with the verification code, which may contain additional information needed for verification.
    /// </summary>
    [Column("verification_code_payload"), DataMember]
    public string? VerificationCodePayload { get; set; }

    /// <summary>
    /// The number of failed attempts made to use the verification code, which can be used to track potential misuse or brute-force attempts.
    /// </summary>
    [Column("verification_code_failed_attempts"), DataMember]
    public int VerificationCodeFailedAttempts { get; set; }
}