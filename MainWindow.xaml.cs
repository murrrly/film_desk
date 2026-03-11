using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			MainFrame.Content = new Pages.DashboardPage(); // стартовая страница
		}

		private void Dashboard_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new Pages.DashboardPage();
		private void Users_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new Pages.UsersPage();
		private void Moderation_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new Pages.ModerationPage();
		private void Monitoring_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new Pages.MonitoringPage();
		private void Analytics_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new Pages.AnalyticsPage();
		private void Settings_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new Pages.SettingsPage();
		private void AdminProfile_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new Pages.AdminProfilePage();
	}
}