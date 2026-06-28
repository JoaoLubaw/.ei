using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pontuei.App.Views;

public partial class SettingsPage : BasePage, INotifyPropertyChanged
{
    private string _userName = "João Paulo Lubawaski";
    private string _userEmail = "joao.lucawaski@example.com";

    private bool _isNameReadOnly = true;
    private bool _isEmailReadOnly = true;
    private bool _isEmailUnverified = true; // Simula e-mail não confirmado

    private bool _isChangingPassword = false;
    private bool _isVerifyingEmail = false;

    public bool IsVerifyingEmail
    {
        get => _isVerifyingEmail;
        set { _isVerifyingEmail = value; OnPropertyChanged(); }
    }

    public bool IsEmailUnverified
    {
        get => _isEmailUnverified;
        set
        {
            _isEmailUnverified = value;
            OnPropertyChanged();
            if (!value) UnverifiedEmailCard.IsVisible = false;
        }
    }

    // Retorna 'true' apenas se o painel estiver fechado
    public bool ShowVerifyButton => !IsVerifyingEmail;


    private bool _pushAccepted;
    private bool _emailNotificationsAccepted;

    public string UserName { get => _userName; set { _userName = value; OnPropertyChanged(); } }
    public string UserEmail { get => _userEmail; set { _userEmail = value; OnPropertyChanged(); } }

    public bool IsNameReadOnly { get => _isNameReadOnly; set { _isNameReadOnly = value; OnPropertyChanged(nameof(IsNameReadOnly)); OnPropertyChanged(nameof(IsNameEditing)); } }
    public bool IsNameEditing => !IsNameReadOnly;

    public bool IsEmailReadOnly { get => _isEmailReadOnly; set { _isEmailReadOnly = value; OnPropertyChanged(nameof(IsEmailReadOnly)); OnPropertyChanged(nameof(IsEmailEditing)); } }
    public bool IsEmailEditing => !IsEmailReadOnly;

    public bool IsChangingPassword { get => _isChangingPassword; set { _isChangingPassword = value; OnPropertyChanged(); OnPropertyChanged(nameof(AlterarSenhaButtonColor)); } }

    public Color AlterarSenhaButtonColor => IsChangingPassword ? Color.FromArgb("#3f3838") : Color.FromArgb("#528F65");

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = this;

        UnverifiedEmailCard.IsVisible = _isEmailUnverified;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.Settings, animate: false);
    }

    // ── ANIMAÇÃO GLOBAL DE PAINÉIS (Deslizar e Aparecer) ──
    private async Task TogglePanelAnimationAsync(VisualElement panel, bool show)
    {
        if (show)
        {
            panel.TranslationY = -15;
            panel.IsVisible = true;

            await Task.WhenAll(
                panel.FadeToAsync(1, 250, Easing.CubicOut),
                panel.TranslateToAsync(0, 0, 250, Easing.CubicOut)
            );
        }
        else
        {
            await Task.WhenAll(
                panel.FadeToAsync(0, 200, Easing.CubicIn),
                panel.TranslateToAsync(0, -15, 200, Easing.CubicIn)
            );
            panel.IsVisible = false;
        }
    }

    // ── GESTÃO VISUAL INLINE: NOME ──
    private void OnEnableNameEditTapped(object sender, EventArgs e) => IsNameReadOnly = false;
    private void OnCancelNameEditTapped(object sender, EventArgs e) { IsNameReadOnly = true; }
    private void OnConfirmNameEditTapped(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text)) return;

        IsNameReadOnly = true;
        // Oculta os botões automaticamente, indicando sucesso de forma limpa. Sem popups!
    }

    // ── GESTÃO VISUAL INLINE: E-MAIL ──
    private void OnEnableEmailEditTapped(object sender, EventArgs e) => IsEmailReadOnly = false;
    private void OnCancelEmailEditTapped(object sender, EventArgs e) => IsEmailReadOnly = true;
    private async void OnConfirmEmailEditTapped(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(email) || !email.Contains('@')) return;

        IsEmailReadOnly = true;
        IsEmailUnverified = true;

        if (!IsVerifyingEmail)
        {
            IsVerifyingEmail = true;
            UnverifiedEmailCard.IsVisible = false; // Esconde o alerta instantaneamente (pois ele não estava na tela antes da edição)
            await TogglePanelAnimationAsync(VerifyEmailPanel, true);
        }
    }

    private async void OnVerifyCurrentEmailClicked(object sender, EventArgs e)
    {
        if (!IsVerifyingEmail)
        {
            IsVerifyingEmail = true;
            // Anima os dois ao mesmo tempo: Esconde o alerta amarelo e Mostra o código
            await Task.WhenAll(
                TogglePanelAnimationAsync(UnverifiedEmailCard, false),
                TogglePanelAnimationAsync(VerifyEmailPanel, true)
            );
        }
    }

    private async void OnCancelVerifyEmailClicked(object sender, EventArgs e)
    {
        IsVerifyingEmail = false;

        var tasks = new List<Task> { TogglePanelAnimationAsync(VerifyEmailPanel, false) };

        // Se cancelou e o e-mail continua não verificado, volta o alerta amarelo animado
        if (IsEmailUnverified)
        {
            tasks.Add(TogglePanelAnimationAsync(UnverifiedEmailCard, true));
        }

        await Task.WhenAll(tasks);
    }

    private async void OnSubmitVerifyEmailClicked(object sender, EventArgs e)
    {
        if (EmailCodeEntry.Text?.Length != 6) return;

        IsVerifyingEmail = false;
        IsEmailUnverified = false;
        EmailCodeEntry.Text = string.Empty;

        // Anima a saída do painel (o alerta amarelo não volta porque a conta foi validada!)
        await TogglePanelAnimationAsync(VerifyEmailPanel, false);
    }

    private void OnNewPasswordChanged(object sender, TextChangedEventArgs e)
    {
        var pwd = e.NewTextValue ?? string.Empty;
        ApplyRule(RuleLength, pwd.Length >= 8);
        ApplyRule(RuleUpper, pwd.Any(char.IsUpper));
        ApplyRule(RuleLower, pwd.Any(char.IsLower));
        ApplyRule(RuleNumber, pwd.Any(char.IsDigit));
        ApplyRule(RuleSpecial, pwd.Any(c => !char.IsLetterOrDigit(c)));
    }

    private static void ApplyRule(Label label, bool met)
    {
        label.TextColor = met ? Color.FromArgb("#427A5B") : Color.FromArgb("#9E9E9E");
        label.TextDecorations = met ? TextDecorations.Strikethrough : TextDecorations.None;
    }

    private async void OnSaveNewPasswordClicked(object sender, EventArgs e)
    {
        if (NewPasswordEntry.Text != ConfirmPasswordEntry.Text || string.IsNullOrWhiteSpace(NewPasswordEntry.Text)) return;

        // Sucesso: Fecha o painel suavemente
        IsChangingPassword = false;
        await TogglePanelAnimationAsync(ChangePasswordPanel, false);

        // Limpa os campos
        CurrentPasswordEntry.Text = string.Empty;
        NewPasswordEntry.Text = string.Empty;
        ConfirmPasswordEntry.Text = string.Empty;
    }

    // ── EXPANSÃO E VALIDAÇÃO DE SENHA ──
    private async void OnToggleChangePasswordClicked(object sender, EventArgs e)
    {
        IsChangingPassword = !IsChangingPassword;
        await TogglePanelAnimationAsync(ChangePasswordPanel, IsChangingPassword);
    }

    // ── NOTIFICAÇÕES (Estilo Checkbox Auth) ──
    private void OnPushNotificationTapped(object sender, EventArgs e)
    {
        _pushAccepted = !_pushAccepted;
        PushCheckMark.IsVisible = _pushAccepted;
        PushCheckBorder.BackgroundColor = _pushAccepted ? Color.FromArgb("#E8F5E9") : Colors.Transparent;
    }

    private void OnEmailNotificationTapped(object sender, EventArgs e)
    {
        _emailNotificationsAccepted = !_emailNotificationsAccepted;
        EmailCheckMark.IsVisible = _emailNotificationsAccepted;
        EmailCheckBorder.BackgroundColor = _emailNotificationsAccepted ? Color.FromArgb("#E8F5E9") : Colors.Transparent;
    }

    // ── FLUXOS DE SAÍDA / EXCLUSÃO ──
    private async void OnDeleteAccountClicked(object sender, EventArgs e)
    {
        // Neste caso excepcional de exclusão, um alerta nativo faz sentido por segurança (Safety Prompt).
        bool confirm = await DisplayAlertAsync("Atenção", "Tem certeza que deseja deletar sua conta? Essa ação é irreversível.", "Deletar", "Cancelar");
        if (confirm) await Shell.Current.GoToAsync("//auth");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//auth");
    }
}