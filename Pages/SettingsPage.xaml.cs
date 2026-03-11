using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class SettingsPage : Page
	{
		public SettingsPage()
		{
			InitializeComponent();
		}

		private void SaveSettings_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Настройки сохранены.", "Система", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}