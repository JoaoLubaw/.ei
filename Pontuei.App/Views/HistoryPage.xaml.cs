using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace Pontuei.App.Views;

public partial class HistoryPage : BasePage
{
    public ObservableCollection<StatusFilterItem> Filters { get; } = [];
    public ObservableCollection<HistoryTransactionItem> Transactions { get; } = [];

    public HistoryPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadMocks();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.History, animate: false);
    }

    private void LoadMocks()
    {
        Filters.Add(new StatusFilterItem { Name = "Atrasadas", Color = Color.FromArgb("#C0392B") });
        Filters.Add(new StatusFilterItem { Name = "Recebidas", Color = Color.FromArgb("#4E8A61") });
        Filters.Add(new StatusFilterItem { Name = "Canceladas", Color = Color.FromArgb("#6B6B6B") });

        Transactions.Add(new HistoryTransactionItem
        {
            TransactionId = Guid.NewGuid(),
            Description = "Tv",
            Store = "Casas Bahia",
            ProgramLogo = "livelo_logo.png",
            Points = 10000,
            ExpectedDate = new DateOnly(2025, 8, 19),
            ReceivedDate = new DateOnly(2025, 8, 20),
            StatusColor = Color.FromArgb("#C0392B")
        });

        Transactions.Add(new HistoryTransactionItem
        {
            TransactionId = Guid.NewGuid(),
            Description = "Tv",
            Store = "Casas Bahia",
            ProgramLogo = "livelo_logo.png",
            Points = 10000,
            ExpectedDate = new DateOnly(2025, 8, 19),
            ReceivedDate = new DateOnly(2025, 8, 20),
            StatusColor = Color.FromArgb("#4E8A61")
        });
    }

    // ── Filtros ───────────────────────────────────────────────────────────

    private void OnFilterTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Border tappedBorder || e.Parameter is not StatusFilterItem tappedItem) return;
        var stack = (HorizontalStackLayout)tappedBorder.Parent;

        foreach (View child in stack.Children)
        {
            if (child is Border border && border.BindingContext is StatusFilterItem item && item.IsSelected && item != tappedItem)
            {
                item.IsSelected = false;
                AnimatePill(border, expand: false);
            }
        }

        tappedItem.IsSelected = !tappedItem.IsSelected;
        AnimatePill(tappedBorder, expand: tappedItem.IsSelected);
    }

    private void AnimatePill(Border border, bool expand)
    {
        if (border.Content is Label label)
        {
            if (expand) label.IsVisible = true;

            var targetWidth = expand ? 115.0 : 24.0;
            var targetOpacity = expand ? 1.0 : 0.0;

            var animation = new Animation();
            animation.Add(0, 1, new Animation(v => border.WidthRequest = v, border.WidthRequest, targetWidth, Easing.CubicOut));
            animation.Add(0, 1, new Animation(v => label.Opacity = v, label.Opacity, targetOpacity, Easing.CubicOut));

            animation.Commit(border, "PillAnimation", length: 250, finished: (v, c) =>
            {
                if (!expand) label.IsVisible = false;
            });
        }
    }

    // ── Navegação ─────────────────────────────────────────────────────────

    private async void OnViewPendingTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//home");

    private async void OnChangeProgramTapped(object sender, TappedEventArgs e)
        => await DisplayAlert("Em breve", "Seletor de programa.", "OK");

    // ── [NOVO] Tap em transação → abre detalhes ───────────────────────────
    private async void OnTransactionTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not HistoryTransactionItem item) return;

        var navParams = new Dictionary<string, object>
        {
            { "transactionId", item.TransactionId }
        };

        await Shell.Current.GoToAsync("transaction-detail", navParams);
    }
}

// ── Modelos ───────────────────────────────────────────────────────────────

public class StatusFilterItem
{
    public required string Name { get; init; }
    public required Color Color { get; init; }
    public bool IsSelected { get; set; }
}

public class HistoryTransactionItem
{
    // [NOVO] Id para navegação
    public Guid TransactionId { get; init; }

    public required string Description { get; init; }
    public required string Store { get; init; }
    public required string ProgramLogo { get; init; }
    public required int Points { get; init; }
    public required Color StatusColor { get; init; }
    public required DateOnly ExpectedDate { get; init; }
    public DateOnly? ReceivedDate { get; init; }

    public string FormattedPoints => $"{Points.ToString("N0", CultureInfo.GetCultureInfo("pt-BR"))} pts";
    public string FormattedExpected => $"Esperado: {ExpectedDate:dd/MM/yyyy}";
    public string FormattedReceived => ReceivedDate.HasValue ? $"Recebido: {ReceivedDate.Value:dd/MM/yyyy}" : string.Empty;
    public bool HasReceivedDate => ReceivedDate.HasValue;
}
