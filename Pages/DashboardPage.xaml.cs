using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MovieApp_Adminpanel.Services;
using MySqlConnector;

namespace MovieApp_Adminpanel.Pages
{
	public partial class DashboardPage : Page
	{
		private DatabaseService db = new DatabaseService();

		public DashboardPage()
		{
			InitializeComponent();
			LoadDashboardData();
			_ = CheckTmdbStatusAsync();
		}

		private void LoadDashboardData()
		{
			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				// 1. Новые пользователи сегодня
				using (var cmdNew = new MySqlCommand(
					"SELECT COUNT(*) FROM users WHERE DATE(created_at) = CURDATE()", conn))
				{
					NewUsersCount.Text = cmdNew.ExecuteScalar()?.ToString() ?? "0";
				}

				// 2. Всего пользователей
				using (var cmdTotal = new MySqlCommand(
					"SELECT COUNT(*) FROM users WHERE is_deleted = FALSE", conn))
				{
					TotalUsersCount.Text = cmdTotal.ExecuteScalar()?.ToString() ?? "0";
				}

				// 3. Очередь модерации
				using (var cmdReviews = new MySqlCommand(
					"SELECT COUNT(*) FROM user_reviews WHERE review_status = 'pending'", conn))
				{
					PendingReviewsCount.Text = cmdReviews.ExecuteScalar()?.ToString() ?? "0";
				}

				// 4. Последние события
				using (var cmdEvents = new MySqlCommand(@"
                    SELECT activity_type, created_at
                    FROM user_activity
                    ORDER BY created_at DESC
                    LIMIT 10", conn))
				using (var reader = cmdEvents.ExecuteReader())
				{
					RecentEventsList.Items.Clear();

					while (reader.Read())
					{
						string time = Convert.ToDateTime(reader["created_at"]).ToString("HH:mm");
						string type = reader["activity_type"]?.ToString() ?? "";
						RecentEventsList.Items.Add($"[{time}]  {type}");
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка БД: " + ex.Message);
			}
		}

		private async Task CheckTmdbStatusAsync()
		{
			try
			{
				using HttpClient client = new HttpClient();
				client.Timeout = TimeSpan.FromSeconds(5);

				var response = await client.GetAsync("https://www.themoviedb.org/");

				if (response.IsSuccessStatusCode)
				{
					TmdbStatus.Text = "ОНЛАЙН";
					StatusDot.Fill = new SolidColorBrush(Colors.Green);
				}
				else
				{
					TmdbStatus.Text = "ОШИБКА";
					StatusDot.Fill = new SolidColorBrush(Colors.Red);
				}
			}
			catch
			{
				TmdbStatus.Text = "OFFLINE";
				StatusDot.Fill = new SolidColorBrush(Colors.Red);
			}
		}

		private void GoToModeration_Click(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new ModerationPage());
		}

		private void GoToMonitoring_Click(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new MonitoringPage());
		}

		private void ExitButton_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Закрыть приложение?", "Выход", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				Application.Current.Shutdown();
		}
	}
}