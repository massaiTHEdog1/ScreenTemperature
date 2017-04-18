using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using FluxLike.Classes;
using Microsoft.Win32;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TimeoutException = System.TimeoutException;

namespace FluxLike
{
	/// <summary>
	/// Logique d'interaction pour MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{

		#region Variables

		private static Int32 _hdc;//Hardware Device Context
		public static string executableDirectory;
		public static string configDirectory;
		private NamedPipeServerStream _pipeServer;
		private NotifyIcon _notifyIcon = new NotifyIcon();
		
		private IntPtr _windowHandle;
		private int _kelvinValue = 6600;
		private ObservableCollection<Config> _configs;
		private int _selectedConfigIndex;
		private Config _selectedConfig;
		private string _textNameConfig;
		private bool _isWaitingForKeyInput;

		public ICommand AssignKeyToConfigCommand { get; private set; }
		public ICommand SaveConfigCommand { get; private set; }
		public ICommand DeleteConfigCommand { get; private set; }
		public ICommand MoveConfigUpCommand { get; private set; }
		public ICommand MoveConfigDownCommand { get; private set; }

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

				ApplyKelvin(value);
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
					ApplyConfig(value);
				}
				else
				{
					TextNameConfig = "";
				}
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

		#endregion

		#region Methods

		#region DLLs

		/// <summary>
		/// Définit les couleurs
		/// </summary>
		/// <param name="hdc">Hardware Device Context (l'écran)</param>
		/// <param name="ramp"></param>
		/// <returns></returns>
		[DllImport("GDI32.dll")]
		private static extern unsafe bool SetDeviceGammaRamp(Int32 hdc, void* ramp);

		/// <summary>
		/// Récupère les couleurs
		/// </summary>
		/// <param name="hdc">Hardware Device Context (l'écran)</param>
		/// <param name="ramp"></param>
		/// <returns></returns>
		[DllImport("GDI32.dll")]
		private static extern unsafe bool GetDeviceGammaRamp(Int32 hdc, void* ramp);

		[DllImport("User32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("User32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		#endregion

		#region Constructor

		public MainWindow()
		{
			executableDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
			configDirectory = executableDirectory + @"\Configs";

			bool exists = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1;

			if (exists)//If this program already has an instance
			{
				try
				{
					NamedPipeClientStream pipeStream = new NamedPipeClientStream(".", "instance", PipeDirection.Out, PipeOptions.None);

					pipeStream.Connect();

					Environment.Exit(0);
				}
				catch (TimeoutException oEX)
				{
					Debug.WriteLine(oEX.Message);
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

			//When we log-in to windows, the screen goes back to normal so with this event we avoid this
			SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;

			_notifyIcon.Icon = Properties.Resources.icon;
			_notifyIcon.Click += NotifyIconOnClick;
			_notifyIcon.Visible = true;

			try
			{
				Directory.CreateDirectory(configDirectory);
			}
			catch (Exception e)
			{
				MessageBox.Show($"Can't create config directory.\r\nTry restarting application.\r\nErrorr:\r\n{e.Message}", "Error");
				Environment.Exit(0);
			}

			Configs = new ObservableCollection<Config>();

			foreach (string file in Directory.GetFiles(configDirectory, "*.bin"))
			{
				try
				{
					IFormatter formatter = new BinaryFormatter();

					using (Stream stream = new FileStream($@"{file}", FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						Configs.Add(formatter.Deserialize(stream) as Config);
					}
				}
				catch (Exception e)
				{
					MessageBox.Show($"File {file} corrupt.\r\nCan't use this config.\r\nError:\r\n{e.Message}", "Error");
				}
			}

			Configs = new ObservableCollection<Config>(Configs.OrderBy(conf => conf.Order).ToList());

			if (Configs.Count > 0)
			{
				SelectedConfig = Configs[0];
			}

			WindowState = WindowState.Minimized;
			Hide();

			InitializeComponent();
		}

		#endregion

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

		/// <summary>
		/// When we log in to the session
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="sessionSwitchEventArgs"></param>
		private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs sessionSwitchEventArgs)
		{
			if (sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionUnlock || sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionLogon)
			{
				ApplyConfig(SelectedConfig);
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
				}
				else
				{
					if (SelectedConfig.KeyBinding != null)
					{
						UnregisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key));
					}

					SelectedConfig.KeyBinding = new KeyData(keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key, Keyboard.IsKeyDown(Key.LeftShift), Keyboard.IsKeyDown(Key.LeftAlt), Keyboard.IsKeyDown(Key.LeftCtrl));

					uint mask = SelectedConfig.KeyBinding.Alt ? (uint)0x0001 : 0;
					mask = mask | (SelectedConfig.KeyBinding.Control ? (uint)0x0002 : 0);
					mask = mask | (SelectedConfig.KeyBinding.Shift ? (uint)0x0004 : 0);
					mask = mask | 0x4000;

					int virtualKeyCode = KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key);

					if (RegisterHotKey(_windowHandle, virtualKeyCode, mask, (uint)virtualKeyCode))
					{
						HwndSource src = HwndSource.FromHwnd(_windowHandle);
						src.AddHook(new HwndSourceHook(WndProc));
					}
					else
					{
						MessageBox.Show("Cannot bind key " + SelectedConfig.KeyBinding);
					}
				}

				IsWaitingForKeyInput = false;
			}
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			_windowHandle = (new WindowInteropHelper(this)).Handle;

			foreach (Config conf in Configs)
			{
				if (conf.KeyBinding != null)
				{
					uint mask = conf.KeyBinding.Alt ? (uint)0x0001 : 0;
					mask = mask | (conf.KeyBinding.Control ? (uint)0x0002 : 0);
					mask = mask | (conf.KeyBinding.Shift ? (uint)0x0004 : 0);
					mask = mask | 0x4000;

					int virtualKeyCode = KeyInterop.VirtualKeyFromKey(conf.KeyBinding.Key);

					if (RegisterHotKey(_windowHandle, virtualKeyCode, mask, (uint)virtualKeyCode))
					{
						HwndSource src = HwndSource.FromHwnd(_windowHandle);
						src.AddHook(new HwndSourceHook(WndProc));
					}
					else
					{
						MessageBox.Show("Cannot bind key " + conf.KeyBinding);
					}
				}
			}
		}


		/// <summary>
		/// Move the config down in the list
		/// </summary>
		/// <param name="obj"></param>
		private void MoveConfigDown(object obj)
		{
			if (SelectedConfigIndex < Configs.Count - 1)
			{
				SelectedConfig.Order++;
				Configs[SelectedConfigIndex + 1].Order--;
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
				Configs[SelectedConfigIndex - 1].Order++;
				Configs.Move(SelectedConfigIndex, SelectedConfigIndex - 1);
			}
		}

		/// <summary>
		/// Save the current screen color
		/// </summary>
		/// <param name="obj"></param>
		private unsafe void SaveConfig(object obj)
		{
			short* gArray = stackalloc short[3 * 256];

			_hdc = Graphics.FromHwnd(IntPtr.Zero).GetHdc().ToInt32();

			bool retVal = GetDeviceGammaRamp(_hdc, gArray);//Get screen data

			if (retVal)//If it' ok
			{
				List<short> rgb = new List<short>();

				for (int i = 0; i < 256 * 3; i++)
				{
					rgb.Add(gArray[i]);
				}

				Config configToModify = Configs.FirstOrDefault(x => x.ConfigName == TextNameConfig);

				if (configToModify == null)//If the config doesn't exist
				{
					configToModify = new Config(TextNameConfig == "" ? "config" : TextNameConfig);
					configToModify.RGB = rgb.ToArray();
					configToModify.Order = Configs.Count;

					Configs.Add(configToModify);
				}
				else//If the config already exists
				{
					if (MessageBox.Show(this, $"Are you sure you want to erase this config: {TextNameConfig}?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						configToModify.RGB = rgb.ToArray();
					}
					else
					{
						return;//Maybe we just want to change the name so we souldn't change current screen color
					}
				}

				SelectedConfig = configToModify;
			}
			else
			{
				MessageBox.Show("Can't get screen data. \r\nTry again or restart application.", "Error");
			}
		}

		private void AssignKeyToConfig(object obj)
		{
			IsWaitingForKeyInput = true;
		}

		private unsafe void ApplyConfig(Config config)
		{
			short* gArray = stackalloc short[3 * 256];

			for (int i = 0; i < 256 * 3; i++)
			{
				gArray[i] = config.RGB[i];
			}

			_hdc = Graphics.FromHwnd(IntPtr.Zero).GetHdc().ToInt32();

			SetDeviceGammaRamp(_hdc, gArray);
		}

		

		private void DeleteConfig(object obj)
		{
			if (SelectedConfig.KeyBinding != null)
			{
				UnregisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(SelectedConfig.KeyBinding.Key));
			}

			SelectedConfig.Delete();
			Configs.RemoveAt(SelectedConfigIndex);
			SelectedConfigIndex = 0;
		}

		

		private unsafe void ApplyKelvin(int value)
		{
			float kelvin = value;
			float Temperature = kelvin / 100;

			float Red, Green, Blue = 0;

			if (Temperature <= 66)
			{
				Red = 255;
			}
			else
			{
				Red = Temperature - 60;
				Red = 329.698727446f * ((float)Math.Pow(Red, -0.1332047592));
				if (Red < 0) Red = 0;
				if (Red > 255) Red = 255;
			}

			if (Temperature <= 66)
			{
				Green = Temperature;
				Green = 99.4708025861f * (float)Math.Log(Green) - 161.1195681661f;
				if (Green < 0)
				{
					Green = 0;
				}

				if (Green > 255)
				{
					Green = 255;
				}
			}
			else
			{
				Green = Temperature - 60;
				Green = 288.1221695283f * ((float)Math.Pow(Green, -0.0755148492));

				if (Green < 0) Green = 0;
				if (Green > 255) Green = 255;
			}


			if (Temperature >= 66)
			{
				Blue = 255;
			}
			else
			{
				if (Temperature <= 19)
				{
					Blue = 0;
				}
				else
				{
					Blue = Temperature - 10;
					Blue = 138.5177312231f * (float)Math.Log(Blue) - 305.0447927307f;
					if (Blue < 0) Blue = 0;
					if (Blue > 255) Blue = 255;
				}
			}

			if(value == 6600)
			{
				Red = 255;
				Green = 255;
				Blue = 255;
			}

			short* gArray = stackalloc short[3 * 256];

			for (int ik = 0; ik < 256; ik++)
			{
				gArray[ik] = (short)(ik * Red);
				gArray[256 + ik] = (short)(ik * Green);
				gArray[512 + ik] = (short)(ik * Blue);
			}

			_hdc = Graphics.FromHwnd(IntPtr.Zero).GetHdc().ToInt32();

			SetDeviceGammaRamp(_hdc, gArray);
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
