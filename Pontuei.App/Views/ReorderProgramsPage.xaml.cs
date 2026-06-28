using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Pontuei.App.Services; // Presumindo que o AppConstants esteja aqui, como na HomePage

namespace Pontuei.App.Views;

public partial class ReorderProgramsPage : BasePage, INotifyPropertyChanged
{
    // Propriedades separadas para os 3 Top Cards, pois possuem layout único
    private ReorderProgramItem? _position1;
    public ReorderProgramItem? Position1
    {
        get => _position1;
        set { _position1 = value; OnPropertyChanged(); }
    }

    private ReorderProgramItem? _position2;
    public ReorderProgramItem? Position2
    {
        get => _position2;
        set { _position2 = value; OnPropertyChanged(); }
    }

    private ReorderProgramItem? _position3;
    public ReorderProgramItem? Position3
    {
        get => _position3;
        set { _position3 = value; OnPropertyChanged(); }
    }

    // A lista das "pílulas" inferiores
    public ObservableCollection<ReorderProgramItem> OtherPrograms { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ReorderProgramsPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadMocks();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.Home, animate: false);
    }

    private void LoadMocks()
    {
        // Posições Iniciais baseadas na HomePage
        Position1 = CreateItem("Livelo", "/pontuei-programs/loyalty-programs/Livelo.webp", "#E4002B", "#B8001F");
        Position2 = CreateItem("Esfera", "/pontuei-programs/loyalty-programs/Esfera.webp", "#003366", "#001F3F");
        Position3 = CreateItem("Dotz", "/pontuei-programs/loyalty-programs/Dotz.webp", "#FF6B00", "#CC5500");

        // Os "Outros"
        OtherPrograms.Add(CreateItem("Itaú", "itau_logo.png", "#EC7000", "#D05A00"));
        OtherPrograms.Add(CreateItem("Inter", "inter_logo.png", "#FF7A00", "#CC6200"));
        OtherPrograms.Add(CreateItem("XP", "xp_logo.png", "#000000", "#222222"));
        OtherPrograms.Add(CreateItem("Smiles", "smiles_logo.png", "#FF5C00", "#CC4A00"));
        OtherPrograms.Add(CreateItem("Latam", "latam_logo.png", "#1B0088", "#120055"));
        OtherPrograms.Add(CreateItem("Atomos", "atomos_logo.png", "#242424", "#111111"));
        OtherPrograms.Add(CreateItem("Stix", "stix_logo.png", "#EA0029", "#B2001E"));
        OtherPrograms.Add(CreateItem("TudoAzul", "tudoazul_logo.png", "#00A8E1", "#007DAB"));
        OtherPrograms.Add(CreateItem("Caixa", "caixa_logo.png", "#005CA9", "#00407A"));
    }

    private static ReorderProgramItem CreateItem(string name, string logo, string primary, string secondary)
    {
        // Se AppConstants não existir aqui, troque para apenas `logo` na propriedade abaixo
        var finalLogoUrl = logo.StartsWith('/') ? AppConstants.ResolveStorageUrl(logo) : logo;

        return new ReorderProgramItem
        {
            Name = name,
            LogoUrl = finalLogoUrl,
            PrimaryColor = Color.FromArgb(primary),
            SecondaryColor = Color.FromArgb(secondary)
        };
    }

    // ── Lógica de Reordenação (Drag & Drop) ─────────────────────────

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        if (sender is Element element && element.BindingContext is ReorderProgramItem item)
        {
            // Guarda na memória o item que está sendo arrastado
            e.Data.Properties.Add("DraggedItem", item);
        }
    }

    private void OnDrop(object sender, DropEventArgs e)
    {
        if (sender is Element element && element.BindingContext is ReorderProgramItem targetItem)
        {
            // Pega o item que foi arrastado e solto aqui
            if (e.Data.Properties.TryGetValue("DraggedItem", out var dragObj) && dragObj is ReorderProgramItem draggedItem)
            {
                if (draggedItem != targetItem)
                {
                    SwapItems(draggedItem, targetItem);
                }
            }
        }
    }

    private void SwapItems(ReorderProgramItem source, ReorderProgramItem target)
    {
        // Encontra quem modifica a posição do "Source" e do "Target"
        var setSourceLoc = GetSetterFor(source);
        var setTargetLoc = GetSetterFor(target);

        // Permuta os valores
        setSourceLoc(target);
        setTargetLoc(source);
    }

    private Action<ReorderProgramItem> GetSetterFor(ReorderProgramItem item)
    {
        if (Position1 == item) return (newItem) => Position1 = newItem;
        if (Position2 == item) return (newItem) => Position2 = newItem;
        if (Position3 == item) return (newItem) => Position3 = newItem;

        int index = OtherPrograms.IndexOf(item);
        if (index >= 0)
        {
            return (newItem) =>
            {
                // Substituição segura para atualizar a UI do FlexLayout corretamente
                OtherPrograms.RemoveAt(index);
                OtherPrograms.Insert(index, newItem);
            };
        }

        return (newItem) => { }; // fallback seguro
    }

    // ── Navegação ───────────────────────────────────────────────────

    private async void OnCancelClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Aqui você acessaria Position1, Position2, Position3 e OtherPrograms para salvar
        await Shell.Current.GoToAsync("..");
    }
}

public class ReorderProgramItem
{
    public required string Name { get; init; }
    public required string LogoUrl { get; init; }
    public required Color PrimaryColor { get; init; }
    public required Color SecondaryColor { get; init; }
}