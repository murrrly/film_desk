using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class UsersPage : Page
	{
		public UsersPage()
		{
			InitializeComponent();
			LoadUsers();
		}

		private void LoadUsers()
		{
			UsersGrid.ItemsSource = new[]
			{
				new { Username="john_doe", Email="john@example.com", IsActive="Active", CreatedAt="2026-03-01" },
				new { Username="jane_smith", Email="jane@example.com", IsActive="Active", CreatedAt="2026-03-02" }
			};
		}

		private void SearchUsers_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Поиск пользователей выполнен", "Пользователи", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void ExportCsv_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Экспорт в CSV завершён", "Пользователи", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void UsersGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			MessageBox.Show("Открыта карточка пользователя", "Пользователи", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}