using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using ScreenTemperature.Services;
using System.Windows;
using System.Windows.Forms;


namespace ScreenTemperature
{
    public partial class MainWindow : Window
    {
        private readonly NotifyIcon _notifyIcon;

#if DEBUG
        public string SourceUrl { get; set; } = "http://localhost:5173/";
#else
        public string SourceUrl { get; set; } = "http://localhost:61983/";
#endif

        public MainWindow()
        {
            InitializeComponent();
            _ = InitializeWebView();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = ScreenTemperature.Resources.icon;
            _notifyIcon.Visible = false;
            _notifyIcon.Text = "ScreenTemperature";
            _notifyIcon.Click += NotifyIconOnClick;
        }

        private async Task InitializeWebView()
        {
            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: App.DataFolder);
            var options = env.CreateCoreWebView2ControllerOptions();
            options.IsInPrivateModeEnabled = true;

            await webView.EnsureCoreWebView2Async(env, options);
            webView.Source = new Uri(SourceUrl);
        }

        private async void Window_OnLoaded(object sender, RoutedEventArgs e)
        {
            // wait until server is built
            await Task.Run(async () => {
                while(WebApp.Instance.WebApplication == null) { await Task.Delay(25);  }
            });

            using(var scope = WebApp.Instance.WebApplication.Services.CreateScope())
            {
                var parametersService = scope.ServiceProvider.GetRequiredService<IParametersService>();

                var parameters = await parametersService.GetParametersAsync();

                if(parameters.MinimizeOnStartup)
                    WindowState = WindowState.Minimized;
            }
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