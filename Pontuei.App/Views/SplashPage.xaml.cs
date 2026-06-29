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

        // 1. Garantees that the AuthService is initialized before checking IsAuthenticated
        await AuthService.InitializeAsync();

        //2. If the user is already authenticated, navigate directly to the main page
        if (AuthService.IsAuthenticated)
        {
            await NavigateToMain();
            return;
        }

        await PlayEntranceAnimation();
    }

    private async Task PlayEntranceAnimation()
    {
        await Task.Delay(200);

        Task<bool> logoFade = LogoLabel.FadeToAsync(1, 600, Easing.CubicOut);
        Task<bool> logoSlide = LogoLabel.TranslateToAsync(0, 0, 600, Easing.CubicOut);
        await Task.WhenAll(logoFade, logoSlide);

        await Task.Delay(150);

        Task<bool> headlineFade = HeadlineLabel.FadeToAsync(1, 500, Easing.CubicOut);
        Task<bool> headlineSlide = HeadlineLabel.TranslateToAsync(0, 0, 500, Easing.CubicOut);
        await Task.WhenAll(headlineFade, headlineSlide);

        await Task.Delay(100);

        Task<bool> btnFade = StartButton.FadeToAsync(1, 400, Easing.CubicOut);
        Task<bool> btnSlide = StartButton.TranslateToAsync(0, 0, 400, Easing.CubicOut);
        await Task.WhenAll(btnFade, btnSlide);
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        // Visual feedback for the button click
        await StartButton.ScaleToAsync(0.96, 80);
        await StartButton.ScaleToAsync(1.0, 80);

        // Navegate to the program selection page if the user is not authenticated
        await Shell.Current.GoToAsync("//auth");
    }

    private static async Task NavigateToMain()
    {
        await Shell.Current.GoToAsync("//home");
    }
}