namespace Pontuei.Api.Interfaces;

public interface IUnitOfWork
{
    Task<bool> CommitAsync();
}