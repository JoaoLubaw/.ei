using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using PanCardView.EventArgs;
using Pontuei.App.Services;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Shared.Enums;

namespace Pontuei.App.Views;

public partial class HomePage : BasePage, INotifyPropertyChanged
{
    private bool _isCensored;

    // Fix 7: lista de grupos para o card Outros
    public ObservableCollection<DashboardCardItem> Cards { get; } = [];
    public ObservableCollection<DashboardTransactionItem> CurrentTransactions { get; } = [];
    public ObservableCollection<DashboardTransactionGroup> CurrentGroups { get; } = [];

    private bool _hasGroups;
    public bool HasGroups
    {
        get => _hasGroups;
        set
        {
            _hasGroups = value;
            OnPropertyChanged(nameof(HasGroups));
            OnPropertyChanged(nameof(HasSimpleList));
        }
    }
    // A lista simples fica visível quando não há grupos
    public bool HasSimpleList => !_hasGroups;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public HomePage()
    {
        try
        {
            InitializeComponent();
            BindingContext = this;
            LoadDashboardMock();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO HomePage] {ex}");
            throw;
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.Home, animate: false);
    }

    private void LoadDashboardMock()
    {
        var livelo = new LoyaltyProgramDto
        {
            LoyaltyProgramId = 3,
            LoyaltyProgramName = "Livelo",
            LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Livelo.webp",
            LoyaltyProgramBrandPrimaryColor = "#E4002B",
            LoyaltyProgramBrandSecondaryColor = "#B8001F",
            LoyaltyProgramIsActive = true
        };

        var dotz = new LoyaltyProgramDto
        {
            LoyaltyProgramId = 2,
            LoyaltyProgramName = "Dotz",
            LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Dotz.webp",
            LoyaltyProgramBrandPrimaryColor = "#FF6B00",
            LoyaltyProgramBrandSecondaryColor = "#CC5500",
            LoyaltyProgramIsActive = true
        };

        var esfera = new LoyaltyProgramDto
        {
            LoyaltyProgramId = 1,
            LoyaltyProgramName = "Esfera",
            LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Esfera.webp",
            LoyaltyProgramBrandPrimaryColor = "#003366",
            LoyaltyProgramBrandSecondaryColor = "#001F3F",
            LoyaltyProgramIsActive = true
        };

        var liveloTransactions = CreateMockTransactions(livelo.LoyaltyProgramId, 3);
        var dotzTransactions = CreateMockTransactions(dotz.LoyaltyProgramId, 2);
        var esferaTransactions = CreateMockTransactions(esfera.LoyaltyProgramId, 1);

        var summary = new GetDashboardSummaryResponseDto
        {
            TopPrograms =
            [
                new DashboardProgramCardDto { LoyaltyProgram = livelo, TotalPendingPoints = 30000, PendingTransactions = liveloTransactions },
                new DashboardProgramCardDto { LoyaltyProgram = dotz, TotalPendingPoints = 15000, PendingTransactions = dotzTransactions },
                new DashboardProgramCardDto { LoyaltyProgram = esfera, TotalPendingPoints = 5000, PendingTransactions = esferaTransactions }
            ],
            Others = new DashboardOthersCardDto
            {
                TotalPendingPoints = 2500,
                // Fix 7: transações Outros com fonte especificada para demonstrar agrupamento
                PendingTransactions =
                [
                    CreateTransaction(99, "Fone Bluetooth", "Magazine Luiza", "Meliuz", 2500, new DateOnly(2025, 7, 10)),
                    CreateTransaction(99, "Tênis Running", "Netshoes", "Meliuz", 1200, new DateOnly(2025, 7, 15)),
                    CreateTransaction(99, "Notebook", "Amazon", "Ame Digital", 8000, new DateOnly(2025, 7, 20)),
                ]
            }
        };

        ApplyDashboardSummary(summary);
    }

    private static List<TransactionDto> CreateMockTransactions(int programId, int count)
    {
        var transactions = new List<TransactionDto>();
        for (int i = 0; i < count; i++)
        {
            transactions.Add(CreateTransaction(programId, "Tv", "Casas Bahia", null, 10_000, new DateOnly(2026, 8, 19)));
            transactions.Add(CreateTransaction(programId, "PC", "Magalu", null, 10_000, new DateOnly(2024, 8, 19)));
        }
        return transactions;
    }

    private static TransactionDto CreateTransaction(int programId, string description, string store, string? source, int points, DateOnly purchaseDate)
    {
        return new TransactionDto
        {
            TransactionId = Guid.NewGuid(),
            LoyaltyProgramId = programId,
            TransactionDescription = description,
            TransactionStore = store,
            TransactionEstimatedPoints = points,
            TransactionPurchaseDate = purchaseDate,
            TransactionStatus = TransactionStatus.Pending
        };
    }

    private void ApplyDashboardSummary(GetDashboardSummaryResponseDto summary)
    {
        Cards.Clear();

        foreach (DashboardProgramCardDto programCard in summary.TopPrograms)
            Cards.Add(DashboardCardItem.FromProgramCard(programCard));

        if (summary.Others.PendingTransactions.Count > 0 || summary.Others.TotalPendingPoints > 0)
            Cards.Add(DashboardCardItem.FromOthersCard(summary.Others));

        if (Cards.Count > 0)
            UpdateSelectedCard(Cards[0]);
    }

    // Fix 2: Loop infinito — o índice agora faz wrap usando módulo
    private void OnCardSwiped(object sender, ItemSwipedEventArgs e)
    {
        if (e.Item is DashboardCardItem)
        {
            var nextIndex = (e.Index + 1) % Cards.Count;
            UpdateSelectedCard(Cards[nextIndex]);
        }
    }

    private void UpdateSelectedCard(DashboardCardItem card)
    {
        // Fix 6: label sem count
        ExpectedCountLabel.Text = "Transações esperadas:";

        // Fix 7: se for card Outros, exibe lista agrupada; senão, exibe lista simples
        if (card.IsOthersCard && card.Groups.Count > 0)
        {
            CurrentTransactions.Clear();
            CurrentGroups.Clear();

            foreach (var group in card.Groups)
            {
                foreach (var item in group.Transactions)
                {
                    item.AccentColor = card.PrimaryColor;
                    item.IsCensored = _isCensored;
                }
                CurrentGroups.Add(group);
            }

            HasGroups = true;
            TransactionsList.IsVisible = false;
            GroupedTransactionsList.IsVisible = true;
        }
        else
        {
            CurrentGroups.Clear();
            CurrentTransactions.Clear();

            foreach (DashboardTransactionItem item in card.Transactions)
            {
                item.AccentColor = card.PrimaryColor;
                item.IsCensored = _isCensored;
                CurrentTransactions.Add(item);
            }

            HasGroups = false;
            TransactionsList.IsVisible = true;
            GroupedTransactionsList.IsVisible = false;
        }
    }

    // Fix 1: Animação de fade + troca de ícone ao censurar
    private async void OnToggleVisibilityTapped(object sender, TappedEventArgs e)
    {
        _isCensored = !_isCensored;

        // Animação: fade out, troca ícone, fade in em todos os cards
        var eyeFadeTasks = Cards.Select(card =>
        {
            card.IsCensored = _isCensored;
            return Task.CompletedTask;
        });
        await Task.WhenAll(eyeFadeTasks);

        foreach (var transaction in CurrentTransactions)
            transaction.IsCensored = _isCensored;

        foreach (var group in CurrentGroups)
            foreach (var transaction in group.Transactions)
                transaction.IsCensored = _isCensored;
    }

    private async void OnViewPastTransactionsTapped(object sender, TappedEventArgs e)
    => await DisplayAlert("Em breve", "Histórico ainda não implementado.", "OK");

    private async void OnCardTapped(object sender, TappedEventArgs e)
    {
        // Navega para a tela de reorganizar passando por cima das abas
        await Shell.Current.GoToAsync("reorder-programs");
    }

}

// Fix 7: grupo de transações para a seção Outros
public sealed class DashboardTransactionGroup
{
    public required string GroupName { get; init; }
    public required List<DashboardTransactionItem> Transactions { get; init; }
}

public sealed class DashboardCardItem : INotifyPropertyChanged
{
    public required string ProgramName { get; init; }
    public required string LogoUrl { get; init; }
    public required Color PrimaryColor { get; init; }
    public required Color SecondaryColor { get; init; }
    public required decimal TotalPendingPoints { get; init; }
    public required List<DashboardTransactionItem> Transactions { get; init; }
    // Fix 7: grupos para o card Outros
    public List<DashboardTransactionGroup> Groups { get; init; } = [];
    public bool IsOthersCard { get; init; }

    private bool _isCensored;
    public bool IsCensored
    {
        get => _isCensored;
        set
        {
            _isCensored = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FormattedPoints)));
            // Fix 1: notifica as propriedades dos ícones
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEyeOpenVisible)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEyeClosedVisible)));
        }
    }

    public string FormattedPoints => IsCensored ? "••••••" : FormatPoints(TotalPendingPoints);
    public bool HasLogo => !IsOthersCard && !string.IsNullOrWhiteSpace(LogoUrl);
    public bool ShowProgramName => IsOthersCard || string.IsNullOrWhiteSpace(LogoUrl);

    // Fix 1: propriedades para controlar qual ícone de olho exibir
    public bool IsEyeOpenVisible => !IsCensored;
    public bool IsEyeClosedVisible => IsCensored;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static DashboardCardItem FromProgramCard(DashboardProgramCardDto card)
    {
        LoyaltyProgramDto program = card.LoyaltyProgram;
        return new DashboardCardItem
        {
            ProgramName = program.LoyaltyProgramName,
            LogoUrl = AppConstants.ResolveStorageUrl(program.LoyaltyProgramLogoUrl),
            PrimaryColor = ParseColor(program.LoyaltyProgramBrandPrimaryColor, "#3A6B4A"),
            SecondaryColor = ParseColor(program.LoyaltyProgramBrandSecondaryColor, "#2A5138"),
            TotalPendingPoints = card.TotalPendingPoints,
            Transactions = card.PendingTransactions.Select(DashboardTransactionItem.FromDto).ToList(),
            IsOthersCard = false
        };
    }

    // Fix 7: FromOthersCard agrupa por fonte (TransactionSource)
    public static DashboardCardItem FromOthersCard(DashboardOthersCardDto card)
    {
        var allItems = card.PendingTransactions.Select(DashboardTransactionItem.FromDto).ToList();

        // Agrupa por fonte; transações sem fonte ficam num grupo "Outros"
        var groups = allItems
            .GroupBy(t => string.IsNullOrWhiteSpace(t.Source) ? "Outros" : t.Source)
            .Select(g => new DashboardTransactionGroup
            {
                GroupName = g.Key,
                Transactions = g.ToList()
            })
            .ToList();

        return new DashboardCardItem
        {
            ProgramName = "Outros",
            LogoUrl = string.Empty,
            PrimaryColor = Color.FromArgb("#6B6B6B"),
            SecondaryColor = Color.FromArgb("#4A4A4A"),
            TotalPendingPoints = card.TotalPendingPoints,
            Transactions = allItems,  // lista plana para fallback
            Groups = groups,
            IsOthersCard = true
        };
    }

    private static Color ParseColor(string? hex, string fallback)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Color.FromArgb(fallback);
        try { return Color.FromArgb(hex.StartsWith('#') ? hex : $"#{hex}"); }
        catch { return Color.FromArgb(fallback); }
    }

    private static string FormatPoints(decimal points) => points.ToString("N0", CultureInfo.GetCultureInfo("pt-BR"));
}

public sealed class DashboardTransactionItem : INotifyPropertyChanged
{
    public required string Description { get; init; }
    public required string Store { get; init; }
    public required int EstimatedPoints { get; init; }
    public bool IsOverdue { get; init; }
    public Color OverdueBorderColor => IsOverdue ? Color.FromArgb("#C0392B") : Colors.Transparent;
    public double OverdueThickness => IsOverdue ? 2 : 0;

    public required DateOnly PurchaseDate { get; init; }
    // Fix 7: fonte da transação (ex: "Meliuz", "Ame Digital")
    public string? Source { get; init; }

    // Fix 7: exibe "Loja • Fonte" quando há fonte diferente, ou só "Loja"
    public string StoreDisplay => string.IsNullOrWhiteSpace(Source) ? Store : $"{Store} • {Source}";

    private Color _accentColor = Color.FromArgb("#C0392B");
    public Color AccentColor
    {
        get => _accentColor;
        set { _accentColor = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccentColor))); }
    }

    private bool _isCensored;
    public bool IsCensored
    {
        get => _isCensored;
        set
        {
            _isCensored = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FormattedPoints)));
        }
    }

    public string FormattedPoints => IsCensored ? "••••" : $"{EstimatedPoints.ToString("N0", CultureInfo.GetCultureInfo("pt-BR"))} pts";
    public string FormattedDate => PurchaseDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));

    public event PropertyChangedEventHandler? PropertyChanged;

    public static DashboardTransactionItem FromDto(TransactionDto dto)
    {
        return new DashboardTransactionItem
        {
            Description = dto.TransactionDescription,
            Store = dto.TransactionStore,
            EstimatedPoints = dto.TransactionEstimatedPoints,
            PurchaseDate = dto.TransactionPurchaseDate,
            IsOverdue = dto.TransactionPurchaseDate < DateOnly.FromDateTime(DateTime.Now)
        };
    }
}

