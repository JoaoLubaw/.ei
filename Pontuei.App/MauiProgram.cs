using FFImageLoading.Maui;

using Microsoft.Extensions.Logging;
using PanCardView;
using Pontuei.App.Services;
using Pontuei.App.Views;

namespace Pontuei.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseFFImageLoading()
            .UseCardsView()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Poppins/poppinsRegular.ttf", "PoppinsRegular");
                fonts.AddFont("Poppins/poppinsSemibold.ttf", "PoppinsSemibold");
                fonts.AddFont("Poppins/poppinsBold.ttf", "PoppinsBold");

                fonts.AddFont("BricolageGrotesque/BricolageGrotesqueRegular.otf", "BricolageGrotesqueRegular");
                fonts.AddFont("BricolageGrotesque/BricolageGrotesqueBold.otf", "BricolageGrotesqueBold");
                fonts.AddFont("BricolageGrotesque/BricolageGrotesqueLight.otf", "BricolageGrotesqueLight");
            });

        // ── Pages registry ──────────────────────────────────────
        // Views
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<AuthPage>();
        builder.Services.AddTransient<ProgramSelectionPage>();
        builder.Services.AddTransient<HomePage>();

        // ── Inicialização assíncrona ──────────────────────────────────────
        // AuthService precisa carregar o token do SecureStorage antes de qualquer
        // tela aparecer. O OnStart do App.xaml.cs cuida disso.

#if DEBUG
        builder.Logging.AddDebug();
#endif

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) =>
        {
#if ANDROID
            handler.PlatformView.Background = null;
#elif IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
        });

        return builder.Build();
    }
}