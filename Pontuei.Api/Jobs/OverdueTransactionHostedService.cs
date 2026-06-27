using Pontuei.Api.Data;
using Pontuei.Api.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Pontuei.Api.Interfaces.Jobs;
using Pontuei.Api.Interfaces.Repositories;

namespace Pontuei.Api.Jobs;

public class OverdueTransactionHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OverdueTransactionHostedService> _logger;

    private static readonly TimeOnly DefaultTargetTime = new TimeOnly(12, 0);

    public OverdueTransactionHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<OverdueTransactionHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[OverdueHostedService] Started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeOnly targetTime = await ReadTargetTimeFromDbAsync();

            TimeSpan delay = CalculateDelayUntilNextRun(targetTime);

            _logger.LogInformation(
                "[OverdueHostedService] Next run at {TargetTime} (Brasília). Waiting {Delay:hh\\:mm\\:ss}.",
                targetTime, delay);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            await RunJobAsync(stoppingToken);
        }
    }

    private async Task<TimeOnly> ReadTargetTimeFromDbAsync()
    {
        try
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            IConfigurationService _configurationService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();

            TimeOnly time = await _configurationService.GetNotificationTimeAsync();

            return time;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[OverdueHostedService] Failed to read notification_time from DB. Using default {Default}.", DefaultTargetTime);
        }

        return DefaultTargetTime;
    }

    private async Task RunJobAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        IOverdueTransactionJob job = scope.ServiceProvider.GetRequiredService<IOverdueTransactionJob>();

        try
        {
            await job.ExecuteAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OverdueHostedService] Unhandled exception during job execution.");
        }
    }

    private static TimeSpan CalculateDelayUntilNextRun(TimeOnly targetTime)
    {
        string tzId = OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo";
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);

        DateTime nowBrazil = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        DateTime nextRunBrazil = nowBrazil.Date.Add(targetTime.ToTimeSpan());

        if (nowBrazil >= nextRunBrazil)
            nextRunBrazil = nextRunBrazil.AddDays(1);

        DateTime nextRunUtc = TimeZoneInfo.ConvertTimeToUtc(nextRunBrazil, tz);
        return nextRunUtc - DateTime.UtcNow;
    }
}