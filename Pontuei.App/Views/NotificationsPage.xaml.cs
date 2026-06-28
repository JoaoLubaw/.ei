using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Pontuei.App.Views;

public partial class NotificationsPage : BasePage, INotifyPropertyChanged
{
    public ObservableCollection<NotificationItem> Notifications { get; } = [];

    private bool _hasNotifications;
    public bool HasNotifications
    {
        get => _hasNotifications;
        set { _hasNotifications = value; OnPropertyChanged(nameof(HasNotifications)); OnPropertyChanged(nameof(HasNoNotifications)); }
    }

    public bool HasNoNotifications => !_hasNotifications;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool _hasUnread;
    public bool HasUnread
    {
        get => _hasUnread;
        set { _hasUnread = value; OnPropertyChanged(nameof(HasUnread)); }
    }

    public NotificationsPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadMockNotifications();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.Notifications, animate: false);
    }

    // ── Mock ──────────────────────────────────────────────────────────────

    private void LoadMockNotifications()
    {
        var items = new List<NotificationItem>
        {
            new()
            {
                NotificationId = Guid.NewGuid(),
                Message = "O prazo de receber seus pontos Livelo expirou, que tal atualizar o status?",
                Points = 10_000,
                ProgramName = "Livelo",
                Date = new DateOnly(2025, 8, 19),
                IsRead = false
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                Message = "O prazo de receber seus pontos Livelo expirou, que tal atualizar o status?",
                Points = 10_000,
                ProgramName = "Livelo",
                Date = new DateOnly(2025, 8, 19),
                IsRead = false
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                Message = "O prazo de receber seus pontos Livelo expirou, que tal atualizar o status?",
                Points = 10_000,
                ProgramName = "Livelo",
                Date = new DateOnly(2025, 8, 19),
                IsRead = true
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                Message = "O prazo de receber seus pontos Livelo expirou, que tal atualizar o status?",
                Points = 10_000,
                ProgramName = "Livelo",
                Date = new DateOnly(2025, 8, 19),
                IsRead = true
            },
        };

        foreach (var item in items)
            Notifications.Add(item);

        RefreshState();
    }

    // ── Handlers ──────────────────────────────────────────────────────────

    private void OnNotificationTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is NotificationItem item && !item.IsRead)
        {
            item.IsRead = true;
            RefreshState();
        }
    }

    private void OnMarkAllReadTapped(object sender, TappedEventArgs e)
    {
        foreach (var n in Notifications)
            n.IsRead = true;

        RefreshState();
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
    public required int Points { get; init; }
    public required string ProgramName { get; init; }
    public required DateOnly Date { get; init; }

    private bool _isRead;
    public bool IsRead
    {
        get => _isRead;
        set
        {
            _isRead = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRead)));
            // Notifica a tela para mudar a transparência quando for lida
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardOpacity)));
        }
    }

    // Se não lida, opacidade 100% (1.0). Se lida, fica mais apagada (ex: 50% = 0.5)
    public double CardOpacity => IsRead ? 0.5 : 1.0;

    public string FormattedPoints =>
        $"{Points.ToString("N0", CultureInfo.GetCultureInfo("pt-BR"))} pts";

    public string FormattedDate =>
        Date.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));

    public event PropertyChangedEventHandler? PropertyChanged;
}