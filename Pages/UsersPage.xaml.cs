using MovieApp_Adminpanel.Services;
using Npgsql;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace MovieApp_Adminpanel.Pages
{
	public partial class UsersPage : Page
	{
		DatabaseService db = new DatabaseService();

		public UsersPage()
		{
			InitializeComponent();
			LoadUsers("");
		}

		private void LoadUsers(string search)
		{
			var users = new List<dynamic>();

			using var conn = db.GetConnection();
			conn.Open();

			string query = @"
                SELECT username, email, is_active, created_at
                FROM users
                WHERE username ILIKE @search
                   OR email ILIKE @search
                ORDER BY created_at DESC";

			using var cmd = new NpgsqlCommand(query, conn);

			cmd.Parameters.AddWithValue("search", $"%{search}%");

			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				users.Add(new
				{
					Username = reader.GetString(0),
					Email = reader.GetString(1),
					IsActive = reader.GetBoolean(2) ? "Active" : "Blocked",
					CreatedAt = reader.GetDateTime(3).ToString("yyyy-MM-dd")
				});
			}

			UsersGrid.ItemsSource = users;
		}

		// поиск при вводе
		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			LoadUsers(SearchBox.Text);
		}

		private void ExportCsv_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (UsersGrid.Items.Count == 0)
				{
					MessageBox.Show("Нет данных для экспорта");
					return;
				}

				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Filter = "JSON file (*.json)|*.json";
				dialog.FileName = "users_export.json";

				if (dialog.ShowDialog() != true)
					return;

				var users = new List<object>();

				foreach (var item in UsersGrid.Items)
				{
					if (item == null) continue;

					dynamic u = item;

					users.Add(new
					{
						Username = u.Username,
						Email = u.Email,
						Status = u.IsActive,
						CreatedAt = u.CreatedAt
					});
				}

				var options = new JsonSerializerOptions
				{
					WriteIndented = true
				};

				string json = JsonSerializer.Serialize(users, options);

				File.WriteAllText(dialog.FileName, json);

				MessageBox.Show("Экспорт успешно выполнен");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка экспорта: " + ex.Message);
			}
		}
	}
}