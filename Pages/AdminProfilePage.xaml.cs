using MovieApp_Adminpanel.Services;
using Npgsql;
using System.Windows;
using System.Windows.Controls;
using BCrypt.Net;

namespace MovieApp_Adminpanel.Pages
{
	public partial class AdminProfilePage : Page
	{
		private readonly DatabaseService db = new DatabaseService();
		private long adminId;

		public AdminProfilePage(long currentAdminId)
		{
			InitializeComponent();
			adminId = currentAdminId;
			LoadAdminProfile();
		}

		private void LoadAdminProfile()
		{
			using var conn = db.GetConnection();
			conn.Open();

			var cmd = new NpgsqlCommand(
				"SELECT username, email, role FROM administrators WHERE id=@id AND is_deleted=false", conn);
			cmd.Parameters.AddWithValue("id", adminId);

			using var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				UsernameText.Text = reader.GetString(0);
				EmailBox.Text = reader.GetString(1);
				RoleText.Text = reader.GetString(2);
			}
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(PasswordBox.Password))
			{
				MessageBox.Show("Пароль не может быть пустым.");
				return;
			}

			string hashedPassword = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password);

			using var conn = db.GetConnection();
			conn.Open();

			var cmd = new NpgsqlCommand(
				@"UPDATE administrators 
                  SET password_hash=@password
                  WHERE id=@id", conn);
			cmd.Parameters.AddWithValue("password", hashedPassword);
			cmd.Parameters.AddWithValue("id", adminId);

			int updated = cmd.ExecuteNonQuery();
			if (updated > 0)
				MessageBox.Show("Пароль успешно обновлён!");
			else
				MessageBox.Show("Ошибка при обновлении пароля.");
		}
	}
}