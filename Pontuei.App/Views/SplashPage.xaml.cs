using Pontuei.App.Services;

namespace Pontuei.App.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Verifica se já tem sessão ativa (JWT salvo) — se sim, pula direto pro Shell
        if (await AuthService.IsLoggedInAsync())
        {
            await NavigateToMain();
            return;
        }

        // Dispara a sequência de animação de entrada
        await PlayEntranceAnimation();
    }

    private async Task PlayEntranceAnimation()
    {
        await Task.Delay(200);

        var logoFade = LogoLabel.FadeTo(1, 600, Easing.CubicOut);
        var logoSlide = LogoLabel.TranslateTo(0, 0, 600, Easing.CubicOut);
        await Task.WhenAll(logoFade, logoSlide);

        await Task.Delay(150);

        var headlineFade = HeadlineLabel.FadeTo(1, 500, Easing.CubicOut);
        var headlineSlide = HeadlineLabel.TranslateTo(0, 0, 500, Easing.CubicOut);
        await Task.WhenAll(headlineFade, headlineSlide);

        await Task.Delay(100);

        var btnFade = StartButton.FadeTo(1, 400, Easing.CubicOut);
        var btnSlide = StartButton.TranslateTo(0, 0, 400, Easing.CubicOut);
        await Task.WhenAll(btnFade, btnSlide);
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        // Feedback visual no botão
        await StartButton.ScaleTo(0.96, 80);
        await StartButton.ScaleTo(1.0, 80);

        // Navega para a tela de login
        await Shell.Current.GoToAsync("program-selection");
    }

    private static async Task NavigateToMain()
    {
        await Shell.Current.GoToAsync("//home");
    }
}