using MovieApp_Adminpanel.Services;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using System.Windows.Media;

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
			var users = new List<dynamic>(); // используем dynamic для удобства
			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				string query = @"
                    SELECT username, email, is_active, created_at
                    FROM users
                    WHERE (username ILIKE @search OR email ILIKE @search)
                      AND is_deleted = false
                    ORDER BY created_at DESC";

				using var cmd = new NpgsqlCommand(query, conn);
				cmd.Parameters.AddWithValue("search", $"%{search}%");

				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					bool isActive = reader.GetBoolean(2);
					users.Add(new
					{
						Username = reader.GetString(0),
						Email = reader.GetString(1),
						IsActive = isActive ? "Активен" : "Забанен",
						StatusColor = isActive ? Brushes.LightGreen : (SolidColorBrush)Application.Current.Resources["PrimaryRed"],
						CreatedAt = reader.GetDateTime(3).ToString("dd.MM.yyyy"),
						ActionText = isActive ? "Заблокировать" : "Разблокировать",
						ActionColor = isActive ? (SolidColorBrush)Application.Current.Resources["PrimaryRed"] : Brushes.DimGray
					});
				}

				UsersGrid.ItemsSource = users;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка загрузки: " + ex.Message);
			}
		}

		// Меняем статус пользователя
		private void ToggleStatus_Click(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			string userEmail = button.Tag.ToString();

			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				string query = "UPDATE users SET is_active = NOT is_active WHERE email = @email";
				using var cmd = new NpgsqlCommand(query, conn);
				cmd.Parameters.AddWithValue("email", userEmail);
				cmd.ExecuteNonQuery();

				LoadUsers(SearchBox.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка смены статуса: " + ex.Message);
			}
		}

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			LoadUsers(SearchBox.Text);
		}

		// Экспорт в JSON
		private void ExportCsv_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SaveFileDialog dialog = new SaveFileDialog
				{
					Filter = "JSON file (*.json)|*.json",
					FileName = "users.json"
				};

				if (dialog.ShowDialog() == true)
				{
					// Создаем чистый список для экспорта
					var exportData = new List<object>();
					foreach (dynamic item in UsersGrid.ItemsSource)
					{
						exportData.Add(new
						{
							item.Username,
							item.Email,
							item.IsActive,
							StatusColor = (item.StatusColor as SolidColorBrush)?.Color.ToString(), // HEX
							item.CreatedAt,
							item.ActionText,
							ActionColor = (item.ActionColor as SolidColorBrush)?.Color.ToString() // HEX
						});
					}

					var options = new JsonSerializerOptions { WriteIndented = true };
					string json = JsonSerializer.Serialize(exportData, options);
					File.WriteAllText(dialog.FileName, json);
					MessageBox.Show("Готово!");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}
}