using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
			_ = CheckTmdbStatusAsync(); // Проверка API в фоне
		}

		private void LoadDashboardData()
		{
			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				// 1. Новые сегодня
				var cmdNew = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE created_at >= CURRENT_DATE", conn);
				NewUsersCount.Text = cmdNew.ExecuteScalar()?.ToString() ?? "0";

				// 2. Всего пользователей (не удаленных)
				var cmdTotal = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE is_deleted = FALSE", conn);
				TotalUsersCount.Text = cmdTotal.ExecuteScalar()?.ToString() ?? "0";

				// 3. Очередь модерации
				var cmdReviews = new NpgsqlCommand("SELECT COUNT(*) FROM user_reviews WHERE review_status = 'pending'", conn);
				PendingReviewsCount.Text = cmdReviews.ExecuteScalar()?.ToString() ?? "0";

				// 4. Последние события
				var cmdEvents = new NpgsqlCommand(@"
                    SELECT activity_type, created_at 
                    FROM user_activity 
                    ORDER BY created_at DESC 
                    LIMIT 10", conn);

				using var reader = cmdEvents.ExecuteReader();
				RecentEventsList.Items.Clear();

				while (reader.Read())
				{
					string time = reader.GetDateTime(1).ToString("HH:mm");
					string type = reader.GetString(0);
					RecentEventsList.Items.Add($"[{time}]  {type}");
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
				// Проверяем доступность API (простой пинг)
				var response = await client.GetAsync("https://www.themoviedb.org/");

				if (response.IsSuccessStatusCode)
				{
					TmdbStatus.Text = "ОНЛАЙН";
					StatusDot.Fill = new SolidColorBrush(Colors.Green);
				}
				else
				{
					TmdbStatus.Text = "ОШИБКА";
					StatusDot.Fill = (SolidColorBrush)Application.Current.Resources["PrimaryRed"];
				}
			}
			catch
			{
				TmdbStatus.Text = "OFFLINE";
				StatusDot.Fill = (SolidColorBrush)Application.Current.Resources["PrimaryRed"];
			}
		}

		private void GoToModeration_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new ModerationPage());
		private void GoToMonitoring_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new MonitoringPage());

		private void ExitButton_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Закрыть приложение?", "Выход", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				Application.Current.Shutdown();
		}
	}
}