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
		public ObservableCollection<Config> Configs;
		private IntPtr _windowHandle;
		private int _kelvinValue = 6600;

		#endregion

		#region Propriétés

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

		#endregion

		#region Methodes

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

		#region Constructeur

		public MainWindow()
		{
			InitializeComponent();

			executableDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
			configDirectory = executableDirectory + @"\Configs";

			bool exists = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1;

			if (exists)//Si le programme est déjà ouvert
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
			else//Si le programme n'est pas encore ouvert
			{
				_pipeServer = new NamedPipeServerStream("instance", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				_pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, _pipeServer);
			}

			SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;

			_notifyIcon.Icon = Properties.Resources.icon;
			_notifyIcon.Click += NotifyIconOnDoubleClick;
			_notifyIcon.Visible = true;
			Visibility = Visibility.Collapsed;

			try
			{
				Directory.CreateDirectory(configDirectory);
			}
			catch (Exception e)
			{
				MessageBox.Show($"Impossible de créer le dossier pour les configs.\r\nEssayez de redémarrer l'application.\r\nErreur:\r\n{e.Message}", "Erreur");
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
					MessageBox.Show($"Fichier {file} corrompu.\r\nImpossible d'utiliser cette configuration.\r\nErreur:\r\n{e.Message}", "Erreur");
				}
			}

			Configs = new ObservableCollection<Config>(Configs.OrderBy(conf => conf.Order).ToList());
			ListBoxConfigs.ItemsSource = Configs;
		}

		#endregion

		/// <summary>
		/// Récupère les messages windows
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

				foreach (Config conf in Configs)
				{
					if (conf.KeyBinding?.Key == key)
					{
						ApplyConfig(conf);
						ListBoxConfigs.SelectedItem = conf;
						break;
					}
				}
			}

			return IntPtr.Zero;
		}

		/// <summary>
		/// Lorsqu'on se connecte à la session
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="sessionSwitchEventArgs"></param>
		private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs sessionSwitchEventArgs)
		{
			if (sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionUnlock || sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionLogon)
			{
				ApplyConfig(ListBoxConfigs.SelectedItem as Config);
			}
		}

		/// <summary>
		/// Lorsque l'on clique sur l'icone de l'application
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void NotifyIconOnDoubleClick(object sender, EventArgs eventArgs)
		{
			Visibility = Visibility.Visible;
			WindowState = WindowState.Normal;
			ShowInTaskbar = true;
			_notifyIcon.Visible = false;
			Activate();
		}

		/// <summary>
		/// Attend si jamais une autre instance de l'application se lance
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
					int index = ListBoxConfigs.SelectedIndex;

					if (index == -1)
					{
						ListBoxConfigs.SelectedIndex = 0;
					}
					else
					{
						if (ListBoxConfigs.SelectedIndex + 1 == ListBoxConfigs.Items.Count)
						{
							ListBoxConfigs.SelectedIndex = 0;
						}
						else
						{
							ListBoxConfigs.SelectedIndex++;
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

		private void ButtonSaveClick(object sender, RoutedEventArgs e)
		{
			SaveCurrentScreen(InputFileName.Text == "" ? "config" : InputFileName.Text);
		}

		/// <summary>
		/// Enregistre la configuration actuelle de l'écran dans le fichier qui aura le nom de configName
		/// </summary>
		/// <param name="configName">Nom du fichier</param>
		private unsafe void SaveCurrentScreen(string configName)
		{
			short* gArray = stackalloc short[3 * 256];

			_hdc = Graphics.FromHwnd(IntPtr.Zero).GetHdc().ToInt32();

			bool retVal = GetDeviceGammaRamp(_hdc, gArray);//On récupère les données de l'écran

			if (retVal)//Si ça a fonctionné
			{
				List<short> rgb = new List<short>();

				for (int i = 0; i < 256 * 3; i++)
				{
					rgb.Add(gArray[i]);
				}

				Config conf = new Config(configName);
				conf.RGB = rgb.ToArray();
				conf.Order = Configs.Count;

				Configs.Add(conf);
			}
			else
			{
				MessageBox.Show("Impossible de récupérer les données de l'écran. \r\nEssayez de redémarrer l'application.", "Erreur");
			}
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

		private void ListBoxSelectedItemChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				ApplyConfig(e.AddedItems[0] as Config);
				ConfigKeyBinding.Text = (e.AddedItems[0] as Config).KeyBinding?.ToString();
			}
		}

		private void MainForm_StateChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
			{
				_notifyIcon.Visible = true;
				ShowInTaskbar = false;
			}
		}

		private void MoveUpConfig(object sender, RoutedEventArgs e)
		{
			if (ListBoxConfigs.SelectedItem == null)
			{
				return;
			}

			int index = ListBoxConfigs.SelectedIndex;
			Config conf = ListBoxConfigs.SelectedItem as Config;

			if (ListBoxConfigs.SelectedIndex > 0)
			{
				ListBoxConfigs.SelectionChanged -= ListBoxSelectedItemChanged;
				Configs.RemoveAt(index);
				Configs.Insert(index - 1, conf);
				ListBoxConfigs.SelectedIndex = index - 1;
				ListBoxConfigs.SelectionChanged += ListBoxSelectedItemChanged;
			}

			SetOrderToConfigs();
		}

		private void MoveDownConfig(object sender, RoutedEventArgs e)
		{
			if (ListBoxConfigs.SelectedItem == null)
			{
				return;
			}

			int index = ListBoxConfigs.SelectedIndex;
			Config conf = ListBoxConfigs.SelectedItem as Config;

			if (ListBoxConfigs.SelectedIndex < Configs.Count - 1)
			{
				ListBoxConfigs.SelectionChanged -= ListBoxSelectedItemChanged;
				Configs.RemoveAt(index);
				Configs.Insert(index + 1, conf);
				ListBoxConfigs.SelectedIndex = index + 1;
				ListBoxConfigs.SelectionChanged += ListBoxSelectedItemChanged;
			}

			SetOrderToConfigs();
		}

		private void DeleteConfig(object sender, RoutedEventArgs e)
		{
			Config conf = (Config)ListBoxConfigs.SelectedItem;

			if (conf.KeyBinding != null)
			{
				UnregisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(conf.KeyBinding.Key));
			}

			conf.Delete();
			Configs.RemoveAt(ListBoxConfigs.SelectedIndex);
		}

		private void AssignKey(object sender, RoutedEventArgs e)
		{
			WaitingBindingGrid.Visibility = Visibility.Visible;
		}

		private void SetOrderToConfigs()
		{
			foreach (Config conf in Configs)
			{
				conf.Order = Configs.IndexOf(conf);
				conf.Save();
			}
		}

		private void Window_OnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			if (WaitingBindingGrid.Visibility == Visibility.Visible)
			{
				Config currentConfig = ((Config)ListBoxConfigs.SelectedItem);

				

				if (keyEventArgs.Key == Key.Escape)
				{

				}
				else if (keyEventArgs.Key == Key.Back)
				{
					if (currentConfig.KeyBinding != null)
					{
						UnregisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(currentConfig.KeyBinding.Key));
					}

					currentConfig.KeyBinding = null;
					currentConfig.Save();
					ConfigKeyBinding.Text = "";
				}
				else
				{
					if (currentConfig.KeyBinding != null)
					{
						UnregisterHotKey(_windowHandle, KeyInterop.VirtualKeyFromKey(currentConfig.KeyBinding.Key));
					}

					currentConfig.KeyBinding = new KeyData(keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key, Keyboard.IsKeyDown(Key.LeftShift), Keyboard.IsKeyDown(Key.LeftAlt), Keyboard.IsKeyDown(Key.LeftCtrl));
					currentConfig.Save();
					ConfigKeyBinding.Text = currentConfig.KeyBinding?.ToString();

					uint mask = currentConfig.KeyBinding.Alt ? (uint)0x0001 : 0;
					mask = mask | (currentConfig.KeyBinding.Control ? (uint)0x0002 : 0);
					mask = mask | (currentConfig.KeyBinding.Shift ? (uint)0x0004 : 0);
					mask = mask | 0x4000;

					int virtualKeyCode = KeyInterop.VirtualKeyFromKey(currentConfig.KeyBinding.Key);

					if (RegisterHotKey(_windowHandle, virtualKeyCode, mask, (uint)virtualKeyCode))
					{
						HwndSource src = HwndSource.FromHwnd(_windowHandle);
						src.AddHook(new HwndSourceHook(WndProc));
					}
					else
					{
						MessageBox.Show("Cannot bind key " + currentConfig.KeyBinding);
					}
				}

				WaitingBindingGrid.Visibility = Visibility.Collapsed;
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
