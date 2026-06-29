using CommunityToolkit.Maui;
using FFImageLoading.Maui;

using Plugin.Firebase.CloudMessaging;
#if ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif

using Plugin.Firebase.Auth;

using Microsoft.Extensions.Logging;
using PanCardView;
using Pontuei.App.Services;
using Pontuei.App.Services.Api;
using Pontuei.App.Views;
using Microsoft.Maui.LifecycleEvents;

namespace Pontuei.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AppConfig.Initialize();

        MauiAppBuilder builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseFFImageLoading()
            .UseCardsView()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Poppins/poppinsRegular.ttf", "PoppinsRegular");
                fonts.AddFont("Poppins/poppinsSemibold.ttf", "PoppinsSemibold");
                fonts.AddFont("Poppins/poppinsBold.ttf", "PoppinsBold");

                fonts.AddFont("BricolageGrotesque/BricolageGrotesqueRegular.otf", "BricolageGrotesqueRegular");
                fonts.AddFont("BricolageGrotesque/BricolageGrotesqueBold.otf", "BricolageGrotesqueBold");
                fonts.AddFont("BricolageGrotesque/BricolageGrotesqueLight.otf", "BricolageGrotesqueLight");
            });

        // ── HTTP Client ──────────────────────────────────────────────────
        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(AppConfig.ApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // ── Api Services ─────────────────────────────────────────────────
        builder.Services.AddTransient<AuthApiService>();
        builder.Services.AddTransient<UserApiService>();
        builder.Services.AddTransient<LoyaltyProgramsApiService>();
        builder.Services.AddTransient<TransactionApiService>();
        builder.Services.AddTransient<NotificationApiService>();

        // ── Pages registry ───────────────────────────────────────────────
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<AuthPage>();
        builder.Services.AddTransient<ProgramSelectionPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<ReorderProgramsPage>();
        builder.Services.AddTransient<TransactionDetailPage>();
        builder.Services.AddTransient<ChangeProgramPage>();
        builder.Services.AddTransient<TransactionMediaPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        // ── Handler Mappers ──────────────────────────────────────────────
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) =>
        {
#if ANDROID
            handler.PlatformView.Background = null;
#elif IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
        });

        // ── Lifecycle Events (Fora do Mapper!) ───────────────────────────
        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity, () => Platform.CurrentActivity!)));
#endif
        });

        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);

        return builder.Build();
    }
}