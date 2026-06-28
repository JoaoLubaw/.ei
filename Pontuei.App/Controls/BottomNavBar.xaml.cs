namespace Pontuei.App.Controls;

public partial class BottomNavBar : ContentView
{
    public BottomNavBar()
    {
        InitializeComponent();
    }

    // Pega a página atual ativa na janela para conseguir exibir o alerta
    private Page CurrentPage => Application.Current?.Windows.FirstOrDefault()?.Page;

    private async void OnAddTransactionTapped(object sender, TappedEventArgs e)
    {
        if (CurrentPage != null)
        {
            // Atualizado para DisplayAlertAsync conforme exigência do .NET MAUI mais recente
            await CurrentPage.DisplayAlertAsync("Em breve", "Adicionar transação ainda não implementado.", "OK");
        }
    }

    private async void OnViewPastTransactionsTapped(object sender, TappedEventArgs e)
    {
        if (CurrentPage != null)
        {
            await CurrentPage.DisplayAlertAsync("Em breve", "Histórico de transações ainda não implementado.", "OK");
        }
    }

    private async void OnViewNotificationsTapped(object sender, TappedEventArgs e)
    {
        if (CurrentPage != null)
        {
            await CurrentPage.DisplayAlertAsync("Em breve", "Notificações ainda não implementadas.", "OK");
        }
    }
}