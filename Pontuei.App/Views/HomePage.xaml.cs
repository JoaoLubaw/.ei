using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using PanCardView.EventArgs;
using Pontuei.App.Services;
using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Shared.Enums;

namespace Pontuei.App.Views;

public partial class HomePage : BasePage, INotifyPropertyChanged
{
    // ── Serviços ──────────────────────────────────────────────────────────
    private readonly TransactionApiService _transactionApi;
    private readonly LoyaltyProgramsApiService _loyaltyProgramsApi;

    // ── Guard de onboarding ───────────────────────────────────────────────
    // static: sobrevive à re-criação da página pelo Shell sem fazer nova
    // chamada de rede a cada retorno à home.
    private static bool _programCheckDone;

    // ── Estado de censura ─────────────────────────────────────────────────
    private bool _isCensored;

    // ── Estado de carregamento ────────────────────────────────────────────
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
    }
    public bool IsNotLoading => !_isLoading;

    // ── Coleções bindadas ─────────────────────────────────────────────────
    public ObservableCollection<DashboardCardItem> Cards { get; } = [];
    public ObservableCollection<DashboardTransactionItem> CurrentTransactions { get; } = [];
    public ObservableCollection<DashboardTransactionGroup> CurrentGroups { get; } = [];

    // ── HasGroups / HasSimpleList ─────────────────────────────────────────
    private bool _hasGroups;
    public bool HasGroups
    {
        get => _hasGroups;
        set
        {
            _hasGroups = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSimpleList));
        }
    }
    public bool HasSimpleList => !_hasGroups;

    // ── INotifyPropertyChanged ────────────────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ── Construtor ────────────────────────────────────────────────────────
    public HomePage(TransactionApiService transactionApi,
                    LoyaltyProgramsApiService loyaltyProgramsApi)
    {
        try
        {
            InitializeComponent();
            BindingContext = this;
            _transactionApi = transactionApi;
            _loyaltyProgramsApi = loyaltyProgramsApi;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO HomePage] {ex}");
            throw;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // CICLO DE VIDA
    // ════════════════════════════════════════════════════════════════════════

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.Home, animate: false);

        // Guard de onboarding — só roda uma vez por sessão de app
        if (!_programCheckDone)
        {
            _programCheckDone = true;
            bool hasProgramas = await CheckUserProgramsAsync();
            if (!hasProgramas) return;   // navegou para program-selection; para aqui
        }

        // Carrega o dashboard real a cada vez que a tela aparece, garantindo
        // que adições/remoções de transações se reflitam ao voltar para a home.
        await LoadDashboardAsync();
    }

    // ════════════════════════════════════════════════════════════════════════
    // GUARD DE ONBOARDING
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifica se o usuário já tem programas vinculados.
    /// Retorna <c>true</c> se tem (ou se a chamada falhou — vai para a home mesmo assim).
    /// Retorna <c>false</c> se não tem — navega para a seleção de programas.
    /// </summary>
    private async Task<bool> CheckUserProgramsAsync()
    {
        Guid? userId = AuthService.CurrentUserId;
        if (userId is null) return true;

        try
        {
            var response = await _loyaltyProgramsApi.GetUserProgramsAsync(
                userId.Value,
                new Pontuei.Shared.Dtos.Requests.GetUserLoyaltyProgramsRequestDto
                {
                    Page = 1,
                    Size = 1   // só precisamos saber se existe ao menos um
                });

            if (response.IsSuccess && response.Data?.TotalElements == 0)
            {
                await Shell.Current.GoToAsync("program-selection", animate: false);
                return false;
            }
        }
        catch (Exception ex)
        {
            // Falha silenciosa — não bloqueia o acesso à home
            System.Diagnostics.Debug.WriteLine($"[HomePage] CheckUserProgramsAsync falhou: {ex.Message}");
        }

        return true;
    }

    // ════════════════════════════════════════════════════════════════════════
    // CARREGAMENTO DO DASHBOARD VIA API
    // ════════════════════════════════════════════════════════════════════════

    private async Task LoadDashboardAsync()
    {
        Guid? userId = AuthService.CurrentUserId;
        if (userId is null) return;

        IsLoading = true;
        Cards.Clear();
        CurrentTransactions.Clear();
        CurrentGroups.Clear();

        try
        {
            ApiResponse<GetDashboardSummaryResponseDto> response =
                await _transactionApi.GetDashboardSummaryAsync(userId.Value);

            if (response.IsSuccess && response.Data != null)
            {
                ApplyDashboardSummary(response.Data);

                if (Cards.Count == 0)
                {
                    await Shell.Current.GoToAsync("program-selection", animate: false);
                    return;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[HomePage] Dashboard falhou: {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomePage] LoadDashboardAsync erro: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
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

    // ════════════════════════════════════════════════════════════════════════
    // INTERAÇÕES DO CAROUSEL
    // ════════════════════════════════════════════════════════════════════════

    private void OnCardSwiped(object sender, ItemSwipedEventArgs e)
    {
        if (e.Item is DashboardCardItem)
        {
            int nextIndex = (e.Index + 1) % Cards.Count;
            UpdateSelectedCard(Cards[nextIndex]);
        }
    }

    private void UpdateSelectedCard(DashboardCardItem card)
    {
        ExpectedCountLabel.Text = "Transações esperadas:";

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

    // ════════════════════════════════════════════════════════════════════════
    // HANDLERS DE UI
    // ════════════════════════════════════════════════════════════════════════

    private async void OnToggleVisibilityTapped(object sender, TappedEventArgs e)
    {
        _isCensored = !_isCensored;

        foreach (var card in Cards)
            card.IsCensored = _isCensored;

        foreach (var transaction in CurrentTransactions)
            transaction.IsCensored = _isCensored;

        foreach (var group in CurrentGroups)
            foreach (var transaction in group.Transactions)
                transaction.IsCensored = _isCensored;

        await Task.CompletedTask;
    }

    private async void OnViewPastTransactionsTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//history");

    private async void OnCardTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("reorder-programs");

    private async void OnTransactionTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not DashboardTransactionItem item) return;

        var navParams = new Dictionary<string, object>
        {
            { "transactionId", item.TransactionId.ToString() }
        };

        await Shell.Current.GoToAsync("transaction-detail", navParams);
    }

    // ════════════════════════════════════════════════════════════════════════
    // HELPERS ESTÁTICOS (chamados por ProgramSelectionPage)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Chamado pela <see cref="ProgramSelectionPage"/> após salvar os programas,
    /// para que a home não repita o guard imediatamente.
    /// </summary>
    public static void MarkProgramCheckDone() => _programCheckDone = true;

    /// <summary>
    /// Deve ser chamado no logout para que o próximo usuário
    /// passe pelo guard normalmente.
    /// </summary>
    public static void ResetProgramCheck() => _programCheckDone = false;
}

// ════════════════════════════════════════════════════════════════════════════
// VIEW MODELS (DashboardCardItem, DashboardTransactionItem, etc.)
// ════════════════════════════════════════════════════════════════════════════

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEyeOpenVisible)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEyeClosedVisible)));
        }
    }

    public string FormattedPoints => IsCensored
        ? "••••••"
        : TotalPendingPoints.ToString("N0", CultureInfo.GetCultureInfo("pt-BR"));

    public bool HasLogo => !IsOthersCard && !string.IsNullOrWhiteSpace(LogoUrl);
    public bool ShowProgramName => IsOthersCard || string.IsNullOrWhiteSpace(LogoUrl);
    public bool IsEyeOpenVisible => !IsCensored;
    public bool IsEyeClosedVisible => IsCensored;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ── Factories ─────────────────────────────────────────────────────────

    public static DashboardCardItem FromProgramCard(DashboardProgramCardDto card)
    {
        LoyaltyProgramDto p = card.LoyaltyProgram;
        return new DashboardCardItem
        {
            ProgramName = p.LoyaltyProgramName,
            LogoUrl = AppConstants.ResolveStorageUrl(p.LoyaltyProgramLogoUrl),
            PrimaryColor = ParseColor(p.LoyaltyProgramBrandPrimaryColor, "#3A6B4A"),
            SecondaryColor = ParseColor(p.LoyaltyProgramBrandSecondaryColor, "#2A5138"),
            TotalPendingPoints = card.TotalPendingPoints,
            Transactions = card.PendingTransactions
                .Select(DashboardTransactionItem.FromDto)
                .ToList(),
            IsOthersCard = false
        };
    }

    public static DashboardCardItem FromOthersCard(DashboardOthersCardDto card)
    {
        List<DashboardTransactionItem> allItems = card.PendingTransactions
            .Select(DashboardTransactionItem.FromDto)
            .ToList();

        List<DashboardTransactionGroup> groups = allItems
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
            Transactions = allItems,
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
}

public sealed class DashboardTransactionItem : INotifyPropertyChanged
{
    public Guid TransactionId { get; init; }
    public required string Description { get; init; }
    public required string Store { get; init; }
    public required int EstimatedPoints { get; init; }
    public bool IsOverdue { get; init; }
    public required DateOnly PurchaseDate { get; init; }
    public string? Source { get; init; }   // nome do programa — usado no card "Outros"

    public Color OverdueBorderColor => IsOverdue ? Color.FromArgb("#C0392B") : Colors.Transparent;
    public double OverdueThickness => IsOverdue ? 2 : 0;
    public string StoreDisplay => string.IsNullOrWhiteSpace(Source) ? Store : $"{Store} • {Source}";
    public string FormattedDate => PurchaseDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));

    private Color _accentColor = Color.FromArgb("#3A6B4A");
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

    public string FormattedPoints => IsCensored
        ? "••••"
        : $"{EstimatedPoints.ToString("N0", CultureInfo.GetCultureInfo("pt-BR"))} pts";

    public event PropertyChangedEventHandler? PropertyChanged;

    public static DashboardTransactionItem FromDto(TransactionDto dto) =>
        new()
        {
            TransactionId = dto.TransactionId,
            Description = dto.TransactionDescription,
            Store = dto.TransactionStore,
            EstimatedPoints = dto.TransactionEstimatedPoints,
            PurchaseDate = dto.TransactionPurchaseDate,
            IsOverdue = dto.IsOverdue
        };
}
