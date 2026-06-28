using System.Collections.ObjectModel;
using Pontuei.App.Services;
using Pontuei.Shared.Dtos.Objects;

namespace Pontuei.App.Views;

[QueryProperty(nameof(CurrentProgramIdParam), "currentProgramId")]
public partial class ChangeProgramPage : BasePage
{
    public ObservableCollection<ProgramSelectItem> ProgramItems { get; } = new();

    // Recebe o ID do programa que estava selecionado na tela de transação para destacá-lo
    public int CurrentProgramIdParam
    {
        set
        {
            foreach (var item in ProgramItems)
            {
                item.IsSelected = (item.Program.LoyaltyProgramId == value);
            }

            // Se for -1, significa que a opção "Outro" estava selecionada
            OtherOptionBorder.Stroke = (value == -1) ? Color.FromArgb("#3A6B4A") : Colors.Transparent;
        }
    }

    public ChangeProgramPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadProgramsMock();
    }

    private void LoadProgramsMock()
    {
        // Mock com cores preenchidas para o Gradiente da "pílula" funcionar ao retornar
        var mockPrograms = new List<LoyaltyProgramDto>
        {
            new() { LoyaltyProgramId = 1,  LoyaltyProgramName = "Esfera",           LoyaltyProgramBrandPrimaryColor = "#E4002B", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Esfera.webp",           LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 2,  LoyaltyProgramName = "Dotz",             LoyaltyProgramBrandPrimaryColor = "#E4002B", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Dotz.webp",               LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 3,  LoyaltyProgramName = "Livelo",           LoyaltyProgramBrandPrimaryColor = "#E4002B", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Livelo.webp",             LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 4,  LoyaltyProgramName = "Inter Loop",       LoyaltyProgramBrandPrimaryColor = "#FF7A00", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/InterLoop.webp",          LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 5,  LoyaltyProgramName = "XP Investimentos", LoyaltyProgramBrandPrimaryColor = "#000000", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/XPInvestimentos.webp",    LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 6,  LoyaltyProgramName = "Átomos",           LoyaltyProgramBrandPrimaryColor = "#000000", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Atomos.webp",             LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 7,  LoyaltyProgramName = "Smiles",           LoyaltyProgramBrandPrimaryColor = "#FF7A00", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Smiles.webp",             LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 8,  LoyaltyProgramName = "Latam Pass",       LoyaltyProgramBrandPrimaryColor = "#1B0088", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/LatamPass.webp",          LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 9,  LoyaltyProgramName = "Tudo Azul",        LoyaltyProgramBrandPrimaryColor = "#0061A8", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/TudoAzul.webp",           LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 10, LoyaltyProgramName = "Itaú",             LoyaltyProgramBrandPrimaryColor = "#EC7000", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Itau.webp",               LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 11, LoyaltyProgramName = "Caixa",            LoyaltyProgramBrandPrimaryColor = "#005CA9", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Caixa.webp",              LoyaltyProgramIsActive = true },
            new() { LoyaltyProgramId = 12, LoyaltyProgramName = "Stix",             LoyaltyProgramBrandPrimaryColor = "#000000", LoyaltyProgramLogoUrl = "/pontuei-programs/loyalty-programs/Stix.webp",               LoyaltyProgramIsActive = true }
        };

        ProgramItems.Clear();
        foreach (var program in mockPrograms)
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
        if (e.Parameter is not ProgramSelectItem selectedItem) return;

        // Ao clicar, envia o objeto escolhido de volta para a tela anterior
        var navParams = new Dictionary<string, object> { { "SelectedProgram", selectedItem.Program } };
        await Shell.Current.GoToAsync("..", navParams);
    }

    private async void OnOtherTapped(object? sender, TappedEventArgs e)
    {
        var otherProgram = new LoyaltyProgramDto
        {
            LoyaltyProgramId = -1,
            LoyaltyProgramName = "Outro",
            LoyaltyProgramBrandPrimaryColor = "#1A1A1A",
            LoyaltyProgramBrandSecondaryColor = "#333333"
        };

        var navParams = new Dictionary<string, object> { { "SelectedProgram", otherProgram } };
        await Shell.Current.GoToAsync("..", navParams);
    }
}