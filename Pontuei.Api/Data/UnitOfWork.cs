using Pontuei.Api.Interfaces.Repositories;

namespace Pontuei.Api.Data;

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