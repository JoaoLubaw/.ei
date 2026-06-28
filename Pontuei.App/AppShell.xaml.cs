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

        // Home e Notifications navegam via //home e //notifications (tab switch)
        // Rotas futuras de push:
        // Routing.RegisterRoute("program-selection", typeof(ProgramSelectionPage));
        // Routing.RegisterRoute("transaction-form", typeof(TransactionFormPage));
    }
}
