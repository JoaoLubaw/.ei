using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pontuei.Api.Data;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class UserLoyaltyProgramRepository : BaseRepository, IUserLoyaltyProgramRepository
{
    private readonly ILogger<UserLoyaltyProgramRepository> _logger;

    public UserLoyaltyProgramRepository(PontueiDbContext dbContext, ILogger<UserLoyaltyProgramRepository> logger) : base(dbContext)
    {
        _logger = logger;
    }

    public IQueryable<UserLoyaltyProgram> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.UserLoyaltyPrograms
            .Include(ulp => ulp.LoyaltyProgram)
            .Where(ulp => ulp.UserId == userId && !ulp.IsDeleted)
            .OrderBy(ulp => ulp.UserLoyaltyProgramDisplayOrder);
    }

    public async Task<UserLoyaltyProgram?> GetAsync(Guid userId, int loyaltyProgramId)
    {
        return await _dbContext.UserLoyaltyPrograms
            .Include(ulp => ulp.LoyaltyProgram)
            .FirstOrDefaultAsync(ulp => ulp.UserId == userId && ulp.LoyaltyProgramId == loyaltyProgramId && !ulp.IsDeleted);
    }

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

    public async Task BulkUpdateAsync(User user, BulkUpdateUserLoyaltyProgramsRequestDto requestDto, string updatedBy)
    {
        _logger.LogInformation("--- STARTING BULK UPDATE FOR USER {UserId} ---", user.UserId);

        // 1. Fetch ALL existing enrollments (including deleted ones) to prevent unique constraint errors upon re-insertion
        List<UserLoyaltyProgram> allExistingEnrollments = await _dbContext.UserLoyaltyPrograms
            .IgnoreQueryFilters()
            .Where(ulp => ulp.UserId == user.UserId)
            .ToListAsync();

        // 2. Fetch programs with transactions in a single query
        List<int> programsWithTransactions = await _dbContext.Set<Transaction>()
            .Where(t => t.UserId == user.UserId && !t.IsDeleted)
            .Select(t => t.LoyaltyProgramId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation("Programs with transactions: {ProgramIds}", string.Join(", ", programsWithTransactions));

        // 3. Filter valid requests
        List<CreateUserLoyaltyProgramRequestDto> validRequests = new List<CreateUserLoyaltyProgramRequestDto>();

        foreach (CreateUserLoyaltyProgramRequestDto req in requestDto.Programs)
        {
            if (req.DisplayOrder < 4)
            {
                validRequests.Add(req);
            }
            else if (programsWithTransactions.Contains(req.LoyaltyProgramId))
            {
                validRequests.Add(req);
            }
        }

        List<int> validProgramIds = validRequests.Select(r => r.LoyaltyProgramId).ToList();
        _logger.LogInformation("Programs surviving the filter (Top 3 + With Transactions): {ValidIds}", string.Join(", ", validProgramIds));

        // 4. Handle Deletions
        List<UserLoyaltyProgram> enrollmentsToDelete = allExistingEnrollments
            .Where(ulp => !ulp.IsDeleted && !validProgramIds.Contains(ulp.LoyaltyProgramId))
            .ToList();

        foreach (UserLoyaltyProgram enrollment in enrollmentsToDelete)
        {
            // Optimization: Reusing the list in memory instead of hitting the database again
            bool hasTransactions = programsWithTransactions.Contains(enrollment.LoyaltyProgramId);

            if (!hasTransactions)
            {
                _logger.LogInformation("PHYSICAL deletion of program: {ProgramId}", enrollment.LoyaltyProgramId);
                _dbContext.UserLoyaltyPrograms.Remove(enrollment); // Hard Delete
            }
            else
            {
                _logger.LogInformation("Program {ProgramId} has transactions, keeping it at position 4", enrollment.LoyaltyProgramId);

                enrollment.UserLoyaltyProgramDisplayOrder = 4; // Guarantees it falls to position 4
                enrollment.UpdateTime = DateTime.UtcNow;
                enrollment.UpdateUser = updatedBy;

                _dbContext.Entry(enrollment).Property(ulp => ulp.UserLoyaltyProgramDisplayOrder).IsModified = true;
                _dbContext.Entry(enrollment).Property(ulp => ulp.UpdateTime).IsModified = true;
                _dbContext.Entry(enrollment).Property(ulp => ulp.UpdateUser).IsModified = true;
            }
        }

        // 5. Handle Upserts (Update existing or Insert new)
        foreach (CreateUserLoyaltyProgramRequestDto req in validRequests)
        {
            UserLoyaltyProgram? existing = allExistingEnrollments.FirstOrDefault(ulp => ulp.LoyaltyProgramId == req.LoyaltyProgramId);

            if (existing != null)
            {
                bool wasDeleted = existing.IsDeleted;
                bool orderChanged = existing.UserLoyaltyProgramDisplayOrder != req.DisplayOrder;

                if (wasDeleted || orderChanged)
                {
                    _logger.LogInformation("Updating program {ProgramId}. Was deleted? {WasDeleted}. Order changing from {OldOrder} to {NewOrder}",
                        req.LoyaltyProgramId, wasDeleted, existing.UserLoyaltyProgramDisplayOrder, req.DisplayOrder);

                    existing.IsDeleted = false;
                    existing.UserLoyaltyProgramDisplayOrder = req.DisplayOrder;
                    existing.UpdateTime = DateTime.UtcNow;
                    existing.UpdateUser = updatedBy;

                    _dbContext.Entry(existing).Property(ulp => ulp.IsDeleted).IsModified = true;
                    _dbContext.Entry(existing).Property(ulp => ulp.UserLoyaltyProgramDisplayOrder).IsModified = true;
                    _dbContext.Entry(existing).Property(ulp => ulp.UpdateTime).IsModified = true;
                    _dbContext.Entry(existing).Property(ulp => ulp.UpdateUser).IsModified = true;
                }
            }
            else
            {
                _logger.LogInformation("Creating NEW link for program {ProgramId} at order {DisplayOrder}", req.LoyaltyProgramId, req.DisplayOrder);

                UserLoyaltyProgram newEnrollment = new UserLoyaltyProgram
                {
                    UserId = user.UserId,
                    LoyaltyProgramId = req.LoyaltyProgramId,
                    UserLoyaltyProgramDisplayOrder = req.DisplayOrder,
                    CreationTime = DateTime.UtcNow,
                    CreationUser = updatedBy,
                    IsDeleted = false
                };

                _dbContext.UserLoyaltyPrograms.Add(newEnrollment);
            }
        }

        _logger.LogInformation("--- END OF BULK UPDATE PROCESSING ---");
    }

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