using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ScreenTemperature.Classes
{
	[Serializable]
	public class KeyData : INotifyPropertyChanged
	{
		#region Variables

		private Key _key;
		private bool _alt;
		private bool _shift;
		private bool _control;

		#endregion

		#region Properties

		public bool Alt
		{
			get => _alt;
			set
			{
				_alt = value;
				NotifyPropertyChanged(nameof(Alt));
			}
		}

		public bool Shift
		{
			get => _shift;
			set
			{
				_shift = value;
				NotifyPropertyChanged(nameof(Shift));
			}
		}

		public bool Control
		{
			get => _control;
			set
			{
				_control = value;
				NotifyPropertyChanged(nameof(Control));
			}
		}

		public Key Key
		{
			get => _key;
			set
			{
				_key = value;
				NotifyPropertyChanged(nameof(Key));
			}
		}

		#endregion
		
		public KeyData(Key key, bool shift, bool alt, bool control)
		{
			_key = key;
			_shift = shift;
			_alt = alt;
			_control = control;
		}

		public override string ToString()
		{
			var text = string.Empty;
			text += Shift ? "Shift + " : "";
			text += Control ? "Ctrl + " : "";
			text += Alt ? "Alt + " : "";
			text += Key.ToString();
			return text;
		}

		#region Implémentation INotifyPropertyChanged

		//INotifyPropertyChanged implementation
		[field:NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
