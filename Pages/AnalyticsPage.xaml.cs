using System.Windows;
using System.Windows.Controls;

namespace MovieApp_Adminpanel.Pages
{
	public partial class AnalyticsPage : Page
	{
		public AnalyticsPage()
		{
			InitializeComponent();
			LoadDummyData();
		}

		private void LoadDummyData()
		{
			UserActivityGrid.ItemsSource = new[]
			{
				new { Username="john_doe", Registrations=1, Activity="Просмотр фильмов: 5" },
				new { Username="jane_smith", Registrations=0, Activity="Просмотр фильмов: 2" }
			};

			PopularMoviesGrid.ItemsSource = new[]
			{
				new { Title="Inception", Views=120, WatchlistAdds=10 },
				new { Title="Titanic", Views=90, WatchlistAdds=8 }
			};

			SearchErrorsGrid.ItemsSource = new[]
			{
				new { Query="Nonexistent movie", ResultsCount=0 }
			};
		}

		private void GenerateReport_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Отчёт сформирован и доступен для экспорта PDF/Excel.", "Аналитика", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}