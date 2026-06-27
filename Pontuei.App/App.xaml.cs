using Pontuei.App.Services;

namespace Pontuei.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Loads the JWT and session data from SecureStorage into memory.
        // This is necessary because the SplashPage checks if the user is logged in
        // and we need to have the session data available before any page is shown.
        await AuthService.InitializeAsync();
    }
}
