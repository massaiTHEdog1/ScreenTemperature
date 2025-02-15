using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScreenTemperature.Services;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Vernou.Swashbuckle.HttpResultsAdapter;
using static ScreenTemperature.Win32;


namespace ScreenTemperature
{
    public partial class MainWindow : Window
    {
        private readonly NotifyIcon _notifyIcon;
        

        public MainWindow()
        {
            InitializeComponent();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = ScreenTemperature.Resources.icon;
            _notifyIcon.Visible = false;
            _notifyIcon.Text = "ScreenTemperature";
            _notifyIcon.Click += NotifyIconOnClick;
        }

        private void Window_OnLoaded(object sender, RoutedEventArgs e)// executed after webapp is started
        {
#if !DEBUG
            WindowState = WindowState.Minimized;
#endif
        }

        ~MainWindow()
        {
            _notifyIcon.Click -= NotifyIconOnClick;
            _notifyIcon.Visible = false;
            _notifyIcon.Icon?.Dispose();
            _notifyIcon.Dispose();
        }

        private void NotifyIconOnClick(object? sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            Show();
            Activate();
            WindowState = WindowState.Normal;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                _notifyIcon.Visible = true;
                Hide();
            }
        }
    }
}