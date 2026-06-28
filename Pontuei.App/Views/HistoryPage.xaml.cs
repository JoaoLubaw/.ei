using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace Pontuei.App.Views;

public partial class HistoryPage : BasePage
{
    public ObservableCollection<StatusFilterItem> Filters { get; } = [];
    public ObservableCollection<HistoryTransactionItem> Transactions { get; } = [];

    public HistoryPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadMocks();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.History, animate: false);
    }

    private void LoadMocks()
    {
        Filters.Add(new StatusFilterItem { Name = "Atrasadas", Color = Color.FromArgb("#C0392B") });
        Filters.Add(new StatusFilterItem { Name = "Recebidas", Color = Color.FromArgb("#4E8A61") });
        Filters.Add(new StatusFilterItem { Name = "Canceladas", Color = Color.FromArgb("#6B6B6B") });

        Transactions.Add(new HistoryTransactionItem
        {
            Description = "Tv",
            Store = "Casas Bahia",
            ProgramLogo = "livelo_logo.png", // Ajuste para o nome da imagem da sua logo
            Points = 10000,
            ExpectedDate = new DateOnly(2025, 8, 19),
            ReceivedDate = new DateOnly(2025, 8, 20),
            StatusColor = Color.FromArgb("#C0392B") // Vermelho - Atrasada
        });

        Transactions.Add(new HistoryTransactionItem
        {
            Description = "Tv",
            Store = "Casas Bahia",
            ProgramLogo = "livelo_logo.png",
            Points = 10000,
            ExpectedDate = new DateOnly(2025, 8, 19),
            ReceivedDate = new DateOnly(2025, 8, 20),
            StatusColor = Color.FromArgb("#4E8A61") // Verde - Recebida
        });
    }

    // Lógica de expansão e animação das pílulas de filtro
    private void OnFilterTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Border tappedBorder || e.Parameter is not StatusFilterItem tappedItem) return;
        var stack = (HorizontalStackLayout)tappedBorder.Parent;

        // Fecha todas as outras pílulas que estiverem abertas
        foreach (View child in stack.Children)
        {
            if (child is Border border && border.BindingContext is StatusFilterItem item && item.IsSelected && item != tappedItem)
            {
                item.IsSelected = false;
                AnimatePill(border, expand: false);
            }
        }

        // Alterna o estado da clicada e anima
        tappedItem.IsSelected = !tappedItem.IsSelected;
        AnimatePill(tappedBorder, expand: tappedItem.IsSelected);
    }

    private void AnimatePill(Border border, bool expand)
    {
        if (border.Content is Label label)
        {
            // Se for expandir, deixa o texto visível antes de animar a opacidade
            if (expand) label.IsVisible = true;

            var targetWidth = expand ? 115.0 : 24.0;
            var targetOpacity = expand ? 1.0 : 0.0;

            var animation = new Animation();

            // Anima a largura da borda
            animation.Add(0, 1, new Animation(v => border.WidthRequest = v, border.WidthRequest, targetWidth, Easing.CubicOut));
            // Anima o surgimento/desaparecimento do texto
            animation.Add(0, 1, new Animation(v => label.Opacity = v, label.Opacity, targetOpacity, Easing.CubicOut));

            animation.Commit(border, "PillAnimation", length: 250, finished: (v, c) =>
            {
                // Esconde o texto da árvore visual para evitar clicks quando estiver colapsado
                if (!expand) label.IsVisible = false;
            });
        }
    }

    private async void OnViewPendingTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//home");

    private async void OnChangeProgramTapped(object sender, TappedEventArgs e)
        => await DisplayAlert("Em breve", "Seletor de programa.", "OK");
}

// Modelos limpos, sem lógica visual vazada
public class StatusFilterItem
{
    public required string Name { get; init; }
    public required Color Color { get; init; }
    public bool IsSelected { get; set; } // Agora o controle de estado é apenas lógico
}

public class HistoryTransactionItem
{
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