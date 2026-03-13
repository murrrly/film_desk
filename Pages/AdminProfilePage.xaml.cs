using MovieApp_Adminpanel.Services;
using Npgsql;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class AdminProfilePage : Page
	{
		private readonly DatabaseService _db;

		public AdminProfilePage()
		{
			InitializeComponent();
			_db = new DatabaseService();

			LoadProfile();
			LoadActivity();
		}

		private void LoadProfile()
		{
			using var conn = _db.GetConnection();
			conn.Open();

			// Берём первого администратора для теста
			var cmd = new NpgsqlCommand("SELECT id, username, email, role, last_login FROM administrators WHERE is_deleted=false ORDER BY id LIMIT 1", conn);
			using var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				UsernameText.Text = reader["username"].ToString();
				EmailText.Text = reader["email"].ToString();
				RoleText.Text = reader["role"].ToString();
				LastLoginText.Text = reader["last_login"] != DBNull.Value
					? Convert.ToDateTime(reader["last_login"]).ToString("yyyy-MM-dd HH:mm")
					: "—";
			}
		}

		private void LoadActivity()
		{
			using var conn = _db.GetConnection();
			conn.Open();

			// Примерные данные активности
			var cmdReviews = new NpgsqlCommand(
				"SELECT COUNT(*) FROM user_reviews WHERE moderated_by IS NOT NULL", conn);
			ModeratedReviewsText.Text = $"Модерировано отзывов: {cmdReviews.ExecuteScalar()}";

			var cmdActions = new NpgsqlCommand(
				"SELECT COUNT(*) FROM admin_activity_log WHERE created_at >= NOW() - INTERVAL '1 day'", conn);
			ActionsTodayText.Text = $"Действий сегодня: {cmdActions.ExecuteScalar()}";

			var cmdSessions = new NpgsqlCommand(
				"SELECT COUNT(*) FROM admin_sessions WHERE expires_at > NOW()", conn);
			ActiveSessionsText.Text = $"Активных сессий: {cmdSessions.ExecuteScalar()}";
		}

		private void ChangePassword_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(PasswordBox.Password))
			{
				MessageBox.Show("Введите новый пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			using var conn = _db.GetConnection();
			conn.Open();

			var cmd = new NpgsqlCommand(
				"UPDATE administrators SET password_hash=@hash WHERE id=(SELECT id FROM administrators ORDER BY id LIMIT 1)", conn);
			cmd.Parameters.AddWithValue("hash", BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password));
			cmd.ExecuteNonQuery();

			MessageBox.Show("Пароль успешно изменён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
			PasswordBox.Clear();
		}
	}
}
