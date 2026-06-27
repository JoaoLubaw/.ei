using Pontuei.Api.Interfaces;
using Pontuei.Api.Models;

namespace Pontuei.Api.Models;

public class UnitOfWork : IUnitOfWork
{
    private readonly PontueiDbContext _dbContext;

    public UnitOfWork(PontueiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CommitAsync()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }
}