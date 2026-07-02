using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Pontuei.App.Services;
using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.App.Views;

public partial class ReorderProgramsPage : BasePage, INotifyPropertyChanged
{
    private readonly LoyaltyProgramsApiService _loyaltyProgramsApi;

    // Duas listas separadas: O "Top 3 VIP" e os "Outros"
    private List<ReorderProgramItem> _topPrograms = new List<ReorderProgramItem>();
    public ObservableCollection<ReorderProgramItem> OtherPrograms { get; } = new ObservableCollection<ReorderProgramItem>();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
    }
    public bool IsNotLoading => !_isLoading;

    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            _isSaving = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotSaving));
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }
    public bool IsNotSaving => !_isSaving;
    public string SaveButtonText => IsSaving ? "Salvando..." : "Salvar";

    private ReorderProgramItem? _position1;
    public ReorderProgramItem? Position1
    {
        get => _position1;
        set { _position1 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPosition1)); }
    }
    public bool HasPosition1 => Position1 != null;

    private ReorderProgramItem? _position2;
    public ReorderProgramItem? Position2
    {
        get => _position2;
        set { _position2 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPosition2)); }
    }
    public bool HasPosition2 => Position2 != null;

    private ReorderProgramItem? _position3;
    public ReorderProgramItem? Position3
    {
        get => _position3;
        set { _position3 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPosition3)); }
    }
    public bool HasPosition3 => Position3 != null;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ReorderProgramsPage(LoyaltyProgramsApiService loyaltyProgramsApi)
    {
        InitializeComponent();
        BindingContext = this;
        _loyaltyProgramsApi = loyaltyProgramsApi;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BottomNav?.SetActiveTab(Controls.BottomNavBar.NavTab.ReorderPrograms, animate: false);
        await LoadUserProgramsAsync();
    }

    private async Task LoadUserProgramsAsync()
    {
        Guid? userId = AuthService.CurrentUserId;
        if (userId is null) return;

        IsLoading = true;

        try
        {
            ApiResponse<GetUserLoyaltyProgramsResponseDto> response =
                await _loyaltyProgramsApi.GetUserProgramsAsync(
                    userId.Value,
                    new GetUserLoyaltyProgramsRequestDto { Page = 1, Size = 100 });

            if (response.IsSuccess && response.Data != null)
            {
                _topPrograms.Clear();
                OtherPrograms.Clear();

                List<UserLoyaltyProgramDto> dbItems = response.Data.UserLoyaltyPrograms
                    .Where(p => !p.LoyaltyProgram.LoyaltyProgramName.Equals("Outro", StringComparison.OrdinalIgnoreCase) &&
                                !p.LoyaltyProgram.LoyaltyProgramName.Equals("Outros", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.UserLoyaltyProgramDisplayOrder)
                    .ToList();

                foreach (UserLoyaltyProgramDto p in dbItems)
                {
                    ReorderProgramItem item = ReorderProgramItem.FromDto(p);

                    // Só entra no Top se a ordem original for < 4 E se tivermos menos de 3.
                    if (p.UserLoyaltyProgramDisplayOrder < 4 && _topPrograms.Count < 3)
                    {
                        _topPrograms.Add(item);
                    }
                    else
                    {
                        OtherPrograms.Add(item);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[ReorderProgramsPage] Loaded: {_topPrograms.Count} top, {OtherPrograms.Count} others.");

                UpdateUIFromLists();
                AnimateCardsEntry();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReorderProgramsPage] LoadUserProgramsAsync error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Lógica de Reordenação (Listas Isoladas) ─────────────────────────

    private void UpdateUIFromLists()
    {
        Position1 = _topPrograms.Count > 0 ? _topPrograms[0] : null;
        Position2 = _topPrograms.Count > 1 ? _topPrograms[1] : null;
        Position3 = _topPrograms.Count > 2 ? _topPrograms[2] : null;
    }

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        if (sender is Element element && element.BindingContext is ReorderProgramItem item)
        {
            e.Data.Properties.Add("DraggedItem", item);
        }
    }

    private async void OnDrop(object sender, DropEventArgs e)
    {
        if (sender is Element element && element.BindingContext is ReorderProgramItem targetItem)
        {
            if (e.Data.Properties.TryGetValue("DraggedItem", out object? dragObj) && dragObj is ReorderProgramItem draggedItem)
            {
                if (draggedItem == targetItem) return;

                bool isSourceTop = _topPrograms.Contains(draggedItem);
                bool isTargetTop = _topPrograms.Contains(targetItem);

                // Protege para que o usuário não esvazie o último cartão do topo
                if (isSourceTop && !isTargetTop && _topPrograms.Count <= 1) return;

                if (element is VisualElement visual)
                    await visual.ScaleTo(0.95, 100, Easing.CubicOut);

                // LOGICA DE SWAP 1:1 DIRETO (Troca exata de lugares)
                if (isSourceTop == isTargetTop)
                {
                    // Arrastou na mesma área (Topo p/ Topo ou Fundo p/ Fundo)
                    if (isSourceTop)
                    {
                        int indexA = _topPrograms.IndexOf(draggedItem);
                        int indexB = _topPrograms.IndexOf(targetItem);
                        _topPrograms[indexA] = targetItem;
                        _topPrograms[indexB] = draggedItem;
                    }
                    else
                    {
                        int indexA = OtherPrograms.IndexOf(draggedItem);
                        int indexB = OtherPrograms.IndexOf(targetItem);
                        OtherPrograms[indexA] = targetItem;
                        OtherPrograms[indexB] = draggedItem;
                    }
                }
                else
                {
                    // Áreas Diferentes (Ex: Fundo para o Topo DIRETO EM CIMA de um cartão existente)
                    if (isSourceTop)
                    {
                        int sourceIndex = _topPrograms.IndexOf(draggedItem);
                        int targetIndex = OtherPrograms.IndexOf(targetItem);
                        _topPrograms[sourceIndex] = targetItem;
                        OtherPrograms[targetIndex] = draggedItem;
                    }
                    else
                    {
                        int sourceIndex = OtherPrograms.IndexOf(draggedItem);
                        int targetIndex = _topPrograms.IndexOf(targetItem);
                        OtherPrograms[sourceIndex] = targetItem;
                        _topPrograms[targetIndex] = draggedItem;
                    }
                }

                UpdateUIFromLists();

                if (element is VisualElement visual2)
                    await visual2.ScaleTo(1.0, 100, Easing.CubicIn);
            }
        }
    }

    // Método NOVO: Recebe cartões jogados em "buracos" (espaço vazio) no topo
    private void OnTopAreaDrop(object sender, DropEventArgs e)
    {
        if (e.Data.Properties.TryGetValue("DraggedItem", out object? dragObj) && dragObj is ReorderProgramItem draggedItem)
        {
            if (_topPrograms.Contains(draggedItem)) return; // Se já tá no Topo, não faz nada

            // Se for do Fundo e o Topo tiver espaço (menos de 3)
            if (_topPrograms.Count < 3)
            {
                OtherPrograms.Remove(draggedItem);
                _topPrograms.Add(draggedItem); // Promove preenchendo a vaga vazia
                UpdateUIFromLists();
            }
        }
    }

    // Recebe cartões rebaixados do topo para a área branca no fundo
    private void OnFlexLayoutDrop(object sender, DropEventArgs e)
    {
        if (e.Data.Properties.TryGetValue("DraggedItem", out object? dragObj) && dragObj is ReorderProgramItem draggedItem)
        {
            if (_topPrograms.Contains(draggedItem))
            {
                if (_topPrograms.Count <= 1) return; // Deve manter pelo menos 1 favorito

                _topPrograms.Remove(draggedItem);
                OtherPrograms.Add(draggedItem);
                UpdateUIFromLists();
            }
        }
    }

    // ── Animações ─────────────────────────────────────────────────────────

    private async void AnimateCardsEntry()
    {
        if (CardPos1 != null) { CardPos1.Opacity = 0; CardPos1.TranslationY = 30; }
        if (CardPos2 != null) { CardPos2.Opacity = 0; CardPos2.TranslationY = 30; }
        if (CardPos3 != null) { CardPos3.Opacity = 0; CardPos3.TranslationY = 30; }
        if (OtherCardsContainer != null) { OtherCardsContainer.Opacity = 0; OtherCardsContainer.TranslationY = 30; }

        List<Task> animations = new List<Task>();

        if (HasPosition1 && CardPos1 != null)
        {
            animations.Add(CardPos1.FadeTo(1, 400));
            animations.Add(CardPos1.TranslateTo(0, 0, 400, Easing.CubicOut));
            await Task.Delay(100);
        }
        if (HasPosition2 && CardPos2 != null)
        {
            animations.Add(CardPos2.FadeTo(1, 400));
            animations.Add(CardPos2.TranslateTo(0, 0, 400, Easing.CubicOut));
            await Task.Delay(100);
        }
        if (HasPosition3 && CardPos3 != null)
        {
            animations.Add(CardPos3.FadeTo(1, 400));
            animations.Add(CardPos3.TranslateTo(0, 0, 400, Easing.CubicOut));
            await Task.Delay(100);
        }
        if (OtherPrograms.Count > 0 && OtherCardsContainer != null)
        {
            animations.Add(OtherCardsContainer.FadeTo(1, 400));
            animations.Add(OtherCardsContainer.TranslateTo(0, 0, 400, Easing.CubicOut));
        }

        await Task.WhenAll(animations);
    }

    // ── Salvamento via API ────────────────────────────────────────────────

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (_isSaving) return;
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_isSaving) return;

        Guid? userId = AuthService.CurrentUserId;
        if (userId is null) return;

        if (_topPrograms.Count == 0 && OtherPrograms.Count == 0)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        IsSaving = true;

        try
        {
            List<CreateUserLoyaltyProgramRequestDto> programsToSave = new List<CreateUserLoyaltyProgramRequestDto>();

            // Itens do Topo ganham DisplayOrder respectivo à sua posição VIP (0, 1 ou 2)
            for (int i = 0; i < _topPrograms.Count; i++)
            {
                programsToSave.Add(new CreateUserLoyaltyProgramRequestDto
                {
                    LoyaltyProgramId = _topPrograms[i].LoyaltyProgramId,
                    DisplayOrder = (short)i
                });
            }

            // Itens da "Planície" recebem 4
            foreach (ReorderProgramItem item in OtherPrograms)
            {
                programsToSave.Add(new CreateUserLoyaltyProgramRequestDto
                {
                    LoyaltyProgramId = item.LoyaltyProgramId,
                    DisplayOrder = 4
                });
            }

            BulkUpdateUserLoyaltyProgramsRequestDto request = new BulkUpdateUserLoyaltyProgramsRequestDto
            {
                Programs = programsToSave
            };

            ApiResponse<GetUserLoyaltyProgramsResponseDto> response =
                await _loyaltyProgramsApi.BulkUpdateUserProgramsAsync(userId.Value, request);

            if (response.IsSuccess)
            {
                HomePage.MarkProgramCheckDone();
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReorderProgramsPage] OnSaveClicked error: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }
}

public class ReorderProgramItem
{
    public required int LoyaltyProgramId { get; init; }
    public required string Name { get; init; }
    public required string LogoUrl { get; init; }
    public required Color PrimaryColor { get; init; }
    public required Color SecondaryColor { get; init; }

    public bool HasLogo => !string.IsNullOrWhiteSpace(LogoUrl);
    public bool ShowProgramName => !HasLogo;

    public static ReorderProgramItem FromDto(UserLoyaltyProgramDto dto)
    {
        LoyaltyProgramDto p = dto.LoyaltyProgram;
        return new ReorderProgramItem
        {
            LoyaltyProgramId = p.LoyaltyProgramId,
            Name = p.LoyaltyProgramName,
            LogoUrl = AppConstants.ResolveStorageUrl(p.LoyaltyProgramLogoUrl),
            PrimaryColor = ParseColor(p.LoyaltyProgramBrandPrimaryColor, "#3A6B4A"),
            SecondaryColor = ParseColor(p.LoyaltyProgramBrandSecondaryColor, "#2A5138"),
        };
    }

    private static Color ParseColor(string? hex, string fallback)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Color.FromArgb(fallback);
        try { return Color.FromArgb(hex.StartsWith('#') ? hex : $"#{hex}"); }
        catch { return Color.FromArgb(fallback); }
    }
}