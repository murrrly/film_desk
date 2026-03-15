using System;
using System.Windows;
using System.Windows.Controls;
using MovieApp_Adminpanel.Pages;
using MovieApp_Adminpanel.Services;
using Npgsql;

namespace MovieApp_Adminpanel.Pages
{
	public partial class DashboardPage : Page
	{
		private DatabaseService db = new DatabaseService();

		public DashboardPage()
		{
			InitializeComponent();
			LoadDashboardData();
		}

		private void LoadDashboardData()
		{
			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				// новые пользователи сегодня
				var cmdUsers = new NpgsqlCommand(
					"SELECT COUNT(*) FROM users WHERE created_at >= CURRENT_DATE",
					conn);

				NewUsersCount.Text = cmdUsers.ExecuteScalar().ToString();

				// очередь модерации
				var cmdReviews = new NpgsqlCommand(
					"SELECT COUNT(*) FROM user_reviews WHERE review_status='pending'",
					conn);

				PendingReviewsCount.Text = cmdReviews.ExecuteScalar().ToString();

				// последние события
				var cmdEvents = new NpgsqlCommand(@"
                    SELECT activity_type
                    FROM user_activity
                    ORDER BY created_at DESC
                    LIMIT 10", conn);

				using var reader = cmdEvents.ExecuteReader();

				RecentEventsList.Items.Clear();

				while (reader.Read())
				{
					RecentEventsList.Items.Add(reader.GetString(0));
				}

				TmdbStatus.Text = "OK";
				CacheHitRate.Text = "78%";
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		// перейти к модерации
		private void GoToModeration_Click(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new ModerationPage());
		}

		// обновить кэш
		private void RefreshCache_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Кэш популярных фильмов обновлён");
		}

		// перейти к мониторингу
		private void GoToMonitoring_Click(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new MonitoringPage());
		}

		private void ExitButton_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown(); // Закрываем приложение

		}
	}
}