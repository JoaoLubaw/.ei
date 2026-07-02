using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Auth.Platforms.Android;
using Pontuei.App.Services;
using Pontuei.App.Services.Api;
using Pontuei.Shared.Common;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Android.App;
using AndroidX.Credentials.Exceptions;

namespace Pontuei.App.Views;

public partial class AuthPage : ContentPage
{
    // ── State Enum ───────────────────────────────────────────────────
    private enum AuthMode
    {
        Login,
        Register,
        ForgotPasswordEmail,
        ForgotPasswordCode,
        ResetPassword,
        VerifyEmail
    }

    // ── Tab Colors ──────────────────────────────────────────────────────
    private static readonly Color TabGreenLight = Color.FromArgb("#4E8A61");
    private static readonly Color TabGreenDark = Color.FromArgb("#343a46");
    private static readonly Color TabActiveText = Color.FromArgb("#3A6B4A");

    // ── Validation Colors ────────────────────────────────────────────────
    private static readonly Color ErrorRed = Color.FromArgb("#E53935");
    private static readonly Color NormalBg = Color.FromArgb("#3f3838");

    // ── State ────────────────────────────────────────────────────────────
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

    private readonly AuthApiService _authApi;

    // ── Temp variables ────────────────────────────────────────────────────────────
    private string _tempEmail = string.Empty;
    private string _tempResetToken = string.Empty;

    public AuthPage(AuthApiService authApi)
    {
        InitializeComponent();
        _authApi = authApi;

        // 1. Creates the radial background. 
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

        if (FormBlock.Content is Grid originalInnerGrid)
        {
            FormBlock.Content = null;

            Grid wrapperGrid = new Grid();
            wrapperGrid.Children.Add(_radialBgView);
            wrapperGrid.Children.Add(originalInnerGrid);

            FormBlock.Content = wrapperGrid;
        }

        SwitchMode(AuthMode.Login, animate: false);
    }

    // ══════════════════════════════════════════════════════════════════════
    // MODE CONTROL
    // ══════════════════════════════════════════════════════════════════════

    private void SwitchMode(AuthMode mode, bool animate = false)
    {
        _currentMode = mode;

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

            if (animate) _ = _radialBgView.FadeToAsync(0, 300, Easing.CubicInOut);
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

            if (animate) _ = _radialBgView.FadeToAsync(1, 300, Easing.CubicInOut);
            else _radialBgView.Opacity = 1;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // TABS SWITCH 
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
        double targetX = _isLoginTab ? 0d : (TabContainer.Width > 0 ? TabContainer.Width / 2 : 0);
        Color targetLeft = _isLoginTab ? TabGreenLight : TabGreenDark;
        Color targetRight = _isLoginTab ? TabGreenDark : TabGreenLight;

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

        Color startLeft = BlockGradientLeft.Color;
        Color startRight = BlockGradientRight.Color;
        double startX = ActiveTabPill.TranslationX;

        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        Animation animation = new Animation(progress =>
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
        await LoginButton.ScaleToAsync(0.96, 80);
        await LoginButton.ScaleToAsync(1.0, 80);

        ClearFieldError(LoginEmailBorder, null);
        ClearFieldError(LoginPasswordBorder, LoginError);

        string email = EmailEntry.Text?.Trim() ?? string.Empty;
        string password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowFieldError(LoginEmailBorder, null, string.Empty);
            ShowFieldError(LoginPasswordBorder, LoginError, "Preencha o e-mail e a senha.");
            return;
        }

        LoginButton.IsEnabled = false;
        LoginButton.Text = "Entrando...";

        LoginRequestDto request = new LoginRequestDto
        {
            UserEmail = EmailEntry.Text?.Trim() ?? string.Empty,
            Password = PasswordEntry.Text ?? string.Empty,
            RememberMe = RememberMeSwitch.IsToggled,
        };

        LoginButton.IsEnabled = true;
        LoginButton.Text = "Login";

        ApiResponse<LoginResponseDto> response = await _authApi.LoginAsync(request);

        if (response.IsSuccess)
        {
            await Shell.Current.GoToAsync("//home");
        }
        else
        {
            ShowFieldError(LoginEmailBorder, null, string.Empty);
            ShowFieldError(LoginPasswordBorder, LoginError, response.ErrorMessage ?? "Usuário ou senha inválidos. Tente novamente.");
        }
    }

    private void OnLoginTextChanged(object sender, TextChangedEventArgs e)
    {
        if (LoginEmailBorder.StrokeThickness > 0 || LoginPasswordBorder.StrokeThickness > 0)
        {
            ClearFieldError(LoginEmailBorder, null);
            ClearFieldError(LoginPasswordBorder, LoginError);
        }
    }

    private void OnTogglePasswordTapped(object sender, TappedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        PasswordEntry.IsPassword = !_passwordVisible;
        EyeIcon.Source = _passwordVisible ? "eye.svg" : "eye_crossed.svg";
    }

    private void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        SwitchMode(AuthMode.ForgotPasswordEmail, animate: true);
    }

    private async void OnGoogleLoginTapped(object sender, TappedEventArgs e)
    {
        try
        {
#if ANDROID
            Activity? activity = Platform.CurrentActivity;
            if (activity is null) return;

            string? idToken = await Pontuei.App.Platforms.Android.GoogleAuthService.GetGoogleIdTokenAsync(activity);

            if (string.IsNullOrEmpty(idToken))
            {
                await Toast.Make("Não foi possível obter credencial do Google.", ToastDuration.Long, 14).Show();
                return;
            }

            GoogleLoginRequestDto request = new GoogleLoginRequestDto { IdToken = idToken };
            ApiResponse<LoginResponseDto> response = await _authApi.GoogleLoginAsync(request);

            if (!response.IsSuccess) return;

            _ = TryRegisterPushTokenAsync();
            await Shell.Current.GoToAsync("//home");
#else
            await Toast.Make("Login com Google não suportado nesta plataforma.", ToastDuration.Long, 14).Show();
#endif
        }
        catch (GetCredentialCancellationException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GoogleSSO] Error: {ex}");
            await Toast.Make("Login com Google falhou. Verifique se está conectado em uma conta google e tente novamente.", ToastDuration.Long, 14).Show();
        }
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
        RegisterEyeIcon.Source = _registerPasswordVisible ? "eye.svg" : "eye_crossed.svg";
    }

    private void OnToggleConfirmPasswordTapped(object sender, TappedEventArgs e)
    {
        _confirmPasswordVisible = !_confirmPasswordVisible;
        ConfirmPasswordEntry.IsPassword = !_confirmPasswordVisible;
        ConfirmEyeIcon.Source = _confirmPasswordVisible ? "eye.svg" : "eye_crossed.svg";
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
        await RegisterButton.ScaleToAsync(0.96, 80);
        await RegisterButton.ScaleToAsync(1.0, 80);

        ClearAllRegisterErrors();
        bool hasError = false;

        string name = RegisterNameEntry.Text?.Trim() ?? string.Empty;
        string email = RegisterEmailEntry.Text?.Trim() ?? string.Empty;
        string password = RegisterPasswordEntry.Text ?? string.Empty;
        string confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrEmpty(name))
        {
            ShowFieldError(RegisterNameFieldBorder, RegisterNameError, "O nome é obrigatório.");
            hasError = true;
        }

        if (string.IsNullOrEmpty(email))
        {
            ShowFieldError(RegisterEmailFieldBorder, RegisterEmailError, "O e-mail é obrigatório.");
            hasError = true;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowFieldError(RegisterPasswordFieldBorder, RegisterPasswordError, "A senha é obrigatória.");
            hasError = true;
        }

        if (!IsPasswordValid(password))
        {
            ShowFieldError(RegisterPasswordFieldBorder, RegisterPasswordError, "A senha deve conter pelo menos 8 caracteres, incluindo letras maiúsculas, minúsculas, números e caracteres especiais.");
            hasError = true;
        }

        if (password != confirmPassword)
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


        RegisterButton.IsEnabled = false;
        RegisterButton.Text = "Cadastrando...";

        CreateUserRequestDto request = new CreateUserRequestDto
        {
            UserName = name,
            UserEmail = email,
            Password = password,
            ConfirmPassword = confirmPassword,
            UserAcceptedTerms = _termsAccepted,
            UserPushNotificationsEnabled = false,
            UserIsAdmin = false,
            UserEmailNotificationsEnabled = _emailNotificationsAccepted
        };

        ApiResponse<LoginResponseDto> response = await _authApi.RegisterAsync(request);

        RegisterButton.IsEnabled = true;
        RegisterButton.Text = "Cadastrar";

        if (response.IsSuccess)
        {
            _tempEmail = email;
            VerifyEmailAddressSpan.Text = email;

            _ = TryRegisterPushTokenAsync();

            SwitchMode(AuthMode.VerifyEmail, animate: true);
        }
        else
        {
            if (response.ErrorMessage != null && response.ErrorMessage.Contains("e-mail", StringComparison.OrdinalIgnoreCase))
            {
                ShowFieldError(RegisterEmailFieldBorder, RegisterEmailError, response.ErrorMessage);
            }
            else
            {
                ShowFieldError(ConfirmPasswordFieldBorder, ConfirmPasswordError, response.ErrorMessage ?? "Falha ao realizar cadastro. Tente novamente.");
            }
        }
    }

    private async Task TryRegisterPushTokenAsync()
    {
        try
        {
            PermissionStatus status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted) return;

            string? token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return;

            await _authApi.UpdatePushTokenAsync(new UpdatePushNotificationTokenRequestDto
            {
                PushNotificationToken = token
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FCM] Push token registration failed: {ex.Message}");
        }
    }

    private void OnRegisterTextChanged(object sender, TextChangedEventArgs e)
    {
        if (RegisterNameFieldBorder.StrokeThickness > 0 ||
            RegisterEmailFieldBorder.StrokeThickness > 0 ||
            RegisterPasswordFieldBorder.StrokeThickness > 0 ||
            ConfirmPasswordFieldBorder.StrokeThickness > 0)
        {
            ClearRegisterErrors();
        }
    }

    private void ClearRegisterErrors()
    {
        ClearFieldError(RegisterNameFieldBorder, RegisterNameError);
        ClearFieldError(RegisterEmailFieldBorder, RegisterEmailError);
        ClearFieldError(RegisterPasswordFieldBorder, RegisterPasswordError);
        ClearFieldError(ConfirmPasswordFieldBorder, ConfirmPasswordError);
        ClearFieldError(null, TermsError); // Se houver erro de termos chumbado
    }

    // ══════════════════════════════════════════════════════════════════════
    // FORGOT PASSWORD
    // ══════════════════════════════════════════════════════════════════════

    private async void OnForgotPasswordSendClicked(object sender, EventArgs e)
    {
        ClearFieldError(ForgotEmailFieldBorder, ForgotEmailError);
        string email = ForgotEmailEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(email) || !ValidationUtils.IsValidEmail(email))
        {
            ShowFieldError(ForgotEmailFieldBorder, ForgotEmailError, "Digite um e-mail válido.");
            return;
        }

        _tempEmail = email;
        ApiResponse<EmptyDto> response = await _authApi.ForgotPasswordAsync(new ForgotPasswordRequestDto { UserEmail = email });

        if (response.IsSuccess)
        {
            SwitchMode(AuthMode.ForgotPasswordCode, animate: true);
        }
        else
        {
            ShowFieldError(ForgotEmailFieldBorder, ForgotEmailError, response.ErrorMessage!);
        }
    }

    private async void OnResendForgotCodeTapped(object sender, TappedEventArgs e)
    {
        ApiResponse<EmptyDto> response = await _authApi.ForgotPasswordAsync(new ForgotPasswordRequestDto { UserEmail = _tempEmail });

        if (response.IsSuccess)
        {
            await Toast.Make("Código reenviado!", ToastDuration.Short).Show();
            _ = StartCooldownAsync(ResendForgotLabel, "Reenviar código");
        }
        else
        {
            await Toast.Make(response.ErrorMessage ?? "Erro ao reenviar.", ToastDuration.Short).Show();
        }
    }

    private async void OnForgotPasswordConfirmCodeClicked(object sender, EventArgs e)
    {
        ClearFieldError(ForgotCodeFieldBorder, ForgotCodeError);
        string code = ForgotCodeEntry.Text?.Trim() ?? string.Empty;

        if (code.Length < 6)
        {
            ShowFieldError(ForgotCodeFieldBorder, ForgotCodeError, "Código inválido.");
            return;
        }

        VerifyResetCodeRequestDto request = new VerifyResetCodeRequestDto { UserEmail = _tempEmail, Code = code };
        ApiResponse<VerifyResetCodeResponseDto> response = await _authApi.VerifyResetCodeAsync(request);

        if (response.IsSuccess && response.Data != null)
        {
            _tempResetToken = response.Data.ResetToken;
            SwitchMode(AuthMode.ResetPassword, animate: true);
        }
        else
        {
            ShowFieldError(ForgotCodeFieldBorder, ForgotCodeError, response.ErrorMessage!);
        }
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
        ResetEyeIcon.Source = _resetPasswordVisible ? "eye.svg" : "eye_crossed.svg";
    }

    private void OnToggleResetConfirmTapped(object sender, TappedEventArgs e)
    {
        _resetConfirmVisible = !_resetConfirmVisible;
        ResetConfirmEntry.IsPassword = !_resetConfirmVisible;
        ResetConfirmEyeIcon.Source = _resetConfirmVisible ? "eye.svg" : "eye_crossed.svg";
    }

    private async void OnResetPasswordSubmitClicked(object sender, EventArgs e)
    {
        ClearFieldError(ResetPasswordFieldBorder, null);
        ClearFieldError(ResetConfirmFieldBorder, ResetConfirmError);

        string pwd = ResetPasswordEntry.Text ?? string.Empty;
        if (!IsPasswordValid(pwd))
        {
            ShowFieldError(ResetPasswordFieldBorder, null, string.Empty);
            return;
        }

        string confirmPwd = ResetConfirmEntry.Text ?? string.Empty;
        if (pwd != confirmPwd)
        {
            ShowFieldError(ResetConfirmFieldBorder, ResetConfirmError, "As senhas não correspondem.");
            return;
        }

        ResetPasswordRequestDto request = new ResetPasswordRequestDto
        {
            ResetToken = _tempResetToken,
            NewPassword = ResetPasswordEntry.Text!,
            ConfirmNewPassword = ResetConfirmEntry.Text!
        };

        ApiResponse<EmptyDto> response = await _authApi.ResetPasswordAsync(request);

        if (response.IsSuccess)
        {
            await Toast.Make("Sua senha foi redefinida. Faça o login para continuar.", ToastDuration.Long, 14).Show();
            SwitchMode(AuthMode.Login, animate: true);
        }
        else
        {
            ShowFieldError(ResetConfirmFieldBorder, ResetConfirmError, response.ErrorMessage!);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // VERIFICAÇÃO DE E-MAIL (PÓS CADASTRO)
    // ══════════════════════════════════════════════════════════════════════

    // No AuthPage.xaml.cs
    private async void OnResendVerifyCodeTapped(object sender, TappedEventArgs e)
    {
        ApiResponse<EmptyDto> response = await _authApi.ResendVerificationEmailAsync(new ResendVerificationEmailRequestDto { UserEmail = _tempEmail });

        if (response.IsSuccess)
        {
            await Toast.Make("E-mail reenviado com sucesso.", ToastDuration.Short).Show();
            _ = StartCooldownAsync(ResendVerifyLabel, "Reenviar e-mail");
        }
        else
        {
            await Toast.Make(response.ErrorMessage ?? "Erro ao reenviar.", ToastDuration.Short).Show();
        }
    }

    private async void OnVerifyEmailConfirmClicked(object sender, EventArgs e)
    {
        ClearFieldError(VerifyCodeFieldBorder, VerifyCodeError);
        string code = VerifyCodeEntry.Text?.Trim() ?? string.Empty;

        if (code.Length < 6)
        {
            ShowFieldError(VerifyCodeFieldBorder, VerifyCodeError, "Código inválido.");
            return;
        }

        ApiResponse<EmptyDto> response = await _authApi.VerifyEmailAsync(new VerifyEmailRequestDto { Code = code });

        if (response.IsSuccess)
        {
            await Shell.Current.GoToAsync("program-selection");
        }
        else
        {
            ShowFieldError(VerifyCodeFieldBorder, VerifyCodeError, response.ErrorMessage!);
        }
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
        ClearFieldError(RegisterNameFieldBorder, RegisterNameError);
        ClearFieldError(RegisterEmailFieldBorder, RegisterEmailError);
        ClearFieldError(RegisterPasswordFieldBorder, RegisterPasswordError);
        ClearFieldError(ConfirmPasswordFieldBorder, ConfirmPasswordError);
        ClearFieldError(null, TermsError);
    }

    private async Task<bool> RequestPushPermissionAsync()
    {
        PermissionStatus status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        return status == PermissionStatus.Granted;
    }

    private async Task RegisterFcmTokenAsync()
    {
        try
        {
            string? token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                await _authApi.UpdatePushTokenAsync(new UpdatePushNotificationTokenRequestDto
                {
                    PushNotificationToken = token
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FCM] Token registration failed: {ex}");
        }
    }

    private async Task StartCooldownAsync(Label label, string originalText, int seconds = 60)
    {
        label.IsEnabled = false;
        label.TextColor = Colors.Gray;

        for (int i = seconds; i > 0; i--)
        {
            label.Text = $"Reenviar em {i}s";
            await Task.Delay(1000);
        }

        label.Text = originalText;
        label.IsEnabled = true;
        label.TextColor = Color.FromArgb("#427A5B");
    }
}