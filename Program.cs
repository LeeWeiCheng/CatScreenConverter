using System.Windows;

namespace CatVideo;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var instance = new Mutex(true, @"Local\CatVideo.DisplayColor", out var firstInstance);
        if (!firstInstance)
        {
            MessageBox.Show("Cat螢幕轉換器已經在值班了，請回到原本的視窗。", "Cat螢幕轉換器");
            return;
        }

        DisplayColorController? display = null;
        try
        {
            var application = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
            application.DispatcherUnhandledException += (_, args) =>
            {
                var restored = display?.TryRestore() ?? true;
                MessageBox.Show($"值班時遇到狀況，程式將先關閉。\n\n{args.Exception.Message}",
                    "Cat螢幕轉換器", MessageBoxButton.OK, MessageBoxImage.Error);
                if (!restored)
                    MessageBox.Show("原本的色彩沒能還原，請重新登入 Windows，讓螢幕恢復正常。", "Cat螢幕轉換器",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                args.Handled = true;
                application.Shutdown(1);
            };
            AppDomain.CurrentDomain.UnhandledException += (_, _) => display?.TryRestore();
            display = new DisplayColorController();
            application.Run(new MainWindow(display));
        }
        catch (Exception exception)
        {
            display?.TryRestore();
            MessageBox.Show(exception.Message, "上工準備暫時沒完成", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            display?.Dispose();
            instance.ReleaseMutex();
        }
    }
}
