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
        Routing.RegisterRoute("reorder-programs", typeof(ReorderProgramsPage));
        Routing.RegisterRoute("transaction-detail", typeof(TransactionDetailPage));
        Routing.RegisterRoute("change-program", typeof(ChangeProgramPage));
        Routing.RegisterRoute("transaction-media", typeof(TransactionMediaPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));

        // Home e Notifications navegam via //home e //notifications (tab switch)
        // Rotas futuras de push:
        // Routing.RegisterRoute("program-selection", typeof(ProgramSelectionPage));
        // Routing.RegisterRoute("transaction-form", typeof(TransactionFormPage));
    }
}
