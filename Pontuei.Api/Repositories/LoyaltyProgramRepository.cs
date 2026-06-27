using Microsoft.EntityFrameworkCore;
using Pontuei.Api.Dtos;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Enums;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class LoyaltyProgramRepository : BaseRepository, ILoyaltyProgramRepository
{
    public LoyaltyProgramRepository(PontueiDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns the loyalty program with the given <paramref name="loyaltyProgramId"/>,
    /// or <c>null</c> when not found.
    /// </summary>
    public async Task<LoyaltyProgram?> GetByIdAsync(int loyaltyProgramId, bool VerifyActive = false)
    {
        return await _dbContext.LoyaltyPrograms.FirstOrDefaultAsync(
            lp => lp.LoyaltyProgramId == loyaltyProgramId
            && !lp.IsDeleted
            && (!VerifyActive || lp.LoyaltyProgramIsActive)
            );
    }

    /// <summary>
    /// Returns all loyalty programs, optionally filtered to active-only records.
    /// Results are ordered by name ascending.
    /// </summary>
    public IQueryable<LoyaltyProgram> GetAllAsync()
    {
        return _dbContext.LoyaltyPrograms.OrderBy(lp => lp.LoyaltyProgramName);
    }

    /// <summary>
    /// Persists a new loyalty program and returns the saved entity.
    /// </summary>
    public async Task<LoyaltyProgram> CreateAsync(CreateLoyaltyProgramRequestDto createDto, string createdBy)
    {
        LoyaltyProgram loyaltyProgram = new LoyaltyProgram
        {
            LoyaltyProgramName = createDto.LoyaltyProgramName,
            LoyaltyProgramBrandPrimaryColor = createDto.LoyaltyProgramBrandPrimaryColor,
            LoyaltyProgramBrandSecondaryColor = createDto.LoyaltyProgramBrandSecondaryColor,
            LoyaltyProgramIsActive = createDto.LoyaltyProgramIsActive,
            LoyaltyProgramLogoUrl = createDto.LoyaltyProgramLogoUrl,
            CreationTime = DateTime.UtcNow,
            CreationUser = createdBy
        };

        _dbContext.LoyaltyPrograms.Add(loyaltyProgram);
        await _dbContext.SaveChangesAsync();

        return loyaltyProgram;
    }

    /// <summary>
    /// Applies changes to an existing loyalty program row and returns the updated entity.
    /// </summary>
    public async Task<LoyaltyProgram> UpdateAsync(LoyaltyProgram loyaltyProgram, UpdateLoyaltyProgramRequestDto updateDto, string updatedBy)
    {
        _dbContext.Attach(loyaltyProgram);

        if (updateDto.LoyaltyProgramName != null && updateDto.LoyaltyProgramName != loyaltyProgram.LoyaltyProgramName)
        {
            loyaltyProgram.LoyaltyProgramName = updateDto.LoyaltyProgramName;
            _dbContext.Entry(loyaltyProgram).Property(lp => lp.LoyaltyProgramName).IsModified = true;
        }

        if (updateDto.LoyaltyProgramLogoUrl != null && updateDto.LoyaltyProgramLogoUrl != loyaltyProgram.LoyaltyProgramLogoUrl)
        {
            loyaltyProgram.LoyaltyProgramLogoUrl = updateDto.LoyaltyProgramLogoUrl;
            _dbContext.Entry(loyaltyProgram).Property(lp => lp.LoyaltyProgramLogoUrl).IsModified = true;
        }

        if (updateDto.LoyaltyProgramBrandPrimaryColor != null && updateDto.LoyaltyProgramBrandPrimaryColor != loyaltyProgram.LoyaltyProgramBrandPrimaryColor)
        {
            loyaltyProgram.LoyaltyProgramBrandPrimaryColor = updateDto.LoyaltyProgramBrandPrimaryColor;
            _dbContext.Entry(loyaltyProgram).Property(lp => lp.LoyaltyProgramBrandPrimaryColor).IsModified = true;
        }

        if (updateDto.LoyaltyProgramBrandSecondaryColor != null && updateDto.LoyaltyProgramBrandSecondaryColor != loyaltyProgram.LoyaltyProgramBrandSecondaryColor)
        {
            loyaltyProgram.LoyaltyProgramBrandSecondaryColor = updateDto.LoyaltyProgramBrandSecondaryColor;
            _dbContext.Entry(loyaltyProgram).Property(lp => lp.LoyaltyProgramBrandSecondaryColor).IsModified = true;
        }

        loyaltyProgram.UpdateTime = DateTime.UtcNow;
        _dbContext.Entry(loyaltyProgram).Property(lp => lp.UpdateTime).IsModified = true;

        loyaltyProgram.UpdateUser = updatedBy;
        _dbContext.Entry(loyaltyProgram).Property(lp => lp.UpdateUser).IsModified = true;

        await _dbContext.SaveChangesAsync();

        return loyaltyProgram;
    }

    /// <summary>
    /// Toggles the active status of the loyalty program.
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    public async Task<bool> ToggleActiveAsync(LoyaltyProgram loyaltyProgram, string updatedBy)
    {
        _dbContext.Attach(loyaltyProgram);

        loyaltyProgram.LoyaltyProgramIsActive = !loyaltyProgram.LoyaltyProgramIsActive;
        _dbContext.Entry(loyaltyProgram).Property(lp => lp.LoyaltyProgramIsActive).IsModified = true;

        loyaltyProgram.UpdateTime = DateTime.UtcNow;
        _dbContext.Entry(loyaltyProgram).Property(lp => lp.UpdateTime).IsModified = true;

        loyaltyProgram.UpdateUser = updatedBy;
        _dbContext.Entry(loyaltyProgram).Property(lp => lp.UpdateUser).IsModified = true;

        await _dbContext.SaveChangesAsync();
        return true;
    }

}