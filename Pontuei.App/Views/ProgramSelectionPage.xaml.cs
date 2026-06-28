using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Pontuei.App.Services;
using Pontuei.Shared.Dtos.Objects;

namespace Pontuei.App.Views;

public partial class ProgramSelectionPage : ContentPage
{
    // Guarda o par (item, border) para poder animar o mais antigo ao desselecionar
    private readonly List<(ProgramSelectItem Item, Border Border)> _selectionOrder = [];
    private bool _isOtherSelected;

    public ObservableCollection<ProgramSelectItem> ProgramItems { get; } = [];

    public ProgramSelectionPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadProgramsMock();
    }

    private void LoadProgramsMock()
    {
        // TODO: substituir pelo GET /loyalty-programs da API
        var mockPrograms = new List<LoyaltyProgramDto>
        {
            new() { LoyaltyProgramId = 1,  LoyaltyProgramName = "Esfera",           LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Esfera.webp",           LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 2,  LoyaltyProgramName = "Dotz",             LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Dotz.webp",               LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 3,  LoyaltyProgramName = "Livelo",           LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Livelo.webp",             LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 4,  LoyaltyProgramName = "Inter Loop",       LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/InterLoop.webp",          LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 5,  LoyaltyProgramName = "XP Investimentos", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/XPInvestimentos.webp",    LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 6,  LoyaltyProgramName = "Átomos",           LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Atomos.webp",             LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 7,  LoyaltyProgramName = "Smiles",           LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Smiles.webp",             LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 8,  LoyaltyProgramName = "Latam Pass",       LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/LatamPass.webp",         LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 9,  LoyaltyProgramName = "Tudo Azul",        LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/TudoAzul.webp",           LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 10, LoyaltyProgramName = "Itaú",             LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Itau.webp",               LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 11, LoyaltyProgramName = "Caixa",            LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Caixa.webp",              LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 12, LoyaltyProgramName = "Stix",             LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Stix.webp",               LoyaltyProgramIsActive = true }
        };

        ProgramItems.Clear();
        foreach (LoyaltyProgramDto program in mockPrograms)
        {
            ProgramItems.Add(new ProgramSelectItem
            {
                Program = program,
                LogoUrl = AppConstants.ResolveStorageUrl(program.LoyaltyProgramLogoUrl)
            });
        }
    }

    private async void OnProgramTapped(object? sender, TappedEventArgs e)
    {
        // O Border é passado diretamente via CommandParameter (definido no XAML)
        if (e.Parameter is not Border border)
            return;

        if (border.BindingContext is not ProgramSelectItem item)
            return;

        if (!item.IsSelected)
        {
            // Se atingiu o limite, desseleciona o mais antigo — COM animação
            if (_selectionOrder.Count >= AppConstants.MaxProgramSelection)
            {
                var (oldestItem, oldestBorder) = _selectionOrder[0];
                _selectionOrder.RemoveAt(0);
                oldestItem.IsSelected = false;
                // Roda em paralelo, sem bloquear a seleção do novo
                _ = AnimateStroke(oldestBorder, Color.FromArgb("#3A6B4A"), Colors.Transparent, 150);
            }

            item.IsSelected = true;
            _selectionOrder.Add((item, border));

            // Pulsa a escala e acende a borda verde simultaneamente
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

    /// <summary>
    /// Interpola a cor do Stroke de um Border ao longo de <paramref name="durationMs"/> ms.
    /// MAUI não tem ColorAnimation nativa, então fazemos frame a frame.
    /// </summary>
    private static Task AnimateStroke(Border border, Color from, Color to, uint durationMs)
    {
        var tcs = new TaskCompletionSource();
        var anim = new Animation(t =>
        {
            border.Stroke = new SolidColorBrush(
                Color.FromRgba(
                    from.Red + (to.Red - from.Red) * t,
                    from.Green + (to.Green - from.Green) * t,
                    from.Blue + (to.Blue - from.Blue) * t,
                    from.Alpha + (to.Alpha - from.Alpha) * t
                ));
        }, 0, 1, Easing.CubicInOut);

        anim.Commit(border, "StrokeAnim", length: durationMs, finished: (_, _) =>
        {
            border.Stroke = new SolidColorBrush(to);
            tcs.TrySetResult();
        });

        return tcs.Task;
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
            ? Color.FromArgb("#3A6B4A")
            : Colors.Transparent;
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

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        var selectedPrograms = _selectionOrder
            .Select(x => x.Item.Program)
            .ToList();

        bool otherSelected = _isOtherSelected;

        // TODO: POST vínculo UserLoyaltyProgram (+ flag "Outro" se otherSelected)

        await Shell.Current.GoToAsync("//home");
    }
}

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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}