using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class ModerationPage : Page
	{
		public ModerationPage()
		{
			InitializeComponent();
			LoadModerationQueue();
		}

		private void LoadModerationQueue()
		{
			ModerationGrid.ItemsSource = new[]
			{
				new { Username="john_doe", MovieTitle="Inception", ReviewText="Отлично!", CreatedAt="2026-03-03" },
				new { Username="jane_smith", MovieTitle="Titanic", ReviewText="Неплохо", CreatedAt="2026-03-03" }
			};
		}

		private void Search_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Фильтр применён", "Модерация", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void Approve_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Рецензия одобрена", "Модерация", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void Reject_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Рецензия отклонена", "Модерация", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}