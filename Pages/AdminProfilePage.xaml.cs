using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class AdminProfilePage : Page
	{
		public AdminProfilePage()
		{
			InitializeComponent();
			LoadAuditLog();
		}

		private void LoadAuditLog()
		{
			AuditLogGrid.ItemsSource = new[]
			{
				new { CreatedAt="2026-03-03 09:00", Action="Вход в систему" },
				new { CreatedAt="2026-03-02 18:00", Action="Одобрена рецензия" }
			};
		}

		private void ChangePassword_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Пароль успешно изменён.", "Профиль", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void Logout_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Вы вышли из системы.", "Профиль", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}