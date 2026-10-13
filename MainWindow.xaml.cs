using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;

namespace CatVideo;

public partial class MainWindow : Window
{
    private const int RestoreHotKey = 1;
    private readonly DisplayColorController _display;
    private HwndSource? _source;
    private IntPtr _windowHandle;
    private bool _hotKeyRegistered;
    private bool _ready;
    private bool _updating;

    internal MainWindow(DisplayColorController display)
    {
        _display = display;
        InitializeComponent();
        Icon = CatDrawing.CreateIcon();
        _ready = true;
        UpdateValues();
        UpdateMode();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        _windowHandle = new WindowInteropHelper(this).Handle;
        CatWindowChrome.Apply(_windowHandle);
        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(WindowMessage);
        _hotKeyRegistered = NativeMethods.RegisterHotKey(_windowHandle, RestoreHotKey, 0x4003, 0x4E);
        if (!_hotKeyRegistered)
            ShortcutText.Text = "快捷鍵暫時不能用，請按「正常」交回人類值班。";
    }

    private IntPtr WindowMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == 0x0312 && wParam.ToInt32() == RestoreHotKey)
        {
            ChangeMode(false);
            handled = true;
        }
        return IntPtr.Zero;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        try { _display.EnableNormal(); }
        catch (Exception exception) { e.Cancel = true; ShowError(exception); }
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_hotKeyRegistered) NativeMethods.UnregisterHotKey(_windowHandle, RestoreHotKey);
        _source?.RemoveHook(WindowMessage);
        base.OnClosed(e);
    }

    private void Normal_Click(object sender, RoutedEventArgs e) => ChangeMode(false);
    private void Cat_Click(object sender, RoutedEventArgs e) => ChangeMode(true);

    private void ChangeMode(bool cat)
    {
        try
        {
            if (cat) _display.EnableCat(ReadSettings());
            else _display.EnableNormal();
        }
        catch (Exception exception) { ShowError(exception); }
        finally { UpdateMode(); }
    }

    private void SettingsChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Initial XAML values are set before all named controls exist.
        if (!_ready) return;
        UpdateValues();
        if (!_updating) ApplySettings();
    }

    private void ApplySettings()
    {
        if (!_display.IsCatMode) return;
        var previous = _display.Settings;
        try { _display.EnableCat(ReadSettings()); }
        catch (Exception exception) { SetSliders(previous); ShowError(exception); }
    }

    private void Defaults_Click(object sender, RoutedEventArgs e)
    {
        SetSliders(CatColorSettings.Default);
        ApplySettings();
    }

    private CatColorSettings ReadSettings() => new((int)Math.Round(StrengthSlider.Value),
        (int)Math.Round(BrightnessSlider.Value), (int)Math.Round(ContrastSlider.Value));

    private void SetSliders(CatColorSettings settings)
    {
        _updating = true;
        try
        {
            StrengthSlider.Value = settings.Strength;
            BrightnessSlider.Value = settings.Brightness;
            ContrastSlider.Value = settings.Contrast;
            UpdateValues();
        }
        finally { _updating = false; }
    }

    private void UpdateValues()
    {
        StrengthValue.Text = $"{Math.Round(StrengthSlider.Value)}%";
        BrightnessValue.Text = $"{Math.Round(BrightnessSlider.Value)}%";
        ContrastValue.Text = $"{Math.Round(ContrastSlider.Value)}%";
    }

    private void UpdateMode()
    {
        NormalMode.IsChecked = !_display.IsCatMode;
        CatMode.IsChecked = _display.IsCatMode;
        StatusText.Text = _display.IsCatMode ? "貓咪已上工，專用色彩準備就緒。" : "人類值班中，畫面維持正常色彩。";
    }

    private void ShowError(Exception exception) => MessageBox.Show(this, exception.Message,
        "工作模式切換暫時卡住了", MessageBoxButton.OK, MessageBoxImage.Warning);
}
