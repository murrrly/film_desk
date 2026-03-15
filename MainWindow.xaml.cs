using System.Windows;
using System.Windows.Controls;
using MovieApp_Adminpanel.Pages;

namespace MovieApp_Adminpanel
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			// При старте показываем Главную
			MainFrame.Navigate(new DashboardPage());
			SetActiveMenu(MenuDashboard);
		}

		/// <summary>
		/// Устанавливает подсветку активного пункта меню
		/// </summary>
		/// <param name="activeItem"></param>
		private void SetActiveMenu(MenuItem activeItem)
		{
			foreach (var item in new[]
			{
				MenuDashboard,
				MenuUsers,
				MenuModeration,
				MenuMonitoring,
				MenuAnalytics,
				MenuSettings,
				MenuProfile
			})
			{
				item.Tag = null;
			}

			activeItem.Tag = "Active";
		}

		private void Dashboard_Click(object sender, RoutedEventArgs e)
		{
			MainFrame.Navigate(new DashboardPage());
			SetActiveMenu(MenuDashboard);
			PageTitle.Text = "Главная";
		}

		private void Users_Click(object sender, RoutedEventArgs e)
		{
			MainFrame.Navigate(new UsersPage());
			SetActiveMenu(MenuUsers);
			PageTitle.Text = "Пользователи";
		}

		private void Moderation_Click(object sender, RoutedEventArgs e)
		{
			MainFrame.Navigate(new ModerationPage());
			SetActiveMenu(MenuModeration);
			PageTitle.Text = "Модерация";
		}

		private void Monitoring_Click(object sender, RoutedEventArgs e)
		{
			MainFrame.Navigate(new MonitoringPage());
			SetActiveMenu(MenuMonitoring);
			PageTitle.Text = "Мониторинг";
		}

		private void Analytics_Click(object sender, RoutedEventArgs e)
		{
			MainFrame.Navigate(new AnalyticsPage());
			SetActiveMenu(MenuAnalytics);
			PageTitle.Text = "Аналитика";
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			MainFrame.Navigate(new SettingsPage());
			SetActiveMenu(MenuSettings);
			PageTitle.Text = "Настройки";
		}

		private void AdminProfile_Click(object sender, RoutedEventArgs e)
		{
			MainFrame.Navigate(new AdminProfilePage());
			SetActiveMenu(MenuProfile);
			PageTitle.Text = "Профиль";
		}
	}
}