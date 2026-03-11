using MovieApp_Adminpanel.Services;
using Npgsql;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class MonitoringPage : Page
	{
		DatabaseService db = new DatabaseService();

		public MonitoringPage()
		{
			InitializeComponent();
			LoadLogs();
		}

		private void LoadLogs()
		{
			var logs = new List<dynamic>();

			using var conn = db.GetConnection();
			conn.Open();

			var cmd = new NpgsqlCommand(@"
                SELECT u.username,
                       ua.activity_type,
                       ua.created_at
                FROM user_activity ua
                JOIN users u ON u.id = ua.user_id
                ORDER BY ua.created_at DESC
                LIMIT 100
            ", conn);

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				logs.Add(new
				{
					Username = reader.GetString(0),
					Activity = reader.GetString(1),
					Date = reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm")
				});
			}

			LogsGrid.ItemsSource = logs;
		}

		private void Refresh_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			LoadLogs();
		}
	}
}