using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Pontuei.App.Services;
using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.App.Views;

public partial class ProgramSelectionPage : ContentPage
{
    // ── Serviços ──────────────────────────────────────────────────────────
    private readonly LoyaltyProgramsApiService _loyaltyProgramsApi;

    // ── Estado de seleção ─────────────────────────────────────────────────
    // Guarda o par (item, border) para animar o mais antigo ao desselecionar
    private readonly List<(ProgramSelectItem Item, Border Border)> _selectionOrder = [];
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

    // ── Coleção bindada ───────────────────────────────────────────────────
    public ObservableCollection<ProgramSelectItem> ProgramItems { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ── Construtor ────────────────────────────────────────────────────────
    public ProgramSelectionPage(LoyaltyProgramsApiService loyaltyProgramsApi)
    {
        InitializeComponent();
        BindingContext = this;
        _loyaltyProgramsApi = loyaltyProgramsApi;
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
                    // O usuário pode usar a opção "Outro" para esses casos.
                    if (string.IsNullOrWhiteSpace(program.LoyaltyProgramLogoUrl))
                        continue;

                    ProgramItems.Add(new ProgramSelectItem
                    {
                        Program = program,
                        LogoUrl = AppConstants.ResolveStorageUrl(program.LoyaltyProgramLogoUrl)
                    });
                }
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

    // ════════════════════════════════════════════════════════════════════════
    // SELEÇÃO DE PROGRAMAS
    // ════════════════════════════════════════════════════════════════════════

    private async void OnProgramTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not Border border) return;
        if (border.BindingContext is not ProgramSelectItem item) return;

        if (!item.IsSelected)
        {
            // Atingiu o limite: desseleciona o mais antigo com animação
            if (_selectionOrder.Count >= AppConstants.MaxProgramSelection)
            {
                var (oldestItem, oldestBorder) = _selectionOrder[0];
                _selectionOrder.RemoveAt(0);
                oldestItem.IsSelected = false;
                _ = AnimateStroke(oldestBorder, Color.FromArgb("#3A6B4A"), Colors.Transparent, 150);
            }

            item.IsSelected = true;
            _selectionOrder.Add((item, border));

            await Task.WhenAll(
                AnimateStroke(border, Colors.Transparent, Color.FromArgb("#3A6B4A"), 200),
                border.ScaleTo(0.92, 100, Easing.CubicOut)
                      .ContinueWith(_ => border.ScaleTo(1.0, 150, Easing.SpringOut))
            );
        }
        else
        {
            item.IsSelected = false;
            _selectionOrder.RemoveAll(x => x.Item == item);
            await AnimateStroke(border, Color.FromArgb("#3A6B4A"), Colors.Transparent, 150);
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
        ContinueButton.IsEnabled = _selectionOrder.Count > 0 || _isOtherSelected;
    }

    // ════════════════════════════════════════════════════════════════════════
    // SALVAR E NAVEGAR
    // ════════════════════════════════════════════════════════════════════════

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        ContinueButton.IsEnabled = false;

        Guid? userId = AuthService.CurrentUserId;

        if (userId is not null && _selectionOrder.Count > 0)
        {
            var programsPayload = _selectionOrder
                .Select((x, index) => new CreateUserLoyaltyProgramRequestDto
                {
                    LoyaltyProgramId = x.Item.Program.LoyaltyProgramId,
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

    // ════════════════════════════════════════════════════════════════════════
    // ANIMAÇÃO DE BORDA
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Interpola a cor do Stroke de um Border ao longo de <paramref name="durationMs"/> ms.
    /// MAUI não tem ColorAnimation nativa — fazemos frame a frame via Animation.
    /// </summary>
    private static Task AnimateStroke(Border border, Color from, Color to, uint durationMs)
    {
        var tcs = new TaskCompletionSource();

        new Animation(t =>
        {
            border.Stroke = new SolidColorBrush(Color.FromRgba(
                from.Red + (to.Red - from.Red) * t,
                from.Green + (to.Green - from.Green) * t,
                from.Blue + (to.Blue - from.Blue) * t,
                from.Alpha + (to.Alpha - from.Alpha) * t));
        }, 0, 1, Easing.CubicInOut)
        .Commit(border, "StrokeAnim", length: durationMs, finished: (_, _) =>
        {
            border.Stroke = new SolidColorBrush(to);
            tcs.TrySetResult();
        });

        return tcs.Task;
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