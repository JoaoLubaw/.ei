namespace Pontuei.Api.Interfaces.Repositories;

public interface IUnitOfWork
{
    Task<bool> CommitAsync();
}