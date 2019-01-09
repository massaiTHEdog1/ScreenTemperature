using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ScreenTemperature.Classes;
using Microsoft.Win32;
using ScreenTemperature.Services;
using ScreenTemperature.Services.Interfaces;
using Color = System.Windows.Media.Color;
using Container = SimpleInjector.Container;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TimeoutException = System.TimeoutException;

namespace ScreenTemperature
{
	/// <summary>
	/// Logique d'interaction pour MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{
		#region Variables

		private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

		private NamedPipeServerStream _pipeServer;
		private readonly NotifyIcon _notifyIcon = new NotifyIcon();

		private IntPtr _windowHandle;
		private int _kelvinValue = 6600;
		private ObservableCollection<Config> _configs;
		private ObservableCollection<Monitor> _monitors;
		private int _selectedConfigIndex;
		private Config _selectedConfig;
		private Monitor _selectedMonitor;
		private string _textNameConfig;
		private bool _isWaitingForKeyInput;
		private bool _isCheckboxStartAtSystemStartupChecked;
		private bool _isRadioButtonUseTannerHellandAlgorithmChecked;
		private bool _isRadioButtonUseImageChecked;
		private int _imageSliderValue = 1000;
		private Color _selectedColor = new Color() { A = 255, R = 255, G = 186, B = 127 };
		private BitmapImage _imageGradient;

		#region Commands

		public ICommand AssignKeyToConfigCommand { get; private set; }
		public ICommand SaveConfigCommand { get; private set; }
		public ICommand DeleteConfigCommand { get; private set; }
		public ICommand MoveConfigUpCommand { get; private set; }
		public ICommand MoveConfigDownCommand { get; private set; } 
        public ICommand RefreshMonitorsCommand { get; private set; }

        #endregion

        #region Services

        private readonly IConfigService _configService;
		private readonly IScreenColorService _temperatureService;
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly IMonitorService _monitorService;

		#endregion


		#endregion

		#region Properties

		/// <summary>
		/// Kelvin slider's value
		/// </summary>
		public int KelvinValue
		{
			get { return _kelvinValue; }
			set
			{
				_kelvinValue = value;
				NotifyPropertyChanged("KelvinValue");

				_temperatureService.ChangeScreenColorFromKelvin(value, SelectedMonitor);
			}
		}

		/// <summary>
		/// List of available configs
		/// </summary>
		public ObservableCollection<Config> Configs
		{
			get
			{
				return _configs;
			}
			set
			{
				_configs = value;
				NotifyPropertyChanged("Configs");
			}
		}

		/// <summary>
		/// Index of the selected config
		/// </summary>
		public int SelectedConfigIndex
		{
			get { return _selectedConfigIndex; }
			set
			{
				_selectedConfigIndex = value;
				NotifyPropertyChanged("SelectedConfigIndex");
			}
		}

		/// <summary>
		/// The selected config
		/// </summary>
		public Config SelectedConfig
		{
			get { return _selectedConfig; }
			set
			{
				_selectedConfig = value;
				NotifyPropertyChanged("SelectedConfig");

				if (value != null)
				{
					TextNameConfig = value.ConfigName;
					_temperatureService.ChangeScreenColorFromConfig(value);
				}
				else
				{
					TextNameConfig = "";
				}
			}
		}

		/// <summary>
		/// List of available monitors
		/// </summary>
		public ObservableCollection<Monitor> Monitors
		{
			get
			{
				return _monitors;
			}
			set
			{
				_monitors = value;
				NotifyPropertyChanged("Monitors");
			}
		}

		/// <summary>
		/// The selected monitor
		/// </summary>
		public Monitor SelectedMonitor
		{
			get { return _selectedMonitor; }
			set
			{
				_selectedMonitor = value;
				NotifyPropertyChanged("SelectedMonitor");
			}
		}

		/// <summary>
		/// Value of textbox for the name of the config
		/// </summary>
		public string TextNameConfig
		{
			get { return _textNameConfig; }
			set
			{
				_textNameConfig = value;
				NotifyPropertyChanged("TextNameConfig");
			}
		}

		/// <summary>
		/// Are we waiting for an input?
		/// </summary>
		public bool IsWaitingForKeyInput
		{
			get { return _isWaitingForKeyInput; }
			set
			{
				_isWaitingForKeyInput = value;
				NotifyPropertyChanged("IsWaitingForKeyInput");
			}
		}

		/// <summary>
		/// Start the software at system startup?
		/// </summary>
		public bool IsCheckboxStartAtSystemStartupChecked
		{
			get { return _isCheckboxStartAtSystemStartupChecked; }
			set
			{
				_isCheckboxStartAtSystemStartupChecked = value;

				if (value)
				{
					RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);

					if (key != null)
					{
						string applicationPath = Assembly.GetExecutingAssembly().Location;

						string applicationName = Process.GetCurrentProcess().ProcessName;

						key.SetValue(applicationName, applicationPath);

						key.Close();
					}
				}
				else
				{
					RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);

					if (key != null)
					{
						string applicationName = Process.GetCurrentProcess().ProcessName;

						object subKey = key.GetValue(applicationName);

						if (subKey != null)
						{
							key.DeleteValue(applicationName);
						}

						key.Close();
					}
				}

				NotifyPropertyChanged("IsCheckboxStartAtSystemStartupChecked");
			}
		}

		public bool IsRadioButtonUseTannerHellandAlgorithmChecked
		{
			get { return _isRadioButtonUseTannerHellandAlgorithmChecked; }
			set
			{
				_isRadioButtonUseTannerHellandAlgorithmChecked = value;
				NotifyPropertyChanged("IsRadioButtonUseTannerHellandAlgorithmChecked");
			}
		}

		public bool IsRadioButtonUseImageChecked
		{
			get { return _isRadioButtonUseImageChecked; }
			set
			{
				_isRadioButtonUseImageChecked = value;
				NotifyPropertyChanged("IsRadioButtonUseImageChecked");
			}
		}

		/// <summary>
		/// Image slider's value
		/// </summary>
		public int ImageSliderValue
		{
			get { return _imageSliderValue; }
			set
			{
				_imageSliderValue = value;
				NotifyPropertyChanged("ImageSliderValue");

				_temperatureService.ChangeScreenColorFromImage(value, SelectedMonitor, ImageGradient);
			}
		}

		public Color SelectedColor
		{
			get { return _selectedColor; }
			set
			{
				_selectedColor = value;
				NotifyPropertyChanged("SelectedColor");

				using (Bitmap bitmap = new Bitmap(1000, 1))
				using (Graphics graphics = Graphics.FromImage(bitmap))
				using (LinearGradientBrush brush = new LinearGradientBrush(
					new Rectangle(0, 0, bitmap.Width, bitmap.Height),
					System.Drawing.Color.FromArgb(SelectedColor.A, SelectedColor.R, SelectedColor.G, SelectedColor.B),
					System.Drawing.Color.White,
					LinearGradientMode.Horizontal))
				using (MemoryStream memory = new MemoryStream())
				{
					graphics.FillRectangle(brush, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
					bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
					memory.Position = 0;
					BitmapImage bitmapimage = new BitmapImage();
					bitmapimage.BeginInit();
					bitmapimage.StreamSource = memory;
					bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapimage.EndInit();

					ImageGradient = bitmapimage;
				}
			}
		}

		public BitmapImage ImageGradient
		{
			get { return _imageGradient; }
			set
			{
				_imageGradient = value;
				NotifyPropertyChanged("ImageGradient");
			}
		}

		#endregion

		#region DLLs

		[DllImport("User32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("User32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		#endregion

		#region Constructor

		public MainWindow()
		{
			var container = new Container();
			container.Register<IConfigService, ConfigService>();
			container.Register<IScreenColorService, ScreenColorService>();
			container.Register<IMonitorService, MonitorService>();
			container.Verify();

			_configService = container.GetInstance<IConfigService>();
			_temperatureService = container.GetInstance<IScreenColorService>();
			_monitorService = container.GetInstance<IMonitorService>();

			Monitors = new ObservableCollection<Monitor>(_monitorService.GetMonitors());
			SelectedMonitor = Monitors.FirstOrDefault();

			bool exists = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Length > 1;

			if (exists)//If this program already has an instance
			{
				try
				{
					NamedPipeClientStream pipeStream = new NamedPipeClientStream(".", "instance", PipeDirection.Out, PipeOptions.None);

					pipeStream.Connect();

					Environment.Exit(0);
				}
				catch (TimeoutException oEx)
				{
					Debug.WriteLine(oEx.Message);
				}
			}
			else//If the program is not already running
			{
				_pipeServer = new NamedPipeServerStream("instance", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				_pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, _pipeServer);
			}

			AssignKeyToConfigCommand = new RelayCommand(AssignKeyToConfig);
			SaveConfigCommand = new RelayCommand(SaveConfig);
			DeleteConfigCommand = new RelayCommand(DeleteConfig);
			MoveConfigUpCommand = new RelayCommand(MoveConfigUp);
			MoveConfigDownCommand = new RelayCommand(MoveConfigDown);
		    RefreshMonitorsCommand = new RelayCommand(RefreshMonitors);


            _notifyIcon.Icon = Properties.Resources.icon;
			_notifyIcon.Click += NotifyIconOnClick;
			_notifyIcon.Visible = true;

			SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
			SystemEvents.UserPreferenceChanging += SystemEvents_UserPreferenceChanging;
			SystemEvents.PaletteChanged += SystemEvents_PaletteChanged;
			SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

			InitializeComponent();
		}

		~MainWindow()
		{
			SystemEvents.UserPreferenceChanging -= SystemEvents_UserPreferenceChanging;
			SystemEvents.PaletteChanged -= SystemEvents_PaletteChanged;
			SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
			SystemEvents.SessionSwitch -= SystemEventsOnSessionSwitch;

			_notifyIcon.Click -= NotifyIconOnClick;
			_notifyIcon.Visible = false;
			_notifyIcon.Icon.Dispose();
			_notifyIcon.Dispose();
		}

		#endregion

		#region Events

		private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
		{
			_temperatureService.ChangeScreenColorFromConfig((SelectedConfig));
		}

		private void SystemEvents_PaletteChanged(object sender, EventArgs e)
		{
			_temperatureService.ChangeScreenColorFromConfig(SelectedConfig);
		}

		private void SystemEvents_UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
		{
			_temperatureService.ChangeScreenColorFromConfig(SelectedConfig);
		}

		/// <summary>
		/// When we log in to the session
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="sessionSwitchEventArgs"></param>
		private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs sessionSwitchEventArgs)
		{
			if (sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionUnlock || sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionLogon)
			{
				_temperatureService.ChangeScreenColorFromConfig(SelectedConfig);
			}
		}

		/// <summary>
		/// When we click on notifyIcon
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void NotifyIconOnClick(object sender, EventArgs eventArgs)
		{
			_notifyIcon.Visible = false;
			Show();
			Activate();
			WindowState = WindowState.Normal;
		}

		/// <summary>
		/// Wait for another instance of the application
		/// </summary>
		/// <param name="iar"></param>
		private void WaitForConnectionCallBack(IAsyncResult iar)
		{
			try
			{
				_pipeServer.EndWaitForConnection(iar);

				_pipeServer.Close();
				_pipeServer = null;

				Dispatcher.Invoke(() =>
				{
					if (SelectedConfigIndex == -1)
					{
						SelectedConfigIndex = 0;
					}
					else
					{
						if (SelectedConfigIndex + 1 == Configs.Count)
						{
							SelectedConfigIndex = 0;
						}
						else
						{
							SelectedConfigIndex++;
						}
					}
				});

				_pipeServer = new NamedPipeServerStream("instance", PipeDirection.In,
				   1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				// On continue en boucle
				_pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, _pipeServer);
			}
			catch (Exception e)
			{
				MessageBox.Show($"Une erreur est survenue dans le pipe.\r\n{e.Message}", "Erreur");
			}
		}

		private void MainForm_StateChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
			{
				_notifyIcon.Visible = true;
				Hide();
			}
		}

		private void Window_OnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			if (IsWaitingForKeyInput)
			{
				if (keyEventArgs.Key == Key.Escape)
				{

				}
				else if (keyEventArgs.Key == Key.Back)
				{
					if (SelectedConfig.KeyBinding != null)
					{
						UnregisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key));
					}

					SelectedConfig.KeyBinding = null;
					_configService.SaveConfig(SelectedConfig);
				}
				else
				{
					if (SelectedConfig.KeyBinding != null)
					{
						UnregisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key));
					}

					SelectedConfig.KeyBinding = new KeyData(keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key, Keyboard.IsKeyDown(Key.LeftShift), Keyboard.IsKeyDown(Key.LeftAlt), Keyboard.IsKeyDown(Key.LeftCtrl));
					_configService.SaveConfig(SelectedConfig);

					uint mask = SelectedConfig.KeyBinding.Alt ? (uint)0x0001 : 0;
					mask = mask | (SelectedConfig.KeyBinding.Control ? (uint)0x0002 : 0);
					mask = mask | (SelectedConfig.KeyBinding.Shift ? (uint)0x0004 : 0);
					mask = mask | 0x4000;

					int virtualKeyCode = KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key);

					if (!RegisterHotKey(_windowHandle, virtualKeyCode, mask, (uint)virtualKeyCode))
					{
						MessageBox.Show("Cannot bind key " + SelectedConfig.KeyBinding);
					}
				}

				IsWaitingForKeyInput = false;
			}
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			IsRadioButtonUseTannerHellandAlgorithmChecked = true;

			SelectedColor = _selectedColor;

			Configs = new ObservableCollection<Config>(_configService.GetConfigs().OrderBy(conf => conf.Order));

			if (Configs.Count > 0)
			{
				SelectedConfig = Configs[0];
			}



			RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);

			if (key != null)
			{
				string applicationName = Process.GetCurrentProcess().ProcessName;

				object subKey = key.GetValue(applicationName);

				IsCheckboxStartAtSystemStartupChecked = subKey != null;

				key.Close();
			}

			_windowHandle = (new WindowInteropHelper(this)).Handle;

			HwndSource src = HwndSource.FromHwnd(_windowHandle);
			src?.AddHook(WndProc);

			foreach (Config conf in Configs)
			{
				if (conf.KeyBinding != null)
				{
					uint mask = conf.KeyBinding.Alt ? (uint)0x0001 : 0;
					mask = mask | (conf.KeyBinding.Control ? (uint)0x0002 : 0);
					mask = mask | (conf.KeyBinding.Shift ? (uint)0x0004 : 0);
					mask = mask | 0x4000;

					int virtualKeyCode = KeyInterop.VirtualKeyFromKey(conf.KeyBinding.Key);

					if (!RegisterHotKey(_windowHandle, virtualKeyCode, mask, (uint)virtualKeyCode))
					{
						MessageBox.Show("Cannot bind key " + conf.KeyBinding);
					}
				}
			}

			WindowState = WindowState.Minimized;
		}

		/// <summary>
		/// Messages handler
		/// </summary>
		/// <param name="hwnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		/// <param name="handled"></param>
		/// <returns></returns>
		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == 0x312)//Si c'est une touche pressée (à partir de registerhotkey)
			{
				Key key = KeyInterop.KeyFromVirtualKey(wParam.ToInt32());

				SelectedConfig = Configs.FirstOrDefault(x => x.KeyBinding.Key == key);
			}

			return IntPtr.Zero;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Move the config down in the list
		/// </summary>
		/// <param name="obj"></param>
		private void MoveConfigDown(object obj)
		{
			if (SelectedConfigIndex < Configs.Count - 1)
			{
				SelectedConfig.Order++;
				_configService.SaveConfig(SelectedConfig);
				Configs[SelectedConfigIndex + 1].Order--;
				_configService.SaveConfig(Configs[SelectedConfigIndex + 1]);
				Configs.Move(SelectedConfigIndex, SelectedConfigIndex + 1);
			}
		}

		/// <summary>
		/// Move the config up in the list
		/// </summary>
		/// <param name="obj"></param>
		private void MoveConfigUp(object obj)
		{
			if (SelectedConfigIndex > 0)
			{
				SelectedConfig.Order--;
				_configService.SaveConfig(SelectedConfig);
				Configs[SelectedConfigIndex - 1].Order++;
				_configService.SaveConfig(Configs[SelectedConfigIndex - 1]);
				Configs.Move(SelectedConfigIndex, SelectedConfigIndex - 1);
			}
		}

		/// <summary>
		/// Save the current screen color
		/// </summary>
		/// <param name="obj"></param>
		private void SaveConfig(object obj)
		{
			Config existingConfig = Configs.FirstOrDefault(x => x.ConfigName == TextNameConfig);//Check if this config alreay exist

			Config config = _temperatureService.SaveCurrentScreenColorToConfig(TextNameConfig);

			if (config != null)
			{
				if (existingConfig == null)
				{
					Configs.Add(config);
				}
				else
				{
					int index = Configs.IndexOf(existingConfig);
					Configs[index] = config;
				}
			}
		}

		private void AssignKeyToConfig(object obj)
		{
			IsWaitingForKeyInput = true;
		}

		private void DeleteConfig(object obj)
		{
			if (SelectedConfig.KeyBinding != null)
			{
				UnregisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key));
			}

			if (_configService.DeleteConfig(SelectedConfig))
			{
				Configs.RemoveAt(SelectedConfigIndex);
				SelectedConfigIndex = 0;
			}
		}

		private void RefreshMonitors(object o)
		{
			Monitors = new ObservableCollection<Monitor>(_monitorService.GetMonitors(true));
			SelectedMonitor = Monitors.FirstOrDefault();
		}

		#endregion

		#region Implémentation INotifyPropertyChanged

		//INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}
