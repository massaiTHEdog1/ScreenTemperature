using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ScreenTemperature.Classes
{
	[Serializable]
	public class KeyData : INotifyPropertyChanged
	{
		private Key _key;
		private bool _alt;
		private bool _shift;
		private bool _control;

		public bool Alt
		{
			get { return _alt; }
			set
			{
				_alt = value;
				NotifyPropertyChanged("Alt");
			}
		}

		public bool Shift
		{
			get { return _shift; }
			set
			{
				_shift = value;
				NotifyPropertyChanged("Shift");
			}
		}

		public bool Control
		{
			get { return _control; }
			set
			{
				_control = value;
				NotifyPropertyChanged("Control");
			}
		}

		public Key Key
		{
			get { return _key; }
			set
			{
				_key = value;
				NotifyPropertyChanged("Key");
			}
		}
		

		public KeyData(Key key, bool shift, bool alt, bool control)
		{
			_key = key;
			_shift = shift;
			_alt = alt;
			_control = control;
		}

		public override string ToString()
		{
			string text = "";
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
