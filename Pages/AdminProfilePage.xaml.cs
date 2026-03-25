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

					string adminQuery = @"
SELECT username, email, role, last_login 
FROM administrators 
WHERE id = @id";

					using (var cmd = new NpgsqlCommand(adminQuery, conn))
					{
						cmd.Parameters.AddWithValue("id", AdminSession.CurrentAdminId);

						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								UsernameText.Text = reader["username"]?.ToString();
								EmailText.Text = reader["email"]?.ToString();
								RoleText.Text = reader["role"]?.ToString()?.ToUpper();

								var lastLogin = reader["last_login"];
								LastLoginText.Text = lastLogin != DBNull.Value
									? Convert.ToDateTime(lastLogin).ToString("dd.MM.yyyy HH:mm")
									: "Первый вход";
							}
						}
					}

					ModeratedReviewsText.Text = ExecuteScalarSafe(conn, "SELECT COUNT(*) FROM users");
					ActionsTodayText.Text = ExecuteScalarSafe(conn, "SELECT COUNT(*) FROM movies");
					ActiveSessionsText.Text = "1";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка загрузки профиля: " + ex.Message);
			}
		}

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

			if (string.IsNullOrEmpty(pass))
			{
				MessageBox.Show("Введите пароль!");
				return;
			}

			try
			{
				using (var conn = _db.GetConnection())
				{
					conn.Open();

					string query = @"
UPDATE administrators 
SET password_hash = crypt(@p, gen_salt('bf'))
WHERE id = @id";

					using (var cmd = new NpgsqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("p", pass);
						cmd.Parameters.AddWithValue("id", AdminSession.CurrentAdminId);
						cmd.ExecuteNonQuery();
					}
				}

				MessageBox.Show("Пароль обновлён 🔐");
				PasswordBox.Clear();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка: " + ex.Message);
			}
		}
	}
}