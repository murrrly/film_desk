using System.Windows;
using System.Windows.Controls;
using MovieApp_Adminpanel.Pages;

namespace MovieApp_Adminpanel.Pages
{
	public partial class DashboardPage : Page
	{
		public DashboardPage()
		{
			InitializeComponent();
			LoadDashboardData();
		}

		private void LoadDashboardData()
		{
			NewUsersCount.Text = "5";
			PendingReviewsCount.Text = "3";
			TmdbStatus.Text = "OK";
			CacheHitRate.Text = "78%";

			RecentEventsList.ItemsSource = new[]
			{
				"John зарегистрировался",
				"Новая рецензия ожидает модерации",
				"Ошибка TMDB: ключ истёк"
			};
		}

		private void GoToModeration_Click(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new ModerationPage());
		}

		private void RefreshCache_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Кэш популярных фильмов обновлён", "Dashboard", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void GoToMonitoring_Click(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new MonitoringPage());
		}
	}
}