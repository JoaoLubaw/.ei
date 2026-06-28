using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Pontuei.App.Services;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Shared.Enums;

namespace Pontuei.App.Views;

/// <summary>
/// Modos de operacao da tela. 
/// </summary>
public enum TransactionDetailMode
{
    Create,   // Nova transacao -- formulario editavel + Cancelar/Salvar
    View,     // Visualizacao -- campos somente-leitura + icones lixeira/lapis + botao Atualizar status
    Edit      // Edicao -- formulario editavel (a partir do View) + Cancelar/Salvar
}

/// <summary>
/// Recebe "transactionId" como query parameter via Shell navigation.
/// Quando presente, abre a tela em modo View carregando os dados da transacao.
/// Quando ausente (navegacao direta pelo FAB), abre em modo Create.
/// </summary>
[QueryProperty(nameof(TransactionIdParam), "transactionId")]
[QueryProperty(nameof(SelectedProgramParam), "SelectedProgram")]
public partial class TransactionDetailPage : BasePage, INotifyPropertyChanged
{
    // ── Modo atual ──────────────────────────────────────────────────────── 
    private TransactionDetailMode _mode;

    // ── Dados do formulario ───────────────────────────────────────────────
    private string _description = string.Empty;
    private string _store = string.Empty;
    private decimal _totalValue;
    private DateOnly _purchaseDate = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly _receiptDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    private int _deadlineDays = 30;
    private short _pointsPerReal = 20;
    private DateOnly _statusDate = DateOnly.FromDateTime(DateTime.Today);
    private LoyaltyProgramDto? _selectedProgram;

    // Id da transacao sendo visualizada/editada (null = criacao)
    private Guid? _transactionId;

    // ── QueryProperty receiver ────────────────────────────────────────────
    /// <summary>
    /// Setado pelo Shell quando a rota e chamada com ?transactionId=...
    /// Aceita Guid diretamente (quando passado via Dictionary) ou string (deep link).
    /// </summary>
    public string TransactionIdParam
    {
        set
        {
            if (Guid.TryParse(value, out var id) && id != Guid.Empty)
            {
                LoadTransactionById(id);
            }
        }
    }

    // ── INotifyPropertyChanged ────────────────────────────────────────────
    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ── Propriedades bindadas ─────────────────────────────────────────────

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    public string Store
    {
        get => _store;
        set { _store = value; OnPropertyChanged(); }
    }

    /// <summary>Valor exibido no campo, formatado como "R$ 1.000,56".</summary>
    public string FormattedTotalValue
    {
        get => _totalValue == 0 ? string.Empty : _totalValue.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
        set
        {
            var clean = value
                .Replace("R$", "")
                .Replace(" ", "")
                .Trim();

            if (decimal.TryParse(clean, NumberStyles.Currency, CultureInfo.GetCultureInfo("pt-BR"), out var parsed))
            {
                _totalValue = parsed;
                OnPropertyChanged();
            }
        }
    }

    public string FormattedPurchaseDate
        => _purchaseDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));

    public string FormattedReceiptDate
        => _receiptDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));

    public string FormattedStatusDate
        => _statusDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));

    public string DeadlineDaysDisplay => $"{_deadlineDays} dias";

    public string PointsPerRealDisplay => $"{_pointsPerReal}pts";

    /// <summary>Quando true, campos de texto ficam somente-leitura (modo View).</summary>
    public bool IsReadOnly => _mode == TransactionDetailMode.View;
    public bool IsEditable => !IsReadOnly;

    // ── Construtor: modo Criar ─────────────────────────────────────────────
    public TransactionDetailPage()
    {
        InitializeComponent();
        BindingContext = this;
        SetMode(TransactionDetailMode.Create);

        _selectedProgram = new LoyaltyProgramDto
        {
            LoyaltyProgramId = 3,
            LoyaltyProgramName = "Livelo",
            LoyaltyProgramBrandPrimaryColor = "#E4002B",
            LoyaltyProgramBrandSecondaryColor = "#B8001F",
            LoyaltyProgramIsActive = true
        };
        UpdateProgramPill();
    }

    // ── Construtor: modo Visualizar (chamada direta por codigo) ────────────
    /// <summary>
    /// Abre a tela no modo somente-leitura exibindo os dados de uma transacao existente.
    /// </summary>
    public TransactionDetailPage(TransactionDetailResponseDto dto) : this()
    {
        LoadFromDto(dto);
        SetMode(TransactionDetailMode.View);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.Details, animate: false);
    }

    // ── Carrega por ID (chamado pelo QueryProperty) ────────────────────────
    private void LoadTransactionById(Guid id)
    {
        _transactionId = id;

        // TODO: substituir pelo servico real quando disponivel.
        // Por ora carrega um mock para demonstrar o modo View.
        var mockDto = new TransactionDetailResponseDto
        {
            TransactionId = id,
            TransactionDescription = "Tv",
            TransactionStore = "Casas Bahia",
            TransactionTotalValue = 1000.56m,
            TransactionPurchaseDate = new DateOnly(2026, 6, 19),
            TransactionItemReceiptDate = new DateOnly(2026, 6, 19),
            TransactionReceiptDeadlineDays = 30,
            TransactionPointsPerReal = 20,
            TransactionEstimatedPoints = 20011,
            TransactionActualReceivedPoints = 0,
            TransactionStatus = TransactionStatus.Pending,
            Deadline = new DateOnly(2026, 7, 19),
            IsOverdue = false,
            LoyaltyProgram = new LoyaltyProgramDto
            {
                LoyaltyProgramId = 3,
                LoyaltyProgramName = "Livelo",
                LoyaltyProgramBrandPrimaryColor = "#E4002B",
                LoyaltyProgramBrandSecondaryColor = "#B8001F",
                LoyaltyProgramIsActive = true
            }
        };

        LoadFromDto(mockDto);
        SetMode(TransactionDetailMode.View);
    }

    // ── Logica de modo ─────────────────────────────────────────────────────

    private void SetMode(TransactionDetailMode mode)
    {
        _mode = mode;

        bool isView = mode == TransactionDetailMode.View;
        bool isCreateOrEdit = mode is TransactionDetailMode.Create or TransactionDetailMode.Edit;

        PageTitleLabel.Text = mode switch
        {
            TransactionDetailMode.Create => "Criar nova transacao",
            TransactionDetailMode.View => "Detalhes de transacao",
            TransactionDetailMode.Edit => "Editar transacao",
            _ => string.Empty
        };

        DeleteButton.IsVisible = isView;
        EditButton.IsVisible = isView;

        ReceiptDateLabel.Text = isView ? "DATA DE RECEBIMENTO" : "RECEBIMENTO DE ITENS";

        SetSteppersEnabled(!isView);

        UpdateStatusButton.IsVisible = isView;
        UpdateStatusPanel.IsVisible = false;

        SaveCancelButtons.IsVisible = isCreateOrEdit;

        OnPropertyChanged(nameof(IsReadOnly));
        OnPropertyChanged(nameof(IsEditable));
    }

    private void SetSteppersEnabled(bool enabled)
    {
        var deadlineGrid = DeadlineDaysLabel.Parent as Grid;
        var pointsGrid = PointsPerRealLabel.Parent as Grid;

        if (deadlineGrid != null) deadlineGrid.InputTransparent = !enabled;
        if (pointsGrid != null) pointsGrid.InputTransparent = !enabled;
    }

    // ── Carrega dados do DTO ───────────────────────────────────────────────

    private void LoadFromDto(TransactionDetailResponseDto dto)
    {
        _transactionId = dto.TransactionId;
        Description = dto.TransactionDescription;
        Store = dto.TransactionStore;
        _totalValue = dto.TransactionTotalValue;
        _purchaseDate = dto.TransactionPurchaseDate;
        _receiptDate = dto.TransactionItemReceiptDate ?? dto.TransactionPurchaseDate;
        _deadlineDays = dto.TransactionReceiptDeadlineDays;
        _pointsPerReal = dto.TransactionPointsPerReal;
        _selectedProgram = dto.LoyaltyProgram;

        OnPropertyChanged(nameof(FormattedTotalValue));
        OnPropertyChanged(nameof(FormattedPurchaseDate));
        OnPropertyChanged(nameof(FormattedReceiptDate));
        OnPropertyChanged(nameof(DeadlineDaysDisplay));
        OnPropertyChanged(nameof(PointsPerRealDisplay));
        UpdateProgramPill();
    }

    // ── Pilula do programa ─────────────────────────────────────────────────

    private void UpdateProgramPill()
    {
        if (_selectedProgram == null) return;

        ProgramNameLabel.Text = _selectedProgram.LoyaltyProgramName;

        var primary = ParseColor(_selectedProgram.LoyaltyProgramBrandPrimaryColor, "#E4002B");
        var secondary = ParseColor(_selectedProgram.LoyaltyProgramBrandSecondaryColor, "#B8001F");

        ProgramGradientStart.Color = primary;
        ProgramGradientMid.Color = secondary;
        ProgramGradientEnd.Color = primary;
    }

    private static Color ParseColor(string? hex, string fallback)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Color.FromArgb(fallback);
        try { return Color.FromArgb(hex.StartsWith('#') ? hex : $"#{hex}"); }
        catch { return Color.FromArgb(fallback); }
    }

    // ── Handlers: steppers ────────────────────────────────────────────────

    private void OnDeadlineMinus(object sender, TappedEventArgs e)
    {
        if (_deadlineDays > 1) _deadlineDays--;
        OnPropertyChanged(nameof(DeadlineDaysDisplay));
    }

    private void OnDeadlinePlus(object sender, TappedEventArgs e)
    {
        _deadlineDays++;
        OnPropertyChanged(nameof(DeadlineDaysDisplay));
    }

    private void OnPointsMinus(object sender, TappedEventArgs e)
    {
        if (_pointsPerReal > 1) _pointsPerReal--;
        OnPropertyChanged(nameof(PointsPerRealDisplay));
    }

    private void OnPointsPlus(object sender, TappedEventArgs e)
    {
        _pointsPerReal++;
        OnPropertyChanged(nameof(PointsPerRealDisplay));
    }

    // ── Handlers: campos de data ───────────────────────────────────────────

    private async void OnPickPurchaseDateTapped(object sender, TappedEventArgs e)
    {
        if (_mode == TransactionDetailMode.View) return;
        var picked = await PickDateAsync(_purchaseDate);
        if (picked.HasValue)
        {
            _purchaseDate = picked.Value;
            OnPropertyChanged(nameof(FormattedPurchaseDate));
        }
    }

    private async void OnPickReceiptDateTapped(object sender, TappedEventArgs e)
    {
        if (_mode == TransactionDetailMode.View) return;
        var picked = await PickDateAsync(_receiptDate);
        if (picked.HasValue)
        {
            _receiptDate = picked.Value;
            OnPropertyChanged(nameof(FormattedReceiptDate));
        }
    }

    private async void OnPickStatusDateTapped(object sender, TappedEventArgs e)
    {
        var picked = await PickDateAsync(_statusDate);
        if (picked.HasValue)
        {
            _statusDate = picked.Value;
            OnPropertyChanged(nameof(FormattedStatusDate));
        }
    }

    private async Task<DateOnly?> PickDateAsync(DateOnly current)
    {
        var result = await DisplayPromptAsync(
            "Data",
            "Digite a data (dd/MM/yyyy):",
            initialValue: current.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR")),
            keyboard: Keyboard.Numeric);

        if (result == null) return null;

        if (DateOnly.TryParseExact(result, "dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"),
                DateTimeStyles.None, out var parsed))
            return parsed;

        await DisplayAlert("Formato invalido", "Use o formato dd/MM/yyyy.", "OK");
        return null;
    }

    // ── Handler: valor total ──────────────────────────────────────────────

    private void OnTotalValueTextChanged(object sender, TextChangedEventArgs e)
    {
        // Mascara de moeda -- integrar com biblioteca em producao
    }

    // ── Handler: troca de programa ─────────────────────────────────────────

    private async void OnChangeProgramTapped(object sender, TappedEventArgs e)
    {
        if (_mode == TransactionDetailMode.View) return;

        var navParams = new Dictionary<string, object>();
        if (_selectedProgram != null)
        {
            navParams.Add("currentProgramId", _selectedProgram.LoyaltyProgramId);
        }

        // Abre a tela de escolha!
        await Shell.Current.GoToAsync("change-program", navParams);
    }

    // ── Handler: midia ─────────────────────────────────────────────────────

    private async void OnMediaTapped(object sender, TappedEventArgs e)
    {
        // Criamos um dicionário para avisar a próxima tela se ela é Somente Leitura
        var navParams = new Dictionary<string, object>
    {
        { "isReadOnly", _mode == TransactionDetailMode.View }
    };

        await Shell.Current.GoToAsync("transaction-media", navParams);
    }

    // ── Handler: icones lixeira / lapis ───────────────────────────────────

    private async void OnDeleteTapped(object sender, TappedEventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Excluir transacao",
            "Tem certeza que deseja excluir esta transacao? Essa acao nao pode ser desfeita.",
            "Excluir",
            "Cancelar");

        if (!confirm) return;

        // TODO: chamar servico de exclusao com _transactionId
        await DisplayAlert("Excluido", "Transacao excluida com sucesso.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    private void OnEditTapped(object sender, TappedEventArgs e)
    {
        UpdateStatusPanel.IsVisible = false;
        SetMode(TransactionDetailMode.Edit);
    }

    // ── Handler: Atualizar status ──────────────────────────────────────────

    private void OnUpdateStatusTapped(object sender, TappedEventArgs e)
    {
        bool willOpen = !UpdateStatusPanel.IsVisible;
        UpdateStatusPanel.IsVisible = willOpen;
        UpdateStatusButton.IsVisible = !willOpen;
    }

    private async void OnContestedTapped(object sender, TappedEventArgs e)
    {
        // TODO: chamar servico com TransactionStatus.Contested e _statusDate
        await DisplayAlert("Status atualizado", $"Transacao marcada como Contestada em {FormattedStatusDate}.", "OK");
        UpdateStatusPanel.IsVisible = false;
        UpdateStatusButton.IsVisible = true;
    }

    private async void OnReceivedTapped(object sender, TappedEventArgs e)
    {
        // TODO: chamar servico com TransactionStatus.Received e _statusDate
        await DisplayAlert("Status atualizado", $"Transacao marcada como Recebida em {FormattedStatusDate}.", "OK");
        UpdateStatusPanel.IsVisible = false;
        UpdateStatusButton.IsVisible = true;
    }

    public LoyaltyProgramDto SelectedProgramParam
    {
        set
        {
            if (value != null)
            {
                _selectedProgram = value;
                UpdateProgramPill();
            }
        }
    }

    // ── Handlers: Cancelar / Salvar ────────────────────────────────────────

    private async void OnCancelTapped(object sender, TappedEventArgs e)
    {
        if (_mode == TransactionDetailMode.Edit)
        {
            SetMode(TransactionDetailMode.View);
            return;
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSaveTapped(object sender, TappedEventArgs e)
    {
        if (!Validate()) return;

        if (_mode == TransactionDetailMode.Create)
        {
            // TODO: chamar servico de criacao
            await DisplayAlert("Sucesso", "Transacao criada com sucesso!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else if (_mode == TransactionDetailMode.Edit)
        {
            // TODO: chamar servico de atualizacao com _transactionId
            await DisplayAlert("Sucesso", "Transacao atualizada com sucesso!", "OK");
            SetMode(TransactionDetailMode.View);
        }
    }

    // ── Validacao basica ───────────────────────────────────────────────────

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Description))
        {
            DisplayAlert("Campo obrigatorio", "Preencha a descricao do item comprado.", "OK");
            return false;
        }
        if (string.IsNullOrWhiteSpace(Store))
        {
            DisplayAlert("Campo obrigatorio", "Preencha o lugar de compra.", "OK");
            return false;
        }
        if (_totalValue <= 0)
        {
            DisplayAlert("Valor invalido", "Informe um valor total maior que zero.", "OK");
            return false;
        }
        if (_selectedProgram == null)
        {
            DisplayAlert("Campo obrigatorio", "Selecione um programa de pontuacao.", "OK");
            return false;
        }
        return true;
    }
}
