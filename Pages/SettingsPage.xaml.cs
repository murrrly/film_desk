using MovieApp_Adminpanel.Services;
using Npgsql;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class SettingsPage : Page
	{
		private DatabaseService db = new DatabaseService();

		public SettingsPage()
		{
			InitializeComponent();
			LoadSettings();
		}

		private void LoadSettings()
		{
			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				// Общие параметры
				using (var cmd = new NpgsqlCommand(
					@"SELECT ""setting_key"", ""setting_value"" 
                      FROM system_settings
                      WHERE ""setting_key"" IN ('request_limit', 'cache_ttl', 'auto_moderation')", conn))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string key = reader["setting_key"].ToString();
						string value = reader["setting_value"].ToString();

						switch (key)
						{
							case "request_limit":
								RequestLimitBox.Text = value;
								break;
							case "cache_ttl":
								CacheHoursBox.Text = value;
								break;
							case "auto_moderation":
								AutoModerationCheck.IsChecked = value == "true";
								break;
						}
					}
				}

				// Безопасность
				using (var cmd = new NpgsqlCommand(
					@"SELECT ""setting_key"", ""setting_value"" 
                      FROM system_settings
                      WHERE ""setting_key"" IN ('twofa_enabled', 'password_policy')", conn))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string key = reader["setting_key"].ToString();
						string value = reader["setting_value"].ToString();

						switch (key)
						{
							case "twofa_enabled":
								TwoFaCheck.IsChecked = value == "true";
								break;
							case "password_policy":
								PasswordPolicyBox.Text = value;
								break;
						}
					}
				}

				// TMDB ключ
				using (var cmd = new NpgsqlCommand(
					@"SELECT ""setting_value"" FROM system_settings WHERE ""setting_key""='tmdb_api_key'", conn))
				{
					var result = cmd.ExecuteScalar();
					TmdbKeyBox.Text = result?.ToString() ?? "";
				}

				// Email для уведомлений
				using (var cmd = new NpgsqlCommand(
					@"SELECT ""setting_value"" FROM system_settings WHERE ""setting_key""='admin_email'", conn))
				{
					var result = cmd.ExecuteScalar();
					EmailBox.Text = result?.ToString() ?? "";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}");
			}
		}

		private void SaveSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				void Upsert(string key, string value)
				{
					using var cmd = new NpgsqlCommand(
						@"INSERT INTO system_settings (""setting_key"", ""setting_value"") 
                          VALUES (@key, @value)
                          ON CONFLICT (""setting_key"") DO UPDATE SET ""setting_value"" = EXCLUDED.""setting_value""", conn);
					cmd.Parameters.AddWithValue("key", key);
					cmd.Parameters.AddWithValue("value", value);
					cmd.ExecuteNonQuery();
				}

				// Общие
				Upsert("request_limit", RequestLimitBox.Text);
				Upsert("cache_ttl", CacheHoursBox.Text);
				Upsert("auto_moderation", AutoModerationCheck.IsChecked == true ? "true" : "false");

				// Безопасность
				Upsert("twofa_enabled", TwoFaCheck.IsChecked == true ? "true" : "false");
				Upsert("password_policy", PasswordPolicyBox.Text);

				// TMDB
				Upsert("tmdb_api_key", TmdbKeyBox.Text);

				// Email
				Upsert("admin_email", EmailBox.Text);

				MessageBox.Show("Настройки успешно сохранены!");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}");
			}
		}
	}
}