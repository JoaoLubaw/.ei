using Microsoft.EntityFrameworkCore;

using Pontuei.Api.Data;

using Pontuei.Shared.Dtos.Requests;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class UserLoyaltyProgramRepository : BaseRepository, IUserLoyaltyProgramRepository
{
    public UserLoyaltyProgramRepository(PontueiDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns all enrollment records for the given user, ordered by
    /// <c>user_loyalty_program_display_order</c> ascending.
    /// Navigation property <c>LoyaltyProgram</c> is included.
    /// </summary>
    public IQueryable<UserLoyaltyProgram> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.UserLoyaltyPrograms
            .Include(ulp => ulp.LoyaltyProgram)
            .Where(ulp => ulp.UserId == userId)
            .OrderBy(ulp => ulp.UserLoyaltyProgramDisplayOrder);
    }

    /// <summary>
    /// Returns a single enrollment record, or <c>null</c> when the user
    /// is not enrolled in the given program.
    /// </summary>
    public async Task<UserLoyaltyProgram?> GetAsync(Guid userId, int loyaltyProgramId)
    {
        return await _dbContext.UserLoyaltyPrograms
            .Include(ulp => ulp.LoyaltyProgram)
            .FirstOrDefaultAsync(ulp => ulp.UserId == userId && ulp.LoyaltyProgramId == loyaltyProgramId);
    }

    /// <summary>
    /// Persists a new enrollment record and returns the saved entity.
    /// </summary>
    public Task<UserLoyaltyProgram> CreateAsync(CreateUserLoyaltyProgramRequestDto dto, Guid userId, string createdBy)
    {
        UserLoyaltyProgram userLoyaltyProgram = new UserLoyaltyProgram
        {
            UserId = userId,
            LoyaltyProgramId = dto.LoyaltyProgramId,
            UserLoyaltyProgramDisplayOrder = dto.DisplayOrder,
            CreationTime = DateTime.UtcNow,
            CreationUser = createdBy
        };

        _dbContext.UserLoyaltyPrograms.Add(userLoyaltyProgram);

        return Task.FromResult(userLoyaltyProgram);
    }

    /// <summary>
    /// Atomically replaces the full enrollment list for a user with the provided set.
    /// Used by the bulk-save operation on the card-reorder screen ("Salvar").
    /// </summary>
    public async Task BulkUpdateAsync(User user, BulkUpdateUserLoyaltyProgramsRequestDto requestDto, string updatedBy)
    {
        List<UserLoyaltyProgram> existingEnrollments = await _dbContext.UserLoyaltyPrograms
            .Where(ulp => ulp.UserId == user.UserId && !ulp.IsDeleted)
            .ToListAsync();

        List<int> newProgramIds = requestDto.Programs.Select(e => e.LoyaltyProgramId).Where(id => !existingEnrollments.Any(ulp => ulp.LoyaltyProgramId == id)).ToList();

        List<UserLoyaltyProgram> enrollmentsToEdit = existingEnrollments
            .Where(ulp => requestDto.Programs.Any(e => e.LoyaltyProgramId == ulp.LoyaltyProgramId))
            .ToList();

        foreach (UserLoyaltyProgram enrollment in enrollmentsToEdit)
        {
            short newDisplayOrder = requestDto.Programs.First(e => e.LoyaltyProgramId == enrollment.LoyaltyProgramId).DisplayOrder;

            if (enrollment.UserLoyaltyProgramDisplayOrder != newDisplayOrder)
            {
                enrollment.UserLoyaltyProgramDisplayOrder = newDisplayOrder;
                _dbContext.Entry(enrollment).Property(ulp => ulp.UserLoyaltyProgramDisplayOrder).IsModified = true;

                enrollment.UpdateTime = DateTime.UtcNow;
                _dbContext.Entry(enrollment).Property(ulp => ulp.UpdateTime).IsModified = true;

                enrollment.UpdateUser = updatedBy;
                _dbContext.Entry(enrollment).Property(ulp => ulp.UpdateUser).IsModified = true;
            }
        }

        foreach (int programId in newProgramIds)
        {
            short displayOrder = requestDto.Programs.First(e => e.LoyaltyProgramId == programId).DisplayOrder;

            UserLoyaltyProgram newEnrollment = new UserLoyaltyProgram
            {
                UserId = user.UserId,
                LoyaltyProgramId = programId,
                UserLoyaltyProgramDisplayOrder = displayOrder,
                CreationTime = DateTime.UtcNow,
                CreationUser = updatedBy
            };

            _dbContext.UserLoyaltyPrograms.Add(newEnrollment);
        }
    }

    /// <summary>
    /// Removes a single enrollment record.
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    public Task DeleteAsync(UserLoyaltyProgram userLoyaltyProgram, string deletedBy)
    {
        _dbContext.Attach(userLoyaltyProgram);

        userLoyaltyProgram.IsDeleted = true;
        _dbContext.Entry(userLoyaltyProgram).Property(ulp => ulp.IsDeleted).IsModified = true;

        userLoyaltyProgram.UpdateTime = DateTime.UtcNow;
        _dbContext.Entry(userLoyaltyProgram).Property(ulp => ulp.UpdateTime).IsModified = true;

        userLoyaltyProgram.UpdateUser = deletedBy;
        _dbContext.Entry(userLoyaltyProgram).Property(ulp => ulp.UpdateUser).IsModified = true;

        return Task.CompletedTask;
    }

    public async Task DeleteAllUserLoyaltyProgramsAsync(User user, string deletedBy)
    {
        await _dbContext.UserLoyaltyPrograms
                .Where(ulp => ulp.UserId == user.UserId && !ulp.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ulp => ulp.IsDeleted, true)
                    .SetProperty(ulp => ulp.UpdateTime, DateTime.UtcNow)
                    .SetProperty(ulp => ulp.UpdateUser, deletedBy));
    }
}