using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class MonitoringPage : Page
	{
		public MonitoringPage()
		{
			InitializeComponent();
			LoadLogs();
		}

		private void LoadLogs()
		{
			LogsGrid.ItemsSource = new[]
			{
				new { CreatedAt = "2026-03-03 12:00", Type="TMDB", Message="Ошибка авторизации" },
				new { CreatedAt = "2026-03-02 16:00", Type="Админ", Message="Блокировка пользователя" }
			};
		}

		private void ApplyFilter_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Фильтры применены", "Мониторинг", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void ClearOldLogs_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Логи старше 30 дней удалены", "Мониторинг", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}