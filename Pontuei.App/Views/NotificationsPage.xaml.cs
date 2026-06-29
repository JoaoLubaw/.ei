using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Pontuei.App.Services;
using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;

namespace Pontuei.App.Views;

public partial class NotificationsPage : BasePage, INotifyPropertyChanged
{
    private readonly NotificationApiService _notificationApi;

    // ── Estado de Loading e Notificações ──────────────────────────────────
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NotificationItem> Notifications { get; } = [];

    private bool _hasNotifications;
    public bool HasNotifications
    {
        get => _hasNotifications;
        set { _hasNotifications = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoNotifications)); }
    }

    public bool HasNoNotifications => !_hasNotifications && !IsLoading;

    private bool _hasUnread;
    public bool HasUnread
    {
        get => _hasUnread;
        set { _hasUnread = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // ── Construtor ────────────────────────────────────────────────────────
    public NotificationsPage(NotificationApiService notificationApi)
    {
        InitializeComponent();
        BindingContext = this;
        _notificationApi = notificationApi;
    }

    // ── Ciclo de Vida ─────────────────────────────────────────────────────
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.Notifications, animate: false);

        // Carrega as notificações reais da API toda vez que a tela abrir
        await LoadNotificationsAsync();
    }

    // ── Integração com a API ──────────────────────────────────────────────

    private async Task LoadNotificationsAsync()
    {
        Guid? userId = AuthService.CurrentUserId;
        if (userId == null) return;

        IsLoading = true;

        try
        {
            // Busca a primeira página de notificações (ajuste o tamanho conforme precisar)
            var request = new GetNotificationsRequestDto { Page = 1, Size = 50 };
            var response = await _notificationApi.GetNotificationsAsync(userId.Value, request);

            if (response.IsSuccess && response.Data != null)
            {
                Notifications.Clear();

                foreach (var dto in response.Data.Notifications)
                {
                    Notifications.Add(NotificationItem.FromDto(dto));
                }

                RefreshState();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[API Erro] Falha ao carregar notificações: {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Erro] LoadNotificationsAsync: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasNoNotifications)); // Re-avalia o estado vazio após o loading terminar
        }
    }

    // ── Handlers ──────────────────────────────────────────────────────────

    private async void OnNotificationTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is NotificationItem item && !item.IsRead)
        {
            try
            {
                // Dispara a chamada para a API para marcar como lida
                var response = await _notificationApi.MarkAsReadAsync(item.NotificationId);

                if (response.IsSuccess)
                {
                    item.IsRead = true;
                    RefreshState();
                    // Opcional: Aqui você pode disparar um evento global para atualizar a bolinha do menu inferior
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erro] OnNotificationTapped: {ex.Message}");
            }
        }
    }

    private async void OnMarkAllReadTapped(object sender, TappedEventArgs e)
    {
        Guid? userId = AuthService.CurrentUserId;
        if (userId == null) return;

        try
        {
            var request = new GetNotificationsRequestDto { Page = 1, Size = 50 };
            var response = await _notificationApi.MarkAllAsReadAsync(userId.Value, request);

            if (response.IsSuccess)
            {
                foreach (var n in Notifications)
                {
                    n.IsRead = true;
                }
                RefreshState();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Erro] OnMarkAllReadTapped: {ex.Message}");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void RefreshState()
    {
        HasNotifications = Notifications.Count > 0;
        HasUnread = Notifications.Any(n => !n.IsRead);
    }
}

// ── Model ─────────────────────────────────────────────────────────────────

public sealed class NotificationItem : INotifyPropertyChanged
{
    public required Guid NotificationId { get; init; }
    public required string Message { get; init; }
    public required decimal Points { get; init; }

    private bool _isRead;
    public bool IsRead
    {
        get => _isRead;
        set
        {
            _isRead = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRead)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardOpacity)));
        }
    }

    public double CardOpacity => IsRead ? 0.5 : 1.0;

    public string FormattedPoints =>
        $"{Points.ToString("N0", CultureInfo.GetCultureInfo("pt-BR"))} pts";

    public event PropertyChangedEventHandler? PropertyChanged;

    // Factory method para converter o DTO que vem da API no modelo da tela
    public static NotificationItem FromDto(NotificationDto dto)
    {
        return new NotificationItem
        {
            NotificationId = dto.NotificationId,
            Message = dto.NotificationMessage ?? "Você tem uma nova notificação.",
            Points = dto.NotificationPointsAmount ?? 0,
            IsRead = dto.NotificationIsRead
        };
    }
}