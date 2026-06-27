using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class BaseRepository
{
    protected readonly PontueiDbContext _dbContext;

    public BaseRepository(PontueiDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}