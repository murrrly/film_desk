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

				LogsGrid.ItemsSource = logs;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка загрузки логов: " + ex.Message);
			}
		}

		private void Refresh_Click(object sender, RoutedEventArgs e)
		{
			LoadLogs();
		}
	}
}