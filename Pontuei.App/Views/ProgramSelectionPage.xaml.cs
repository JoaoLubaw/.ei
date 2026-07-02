using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Pontuei.App.Services;
using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.App.Views;

/// <summary>
/// Modo de operação da tela: onboarding (multi-seleção + persiste via API,
/// navega pra home) ou filter (seleção única, não persiste nada, apenas
/// devolve o programa escolhido pra quem chamou a tela).
/// </summary>
public enum ProgramSelectionMode
{
    Onboarding,
    Filter
}

public partial class ProgramSelectionPage : ContentPage, IQueryAttributable
{
    // ── Serviços ──────────────────────────────────────────────────────────
    private readonly LoyaltyProgramsApiService _loyaltyProgramsApi;

    // ── Modo / parâmetros de navegação ─────────────────────────────────── 
    private ProgramSelectionMode _mode = ProgramSelectionMode.Onboarding;
    private int? _preSelectedProgramId;

    // ── Estado de seleção ─────────────────────────────────────────────────
    // Ordem em que os itens foram selecionados (o mais antigo é removido
    // primeiro ao estourar o limite). Não depende do Border visual — assim
    // também funciona para pré-seleção vinda por navegação, antes de o
    // CollectionView ter realizado os itens na tela.
    private readonly List<ProgramSelectItem> _selectionOrder = [];
    private bool _isOtherSelected;

    // ── Estado de carregamento ────────────────────────────────────────────
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotLoading));
        }
    }
    public bool IsNotLoading => !_isLoading;

    // ── Textos contextuais (mudam conforme o modo) ───────────────────────
    // Substitua as suas propriedades automáticas por estas:

    private string _headerEyebrow = "A gente precisa saber...";
    public string HeaderEyebrow
    {
        get => _headerEyebrow;
        set { _headerEyebrow = value; OnPropertyChanged(); }
    }

    private string _headerTitle = "Quais os seus principais sistemas de pontuação atualmente?";
    public string HeaderTitle
    {
        get => _headerTitle;
        set { _headerTitle = value; OnPropertyChanged(); }
    }

    private string _continueButtonText = "Continuar";
    public string ContinueButtonText
    {
        get => _continueButtonText;
        set { _continueButtonText = value; OnPropertyChanged(); }
    }

    private string _otherOptionLabel = "Outro";
    public string OtherOptionLabel
    {
        get => _otherOptionLabel;
        set { _otherOptionLabel = value; OnPropertyChanged(); }
    }

    private bool _isHeaderVisible = true;
    public bool IsHeaderVisible
    {
        get => _isHeaderVisible;
        set { _isHeaderVisible = value; OnPropertyChanged(); }
    }

    // ── Coleção bindada ───────────────────────────────────────────────────
    public ObservableCollection<ProgramSelectItem> ProgramItems { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ── Construtor ────────────────────────────────────────────────────────
    public ProgramSelectionPage(LoyaltyProgramsApiService loyaltyProgramsApi)
    {
        InitializeComponent();
        _loyaltyProgramsApi = loyaltyProgramsApi;
    }

    // ════════════════════════════════════════════════════════════════════════
    // PARÂMETROS DE NAVEGAÇÃO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Recebe o modo ("Onboarding" | "Filter") e, no modo filtro, o programa
    /// atualmente selecionado (para vir pré-marcado na grade). Chamado pelo
    /// Shell ANTES de OnAppearing, então o LoadProgramsAsync já enxerga esse
    /// estado quando roda.
    /// </summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // 1. Buscamos a chave ignorando maiúsculas/minúsculas para blindar o código
        var modeKey = query.Keys.FirstOrDefault(k => k.Equals("mode", StringComparison.OrdinalIgnoreCase));
        if (modeKey != null)
        {
            var modeValue = query[modeKey];
            if (modeValue is ProgramSelectionMode enumMode)
            {
                _mode = enumMode;
            }
            else if (modeValue?.ToString() is string modeStr && Enum.TryParse<ProgramSelectionMode>(modeStr, true, out var parsedMode))
            {
                _mode = parsedMode;
            }
        }

        var idKey = query.Keys.FirstOrDefault(k => k.Equals("selectedProgramId", StringComparison.OrdinalIgnoreCase));
        if (idKey != null)
        {
            var idValue = query[idKey];
            if (idValue is int id)
            {
                _preSelectedProgramId = id;
            }
            else if (idValue?.ToString() is string idStr && int.TryParse(idStr, out int parsedId))
            {
                _preSelectedProgramId = parsedId;
            }
        }

        // 2. Ajustamos os textos dependendo do modo (Botão sempre como "Outro")
        if (_mode == ProgramSelectionMode.Filter)
        {
            HeaderEyebrow = "Filtrar histórico";
            HeaderTitle = "Qual programa você quer visualizar?";
            ContinueButtonText = "Aplicar filtro";
        }
        else
        {
            HeaderEyebrow = "A gente precisa saber...";
            HeaderTitle = "Quais os seus principais sistemas de pontuação atualmente?";
            ContinueButtonText = "Continuar";
        }

        OtherOptionLabel = "Outro";
        IsHeaderVisible = true;

        BindingContext = null;
        BindingContext = this;
    }

    // ════════════════════════════════════════════════════════════════════════
    // LIFE CYCLE
    // ════════════════════════════════════════════════════════════════════════

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProgramsAsync();
    }

    // ════════════════════════════════════════════════════════════════════════
    // CARREGAMENTO DE PROGRAMAS VIA API
    // ════════════════════════════════════════════════════════════════════════

    private async Task LoadProgramsAsync()
    {
        IsLoading = true;
        ProgramItems.Clear();
        _selectionOrder.Clear();
        SetOtherSelected(false);

        try
        {
            // Busca todos os programas ativos do catálogo (sem filtro de usuário).
            // A API pagina de 100 em 100 — improvável passar disso no catálogo,
            // mas se necessário implemente paginação incremental aqui.
            ApiResponse<GetLoyaltyProgramsResponseDto> response =
                await _loyaltyProgramsApi.GetAllProgramsAsync(
                    new GetLoyaltyProgramsRequestDto
                    {
                        Page = 1,
                        Size = 100,
                        Filters = new LoyaltyProgramFiltersDto { LoyaltyProgramIsActive = true }
                    });

            if (response.IsSuccess && response.Data?.LoyaltyPrograms is { } programs)
            {
                foreach (LoyaltyProgramDto program in programs)
                {
                    // Programas sem logo não entram na grade — evita células brancas.
                    // O usuário pode usar a opção "Outro"/"Exibir todos" para esses casos.
                    if (string.IsNullOrWhiteSpace(program.LoyaltyProgramLogoUrl))
                        continue;

                    ProgramItems.Add(new ProgramSelectItem
                    {
                        Program = program,
                        LogoUrl = AppConstants.ResolveStorageUrl(program.LoyaltyProgramLogoUrl)
                    });
                }

                ApplyPreSelection();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ProgramSelectionPage] Falha ao carregar programas: {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ProgramSelectionPage] LoadProgramsAsync erro: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Aplica o estado inicial de seleção vindo da navegação: marca o programa
    /// já selecionado (se houver) ou, no modo filtro sem programa nenhum
    /// (equivale a "Todos"), marca a opção "Exibir todos".
    /// </summary>
    private void ApplyPreSelection()
    {
        if (_preSelectedProgramId.HasValue)
        {
            ProgramSelectItem? match = ProgramItems
                .FirstOrDefault(p => p.Program.LoyaltyProgramId == _preSelectedProgramId.Value);

            if (match != null)
            {
                match.IsSelected = true;
                _selectionOrder.Add(match);
            }
        }
        else if (_mode == ProgramSelectionMode.Filter)
        {
            // Sem programa específico no filtro atual = "Exibir todos"
            SetOtherSelected(true);
        }

        UpdateContinueButton();
    }

    // ════════════════════════════════════════════════════════════════════════
    // SELEÇÃO DE PROGRAMAS
    // ════════════════════════════════════════════════════════════════════════

    private async void OnProgramTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not Border border) return;
        if (border.BindingContext is not ProgramSelectItem item) return;

        // No modo filtro a seleção é única; no onboarding, respeita o limite normal.
        int effectiveMax = _mode == ProgramSelectionMode.Filter ? 1 : AppConstants.MaxProgramSelection;

        if (!item.IsSelected)
        {
            // Selecionar um programa cancela a opção "Outro"/"Exibir todos"
            if (_isOtherSelected) SetOtherSelected(false);

            // Atingiu o limite: desseleciona o(s) mais antigo(s).
            // A cor da borda é controlada por DataTrigger (Border.Triggers no XAML),
            // então só precisamos atualizar o IsSelected do modelo.
            while (_selectionOrder.Count >= effectiveMax)
            {
                ProgramSelectItem oldest = _selectionOrder[0];
                _selectionOrder.RemoveAt(0);
                oldest.IsSelected = false;
            }

            item.IsSelected = true;
            _selectionOrder.Add(item);

            await border.ScaleTo(0.92, 100, Easing.CubicOut);
            await border.ScaleTo(1.0, 150, Easing.SpringOut);
        }
        else
        {
            item.IsSelected = false;
            _selectionOrder.Remove(item);
        }

        UpdateContinueButton();
    }

    private void OnOtherTapped(object? sender, TappedEventArgs e)
    {
        if (_isOtherSelected)
        {
            SetOtherSelected(false);
        }
        else
        {
            ClearProgramSelections();
            SetOtherSelected(true);
        }

        UpdateContinueButton();
    }

    private void SetOtherSelected(bool selected)
    {
        _isOtherSelected = selected;
        OtherOptionBorder.Stroke = selected
            ? new SolidColorBrush(Color.FromArgb("#3A6B4A"))
            : new SolidColorBrush(Colors.Transparent);
    }

    private void ClearProgramSelections()
    {
        foreach (ProgramSelectItem item in ProgramItems)
            item.IsSelected = false;
        _selectionOrder.Clear();
    }

    private void UpdateContinueButton()
    {
        // No modo filtro, não selecionar nada é uma opção válida (= "Todos",
        // sem filtro de programa no request), então o botão nunca fica travado.
        ContinueButton.IsEnabled = _mode == ProgramSelectionMode.Filter
            || _selectionOrder.Count > 0
            || _isOtherSelected;
    }

    // ════════════════════════════════════════════════════════════════════════
    // SALVAR/DEVOLVER E NAVEGAR
    // ════════════════════════════════════════════════════════════════════════

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        ContinueButton.IsEnabled = false;

        if (_mode == ProgramSelectionMode.Filter)
        {
            await ApplyFilterSelectionAsync();
            return;
        }

        Guid? userId = AuthService.CurrentUserId;

        if (userId is not null && _selectionOrder.Count > 0)
        {
            var programsPayload = _selectionOrder
                .Select((item, index) => new CreateUserLoyaltyProgramRequestDto
                {
                    LoyaltyProgramId = item.Program.LoyaltyProgramId,
                    DisplayOrder = (short)index
                })
                .ToList();

            var bulkRequest = new BulkUpdateUserLoyaltyProgramsRequestDto
            {
                Programs = programsPayload
            };

            ApiResponse<GetUserLoyaltyProgramsResponseDto> result =
                await _loyaltyProgramsApi.BulkUpdateUserProgramsAsync(userId.Value, bulkRequest);

            if (!result.IsSuccess)
            {
                // Falha não-bloqueante: o usuário chega na home e o dashboard
                // tentará carregar. O guard não rodará novamente (MarkProgramCheckDone
                // é chamado abaixo de qualquer forma para evitar loop).
                System.Diagnostics.Debug.WriteLine(
                    $"[ProgramSelectionPage] BulkUpdate falhou: {result.ErrorMessage}");
            }
        }

        // Informa à HomePage que o guard não precisa repetir.
        // Evita o loop: home → program-selection → home → program-selection
        // caso a chamada acima falhe por algum motivo de rede.
        HomePage.MarkProgramCheckDone();

        await Shell.Current.GoToAsync("//home");
    }

    /// <summary>
    /// Modo filtro: NÃO chama a API de bulk update (isso persistiria os
    /// programas do usuário, o que não faz sentido pra um filtro de tela).
    /// Apenas devolve o programa escolhido — ou "Todos" — pra quem navegou
    /// pra cá, usando o padrão de retorno de valor do Shell (GoToAsync("..", params)
    /// entrega os parâmetros via IQueryAttributable da página anterior).
    /// </summary>
    private async Task ApplyFilterSelectionAsync()
    {
        ProgramSelectItem? selected = _selectionOrder.Count > 0 ? _selectionOrder[0] : null;

        var resultParams = new Dictionary<string, object>();

        if (selected is null || _isOtherSelected)
        {
            resultParams["selectedProgramId"] = null!;
            resultParams["selectedProgramName"] = "Todos";
            resultParams["selectedProgramLogo"] = string.Empty;
            resultParams["selectedProgramPrimaryColor"] = string.Empty;
            resultParams["selectedProgramSecondaryColor"] = string.Empty;
        }
        else
        {
            resultParams["selectedProgramId"] = selected.Program.LoyaltyProgramId;
            resultParams["selectedProgramName"] = selected.Program.LoyaltyProgramName;
            resultParams["selectedProgramLogo"] = selected.LogoUrl;
            resultParams["selectedProgramPrimaryColor"] = selected.Program.LoyaltyProgramBrandPrimaryColor ?? string.Empty;
            resultParams["selectedProgramSecondaryColor"] = selected.Program.LoyaltyProgramBrandSecondaryColor ?? string.Empty;
        }

        await Shell.Current.GoToAsync("..", resultParams);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// VIEW MODEL
// ════════════════════════════════════════════════════════════════════════════

public sealed class ProgramSelectItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public required LoyaltyProgramDto Program { get; init; }
    public required string LogoUrl { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
