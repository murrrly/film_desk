using MovieApp_Adminpanel.Services;
using Npgsql;
using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class AdminProfilePage : Page
	{
		DatabaseService db = new DatabaseService();

		int adminId = 1;

		public AdminProfilePage()
		{
			InitializeComponent();
			LoadProfile();
		}

		private void LoadProfile()
		{
			using var conn = db.GetConnection();
			conn.Open();

			var cmd = new NpgsqlCommand(
				"SELECT username,email FROM admins WHERE id=@id", conn);

			cmd.Parameters.AddWithValue("id", adminId);

			using var reader = cmd.ExecuteReader();

			if (reader.Read())
			{
				UsernameText.Text = reader.GetString(0);
				EmailBox.Text = reader.GetString(1);
			}
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			using var conn = db.GetConnection();
			conn.Open();

			var cmd = new NpgsqlCommand(
				"UPDATE admins SET email=@email WHERE id=@id", conn);

			cmd.Parameters.AddWithValue("email", EmailBox.Text);
			cmd.Parameters.AddWithValue("id", adminId);

			cmd.ExecuteNonQuery();

			MessageBox.Show("Профиль обновлён");
		}

		private void ChangePassword_Click(object sender, RoutedEventArgs e)
		{
			using var conn = db.GetConnection();
			conn.Open();

			var cmd = new NpgsqlCommand(
				"UPDATE admins SET password_hash=@pass WHERE id=@id", conn);

			cmd.Parameters.AddWithValue("pass", PasswordBox.Password);
			cmd.Parameters.AddWithValue("id", adminId);

			cmd.ExecuteNonQuery();

			MessageBox.Show("Пароль изменён");
		}
	}
}