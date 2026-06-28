using Microsoft.EntityFrameworkCore;
using Pontuei.Api.Data;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Enums;
using Pontuei.Api.Interfaces.Jobs;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models;

namespace Pontuei.Api.Jobs;

/// <summary>
/// Background job that checks for overdue transactions and sends notifications (email + push) to users.
/// </summary>
public class OverdueTransactionJob : IOverdueTransactionJob
{
    private readonly PontueiDbContext _dbContext;
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OverdueTransactionJob> _logger;

    private const string JobUser = "overdue-job";

    public OverdueTransactionJob(
        PontueiDbContext dbContext,
        INotificationRepository notificationRepository,
        IEmailService emailService,
        IPushNotificationService pushService,
        IUnitOfWork unitOfWork,
        ILogger<OverdueTransactionJob> logger)
    {
        _dbContext = dbContext;
        _notificationRepository = notificationRepository;
        _emailService = emailService;
        _pushService = pushService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[OverdueJob] Starting overdue transaction check at {Time} UTC", DateTime.UtcNow);

        DateOnly todayBrazil = GetTodayInBrazil();

        List<Transaction> overdueTransactions = await _dbContext.Transactions
            .Include(t => t.User)
            .Include(t => t.LoyaltyProgram)
            .Where(t =>
                !t.IsDeleted &&
                t.TransactionStatus == TransactionStatus.Pending &&
                t.TransactionItemReceiptDate != null &&
                t.TransactionItemReceiptDate.Value.AddDays(t.TransactionReceiptDeadlineDays) < todayBrazil)
            .ToListAsync(cancellationToken);

        if (!overdueTransactions.Any())
        {
            _logger.LogInformation("[OverdueJob] No overdue transactions found. Exiting.");
            return;
        }

        _logger.LogInformation("[OverdueJob] Found {Count} overdue transaction(s) to process.", overdueTransactions.Count);

        // Agrupa por usuário para buscar as sessions de push em batch
        List<Guid> userIds = overdueTransactions.Select(t => t.UserId).Distinct().ToList();

        // Busca todas as sessions ativas com push token dos usuários afetados
        Dictionary<Guid, List<string>> pushTokensByUser = await _dbContext.UserSessions
            .Where(s =>
                userIds.Contains(s.UserId) &&
                !s.UserSessionIsRevoked &&
                !s.IsDeleted &&
                s.UserSessionRefreshTokenExpiration > DateTime.UtcNow &&
                s.UserSessionPushNotificationToken != null)
            .GroupBy(s => s.UserId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(s => s.UserSessionPushNotificationToken!).ToList(),
                cancellationToken);

        foreach (Transaction transaction in overdueTransactions)
        {
            if (cancellationToken.IsCancellationRequested) break;

            await ProcessOverdueTransactionAsync(transaction, pushTokensByUser, cancellationToken);
        }

        await _unitOfWork.CommitAsync();

        _logger.LogInformation("[OverdueJob] Finished. Processed {Count} overdue transaction(s).", overdueTransactions.Count);
    }

    private async Task ProcessOverdueTransactionAsync(
        Transaction transaction,
        Dictionary<Guid, List<string>> pushTokensByUser,
        CancellationToken cancellationToken)
    {
        User user = transaction.User!;
        string programName = transaction.LoyaltyProgram?.LoyaltyProgramName ?? "programa de pontos";

        _logger.LogInformation("[OverdueJob] Processing transaction {TransactionId} for user {UserId}.",
            transaction.TransactionId, user.UserId);

        // 1. Atualiza status para Late
        transaction.TransactionStatus = TransactionStatus.Late;
        transaction.TransactionStatusUpdatedAt = DateTime.UtcNow;
        transaction.UpdateTime = DateTime.UtcNow;
        transaction.UpdateUser = JobUser;

        _dbContext.Entry(transaction).Property(t => t.TransactionStatus).IsModified = true;
        _dbContext.Entry(transaction).Property(t => t.TransactionStatusUpdatedAt).IsModified = true;
        _dbContext.Entry(transaction).Property(t => t.UpdateTime).IsModified = true;
        _dbContext.Entry(transaction).Property(t => t.UpdateUser).IsModified = true;

        // 2. Cria notificação no banco
        string message = $"O prazo para creditação dos pontos da sua compra em " +
                         $"\"{transaction.TransactionStore}\" ({programName}) expirou. " +
                         $"Verifique se os pontos foram creditados corretamente.";

        await _notificationRepository.CreateAsync(new NotificationDto
        {
            UserId = user.UserId,
            TransactionId = transaction.TransactionId,
            LoyaltyProgramId = transaction.LoyaltyProgramId,
            NotificationMessage = message,
            NotificationPointsAmount = (int)transaction.TransactionEstimatedPoints
        }, JobUser);

        // 3. E-mail (se o usuário optou)
        if (user.UserEmailNotificationsEnabled && user.UserEmailVerified)
        {
            try
            {
                await _emailService.SendOverdueTransactionEmailAsync(
                    user.UserEmail,
                    user.UserName,
                    transaction.TransactionStore,
                    programName,
                    transaction.Deadline!.Value,
                    (int)transaction.TransactionEstimatedPoints);
            }
            catch (Exception ex)
            {
                // Não deixa um e-mail falho derrubar o job inteiro
                _logger.LogError(ex, "[OverdueJob] Failed to send overdue email to user {UserId}.", user.UserId);
            }
        }

        // 4. Push notification (se o usuário optou e tem devices registrados)
        if (user.UserPushNotificationsEnabled && pushTokensByUser.TryGetValue(user.UserId, out List<string>? tokens))
        {
            string pushTitle = "Prazo expirado! ⏰";
            string pushBody = $"Os pontos de \"{transaction.TransactionStore}\" podem não ter sido creditados. Confira agora!";

            foreach (string token in tokens)
            {
                if (cancellationToken.IsCancellationRequested) break;
                try
                {
                    await _pushService.SendAsync(token, pushTitle, pushBody, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[OverdueJob] Failed to send push to token for user {UserId}.", user.UserId);
                }
            }
        }
    }

    private static DateOnly GetTodayInBrazil()
    {
        string tzId = OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo";
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
    }
}