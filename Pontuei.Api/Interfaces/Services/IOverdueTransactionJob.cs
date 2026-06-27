namespace Pontuei.Api.Interfaces.Services;

public interface IOverdueTransactionJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}