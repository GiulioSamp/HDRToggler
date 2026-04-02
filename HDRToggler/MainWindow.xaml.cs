using System.Windows;
using System.Windows.Input;

namespace HDRToggler;

public partial class MainWindow : Window
{
    private List<HdrMonitor> _monitors;

    public MainWindow(IEnumerable<HdrMonitor> monitors)
    {
        InitializeComponent();
        _monitors = monitors.ToList();
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

    private void Root_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.Source == Root && e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void MonitorBox_Click(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (((FrameworkElement)sender).DataContext is not HdrMonitor monitor) return;

        // Optimistic update — flip immediately without waiting for Windows
        _monitors = _monitors
            .Select(m => m.TargetId == monitor.TargetId ? m with { HdrEnabled = !m.HdrEnabled } : m)
            .ToList();
        MonitorList.ItemsSource = _monitors;

        HdrService.ToggleHdr(monitor, source: this); // notifies other windows
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
