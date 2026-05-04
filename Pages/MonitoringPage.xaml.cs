using MovieApp_Adminpanel.Services;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class MonitoringPage : Page
	{
		private DatabaseService db = new DatabaseService();

		public MonitoringPage()
		{
			InitializeComponent();
			LoadLogs();
		}

		private void LoadLogs()
		{
			var logs = new List<dynamic>();

			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				// Проверяем существование таблицы user_activity
				bool tableExists = CheckTableExists(conn, "user_activity");

				if (tableExists)
				{
					// Если таблица есть - загружаем из нее
					using var cmd = new MySqlCommand(@"
						SELECT u.username,
							   ua.activity_type,
							   ua.created_at
						FROM user_activity ua
						JOIN users u ON u.id = ua.user_id
						ORDER BY ua.created_at DESC
						LIMIT 100", conn);

					using var reader = cmd.ExecuteReader();

					while (reader.Read())
					{
						logs.Add(new
						{
							Username = reader["username"]?.ToString(),
							Activity = reader["activity_type"]?.ToString(),
							Date = Convert.ToDateTime(reader["created_at"]).ToString("yyyy-MM-dd HH:mm")
						});
					}
				}
				else
				{
					// Если таблицы нет - загружаем из других таблиц

					// 1. Отзывы пользователей
					using var cmdReviews = new MySqlCommand(@"
						SELECT u.username,
							   CONCAT('Оставил отзыв на ', ur.media_type) as activity_type,
							   ur.created_at
						FROM user_reviews ur
						JOIN users u ON u.id = ur.user_id
						ORDER BY ur.created_at DESC
						LIMIT 50", conn);

					using var readerReviews = cmdReviews.ExecuteReader();

					while (readerReviews.Read())
					{
						logs.Add(new
						{
							Username = readerReviews["username"]?.ToString(),
							Activity = readerReviews["activity_type"]?.ToString(),
							Date = Convert.ToDateTime(readerReviews["created_at"]).ToString("yyyy-MM-dd HH:mm")
						});
					}
					readerReviews.Close();

					// 2. Добавление в списки
					using var cmdLists = new MySqlCommand(@"
						SELECT u.username,
							   CONCAT('Добавил в список (', li.media_type, ')') as activity_type,
							   li.added_at as created_at
						FROM list_items li
						JOIN user_lists ul ON ul.id = li.list_id
						JOIN users u ON u.id = ul.user_id
						ORDER BY li.added_at DESC
						LIMIT 50", conn);

					using var readerLists = cmdLists.ExecuteReader();

					while (readerLists.Read())
					{
						logs.Add(new
						{
							Username = readerLists["username"]?.ToString(),
							Activity = readerLists["activity_type"]?.ToString(),
							Date = Convert.ToDateTime(readerLists["created_at"]).ToString("yyyy-MM-dd HH:mm")
						});
					}
					readerLists.Close();

					// 3. Оценки пользователей
					using var cmdRatings = new MySqlCommand(@"
						SELECT u.username,
							   CONCAT('Поставил оценку ', ur.rating, ' на ', ur.media_type) as activity_type,
							   ur.rated_at as created_at
						FROM user_ratings ur
						JOIN users u ON u.id = ur.user_id
						ORDER BY ur.rated_at DESC
						LIMIT 50", conn);

					using var readerRatings = cmdRatings.ExecuteReader();

					while (readerRatings.Read())
					{
						logs.Add(new
						{
							Username = readerRatings["username"]?.ToString(),
							Activity = readerRatings["activity_type"]?.ToString(),
							Date = Convert.ToDateTime(readerRatings["created_at"]).ToString("yyyy-MM-dd HH:mm")
						});
					}
					readerRatings.Close();

					// Сортируем все логи по дате и берем топ 100
					logs.Sort((a, b) => DateTime.Compare(
						DateTime.ParseExact(b.Date, "yyyy-MM-dd HH:mm", null),
						DateTime.ParseExact(a.Date, "yyyy-MM-dd HH:mm", null)));

					if (logs.Count > 100)
						logs = logs.GetRange(0, 100);
				}

				LogsGrid.ItemsSource = logs;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка загрузки логов: " + ex.Message);
			}
		}

		private bool CheckTableExists(MySqlConnection conn, string tableName)
		{
			try
			{
				using var cmd = new MySqlCommand(@"
					SELECT COUNT(*) 
					FROM information_schema.tables 
					WHERE table_schema = DATABASE() 
					AND table_name = @tableName", conn);

				cmd.Parameters.AddWithValue("@tableName", tableName);
				return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
			}
			catch
			{
				return false;
			}
		}

		private void Refresh_Click(object sender, RoutedEventArgs e)
		{
			LoadLogs();
		}
	}
}