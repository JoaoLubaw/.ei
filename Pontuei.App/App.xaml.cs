using Pontuei.App.Services;

namespace Pontuei.App;

public partial class App : Application
{
    public App()
    {
        // Captura exceções não tratadas antes do crash
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine($"[CRASH] UnhandledException: {ex}");
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[CRASH] UnobservedTaskException: {e.Exception}");
            e.SetObserved();
        };

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();
        try
        {
            await AuthService.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] Falhou na init: {ex.Message}");
            // Continua mesmo assim — estamos mockando
        }
    }
}