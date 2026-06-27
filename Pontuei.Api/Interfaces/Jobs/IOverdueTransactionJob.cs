namespace Pontuei.Api.Interfaces.Jobs;

public interface IOverdueTransactionJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}