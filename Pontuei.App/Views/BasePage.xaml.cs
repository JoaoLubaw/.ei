using System.Runtime.CompilerServices;
using Pontuei.App.Controls;

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

        // Grid sem RowDefinitions cria o comportamento de "camadas" (Z-Index).
        _rootGrid = new Grid();

        // 1º Camada (Fundo): O conteúdo da tela
        _rootGrid.Children.Add(_pageContent);

        // 2º Camada (Frente): A Navbar flutuante alinhada no rodapé
        _rootGrid.Children.Add(_navBar);

        // Define o rootGrid como o conteúdo inicial base
        base.Content = _rootGrid;
    }

    // Essa é a grande jogada: interceptamos quando o MAUI tentar definir 
    // o ScrollView da HomePage como conteúdo da tela.
    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        // Se a propriedade alterada for o Content da página...
        if (propertyName == nameof(Content))
        {
            // E se esse conteúdo for diferente do nosso Grid base (como o ScrollView do XAML)
            if (base.Content != _rootGrid && base.Content != null)
            {
                // 1. Salvamos o conteúdo que veio do XAML
                var xamlContent = base.Content;

                // 2. Restauramos o nosso layout base com a NavBar
                base.Content = _rootGrid;

                // 3. Jogamos o conteúdo da tela para trás da NavBar
                _pageContent.Content = xamlContent;
            }
        }
    }

    public BottomNavBar BottomNav => _navBar;
}