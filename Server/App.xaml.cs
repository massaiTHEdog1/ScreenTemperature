using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTemperature.Hubs;
using ScreenTemperature.Middlewares;
using ScreenTemperature.Services;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using Vernou.Swashbuckle.HttpResultsAdapter;

namespace ScreenTemperature
{
    public partial class App : Application
    {
        private CancellationTokenSource _cancellationTokenSource;
        public static string DataFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ScreenTemperature");

        public App()
        {
#if DEBUG
            Win32.AllocConsole();// add a console for server debug
#endif

            _cancellationTokenSource = new CancellationTokenSource();
            HotKeyManager.Instance.Start(_cancellationTokenSource.Token);// start hotkey manager
            WebApp.Instance.Start(_cancellationTokenSource.Token);// start server
        }

        protected async override void OnExit(ExitEventArgs e)
        {
            _cancellationTokenSource.Cancel();// close server
            await Task.WhenAll(HotKeyManager.Instance.Task, WebApp.Instance.Task);
        }
    }
}