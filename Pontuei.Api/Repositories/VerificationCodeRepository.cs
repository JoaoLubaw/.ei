using Microsoft.EntityFrameworkCore;

using Pontuei.Api.Data;

using Pontuei.Api.Enums;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class VerificationCodeRepository : BaseRepository, IVerificationCodeRepository
{
    public VerificationCodeRepository(PontueiDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns the most recent unused, non-expired verification code of the given
    /// <paramref name="type"/> for the specified user, or <c>null</c> when none exists.
    /// </summary>
    public async Task<VerificationCode?> GetPendingAsync(
        Guid userId,
        VerificationCodeType type
    )
    {
        return await _dbContext.VerificationCodes
            .Where(vc => vc.UserId == userId && vc.VerificationCodeType == type
                && vc.VerificationCodeUsedAt == null
                && vc.VerificationCodeExpiresAt > DateTime.UtcNow)
            .OrderByDescending(vc => vc.CreationTime)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Returns a verification code by its primary key, or <c>null</c> when not found.
    /// </summary>
    public async Task<VerificationCode?> GetByIdAsync(Guid verificationCodeId, bool verifyActive = true)
    {
        return await _dbContext.VerificationCodes
            .FirstOrDefaultAsync(vc => vc.VerificationCodeId == verificationCodeId && (!verifyActive || (vc.VerificationCodeUsedAt == null && vc.VerificationCodeExpiresAt > DateTime.UtcNow)));
    }

    /// <summary>
    /// Returns a verification code by its hash, or <c>null</c> when not found.
    /// </summary>
    /// <param name="verificationCodeHash"></param>
    /// <param name="type"></param>
    /// <param name="verifyActive"></param>
    /// <returns></returns>
    public async Task<VerificationCode?> GetByHashAsync(string verificationCodeHash, VerificationCodeType type, bool verifyActive = true)
    {
        return await _dbContext.VerificationCodes
            .FirstOrDefaultAsync(vc => vc.VerificationCodeHash == verificationCodeHash
             && vc.VerificationCodeType == type && (!verifyActive || (vc.VerificationCodeUsedAt == null
             && vc.VerificationCodeExpiresAt > DateTime.UtcNow))
        );
    }

    /// <summary>
    /// Persists a new verification code row and returns the saved entity.
    /// </summary>
    public Task<VerificationCode> CreateAsync(
        Guid userId,
        VerificationCodeType type,
        string code,
        string? payload,
        DateTime expiresAt,
        string createdBy
    )
    {
        VerificationCode verificationCode = new VerificationCode
        {
            UserId = userId,
            VerificationCodeType = type,
            VerificationCodeHash = code,
            VerificationCodePayload = payload,
            VerificationCodeExpiresAt = expiresAt,
            CreationTime = DateTime.UtcNow,
            CreationUser = createdBy
        };

        _dbContext.VerificationCodes.Add(verificationCode);

        return Task.FromResult(verificationCode);
    }

    /// <summary>
    /// Records a successful code use by setting <c>verification_code_used_at</c>.
    /// </summary>
    public Task MarkAsUsedAsync(VerificationCode verificationCode, string updatedBy)
    {
        _dbContext.VerificationCodes.Attach(verificationCode);

        verificationCode.VerificationCodeUsedAt = DateTime.UtcNow;
        verificationCode.UpdateTime = DateTime.UtcNow;
        verificationCode.UpdateUser = updatedBy;

        _dbContext.Entry(verificationCode).Property(vc => vc.VerificationCodeUsedAt).IsModified = true;
        _dbContext.Entry(verificationCode).Property(vc => vc.UpdateTime).IsModified = true;
        _dbContext.Entry(verificationCode).Property(vc => vc.UpdateUser).IsModified = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Increments <c>verification_code_failed_attempts</c> by 1 for the given code.
    /// Used to detect brute-force attempts.
    /// </summary>
    public Task IncrementFailedAttemptsAsync(VerificationCode verificationCode, string updatedBy)
    {
        _dbContext.VerificationCodes.Attach(verificationCode);
        verificationCode.VerificationCodeFailedAttempts++;
        verificationCode.UpdateTime = DateTime.UtcNow;
        verificationCode.UpdateUser = updatedBy;

        _dbContext.Entry(verificationCode).Property(vc => vc.VerificationCodeFailedAttempts).IsModified = true;
        _dbContext.Entry(verificationCode).Property(vc => vc.UpdateTime).IsModified = true;
        _dbContext.Entry(verificationCode).Property(vc => vc.UpdateUser).IsModified = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Invalidates all unused codes of the given <paramref name="type"/> for a user
    /// by setting their expiration to the past. Called before issuing a fresh code.
    /// </summary>
    public async Task InvalidatePendingAsync(Guid userId, VerificationCodeType type, string updatedBy)
    {
        await _dbContext.VerificationCodes
            .Where(vc => vc.UserId == userId && vc.VerificationCodeType == type && vc.VerificationCodeUsedAt == null && vc.VerificationCodeExpiresAt > DateTime.UtcNow)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(vc => vc.VerificationCodeExpiresAt, DateTime.UtcNow.AddSeconds(-1))
                .SetProperty(vc => vc.UpdateTime, DateTime.UtcNow)
                .SetProperty(vc => vc.UpdateUser, updatedBy));
    }
}
