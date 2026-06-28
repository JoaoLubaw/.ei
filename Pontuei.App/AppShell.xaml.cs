using Pontuei.App.Views;

namespace Pontuei.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private static void RegisterRoutes()
    {
        // ── Fluxo de autenticação ──────────────────────────────────────────
        // Tela 2: Login / Cadastro
        Routing.RegisterRoute("auth", typeof(AuthPage));

        // ── Onboarding ────────────────────────────────────────────────────
        Routing.RegisterRoute("program-selection", typeof(ProgramSelectionPage));

        // ── Fluxo principal (dentro do Shell com tab bar) ─────────────────
        // Tela 9/10: Transação — criar / editar
        // Routing.RegisterRoute("transaction-form", typeof(TransactionFormPage));

        // Tela 13: Picker de programa de pontuação (modal)
        // Routing.RegisterRoute("program-picker", typeof(ProgramPickerPage));

        // Tela 14: Mídias da transação
        // Routing.RegisterRoute("transaction-media", typeof(TransactionMediaPage));

        // ── Configurações ─────────────────────────────────────────────────
        // Tela 17: Configurações / Perfil
        // Routing.RegisterRoute("settings", typeof(SettingsPage));

        // NOTA: As linhas acima estão comentadas pois as pages ainda não existem.
        // Descomente cada uma conforme for criando as telas correspondentes.
    }
}
