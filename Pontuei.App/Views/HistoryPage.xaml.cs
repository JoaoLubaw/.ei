using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Pontuei.App.Services;
using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Shared.Enums;

namespace Pontuei.App.Views;

public partial class HistoryPage : BasePage, INotifyPropertyChanged
{
    private readonly TransactionApiService _transactionApi;

    // ── Estados da Página ─────────────────────────────────────────────────
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
    }
    public bool IsNotLoading => !_isLoading;

    // ── Propriedades do Programa Selecionado ──────────────────────────────
    private string _selectedProgramName = "Todos";
    public string SelectedProgramName
    {
        get => _selectedProgramName;
        set { _selectedProgramName = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowProgramName)); }
    }

    private string _selectedProgramLogo = string.Empty;
    public string SelectedProgramLogo
    {
        get => _selectedProgramLogo;
        set { _selectedProgramLogo = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasProgramLogo)); }
    }

    private Color _selectedProgramPrimaryColor = Color.FromArgb("#6B6B6B");
    public Color SelectedProgramPrimaryColor
    {
        get => _selectedProgramPrimaryColor;
        set { _selectedProgramPrimaryColor = value; OnPropertyChanged(); }
    }

    private Color _selectedProgramSecondaryColor = Color.FromArgb("#4A4A4A");
    public Color SelectedProgramSecondaryColor
    {
        get => _selectedProgramSecondaryColor;
        set { _selectedProgramSecondaryColor = value; OnPropertyChanged(); }
    }

    public bool HasProgramLogo => !string.IsNullOrWhiteSpace(SelectedProgramLogo);
    public bool ShowProgramName => string.IsNullOrWhiteSpace(SelectedProgramLogo);

    // ── Coleções ──────────────────────────────────────────────────────────
    public ObservableCollection<StatusFilterItem> Filters { get; } = [];
    public ObservableCollection<HistoryTransactionItem> Transactions { get; } = [];

    // Se você tiver um ID de programa selecionado atualmente
    private int? _currentLoyaltyProgramId = null;

    // ── INotifyPropertyChanged ────────────────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public HistoryPage(TransactionApiService transactionApi)
    {
        InitializeComponent();
        BindingContext = this;
        _transactionApi = transactionApi;

        SetupFilters();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.History, animate: false);

        // Carrega as transações ao entrar na tela
        await LoadTransactionsAsync();
    }

    private void SetupFilters()
    {
        Filters.Clear();
        // Inicializa os filtros. Você pode mudar qual começa selecionado ajustando IsSelected
        Filters.Add(new StatusFilterItem { Name = "Atrasadas", Color = Color.FromArgb("#C0392B"), IsSelected = false });
        Filters.Add(new StatusFilterItem { Name = "Recebidas", Color = Color.FromArgb("#4E8A61"), IsSelected = false });
        Filters.Add(new StatusFilterItem { Name = "Canceladas", Color = Color.FromArgb("#6B6B6B"), IsSelected = false });
    }

    // ── Integração com API ────────────────────────────────────────────────

    private async Task LoadTransactionsAsync()
    {
        Guid? userId = AuthService.CurrentUserId;
        if (userId is null) return;

        IsLoading = true;
        Transactions.Clear();

        try
        {
            // Descobre qual filtro está ativo no momento
            var activeFilter = Filters.FirstOrDefault(f => f.IsSelected)?.Name;

            var request = new GetTransactionsRequestDto
            {
                Page = 1,
                Size = 50, // Ajuste para implementar paginação se necessário
                Filters = new TransactionFiltersDto
                {
                    LoyaltyProgramId = _currentLoyaltyProgramId
                }
            };

            // Aplica os parâmetros de GET com base no botão de status selecionado
            if (activeFilter == "Atrasadas")
            {
                request.Filters.IsOverdue = true;
                // Dependendo da sua regra de negócio, pode precisar filtrar por TransactionStatus.Pending também
            }
            else if (activeFilter == "Recebidas")
            {
                // Assumindo que o enum possui o valor Received (como visto em UpdateTransactionStatusRequestDto)
                if (Enum.TryParse<TransactionStatus>("Received", out var receivedStatus))
                    request.Filters.TransactionStatus = receivedStatus;
            }
            else if (activeFilter == "Canceladas")
            {
                // Ajuste o nome do Enum para o que representa "Cancelado" no seu back-end (ex: Cancelled, Disputed)
                if (Enum.TryParse<TransactionStatus>("Cancelled", out var cancelledStatus))
                    request.Filters.TransactionStatus = cancelledStatus;
            }

            var response = await _transactionApi.GetTransactionsAsync(userId.Value, request);

            if (response.IsSuccess && response.Data != null)
            {
                foreach (var dto in response.Data.Transactions)
                {
                    Transactions.Add(new HistoryTransactionItem
                    {
                        TransactionId = dto.TransactionId,
                        Description = dto.TransactionDescription,
                        Store = dto.TransactionStore,
                        ProgramLogo = "", // Se a API de transação não retorna a logo, você precisará preencher via cache ou relacionamento
                        Points = dto.TransactionEstimatedPoints,
                        ExpectedDate = dto.Deadline,
                        ReceivedDate = dto.TransactionItemReceiptDate,
                        // Define a cor de status do card baseada no status/overdue
                        StatusColor = GetColorForTransaction(dto)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryPage] Erro ao carregar transações: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Color GetColorForTransaction(TransactionDto dto)
    {
        if (dto.IsOverdue) return Color.FromArgb("#C0392B"); // Vermelho
        if (dto.TransactionStatus.ToString() == "Received") return Color.FromArgb("#4E8A61"); // Verde
        return Color.FromArgb("#6B6B6B"); // Cinza padrão
    }

    // Método para ser chamado quando o usuário selecionar um programa no Switcher
    public void UpdateSelectedProgram(int? programId, string name, string logoUrl, string primaryHex, string secondaryHex)
    {
        _currentLoyaltyProgramId = programId;
        SelectedProgramName = name;
        SelectedProgramLogo = logoUrl;

        SelectedProgramPrimaryColor = string.IsNullOrWhiteSpace(primaryHex)
            ? Color.FromArgb("#6B6B6B")
            : Color.FromArgb(primaryHex.StartsWith('#') ? primaryHex : $"#{primaryHex}");

        SelectedProgramSecondaryColor = string.IsNullOrWhiteSpace(secondaryHex)
            ? Color.FromArgb("#4A4A4A")
            : Color.FromArgb(secondaryHex.StartsWith('#') ? secondaryHex : $"#{secondaryHex}");

        _ = LoadTransactionsAsync();
    }

    // ── Filtros Animados ──────────────────────────────────────────────────

    private async void OnFilterTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Border tappedBorder || e.Parameter is not StatusFilterItem tappedItem) return;

        var stack = (HorizontalStackLayout)tappedBorder.Parent;
        bool wasAlreadySelected = tappedItem.IsSelected;

        // Desmarca todos
        foreach (View child in stack.Children)
        {
            if (child is Border border && border.BindingContext is StatusFilterItem item && item.IsSelected)
            {
                item.IsSelected = false;
                AnimatePill(border, expand: false);
            }
        }

        // Se clicou em um que já estava marcado, ele apenas desmarca (limpa o filtro). 
        // Se for um novo, marca ele.
        if (!wasAlreadySelected)
        {
            tappedItem.IsSelected = true;
            AnimatePill(tappedBorder, expand: true);
        }

        // Dispara requisição com o novo filtro de status
        await LoadTransactionsAsync();
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
    {
        // Aqui você chamaria a página de seleção de programas do histórico
        await DisplayAlert("Em breve", "Seletor de programa.", "OK");
    }

    private async void OnTransactionTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not HistoryTransactionItem item) return;

        var navParams = new Dictionary<string, object>
        {
            { "transactionId", item.TransactionId.ToString() }
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