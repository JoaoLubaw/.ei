using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pontuei.App.Views;

[QueryProperty(nameof(IsReadOnlyParam), "isReadOnly")]
public partial class TransactionMediaPage : BasePage, INotifyPropertyChanged
{
    private bool _isReadOnly;

    // Recebe o parâmetro da navegação
    public bool IsReadOnlyParam
    {
        set
        {
            _isReadOnly = value;
            OnPropertyChanged(nameof(IsEditable));
            OnPropertyChanged(nameof(IsReadOnly));
        }
    }

    // Propriedades que o XAML vai usar para mostrar/esconder as coisas
    public bool IsEditable => !_isReadOnly;
    public bool IsReadOnly => _isReadOnly;

    public TransactionMediaPage()
    {
        InitializeComponent();
        BindingContext = this; // Liga o código ao layout
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.Details, animate: false);
    }

    private async void OnAddMediaTapped(object sender, TappedEventArgs e)
    {
        // Se estiver bloqueado, nem tenta abrir a galeria
        if (!IsEditable) return;

        var result = await MediaPicker.Default.PickPhotoAsync();
        if (result != null)
        {
            await DisplayAlertAsync("Sucesso", $"Mídia selecionada: {result.FileName}", "OK");
        }
    }

    private async void OnRemoveMediaTapped(object sender, TappedEventArgs e)
    {
        if (!IsEditable) return;

        bool confirm = await DisplayAlertAsync("Remover", "Deseja remover esta mídia?", "Sim", "Não");
        // Lógica de remoção
    }

    private async void OnCancelTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSaveTapped(object sender, TappedEventArgs e)
    {
        await DisplayAlertAsync("Sucesso", "Mídias atualizadas com sucesso!", "OK");
        await Shell.Current.GoToAsync("..");
    }
}