using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
using Pontuei.App.Controls;

using Pontuei.App.Services;
using Pontuei.App.Services.Api;

namespace Pontuei.App.Views;

public partial class BasePage : ContentPage
{
    private readonly Grid _rootGrid;
    private readonly ContentView _pageContent;
    private readonly BottomNavBar _navBar;

    public BasePage()
    {
        InitializeComponent();

        _pageContent = new ContentView
        {
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
        };

        _navBar = new BottomNavBar
        {
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Fill,
            InputTransparent = false,
        };

        // ─── 1. CRIANDO O HEADER GLOBAL FLUTUANTE ───
        var headerGrid = new Grid
        {
            Padding = new Thickness(24, 16, 16, 48),
            VerticalOptions = LayoutOptions.Start, // Garante que o grid ocupe apenas o topo, sem bloquear o toque na tela toda
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        // Logo sem fundo
        var logoImage = new Image
        {
            Source = "logo.svg",
            HeightRequest = 48,
            WidthRequest = 80,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Aspect = Aspect.AspectFit
        };

        // Botão de Configurações em formato de círculo
        var settingsBorder = new Border
        {
            WidthRequest = 44,
            HeightRequest = 44,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) },
            // Uma sombra bem suave para dar profundidade e destacar que é um botão flutuante
            Shadow = new Shadow { Brush = Brush.Black, Offset = new Point(0, 4), Radius = 8, Opacity = 0.05f }
        };

        var settingsIcon = new Image
        {
            Source = "settings.svg",
            WidthRequest = 24,
            HeightRequest = 24,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        settingsBorder.Content = settingsIcon;

        var settingsTap = new TapGestureRecognizer();
        settingsTap.Tapped += async (s, e) => await Shell.Current.GoToAsync("settings");
        settingsBorder.GestureRecognizers.Add(settingsTap);

        headerGrid.Add(logoImage, 0);
        headerGrid.Add(settingsBorder, 1);

        // ─── 2. MONTANDO O LAYOUT BASE EM CAMADAS SOBREPOSTAS ───
        _rootGrid = new Grid(); // Removi as RowDefinitions! Tudo agora compartilha o mesmo espaço.

        // Camada 1 (Fundo): O conteúdo rolável da tela
        _rootGrid.Children.Add(_pageContent);

        // Camada 2 (Topo): Header flutuando sobre o conteúdo
        _rootGrid.Children.Add(headerGrid);

        // Camada 3 (Rodapé): Navbar flutuando embaixo
        _rootGrid.Children.Add(_navBar);

        base.Content = _rootGrid;
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(Content))
        {
            if (base.Content != _rootGrid && base.Content != null)
            {
                var xamlContent = base.Content;

                // ─── O TRUQUE MÁGICO DO PADDING ───
                // Empurra o conteúdo do ScrollView para baixo do header flutuante, 
                // para que a tela inicie limpa, mas permitindo rolar por trás dele!
                // Antes só funcionava quando o ScrollView ERA o Content raiz da página.
                // Telas como a HistoryPage têm um Grid como raiz (para sobrepor o
                // ActivityIndicator de loading por cima do ScrollView), então agora
                // procuramos o ScrollView recursivamente em qualquer profundidade.
                ScrollView? scroll = FindScrollView(xamlContent);
                if (scroll != null)
                {
                    var currentPadding = scroll.Padding;
                    if (currentPadding.Top < 100) // Evita adicionar o espaço repetidas vezes
                    {
                        scroll.Padding = new Thickness(currentPadding.Left, currentPadding.Top + 86, currentPadding.Right, currentPadding.Bottom);
                    }
                }

                base.Content = _rootGrid;
                _pageContent.Content = xamlContent;
            }
        }
    }

    /// <summary>
    /// Procura recursivamente por um ScrollView dentro da árvore de elementos da página,
    /// para poder aplicar o padding do header flutuante mesmo quando o ScrollView não é
    /// o elemento raiz (ex: está dentro de um Grid junto com um overlay de loading).
    /// </summary>
    private static ScrollView? FindScrollView(Element element)
    {
        if (element is ScrollView scrollView)
            return scrollView;

        if (element is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                if (child is Element childElement)
                {
                    var found = FindScrollView(childElement);
                    if (found != null) return found;
                }
            }
        }
        else if (element is ContentView contentView && contentView.Content is Element contentViewChild)
        {
            return FindScrollView(contentViewChild);
        }
        else if (element is Border border && border.Content is Element borderChild)
        {
            return FindScrollView(borderChild);
        }

        return null;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Sempre que uma página baseada na BasePage aparecer, vamos buscar a contagem atualizada
        await LoadUnreadBadgeCountAsync();
    }

    private async Task LoadUnreadBadgeCountAsync()
    {
        // Verifica se o usuário está logado
        Guid? userId = AuthService.CurrentUserId;
        if (userId == null) return;

        try
        {
            // Resolve o serviço de API através da injeção de dependência do MAUI
            var apiService = Handler?.MauiContext?.Services.GetService<NotificationApiService>();

            if (apiService != null)
            {
                // Essa chamada bate no Redis da sua arquitetura e retorna rápido
                var response = await apiService.GetUnreadCountAsync();

                if (response.IsSuccess)
                {
                    // Atualiza a interface
                    _navBar.UpdateUnreadBadge(response.Data);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BasePage] Erro ao carregar badge: {ex.Message}");
        }
    }

    public BottomNavBar BottomNav => _navBar;
}
