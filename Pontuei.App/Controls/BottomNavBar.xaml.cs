namespace Pontuei.App.Controls;

public partial class BottomNavBar : ContentView
{
    // ── Cores do gradiente ────────────────────────────────────────────────
    private static readonly Color GradientDark = Color.FromArgb("#343a46");
    private static readonly Color GradientLight = Color.FromArgb("#4E8A61");

    // ── Estado ────────────────────────────────────────────────────────────
    private NavTab _currentTab = NavTab.Home;
    private bool _isAnimating;

    public enum NavTab { Home, Notifications, History, Details, Settings }

    public BottomNavBar()
    {
        InitializeComponent();
        ApplyTabState(animate: false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private Page? CurrentPage =>
        Application.Current?.Windows.FirstOrDefault()?.Page;

    // ── Tap handlers ──────────────────────────────────────────────────────

    private async void OnFabTapped(object sender, TappedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"FAB Tapped! Current Tab: {_currentTab}");

        if (_currentTab == NavTab.Home)
        {
            await Shell.Current.GoToAsync("transaction-detail");
        }
        else
        {
            await NavigateToTab(NavTab.Home);
        }
    }

    private async void OnViewPastTransactionsTapped(object sender, TappedEventArgs e)
    {
        if (_currentTab == NavTab.History || _isAnimating) return;
        await NavigateToTab(NavTab.History);
    }

    private async void OnViewNotificationsTapped(object sender, TappedEventArgs e)
    {
        if (_currentTab == NavTab.Notifications || _isAnimating) return;
        await NavigateToTab(NavTab.Notifications);
    }

    // ── Navegação ─────────────────────────────────────────────────────────

    private async Task NavigateToTab(NavTab tab)
    {
        _currentTab = tab;
        _ = ApplyTabStateAsync(animate: true);

        if (tab == NavTab.Notifications)
            await Shell.Current.GoToAsync("//notifications");
        else if (tab == NavTab.History)
            await Shell.Current.GoToAsync("//history");
        else
            await Shell.Current.GoToAsync("//home");
    }

    // ── Estado visual ─────────────────────────────────────────────────────

    /// <summary>
    /// Versão síncrona usada no construtor (sem animação).
    /// </summary>
    private void ApplyTabState(bool animate)
    {
        bool isNotifications = _currentTab == NavTab.Notifications;

        // Gradiente: claro no lado onde está a tab ativa
        NavGradientLeft.Color = isNotifications ? GradientDark : GradientDark;
        NavGradientRight.Color = isNotifications ? GradientLight : GradientDark;

        // Na Home o centro do gradiente já é claro pela stop do meio (#4E8A61 offset 0.5)
        // Não alteramos a stop do meio para preservar o brilho central quando na home.

        // Ícones da campainha
        BellIcon.IsVisible = !isNotifications;
        BellSelectedIcon.IsVisible = isNotifications;

        // FAB
        FabPlusLabel.IsVisible = !isNotifications;
        FabHomeIcon.IsVisible = isNotifications;

        // Badge: esconde quando está na tela de notificações
        NotificationBadge.IsVisible = !isNotifications;
    }

    /// <summary>
    /// Versão assíncrona com animação de gradiente (igual ao padrão da AuthPage).
    /// </summary>
    private async Task ApplyTabStateAsync(bool animate)
    {
        bool isNotifications = _currentTab == NavTab.Notifications;
        bool isHistory = _currentTab == NavTab.History;

        // Atualiza ícones imediatamente
        HistoryIcon.IsVisible = !isHistory;
        HistorySelectedIcon.IsVisible = isHistory;
        BellIcon.IsVisible = !isNotifications;
        BellSelectedIcon.IsVisible = isNotifications;

        FabPlusLabel.IsVisible = _currentTab == NavTab.Home;
        FabHomeIcon.IsVisible = _currentTab != NavTab.Home;
        NotificationBadge.IsVisible = !isNotifications; // se existir badge

        // Cores alvo do gradiente - History acende o lado ESQUERDO
        var targetLeft = isHistory ? GradientLight : GradientDark;
        var targetRight = isNotifications ? GradientLight : GradientDark;

        if (!animate)
        {
            NavGradientLeft.Color = targetLeft;
            NavGradientRight.Color = targetRight;
            return;
        }

        _isAnimating = true;

        var startLeft = NavGradientLeft.Color;
        var startRight = NavGradientRight.Color;

        var tcs = new TaskCompletionSource<bool>();
        var animation = new Animation(progress =>
        {
            NavGradientLeft.Color = InterpolateColor(startLeft, targetLeft, progress);
            NavGradientRight.Color = InterpolateColor(startRight, targetRight, progress);
        }, 0, 1, Easing.CubicInOut);

        animation.Commit(
                this,
                "NavGradientAnimation",
                length: 150,
                finished: (_, _) => tcs.TrySetResult(true));

        await tcs.Task;

        // Garante estado final exato
        NavGradientLeft.Color = targetLeft;
        NavGradientRight.Color = targetRight;

        _isAnimating = false;
    }

    // ── Interpolação de cor (mesmo padrão da AuthPage) ────────────────────

    private static Color InterpolateColor(Color from, Color to, double t) =>
        Color.FromRgba(
            from.Red + (to.Red - from.Red) * t,
            from.Green + (to.Green - from.Green) * t,
            from.Blue + (to.Blue - from.Blue) * t,
            from.Alpha + (to.Alpha - from.Alpha) * t);

    // ── API pública (para a página host resetar o estado) ─────────────────

    /// <summary>
    /// Deve ser chamado pela página host ao aparecer (OnAppearing),
    /// para garantir que o estado da navbar reflita a página atual.
    /// </summary>
    public async Task SetActiveTab(NavTab tab, bool animate = true)
    {
        // if (_currentTab == tab) return;

        _currentTab = tab;
        await ApplyTabStateAsync(animate);
    }

}