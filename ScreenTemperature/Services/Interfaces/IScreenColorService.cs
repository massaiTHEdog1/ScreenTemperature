using System.Windows.Media.Imaging;
using ScreenTemperature.Classes;

namespace ScreenTemperature.Services.Interfaces
{
	interface IScreenColorService
	{
		/// <summary>
		/// Changes screen color from kelvin value
		/// </summary>
		void ChangeScreenColorFromKelvin(int kelvin, Monitor monitor);

		/// <summary>
		/// Changes screen color from a Config
		/// </summary>
		void ChangeScreenColorFromConfig(Config config);

		/// <summary>
		/// Saves the screen color in a config file
		/// </summary>
		Config SaveCurrentScreenColorToConfig(string configName);

		/// <summary>
		/// Changes screen color from an image
		/// </summary>
		void ChangeScreenColorFromImage(int value, Monitor monitor, BitmapImage image);
	}
}
