using Pontuei.Api.Dtos;
using Pontuei.Api.Enums;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Repositories;

/// <summary>
/// Data-access contract for the <c>verification_code</c> table.
/// Covers both e-mail confirmation codes and password-reset codes.
/// </summary>
public interface IVerificationCodeRepository
{
    /// <summary>
    /// Returns the most recent unused, non-expired verification code of the given
    /// <paramref name="type"/> for the specified user, or <c>null</c> when none exists.
    /// </summary>
    Task<VerificationCode?> GetPendingAsync(
        Guid userId,
        VerificationCodeType type
    );

    /// <summary>
    /// Returns a verification code by its primary key, or <c>null</c> when not found.
    /// </summary>
    Task<VerificationCode?> GetByIdAsync(Guid verificationCodeId);

    /// <summary>
    /// Persists a new verification code row and returns the saved entity.
    /// </summary>
    Task<VerificationCode> CreateAsync(
        Guid userId,
        VerificationCodeType type,
        string code,
        string? payload,
        DateTime expiresAt,
        string createdBy
    );

    /// <summary>
    /// Records a successful code use by setting <c>verification_code_used_at</c>.
    /// </summary>
    Task MarkAsUsedAsync(VerificationCode verificationCode, string updatedBy);

    /// <summary>
    /// Increments <c>verification_code_failed_attempts</c> by 1 for the given code.
    /// Used to detect brute-force attempts.
    /// </summary>
    Task IncrementFailedAttemptsAsync(VerificationCode verificationCode, string updatedBy);

    /// <summary>
    /// Invalidates all unused codes of the given <paramref name="type"/> for a user
    /// by setting their expiration to the past. Called before issuing a fresh code.
    /// </summary>
    Task InvalidatePendingAsync(Guid userId, VerificationCodeType type, string updatedBy);
}
