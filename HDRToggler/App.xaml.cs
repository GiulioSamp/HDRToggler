using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace HDRToggler;

public partial class App : System.Windows.Application
{
    private NotifyIcon _trayIcon = null!;
    private System.Drawing.Icon? _appIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _appIcon = LoadEmbeddedIcon();

        _trayIcon = new NotifyIcon
        {
            Icon    = _appIcon ?? System.Drawing.SystemIcons.Application,
            Visible = true,
            Text    = "HDR Toggler",
        };

        _trayIcon.MouseClick += TrayIcon_MouseClick;
    }

    private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            OpenMainWindow();
        }
        else if (e.Button == MouseButtons.Right)
        {
            var menu = new TrayMenuWindow(DoExit);
            menu.PositionNearCursor();
            menu.Show();
            menu.Activate();
        }
    }

    private void OpenMainWindow()
    {
        if (_mainWindow is { IsVisible: true })
        {
            _mainWindow.Activate();
            return;
        }

        _mainWindow = new MainWindow(HdrService.GetMonitors());
        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void DoExit()
    {
        _appIcon?.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Shutdown();
    }

    private static System.Drawing.Icon? LoadEmbeddedIcon()
    {
        var asm = Assembly.GetExecutingAssembly();
        var stream = asm.GetManifestResourceStream("HDRToggler.icon.HDRToggle.png");
        if (stream is null) return null;
        using var bmp = new System.Drawing.Bitmap(stream);
        return System.Drawing.Icon.FromHandle(bmp.GetHicon());
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _appIcon?.Dispose();
        _trayIcon.Dispose();
        base.OnExit(e);
    }
}
