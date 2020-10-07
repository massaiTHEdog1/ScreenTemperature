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
using Color = System.Windows.Media.Color;
using Container = SimpleInjector.Container;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TimeoutException = System.TimeoutException;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
		private List<Monitor> _monitors;
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
		private string _configDirectory;
		private bool _flagIgnoreChanges = true;

		#region Commands

		public ICommand AssignKeyToConfigCommand { get; private set; }
		public ICommand OnClickButtonSaveConfigCommand { get; private set; }
		public ICommand DeleteConfigCommand { get; private set; }
		public ICommand MoveConfigUpCommand { get; private set; }
		public ICommand MoveConfigDownCommand { get; private set; }

		#endregion

		#endregion

		#region Properties

		/// <summary>
		/// Kelvin slider's value
		/// </summary>
		public int KelvinValue
		{
			get => _kelvinValue;
			set
			{
				_kelvinValue = value;
				NotifyPropertyChanged(nameof(KelvinValue));

				SelectedMonitor.TannerHellandSliderValue = value;

				ApplySelectedMonitor();
			}
		}

		/// <summary>
		/// List of available configs
		/// </summary>
		public ObservableCollection<Config> Configs
		{
			get => _configs;
			set
			{
				_configs = value;
				NotifyPropertyChanged(nameof(Configs));
			}
		}

		/// <summary>
		/// Index of the selected config
		/// </summary>
		public int SelectedConfigIndex
		{
			get => _selectedConfigIndex;
			set
			{
				_selectedConfigIndex = value;
				NotifyPropertyChanged(nameof(SelectedConfigIndex));
			}
		}

		/// <summary>
		/// The selected config
		/// </summary>
		public Config SelectedConfig
		{
			get => _selectedConfig;
			set
			{
				_selectedConfig = value;
				NotifyPropertyChanged(nameof(SelectedConfig));

				if (value != null)
				{
					TextNameConfig = value.ConfigName;
					ApplySelectedConfig();
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
		public List<Monitor> Monitors
		{
			get => _monitors;
			set
			{
				_monitors = value;
				NotifyPropertyChanged(nameof(Monitors));
			}
		}

		/// <summary>
		/// The selected monitor
		/// </summary>
		public Monitor SelectedMonitor
		{
			get => _selectedMonitor;
			set
			{
				_selectedMonitor = value;
				NotifyPropertyChanged(nameof(SelectedMonitor));

				if(value != null && SelectedConfig != null)
				{
					var monitor = SelectedConfig.Monitors.FirstOrDefault(x => x.DeviceName == value.DeviceName);

					if(monitor != null)
					{
						_flagIgnoreChanges = true;

						KelvinValue = monitor.TannerHellandSliderValue;
						ImageSliderValue = monitor.CustomColorSliderValue;
						//SelectedColor = new Color() { }
						IsRadioButtonUseTannerHellandAlgorithmChecked = monitor.AlgorithmType == Enums.AlgorithmType.TannerHelland;
						IsRadioButtonUseImageChecked = monitor.AlgorithmType == Enums.AlgorithmType.Custom;

						_flagIgnoreChanges = false;
					}
				}
			}
		}

		/// <summary>
		/// Value of text-box for the name of the config
		/// </summary>
		public string TextNameConfig
		{
			get => _textNameConfig;
			set
			{
				_textNameConfig = value;
				NotifyPropertyChanged(nameof(TextNameConfig));
			}
		}

		/// <summary>
		/// Are we waiting for an input?
		/// </summary>
		public bool IsWaitingForKeyInput
		{
			get => _isWaitingForKeyInput;
			set
			{
				_isWaitingForKeyInput = value;
				NotifyPropertyChanged(nameof(IsWaitingForKeyInput));
			}
		}

		/// <summary>
		/// Start the software at system startup?
		/// </summary>
		public bool IsCheckboxStartAtSystemStartupChecked
		{
			get => _isCheckboxStartAtSystemStartupChecked;
			set
			{
				_isCheckboxStartAtSystemStartupChecked = value;

				if (value)
				{
					var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);

					if (key != null)
					{
						var applicationPath = Assembly.GetExecutingAssembly().Location;

						var applicationName = Process.GetCurrentProcess().ProcessName;

						key.SetValue(applicationName, applicationPath);

						key.Close();
					}
				}
				else
				{
					var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);

					if (key != null)
					{
						var applicationName = Process.GetCurrentProcess().ProcessName;

						var subKey = key.GetValue(applicationName);

						if (subKey != null)
						{
							key.DeleteValue(applicationName);
						}

						key.Close();
					}
				}

				NotifyPropertyChanged(nameof(IsCheckboxStartAtSystemStartupChecked));
			}
		}

		public bool IsRadioButtonUseTannerHellandAlgorithmChecked
		{
			get => _isRadioButtonUseTannerHellandAlgorithmChecked;
			set
			{
				_isRadioButtonUseTannerHellandAlgorithmChecked = value;
				NotifyPropertyChanged(nameof(IsRadioButtonUseTannerHellandAlgorithmChecked));

				if(value)
					SelectedMonitor.AlgorithmType = Enums.AlgorithmType.TannerHelland;
			}
		}

		public bool IsRadioButtonUseImageChecked
		{
			get => _isRadioButtonUseImageChecked;
			set
			{
				_isRadioButtonUseImageChecked = value;
				NotifyPropertyChanged(nameof(IsRadioButtonUseImageChecked));

				if(value)
					SelectedMonitor.AlgorithmType = Enums.AlgorithmType.Custom;
			}
		}

		/// <summary>
		/// Image slider's value
		/// </summary>
		public int ImageSliderValue
		{
			get => _imageSliderValue;
			set
			{
				_imageSliderValue = value;
				NotifyPropertyChanged(nameof(ImageSliderValue));

				SelectedMonitor.CustomColorSliderValue = value;

				ApplySelectedMonitor();
			}
		}

		private void ApplySelectedMonitor()
		{
			if(SelectedMonitor.AlgorithmType == Enums.AlgorithmType.Custom)
			{
				ApplySelectedImageSliderValueToMonitor(SelectedMonitor);
			}
			else if(SelectedMonitor.AlgorithmType == Enums.AlgorithmType.TannerHelland)
			{
				ApplyKelvinToMonitor(KelvinValue, SelectedMonitor);
			}
		}

		public Color SelectedColor
		{
			get => _selectedColor;
			set
			{
				_selectedColor = value;
				NotifyPropertyChanged(nameof(SelectedColor));

				using (var bitmap = new Bitmap(1000, 1))
				using (var graphics = Graphics.FromImage(bitmap))
				using (var brush = new LinearGradientBrush(
					new Rectangle(0, 0, bitmap.Width, bitmap.Height),
					System.Drawing.Color.FromArgb(SelectedColor.A, SelectedColor.R, SelectedColor.G, SelectedColor.B),
					System.Drawing.Color.White,
					LinearGradientMode.Horizontal))
				using (var memory = new MemoryStream())
				{
					graphics.FillRectangle(brush, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
					bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
					memory.Position = 0;
					var bitmapImage = new BitmapImage();
					bitmapImage.BeginInit();
					bitmapImage.StreamSource = memory;
					bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapImage.EndInit();

					ImageGradient = bitmapImage;
				}

				SelectedMonitor.CustomColorColorValue = value.ToString();
			}
		}

		public BitmapImage ImageGradient
		{
			get => _imageGradient;
			set
			{
				_imageGradient = value;
				NotifyPropertyChanged(nameof(ImageGradient));
			}
		}

		public string Version
		{
			get => Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		#endregion

		#region DLLs

		[DllImport("User32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("User32.dll")]
		private static extern bool UnRegisterHotKey(IntPtr hWnd, int id);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

		[DllImport("GDI32.dll")]
		private static extern unsafe bool SetDeviceGammaRamp(int hdc, void* ramp);

		[DllImport("GDI32.dll")]
		private static extern unsafe bool GetDeviceGammaRamp(int hdc, void* ramp);

		#endregion

		#region Constructor

		public MainWindow()
		{
			var exists = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Length > 1;

			if (exists)//If this program already has an instance
			{
				try
				{
					var pipeStream = new NamedPipeClientStream(".", "instance", PipeDirection.Out, PipeOptions.None);

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
			OnClickButtonSaveConfigCommand = new RelayCommand(OnClickButtonSaveConfig);
			DeleteConfigCommand = new RelayCommand(DeleteConfig);
			MoveConfigUpCommand = new RelayCommand(MoveConfigUp);
			MoveConfigDownCommand = new RelayCommand(MoveConfigDown);

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
			ApplySelectedConfig();
		}

		private void SystemEvents_PaletteChanged(object sender, EventArgs e)
		{
			ApplySelectedConfig();
		}

		private void SystemEvents_UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
		{
			ApplySelectedConfig();
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
				ApplySelectedConfig();
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
						UnRegisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key));
					}

					SelectedConfig.KeyBinding = null;
					SaveConfig(SelectedConfig);
				}
				else
				{
					if (SelectedConfig.KeyBinding != null)
					{
						UnRegisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key));
					}

					SelectedConfig.KeyBinding = new KeyData(keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key, Keyboard.IsKeyDown(Key.LeftShift), Keyboard.IsKeyDown(Key.LeftAlt), Keyboard.IsKeyDown(Key.LeftCtrl));
					SaveConfig(SelectedConfig);

					var mask = SelectedConfig.KeyBinding.Alt ? (uint)0x0001 : 0;
					mask = mask | (SelectedConfig.KeyBinding.Control ? (uint)0x0002 : 0);
					mask = mask | (SelectedConfig.KeyBinding.Shift ? (uint)0x0004 : 0);
					mask = mask | 0x4000;

					var virtualKeyCode = KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key);

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
			#region Load monitors

			Monitors = new List<Monitor>();

			for (int i = 0; i < Screen.AllScreens.Length; i++)
			{
				var hdc = CreateDC(Screen.AllScreens[i].DeviceName, null, null, IntPtr.Zero);

				Monitors.Add(new Monitor()
				{
					DeviceName = Screen.AllScreens[i].DeviceName,
					Label = "Screen " + (i + 1),
					Hdc = hdc
				});
			}

			SelectedMonitor = Monitors.FirstOrDefault();

			#endregion

			IsRadioButtonUseTannerHellandAlgorithmChecked = true;

			SelectedColor = _selectedColor;

			#region Load configs

			_configDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ScreenTemperature\Configs";

			try
			{
				Directory.CreateDirectory(_configDirectory);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Can't create config directory.\r\nTry restarting application.\r\nError:\r\n{ex.Message}", "Error");
				Environment.Exit(0);
			}

			var configs = new List<Config>();

			foreach (var file in Directory.GetFiles(_configDirectory, "*.bin"))
			{
				try
				{
					IFormatter formatter = new BinaryFormatter();

					using (Stream stream = new FileStream($@"{file}", FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						configs.Add((Config)formatter.Deserialize(stream));
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"File {file} corrupt.\r\nCan't use this config.\r\nError:\r\n{ex.Message}", "Error");
				}
			}

			Configs = new ObservableCollection<Config>(configs.OrderBy(conf => conf.Order));

			if (Configs.Count > 0)
			{
				SelectedConfig = Configs[0];
			}
			else
			{
				foreach (var monitor in Monitors)
				{
					if (IsRadioButtonUseTannerHellandAlgorithmChecked)
					{
						monitor.AlgorithmType = Enums.AlgorithmType.TannerHelland;
						monitor.TannerHellandSliderValue = KelvinValue;
					}
					else if (IsRadioButtonUseImageChecked)
					{
						monitor.AlgorithmType = Enums.AlgorithmType.Custom;
						monitor.CustomColorColorValue = SelectedColor.ToString();
						monitor.CustomColorSliderValue = ImageSliderValue;
					}
				}
			}

			#endregion

			var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);

			if (key != null)
			{
				var applicationName = Process.GetCurrentProcess().ProcessName;

				var subKey = key.GetValue(applicationName);

				IsCheckboxStartAtSystemStartupChecked = subKey != null;

				key.Close();
			}

			_windowHandle = (new WindowInteropHelper(this)).Handle;

			var src = HwndSource.FromHwnd(_windowHandle);
			src?.AddHook(WndProc);

			foreach (var conf in Configs)
			{
				if (conf.KeyBinding != null)
				{
					var mask = conf.KeyBinding.Alt ? (uint)0x0001 : 0;
					mask = mask | (conf.KeyBinding.Control ? (uint)0x0002 : 0);
					mask = mask | (conf.KeyBinding.Shift ? (uint)0x0004 : 0);
					mask = mask | 0x4000;

					var virtualKeyCode = KeyInterop.VirtualKeyFromKey(conf.KeyBinding.Key);

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
				var key = KeyInterop.KeyFromVirtualKey(wParam.ToInt32());

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
				SaveConfig(SelectedConfig);
				Configs[SelectedConfigIndex + 1].Order--;
				SaveConfig(Configs[SelectedConfigIndex + 1]);
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
				SaveConfig(SelectedConfig);
				Configs[SelectedConfigIndex - 1].Order++;
				SaveConfig(Configs[SelectedConfigIndex - 1]);
				Configs.Move(SelectedConfigIndex, SelectedConfigIndex - 1);
			}
		}

		
		private unsafe void OnClickButtonSaveConfig(object obj)
		{
			var configToModify = Configs.FirstOrDefault(x => x.ConfigName == TextNameConfig);

			//If the config doesn't exist
			if (configToModify == null)
			{
				configToModify = new Config()
				{
					ConfigName = TextNameConfig == "" ? "config" : TextNameConfig,
					Monitors = Monitors.Select(x => x.Clone()).ToList(),
					Order = Configs.Count
				};
			}
			//If the config already exists
			else
			{
				if (MessageBox.Show($"Are you sure you want to erase this config : {TextNameConfig}?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
				{
					configToModify.Monitors = Monitors.Select(x => x.Clone()).ToList();
				}
				else
				{
					return;
				}
			}

			SaveConfig(configToModify);
		}

		private void AssignKeyToConfig(object obj)
		{
			IsWaitingForKeyInput = true;
		}

		private void DeleteConfig(object obj)
		{
			if (SelectedConfig.KeyBinding != null)
			{
				UnRegisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key));
			}

			try
			{
				File.Delete(SelectedConfig.ConfigPath);
				Configs.Remove(SelectedConfig);
				SelectedConfigIndex = 0;
			}
			catch (Exception)
			{
				
			}
		}

		private unsafe void ApplySelectedConfig()
		{
			ushort* gArray = stackalloc ushort[3 * 256];

			foreach (var monitor in Monitors)
			{
				ApplyKelvinToMonitor(6600, monitor);
			}

			foreach (var monitor in SelectedConfig.Monitors)
			{
				for (var i = 0; i < 256 * 3; i++)
				{
					//gArray[i] = monitor.Rgb[i];
				}

				SetDeviceGammaRamp(monitor.Hdc.ToInt32(), gArray);
			}
		}

		/// <summary>
		/// Changes screen color from kelvin value
		/// Thanks to Tanner Helland for his algorithm http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/
		/// </summary>
		private unsafe void ApplyKelvinToMonitor(int value, Monitor monitor)
		{
			float kelvin = value;
			var temperature = kelvin / 100;

			float red, green, blue;

			if (temperature <= 66)
			{
				red = 255;
			}
			else
			{
				red = temperature - 60;
				red = 329.698727446f * ((float)Math.Pow(red, -0.1332047592));
				if (red < 0) red = 0;
				if (red > 255) red = 255;
			}

			if (temperature <= 66)
			{
				green = temperature;
				green = 99.4708025861f * (float)Math.Log(green) - 161.1195681661f;
				if (green < 0)
				{
					green = 0;
				}

				if (green > 255)
				{
					green = 255;
				}
			}
			else
			{
				green = temperature - 60;
				green = 288.1221695283f * ((float)Math.Pow(green, -0.0755148492));

				if (green < 0) green = 0;
				if (green > 255) green = 255;
			}


			if (temperature >= 66)
			{
				blue = 255;
			}
			else
			{
				if (temperature <= 19)
				{
					blue = 0;
				}
				else
				{
					blue = temperature - 10;
					blue = 138.5177312231f * (float)Math.Log(blue) - 305.0447927307f;
					if (blue < 0) blue = 0;
					if (blue > 255) blue = 255;
				}
			}

			if (value == 6600)
			{
				red = 255;
				green = 255;
				blue = 255;
			}

			ushort* gArray = stackalloc ushort[3 * 256];

			for (var ik = 0; ik < 256; ik++)
			{
				gArray[ik] = (ushort)(ik * red);
				gArray[256 + ik] = (ushort)(ik * green);
				gArray[512 + ik] = (ushort)(ik * blue);
			}

			SetDeviceGammaRamp(monitor.Hdc.ToInt32(), gArray);
		}

		/// <summary>
		/// Changes screen color from an image
		/// </summary>
		private unsafe void ApplySelectedImageSliderValueToMonitor(Monitor monitor)
		{
			float red, green, blue;

			using (var outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(ImageGradient));
				enc.Save(outStream);

				using (var bmp = new Bitmap(outStream))
				{
					if (ImageSliderValue < 0)
						ImageSliderValue = 0;
					if (ImageSliderValue >= bmp.Width)
						ImageSliderValue = bmp.Width - 1;


					var clr = bmp.GetPixel(ImageSliderValue, 0);

					red = clr.R;
					green = clr.G;
					blue = clr.B;
				}
			}

			ushort* gArray = stackalloc ushort[3 * 256];

			for (var ik = 0; ik < 256; ik++)
			{
				gArray[ik] = (ushort)(ik * red);
				gArray[256 + ik] = (ushort)(ik * green);
				gArray[512 + ik] = (ushort)(ik * blue);
			}

			SetDeviceGammaRamp(monitor.Hdc.ToInt32(), gArray);
		}

		/// <summary>
		/// Saves the config
		/// </summary>
		private Config SaveConfig(Config config)
		{
			var addConfigToList = false;

			if (string.IsNullOrEmpty(config.ConfigPath))//if this config doesn't exist
			{
				var i = 0;

				while (true)
				{
					if (!File.Exists($@"{_configDirectory}\config{i}.bin"))
					{
						using (var fs = File.Create($@"{_configDirectory}\config{i}.bin"))
						{
							fs.Close();
						}

						config.ConfigPath = $@"{_configDirectory}\config{i}.bin";
						addConfigToList = true;
						break;
					}

					i++;
				}

				config.ConfigName = string.IsNullOrEmpty(config.ConfigName) ? $"config{i}" : config.ConfigName;
			}

			try
			{
				IFormatter formatter = new BinaryFormatter();

				using (Stream stream = new FileStream(config.ConfigPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
				{
					formatter.Serialize(stream, config);
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show("Impossible d'enregistrer le fichier config.\r\nErreur:\r\n" + exception.Message, "Erreur");
			}

			if (addConfigToList)
				Configs.Add(config);

			return config;
		}

		#endregion

		#region Implémentation INotifyPropertyChanged

		//INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
