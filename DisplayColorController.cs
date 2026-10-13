namespace CatVideo;

internal sealed class DisplayColorController : IDisposable
{
    private ColorEffect _original;
    private ColorEffect _applied;
    private bool _needsRestore;
    private bool _disposed;

    public bool IsCatMode { get; private set; }
    public CatColorSettings Settings { get; private set; } = CatColorSettings.Default;

    public DisplayColorController()
    {
        if (!Environment.Is64BitProcess)
            throw new InvalidOperationException("上工需要 64 位元版本，請改用 64 位元的 Cat螢幕轉換器。");
        if (!NativeMethods.MagInitialize())
            throw new InvalidOperationException("色彩轉換功能暫時還沒準備好，請稍後重新開啟 Cat螢幕轉換器。");
    }

    public void EnableCat(CatColorSettings settings)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_needsRestore && !IsCatMode)
            throw new InvalidOperationException("先前的色彩還沒還原，請先按「正常」，再讓貓咪上工。");
        if (!IsCatMode && !NativeMethods.MagGetFullscreenColorEffect(out _original))
            throw new InvalidOperationException("暫時讀不到原本的螢幕色彩，這次先不切換。");

        var previous = IsCatMode ? _applied : _original;
        var requested = settings.CreateEffect();
        _needsRestore = true;
        if (!NativeMethods.MagSetFullscreenColorEffect(ref requested))
        {
            // A failure must not be presented as an active cat mode.
            if (!NativeMethods.MagSetFullscreenColorEffect(ref previous))
                throw new InvalidOperationException("色彩切換暫時卡住了，原本的色彩也沒能還原。請重新登入 Windows，讓螢幕恢復正常。");
            _needsRestore = IsCatMode;
            throw new InvalidOperationException("這次沒切換成功，先維持原本的工作模式。");
        }

        _applied = requested;
        Settings = settings;
        IsCatMode = true;
    }

    public void EnableNormal()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_needsRestore)
            return;
        if (!NativeMethods.MagSetFullscreenColorEffect(ref _original))
            throw new InvalidOperationException("原本的色彩還沒回來，請再按一次「正常」，讓人類接班。");
        IsCatMode = false;
        _needsRestore = false;
    }

    public bool TryRestore()
    {
        if (_disposed || !_needsRestore)
            return true;
        if (!NativeMethods.MagSetFullscreenColorEffect(ref _original))
            return false;
        IsCatMode = false;
        _needsRestore = false;
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        TryRestore();
        NativeMethods.MagUninitialize();
        _disposed = true;
    }
}
