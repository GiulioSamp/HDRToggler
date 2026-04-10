using System.Windows;
using System.Windows.Input;

namespace HDRToggler;

public partial class TrayMenuWindow : Window
{
    private readonly Action _onExit;
    private List<HdrMonitor> _monitors;
    private bool _closing;

    public TrayMenuWindow(Action onExit)
    {
        InitializeComponent();
        _onExit = onExit;
        _monitors = HdrService.GetMonitors();
        MonitorList.ItemsSource = _monitors;

        HdrService.StateChanged += OnExternalStateChanged;
        Closed += (_, _) => HdrService.StateChanged -= OnExternalStateChanged;
    }

    // Called when another window triggers a toggle — re-query from Windows
    private void OnExternalStateChanged(object? source)
    {
        if (source == this) return;
        Dispatcher.Invoke(() =>
        {
            _monitors = HdrService.GetMonitors();
            MonitorList.ItemsSource = _monitors;
        });
    }

    /// <summary>
    /// Snapshot cursor position now; actual repositioning happens in Loaded
    /// once the window has a PresentationSource and a real rendered size.
    /// </summary>
    public void ShowNearCursor()
    {
        _cursorSnapshot = System.Windows.Forms.Cursor.Position;

        // Start off-screen so nothing flickers
        Left = -10000;
        Top  = -10000;
        Show();
        Activate();
    }

    private System.Drawing.Point _cursorSnapshot;

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget is null) return;

        // TransformFromDevice converts physical device pixels → WPF logical units
        var transform = source.CompositionTarget.TransformFromDevice;

        var cursor = _cursorSnapshot;
        var screen = System.Windows.Forms.Screen.FromPoint(cursor);
        var work   = screen.WorkingArea; // physical pixels, excludes taskbar

        var curLogical  = transform.Transform(new System.Windows.Point(cursor.X, cursor.Y));
        var workTopLeft = transform.Transform(new System.Windows.Point(work.Left, work.Top));
        var workBotRight = transform.Transform(new System.Windows.Point(work.Right, work.Bottom));

        double w = ActualWidth;
        double h = ActualHeight;

        // Prefer placing above & left of cursor
        double x = curLogical.X - w;
        double y = curLogical.Y - h - 8;

        Left = Math.Max(workTopLeft.X, Math.Min(x, workBotRight.X - w));
        Top  = Math.Max(workTopLeft.Y, Math.Min(y, workBotRight.Y - h));
    }

    private void MonitorItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not HdrMonitor monitor) return;

        // Optimistic update
        _monitors = _monitors
            .Select(m => m.TargetId == monitor.TargetId ? m with { HdrEnabled = !m.HdrEnabled } : m)
            .ToList();
        MonitorList.ItemsSource = _monitors;

        HdrService.ToggleHdr(monitor, source: this);
    }

    private void ExitItem_Click(object sender, MouseButtonEventArgs e)
    {
        _closing = true;
        Close();
        _onExit();
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (!_closing)
            Close();
    }
}
