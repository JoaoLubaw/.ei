using Pontuei.App.Services;

namespace Pontuei.App.Views;

public partial class AuthPage : ContentPage
{
    // ── Enums de Estado ───────────────────────────────────────────────────
    private enum AuthMode
    {
        Login,
        Register,
        ForgotPasswordEmail,
        ForgotPasswordCode,
        ResetPassword,
        VerifyEmail
    }

    // ── Cores da tab ──────────────────────────────────────────────────────
    private static readonly Color TabGreenLight = Color.FromArgb("#4E8A61");
    private static readonly Color TabGreenDark = Color.FromArgb("#343a46");
    private static readonly Color TabActiveText = Color.FromArgb("#3A6B4A");

    // ── Cores de validação ────────────────────────────────────────────────
    private static readonly Color ErrorRed = Color.FromArgb("#E53935");
    private static readonly Color NormalBg = Color.FromArgb("#3f3838");

    // ── Estado ────────────────────────────────────────────────────────────
    private bool _passwordVisible;
    private bool _registerPasswordVisible;
    private bool _confirmPasswordVisible;
    private bool _resetPasswordVisible;
    private bool _resetConfirmVisible;

    private bool _termsAccepted;
    private bool _emailNotificationsAccepted;
    private bool _isLoginTab = true;
    private bool _isAnimatingTab;

    private AuthMode _currentMode = AuthMode.Login;
    private BoxView _radialBgView;

    public AuthPage()
    {
        InitializeComponent();

        // 1. Cria a camada radial. 
        // Deslocamos o 'Center' para o topo (Y=0.06) para o centro claro não ficar 
        // escondido atrás do cartão bege, criando um brilho em volta da Pill!
        _radialBgView = new BoxView
        {
            Opacity = 0,
            InputTransparent = true,
            Background = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.06),
                Radius = 0.5,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#4E8A61"), 0.0f),
                    new GradientStop(Color.FromArgb("#3A6B4A"), 0.4f),
                    new GradientStop(Color.FromArgb("#343a46"), 1.0f)
                }
            }
        };

        // 2. Embrulhamos os conteúdos em um novo Grid para evitar que 
        // o Padding (0,20,0,0) corte as bordas arredondadas do BoxView.
        if (FormBlock.Content is Grid originalInnerGrid)
        {
            FormBlock.Content = null;

            var wrapperGrid = new Grid();
            wrapperGrid.Children.Add(_radialBgView);       // Fundo Radial
            wrapperGrid.Children.Add(originalInnerGrid);   // Cartão Bege (acima)

            FormBlock.Content = wrapperGrid;
        }

        SwitchMode(AuthMode.Login, animate: false);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CONTROLE DE MODOS (NAVEGAÇÃO INTERNA)
    // ══════════════════════════════════════════════════════════════════════

    private void SwitchMode(AuthMode mode, bool animate = false)
    {
        _currentMode = mode;

        // Oculta todos os painéis
        LoginPanel.IsVisible = false;
        RegisterPanel.IsVisible = false;
        ForgotPasswordPanel.IsVisible = false;
        ForgotPasswordCodePanel.IsVisible = false;
        ResetPasswordPanel.IsVisible = false;
        VerifyEmailPanel.IsVisible = false;

        bool isAuthTab = (mode == AuthMode.Login || mode == AuthMode.Register);

        if (isAuthTab)
        {
            TabContainer.IsVisible = true;
            SinglePillContainer.IsVisible = false;
            SocialAndFooterPanel.IsVisible = true;

            if (mode == AuthMode.Login)
            {
                LoginPanel.IsVisible = true;
                _isLoginTab = true;
            }
            else
            {
                RegisterPanel.IsVisible = true;
                _isLoginTab = false;
            }

            UpdateTabState(animate);

            // Animação de saída do fundo radial (volta suavemente pro linear)
            if (animate) _ = _radialBgView.FadeTo(0, 300, Easing.CubicInOut);
            else _radialBgView.Opacity = 0;
        }
        else
        {
            TabContainer.IsVisible = false;
            SinglePillContainer.IsVisible = true;
            SocialAndFooterPanel.IsVisible = false;

            switch (mode)
            {
                case AuthMode.ForgotPasswordEmail:
                    SinglePillLabel.Text = "Esqueci minha senha";
                    ForgotPasswordPanel.IsVisible = true;
                    break;
                case AuthMode.ForgotPasswordCode:
                    SinglePillLabel.Text = "Esqueci minha senha";
                    ForgotPasswordCodePanel.IsVisible = true;
                    break;
                case AuthMode.ResetPassword:
                    SinglePillLabel.Text = "Esqueci minha senha";
                    ResetPasswordPanel.IsVisible = true;
                    break;
                case AuthMode.VerifyEmail:
                    SinglePillLabel.Text = "Confirme seu email";
                    VerifyEmailPanel.IsVisible = true;
                    break;
            }

            // Animação de entrada do fundo radial
            if (animate) _ = _radialBgView.FadeTo(1, 300, Easing.CubicInOut);
            else _radialBgView.Opacity = 1;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // TABS (LOGIN / REGISTER)
    // ══════════════════════════════════════════════════════════════════════

    private void OnLoginTabTapped(object sender, TappedEventArgs e)
    {
        if (_isLoginTab || _isAnimatingTab) return;
        SwitchMode(AuthMode.Login, animate: true);
    }

    private void OnRegisterTabTapped(object sender, TappedEventArgs e)
    {
        if (!_isLoginTab || _isAnimatingTab) return;
        SwitchMode(AuthMode.Register, animate: true);
    }

    private void OnFooterActionTapped(object sender, TappedEventArgs e)
    {
        if (_isAnimatingTab) return;
        SwitchMode(_isLoginTab ? AuthMode.Register : AuthMode.Login, animate: true);
    }

    private async void UpdateTabState(bool animate)
    {
        var targetX = _isLoginTab ? 0d : (TabContainer.Width > 0 ? TabContainer.Width / 2 : 0);
        var targetLeft = _isLoginTab ? TabGreenLight : TabGreenDark;
        var targetRight = _isLoginTab ? TabGreenDark : TabGreenLight;

        ApplyTabLabelStyles();

        FooterPromptLabel.Text = _isLoginTab ? "Ainda não tem uma conta?" : "Já tem uma conta?";
        FooterActionLabel.Text = _isLoginTab ? "Crie uma!" : "Faça login!";

        ActiveTabPill.Margin = _isLoginTab
            ? new Thickness(4, 4, 2, 4)
            : new Thickness(2, 4, 4, 4);

        if (!animate)
        {
            BlockGradientLeft.Color = targetLeft;
            BlockGradientRight.Color = targetRight;
            Grid.SetColumn(ActiveTabPill, 0);
            ActiveTabPill.TranslationX = targetX;
            return;
        }

        _isAnimatingTab = true;

        var startLeft = BlockGradientLeft.Color;
        var startRight = BlockGradientRight.Color;
        var startX = ActiveTabPill.TranslationX;

        var tcs = new TaskCompletionSource<bool>();
        var animation = new Animation(progress =>
        {
            BlockGradientLeft.Color = InterpolateColor(startLeft, targetLeft, progress);
            BlockGradientRight.Color = InterpolateColor(startRight, targetRight, progress);
            ActiveTabPill.TranslationX = startX + ((targetX - startX) * progress);
        }, 0, 1, Easing.CubicInOut);

        animation.Commit(this, "TabGradientAnimation", length: 280, finished: (_, _) => tcs.TrySetResult(true));
        await tcs.Task;

        Grid.SetColumn(ActiveTabPill, 0);
        ActiveTabPill.TranslationX = targetX;
        BlockGradientLeft.Color = targetLeft;
        BlockGradientRight.Color = targetRight;
        _isAnimatingTab = false;
    }

    private void ApplyTabLabelStyles()
    {
        LoginTabLabel.TextColor = _isLoginTab ? TabActiveText : Colors.White;
        LoginTabLabel.FontFamily = _isLoginTab ? "PoppinsSemibold" : "PoppinsRegular";
        LoginTabLabel.FontAttributes = _isLoginTab ? FontAttributes.Bold : FontAttributes.None;

        RegisterTabLabel.TextColor = _isLoginTab ? Colors.White : TabActiveText;
        RegisterTabLabel.FontFamily = _isLoginTab ? "PoppinsRegular" : "PoppinsSemibold";
        RegisterTabLabel.FontAttributes = _isLoginTab ? FontAttributes.None : FontAttributes.Bold;
    }

    private static Color InterpolateColor(Color from, Color to, double progress) =>
        new(
            (float)(from.Red + ((to.Red - from.Red) * progress)),
            (float)(from.Green + ((to.Green - from.Green) * progress)),
            (float)(from.Blue + ((to.Blue - from.Blue) * progress)),
            (float)(from.Alpha + ((to.Alpha - from.Alpha) * progress))
        );

    // ══════════════════════════════════════════════════════════════════════
    // LOGIN
    // ══════════════════════════════════════════════════════════════════════

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await LoginButton.ScaleTo(0.96, 80);
        await LoginButton.ScaleTo(1.0, 80);

        // TODO: AuthService.LoginAsync(EmailEntry.Text, PasswordEntry.Text)
        await Shell.Current.GoToAsync("//home");
    }

    private void OnTogglePasswordTapped(object sender, TappedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        PasswordEntry.IsPassword = !_passwordVisible;
        EyeIcon.Source = _passwordVisible ? "eye.svg" : "eve_crossed.svg";
    }

    private void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        SwitchMode(AuthMode.ForgotPasswordEmail, animate: true);
    }

    private async void OnGoogleLoginTapped(object sender, TappedEventArgs e)
    {
        // TODO: implementar OAuth Google
        await Task.CompletedTask;
    }

    // ══════════════════════════════════════════════════════════════════════
    // CADASTRO
    // ══════════════════════════════════════════════════════════════════════

    private void OnRegisterPasswordChanged(object sender, TextChangedEventArgs e)
    {
        var pwd = e.NewTextValue ?? string.Empty;

        ApplyRule(RuleLength, pwd.Length >= 8);
        ApplyRule(RuleUpper, pwd.Any(char.IsUpper));
        ApplyRule(RuleLower, pwd.Any(char.IsLower));
        ApplyRule(RuleNumber, pwd.Any(char.IsDigit));
        ApplyRule(RuleSpecial, pwd.Any(c => !char.IsLetterOrDigit(c)));

        if (RegisterPasswordFieldBorder.StrokeThickness > 0)
            ClearFieldError(RegisterPasswordFieldBorder, null);
    }

    private static void ApplyRule(Label label, bool met)
    {
        label.TextColor = met ? Color.FromArgb("#427A5B") : Color.FromArgb("#9E9E9E");
        label.TextDecorations = met ? TextDecorations.Strikethrough : TextDecorations.None;
    }

    private bool IsPasswordValid(string pwd)
    {
        return pwd.Length >= 8
            && pwd.Any(char.IsUpper)
            && pwd.Any(char.IsLower)
            && pwd.Any(char.IsDigit)
            && pwd.Any(c => !char.IsLetterOrDigit(c));
    }

    private void OnToggleRegisterPasswordTapped(object sender, TappedEventArgs e)
    {
        _registerPasswordVisible = !_registerPasswordVisible;
        RegisterPasswordEntry.IsPassword = !_registerPasswordVisible;
        RegisterEyeIcon.Source = _registerPasswordVisible ? "eye.svg" : "eve_crossed.svg";
    }

    private void OnToggleConfirmPasswordTapped(object sender, TappedEventArgs e)
    {
        _confirmPasswordVisible = !_confirmPasswordVisible;
        ConfirmPasswordEntry.IsPassword = !_confirmPasswordVisible;
        ConfirmEyeIcon.Source = _confirmPasswordVisible ? "eye.svg" : "eve_crossed.svg";
    }

    private void OnTermsTapped(object sender, TappedEventArgs e)
    {
        _termsAccepted = !_termsAccepted;
        TermsCheckMark.IsVisible = _termsAccepted;
        TermsCheckBorder.BackgroundColor = _termsAccepted ? Color.FromArgb("#E8F5E9") : Colors.Transparent;
        TermsCheckBorder.Stroke = Color.FromArgb("#3A6B4A");

        if (_termsAccepted)
            ClearFieldError(null, TermsError);
    }

    private void OnEmailNotificationsTapped(object sender, TappedEventArgs e)
    {
        _emailNotificationsAccepted = !_emailNotificationsAccepted;
        EmailNotificationsCheckMark.IsVisible = _emailNotificationsAccepted;
        EmailNotificationsCheckBorder.BackgroundColor = _emailNotificationsAccepted ? Color.FromArgb("#E8F5E9") : Colors.Transparent;
    }

    private async void OnRegisterSubmitClicked(object sender, EventArgs e)
    {
        ClearAllRegisterErrors();
        bool hasError = false;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            ShowFieldError(NameFieldBorder, NameError, "Informe seu nome completo.");
            hasError = true;
        }

        var email = RegisterEmailEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
        {
            ShowFieldError(RegisterEmailFieldBorder, RegisterEmailError, "Digite um e-mail válido.");
            hasError = true;
        }

        var pwd = RegisterPasswordEntry.Text ?? string.Empty;
        if (!IsPasswordValid(pwd))
        {
            ShowFieldError(RegisterPasswordFieldBorder, null, string.Empty);
            hasError = true;
        }

        if (pwd != ConfirmPasswordEntry.Text)
        {
            ShowFieldError(ConfirmPasswordFieldBorder, ConfirmPasswordError, "As senhas não correspondem.");
            hasError = true;
        }

        if (!_termsAccepted)
        {
            ShowFieldError(null, TermsError, "Você precisa aceitar os Termos de Uso para continuar.");
            hasError = true;
        }

        if (hasError) return;

        // TODO: AuthService.RegisterAsync(...)
        await Task.CompletedTask;

        VerifyEmailAddressSpan.Text = email;
        SwitchMode(AuthMode.VerifyEmail, animate: true);
    }

    // ══════════════════════════════════════════════════════════════════════
    // ESQUECI MINHA SENHA
    // ══════════════════════════════════════════════════════════════════════

    private void OnForgotPasswordSendClicked(object sender, EventArgs e)
    {
        ClearFieldError(ForgotEmailFieldBorder, ForgotEmailError);
        var email = ForgotEmailEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
        {
            ShowFieldError(ForgotEmailFieldBorder, ForgotEmailError, "Digite um e-mail válido.");
            return;
        }

        // TODO: Disparar API para enviar código
        SwitchMode(AuthMode.ForgotPasswordCode, animate: true);
    }

    private void OnResendForgotCodeTapped(object sender, TappedEventArgs e)
    {
        // TODO: Lógica para reenviar código de recuperação
    }

    private void OnForgotPasswordConfirmCodeClicked(object sender, EventArgs e)
    {
        ClearFieldError(ForgotCodeFieldBorder, ForgotCodeError);
        var code = ForgotCodeEntry.Text?.Trim() ?? string.Empty;

        if (code.Length < 6)
        {
            ShowFieldError(ForgotCodeFieldBorder, ForgotCodeError, "Código inválido.");
            return;
        }

        // TODO: Validar código na API
        SwitchMode(AuthMode.ResetPassword, animate: true);
    }

    private void OnAlreadyRememberedTapped(object sender, TappedEventArgs e)
    {
        SwitchMode(AuthMode.Login, animate: true);
    }

    // ══════════════════════════════════════════════════════════════════════
    // REDEFINIR SENHA
    // ══════════════════════════════════════════════════════════════════════

    private void OnResetPasswordChanged(object sender, TextChangedEventArgs e)
    {
        var pwd = e.NewTextValue ?? string.Empty;

        ApplyRule(ResetRuleLength, pwd.Length >= 8);
        ApplyRule(ResetRuleUpper, pwd.Any(char.IsUpper));
        ApplyRule(ResetRuleLower, pwd.Any(char.IsLower));
        ApplyRule(ResetRuleNumber, pwd.Any(char.IsDigit));
        ApplyRule(ResetRuleSpecial, pwd.Any(c => !char.IsLetterOrDigit(c)));

        if (ResetPasswordFieldBorder.StrokeThickness > 0)
            ClearFieldError(ResetPasswordFieldBorder, null);
    }

    private void OnToggleResetPasswordTapped(object sender, TappedEventArgs e)
    {
        _resetPasswordVisible = !_resetPasswordVisible;
        ResetPasswordEntry.IsPassword = !_resetPasswordVisible;
        ResetEyeIcon.Source = _resetPasswordVisible ? "eye.svg" : "eve_crossed.svg";
    }

    private void OnToggleResetConfirmTapped(object sender, TappedEventArgs e)
    {
        _resetConfirmVisible = !_resetConfirmVisible;
        ResetConfirmEntry.IsPassword = !_resetConfirmVisible;
        ResetConfirmEyeIcon.Source = _resetConfirmVisible ? "eye.svg" : "eve_crossed.svg";
    }

    private void OnResetPasswordSubmitClicked(object sender, EventArgs e)
    {
        ClearFieldError(ResetPasswordFieldBorder, null);
        ClearFieldError(ResetConfirmFieldBorder, ResetConfirmError);

        var pwd = ResetPasswordEntry.Text ?? string.Empty;
        if (!IsPasswordValid(pwd))
        {
            ShowFieldError(ResetPasswordFieldBorder, null, string.Empty);
            return;
        }

        if (pwd != ResetConfirmEntry.Text)
        {
            ShowFieldError(ResetConfirmFieldBorder, ResetConfirmError, "As senhas não correspondem.");
            return;
        }

        // TODO: Consumir API para trocar senha
        SwitchMode(AuthMode.Login, animate: true);
    }

    // ══════════════════════════════════════════════════════════════════════
    // VERIFICAÇÃO DE E-MAIL (PÓS CADASTRO)
    // ══════════════════════════════════════════════════════════════════════

    private void OnResendVerifyCodeTapped(object sender, TappedEventArgs e)
    {
        // TODO: Lógica para reenviar código de verificação
    }

    private async void OnVerifyEmailConfirmClicked(object sender, EventArgs e)
    {
        ClearFieldError(VerifyCodeFieldBorder, VerifyCodeError);
        var code = VerifyCodeEntry.Text?.Trim() ?? string.Empty;

        if (code.Length < 6)
        {
            ShowFieldError(VerifyCodeFieldBorder, VerifyCodeError, "Código inválido.");
            return;
        }

        // TODO: Validar e-mail na API e efetivar sessão
        await Shell.Current.GoToAsync("program-selection");
    }

    private async void OnSkipVerifyEmailTapped(object sender, TappedEventArgs e)
    {
        // Permite avançar mantendo o status pendente no backend
        await Shell.Current.GoToAsync("program-selection");
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS DE ERRO
    // ══════════════════════════════════════════════════════════════════════

    private static void ShowFieldError(Border? fieldBorder, Label? errorLabel, string message)
    {
        if (fieldBorder is not null)
        {
            fieldBorder.Stroke = Color.FromArgb("#E53935");
            fieldBorder.StrokeThickness = 2;
        }

        if (errorLabel is not null)
        {
            errorLabel.Text = message;
            errorLabel.IsVisible = true;
        }
    }

    private static void ClearFieldError(Border? fieldBorder, Label? errorLabel)
    {
        if (fieldBorder is not null)
        {
            fieldBorder.Stroke = Colors.Transparent;
            fieldBorder.StrokeThickness = 0;
        }

        if (errorLabel is not null)
            errorLabel.IsVisible = false;
    }

    private void ClearAllRegisterErrors()
    {
        ClearFieldError(NameFieldBorder, NameError);
        ClearFieldError(RegisterEmailFieldBorder, RegisterEmailError);
        ClearFieldError(RegisterPasswordFieldBorder, null);
        ClearFieldError(ConfirmPasswordFieldBorder, ConfirmPasswordError);
        ClearFieldError(null, TermsError);
    }
}