using System.Windows;
using System.Windows.Input;

namespace HDRToggler;

public partial class TrayMenuWindow : Window
{
    private readonly Action _onExit;
    private List<HdrMonitor> _monitors;

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

    public void PositionNearCursor()
    {
        Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
        Arrange(new System.Windows.Rect(DesiredSize));

        var cursor = System.Windows.Forms.Cursor.Position;
        var area   = SystemParameters.WorkArea;

        double x = cursor.X - DesiredSize.Width;
        double y = cursor.Y - DesiredSize.Height - 8;

        Left = Math.Max(0, Math.Min(x, area.Right  - DesiredSize.Width));
        Top  = Math.Max(0, Math.Min(y, area.Bottom - DesiredSize.Height));
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
        Close();
        _onExit();
    }

    private void Window_Deactivated(object sender, EventArgs e) => Close();
}
