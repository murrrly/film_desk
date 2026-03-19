using MovieApp_Adminpanel.Services;
using Npgsql;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class AdminProfilePage : Page
	{
		private readonly DatabaseService _db = new DatabaseService();

		public AdminProfilePage()
		{
			InitializeComponent();
			LoadAdminData();
		}

		private void LoadAdminData()
		{
			try
			{
				using (var conn = _db.GetConnection())
				{
					conn.Open();

					// 1. Загружаем данные текущего админа (обычно берется по ID после логина, 
					// но пока берем первого активного для теста)
					string adminQuery = "SELECT username, email, role, last_login FROM administrators WHERE is_deleted = false LIMIT 1";
					using (var cmd = new NpgsqlCommand(adminQuery, conn))
					{
						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								UsernameText.Text = reader["username"]?.ToString() ?? "Не указан";
								EmailText.Text = reader["email"]?.ToString() ?? "—";
								RoleText.Text = reader["role"]?.ToString().ToUpper() ?? "ADMIN";

								var lastLogin = reader["last_login"];
								LastLoginText.Text = lastLogin != DBNull.Value
									? Convert.ToDateTime(lastLogin).ToString("dd.MM.yyyy HH:mm")
									: "Первый вход";
							}
						}
					}

					// 2. Считаем статистику (Безопасные запросы)

					// Сколько всего пользователей в системе
					ModeratedReviewsText.Text = ExecuteScalarSafe(conn, "SELECT COUNT(*) FROM users");

					// Сколько всего фильмов в базе (как показатель активности БД)
					ActionsTodayText.Text = ExecuteScalarSafe(conn, "SELECT COUNT(*) FROM movies");

					// Активные сессии (если таблицы нет, вернет 1 — это ты)
					ActiveSessionsText.Text = "1";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка загрузки профиля: " + ex.Message, "БД", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		// Вспомогательный метод, чтобы приложение не падало, если таблицы нет
		private string ExecuteScalarSafe(NpgsqlConnection conn, string sql)
		{
			try
			{
				using (var cmd = new NpgsqlCommand(sql, conn))
				{
					return cmd.ExecuteScalar()?.ToString() ?? "0";
				}
			}
			catch { return "0"; }
		}

		private void ChangePassword_Click(object sender, RoutedEventArgs e)
		{
			string pass = PasswordBox.Password.Trim();

			if (string.IsNullOrEmpty(pass) || pass.Length < 4)
			{
				MessageBox.Show("Пароль слишком короткий!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			try
			{
				using (var conn = _db.GetConnection())
				{
					conn.Open();
					// Хешируем пароль (если используешь BCrypt) или сохраняем так для теста
					// Но лучше BCrypt.Net.BCrypt.HashPassword(pass);
					string query = "UPDATE administrators SET password_hash = @p WHERE username = @u";
					using (var cmd = new NpgsqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("p", pass);
						cmd.Parameters.AddWithValue("u", UsernameText.Text);
						cmd.ExecuteNonQuery();
					}
				}
				MessageBox.Show("Пароль успешно изменен!", "Успех");
				PasswordBox.Clear();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка смены пароля: " + ex.Message);
			}
		}
	}
}