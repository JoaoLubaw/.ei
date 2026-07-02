using System.ComponentModel;
using System.Runtime.CompilerServices;
using Pontuei.App.Services;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;

namespace Pontuei.App.Views;

public partial class SettingsPage : BasePage, INotifyPropertyChanged
{
    private string _userName = string.Empty;
    private string _userEmail = string.Empty;

    // Backups usados para reverter o Entry caso o usuário cancele a edição
    private string _userNameBackup = string.Empty;
    private string _userEmailBackup = string.Empty;

    private bool _isNameReadOnly = true;
    private bool _isEmailReadOnly = true;
    private bool _isEmailUnverified = false;

    private bool _isChangingPassword = false;
    private bool _isVerifyingEmail = false;

    private bool _isBusy = false;

    // Guid do usuário autenticado, usado em todas as chamadas de /users/{userId}
    private Guid _userId;

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

    public Color AlterarSenhaButtonColor => IsChangingPassword ? Color.FromArgb("#3f3838") : Color.FromArgb("#615454");

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private readonly AuthApiService _authApi;
    private readonly UserApiService _userApi;

    public SettingsPage(
        AuthApiService authApi,
        UserApiService userApi
    )
    {
        InitializeComponent();
        BindingContext = this;
        _authApi = authApi;
        _userApi = userApi;
        UnverifiedEmailCard.IsVisible = _isEmailUnverified;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BottomNav.SetActiveTab(Controls.BottomNavBar.NavTab.Settings, animate: false);
        await LoadUserAsync();
    }

    // ── CARREGAMENTO INICIAL ────────────────────────────────────────────

    private async Task LoadUserAsync()
    {
        _userId = AuthService.CurrentUserId ?? Guid.Empty;
        if (_userId == Guid.Empty) return;

        var response = await _userApi.GetUserByIdAsync(_userId);
        if (!response.IsSuccess || response.Data is null) return;

        UserDto user = response.Data;

        UserName = user.UserName;
        UserEmail = user.UserEmail;
        _userNameBackup = user.UserName;
        _userEmailBackup = user.UserEmail;

        IsEmailUnverified = !user.UserEmailVerified;
        UnverifiedEmailCard.IsVisible = IsEmailUnverified;

        _pushAccepted = user.UserPushNotificationsEnabled;
        PushCheckMark.IsVisible = _pushAccepted;
        PushCheckBorder.BackgroundColor = _pushAccepted ? Color.FromArgb("#E8F5E9") : Colors.Transparent;

        _emailNotificationsAccepted = user.UserEmailNotificationsEnabled;
        EmailCheckMark.IsVisible = _emailNotificationsAccepted;
        EmailCheckBorder.BackgroundColor = _emailNotificationsAccepted ? Color.FromArgb("#E8F5E9") : Colors.Transparent;
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
    private void OnEnableNameEditTapped(object sender, EventArgs e)
    {
        _userNameBackup = UserName;
        IsNameReadOnly = false;
    }

    private void OnCancelNameEditTapped(object sender, EventArgs e)
    {
        UserName = _userNameBackup;
        IsNameReadOnly = true;
    }

    private async void OnConfirmNameEditTapped(object sender, EventArgs e)
    {
        if (_isBusy) return;

        var newName = NameEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(newName))
        {
            UserName = _userNameBackup;
            IsNameReadOnly = true;
            return;
        }

        // Nada mudou, não precisa chamar a API
        if (newName == _userNameBackup)
        {
            IsNameReadOnly = true;
            return;
        }

        _isBusy = true;
        var request = new UpdateUserRequestDto { UserName = newName };
        var response = await _userApi.UpdateUserAsync(_userId, request);
        _isBusy = false;

        if (response.IsSuccess && response.Data != null)
        {
            UserName = response.Data.UserName;
            _userNameBackup = response.Data.UserName;
            IsNameReadOnly = true;
        }
        else
        {
            // Erro já mostrado via toast pelo ApiClient — só reverte o campo
            UserName = _userNameBackup;
        }
        // Oculta os botões automaticamente, indicando sucesso de forma limpa. Sem popups!
    }

    // ── GESTÃO VISUAL INLINE: E-MAIL ──
    private void OnEnableEmailEditTapped(object sender, EventArgs e)
    {
        _userEmailBackup = UserEmail;
        IsEmailReadOnly = false;
    }

    private void OnCancelEmailEditTapped(object sender, EventArgs e)
    {
        UserEmail = _userEmailBackup;
        IsEmailReadOnly = true;
    }

    private async void OnConfirmEmailEditTapped(object sender, EventArgs e)
    {
        if (_isBusy) return;

        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
        {
            UserEmail = _userEmailBackup;
            IsEmailReadOnly = true;
            return;
        }

        if (email == _userEmailBackup)
        {
            IsEmailReadOnly = true;
            return;
        }

        _isBusy = true;
        var request = new UpdateUserRequestDto { UserEmail = email };
        var response = await _userApi.UpdateUserAsync(_userId, request);
        _isBusy = false;

        if (!response.IsSuccess)
        {
            UserEmail = _userEmailBackup;
            IsEmailReadOnly = true;
            return;
        }

        // O backend dispara o fluxo de verificação automaticamente ao trocar o e-mail
        UserEmail = email;
        _userEmailBackup = email;
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
            // Reenvia o código para o e-mail atual antes de abrir o painel
            await _authApi.ResendVerificationEmailAsync(new ResendVerificationEmailRequestDto { UserEmail = UserEmail });

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
        if (_isBusy) return;
        if (EmailCodeEntry.Text?.Length != 6) return;

        _isBusy = true;
        var response = await _authApi.VerifyEmailAsync(new VerifyEmailRequestDto { Code = EmailCodeEntry.Text });
        _isBusy = false;

        if (!response.IsSuccess) return; // erro já mostrado via toast; painel permanece aberto para nova tentativa

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
        if (_isBusy) return;

        var request = new ChangePasswordRequestDto
        {
            CurrentPassword = CurrentPasswordEntry.Text ?? string.Empty,
            NewPassword = NewPasswordEntry.Text ?? string.Empty,
            ConfirmNewPassword = ConfirmPasswordEntry.Text ?? string.Empty
        };

        if (!request.PasswordsMatch() || !request.IsValidPassword() || string.IsNullOrWhiteSpace(request.CurrentPassword))
            return;

        _isBusy = true;
        var response = await _authApi.ChangePasswordAsync(request);
        _isBusy = false;

        if (!response.IsSuccess) return; // erro já mostrado via toast; painel permanece aberto

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
    private async void OnPushNotificationTapped(object sender, EventArgs e)
    {
        bool previous = _pushAccepted;
        _pushAccepted = !_pushAccepted;
        PushCheckMark.IsVisible = _pushAccepted;
        PushCheckBorder.BackgroundColor = _pushAccepted ? Color.FromArgb("#E8F5E9") : Colors.Transparent;

        var response = await _userApi.UpdateUserAsync(_userId, new UpdateUserRequestDto { UserPushNotificationsEnabled = _pushAccepted });

        if (!response.IsSuccess)
        {
            // Reverte visualmente se a API recusou a mudança
            _pushAccepted = previous;
            PushCheckMark.IsVisible = _pushAccepted;
            PushCheckBorder.BackgroundColor = _pushAccepted ? Color.FromArgb("#E8F5E9") : Colors.Transparent;
        }
    }

    private async void OnEmailNotificationTapped(object sender, EventArgs e)
    {
        bool previous = _emailNotificationsAccepted;
        _emailNotificationsAccepted = !_emailNotificationsAccepted;
        EmailCheckMark.IsVisible = _emailNotificationsAccepted;
        EmailCheckBorder.BackgroundColor = _emailNotificationsAccepted ? Color.FromArgb("#E8F5E9") : Colors.Transparent;

        var response = await _userApi.UpdateUserAsync(_userId, new UpdateUserRequestDto { UserEmailNotificationsEnabled = _emailNotificationsAccepted });

        if (!response.IsSuccess)
        {
            _emailNotificationsAccepted = previous;
            EmailCheckMark.IsVisible = _emailNotificationsAccepted;
            EmailCheckBorder.BackgroundColor = _emailNotificationsAccepted ? Color.FromArgb("#E8F5E9") : Colors.Transparent;
        }
    }

    // ── FLUXOS DE SAÍDA / EXCLUSÃO ──
    private async void OnDeleteAccountClicked(object sender, EventArgs e)
    {
        // Neste caso excepcional de exclusão, um alerta nativo faz sentido por segurança (Safety Prompt).
        bool confirm = await DisplayAlertAsync("Atenção", "Tem certeza que deseja deletar sua conta? Essa ação é irreversível.", "Deletar", "Cancelar");
        if (!confirm) return;

        var response = await _userApi.DeleteAccountAsync(_userId);
        if (!response.IsSuccess) return; // erro já mostrado via toast

        await AuthService.LogoutAsync();
        await Shell.Current.GoToAsync("//auth");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sair", "Tem certeza que deseja sair da sua conta?", "Sim", "Cancelar");

        if (confirm)
        {
            await _authApi.LogoutAsync();

            HomePage.ResetProgramCheck();

            await Shell.Current.GoToAsync("//auth");
        }
    }

}
