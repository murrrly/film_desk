using System;
using System.Windows;
using System.Windows.Controls;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using MovieApp_Adminpanel.Services;
using Npgsql;

namespace MovieApp_Adminpanel.Pages
{
	public partial class AnalyticsPage : Page
	{
		private readonly DatabaseService db = new DatabaseService();

		public AnalyticsPage()
		{
			InitializeComponent();
			LoadAnalytics();
		}

		private void LoadAnalytics()
		{
			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				// Всего пользователей
				var cmdUsers = new NpgsqlCommand(
					"SELECT COUNT(*) FROM users WHERE is_deleted=false", conn);
				TotalUsersText.Text = cmdUsers.ExecuteScalar()?.ToString() ?? "0";

				// Всего одобренных отзывов
				var cmdReviews = new NpgsqlCommand(
					"SELECT COUNT(*) FROM user_reviews WHERE review_status='approved'", conn);
				TotalReviewsText.Text = cmdReviews.ExecuteScalar()?.ToString() ?? "0";

				// Средний рейтинг из user_ratings
				var cmdRating = new NpgsqlCommand(
					"SELECT ROUND(AVG(rating),2) FROM user_ratings", conn);
				AvgRatingText.Text = cmdRating.ExecuteScalar()?.ToString() ?? "0";

				// Активные пользователи за последние 24 часа
				var cmdActive = new NpgsqlCommand(
					"SELECT COUNT(DISTINCT user_id) FROM user_activity WHERE created_at >= NOW() - INTERVAL '24 hours'", conn);
				ActiveUsersText.Text = cmdActive.ExecuteScalar()?.ToString() ?? "0";

				// Загружаем активность пользователей (UserActivityGrid)
				var usersActivity = new List<dynamic>();
				using (var cmd = new NpgsqlCommand(
					@"SELECT u.username,
                             COUNT(DISTINCT ul.id) as Registrations,
                             COUNT(DISTINCT ua.id) as Activity
                      FROM users u
                      LEFT JOIN user_lists ul ON ul.user_id = u.id
                      LEFT JOIN user_activity ua ON ua.user_id = u.id
                      GROUP BY u.username
                      ORDER BY Activity DESC
                      LIMIT 20", conn))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						usersActivity.Add(new
						{
							Username = reader["username"].ToString(),
							Registrations = reader["registrations"].ToString(),
							Activity = reader["activity"].ToString()
						});
					}
				}
				UserActivityGrid.ItemsSource = usersActivity;

				// Популярные фильмы (PopularMoviesGrid)
				var popularMovies = new List<dynamic>();
				using (var cmd = new NpgsqlCommand(
					@"SELECT tmdb_id, media_type, COUNT(*) as Views,
                             SUM(CASE WHEN watched THEN 1 ELSE 0 END) as WatchlistAdds
                      FROM list_items
                      GROUP BY tmdb_id, media_type
                      ORDER BY Views DESC
                      LIMIT 20", conn))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						popularMovies.Add(new
						{
							Title = $"{reader["media_type"]} #{reader["tmdb_id"]}",
							Views = reader["views"].ToString(),
							WatchlistAdds = reader["watchlistadds"].ToString()
						});
					}
				}
				PopularMoviesGrid.ItemsSource = popularMovies;

				// Ошибки поиска (SearchErrorsGrid)
				var searchErrors = new List<dynamic>();
				using (var cmd = new NpgsqlCommand(
					@"SELECT search_query AS Query,
                             search_results_count AS ResultsCount
                      FROM search_history
                      WHERE search_results_count = 0
                      ORDER BY created_at DESC
                      LIMIT 20", conn))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						searchErrors.Add(new
						{
							Query = reader["query"].ToString(),
							ResultsCount = reader["resultscount"].ToString()
						});
					}
				}
				SearchErrorsGrid.ItemsSource = searchErrors;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка загрузки аналитики: {ex.Message}");
			}
		}

		private void Refresh_Click(object sender, RoutedEventArgs e)
		{
			LoadAnalytics();
		}

		private void GenerateReport_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog
			{
				Filter = "Word Document (*.docx)|*.docx",
				FileName = "AnalyticsReport.docx"
			};

			if (dialog.ShowDialog() != true) return;

			string path = dialog.FileName;

			using (WordprocessingDocument word = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document))
			{
				MainDocumentPart mainPart = word.AddMainDocumentPart();
				mainPart.Document = new Document();
				Body body = new Body();

				// Заголовок
				body.Append(new Paragraph(new Run(new RunProperties(new Bold(), new FontSize() { Val = "32" }),
					new Text("Отчёт аналитики MovieApp"))));
				body.Append(new Paragraph(new Run(new Text(""))));

				// Период
				string from = DateFromPicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "—";
				string to = DateToPicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "—";
				body.Append(new Paragraph(new Run(new Text($"Период: {from} — {to}"))));
				body.Append(new Paragraph(new Run(new Text(""))));

				// Общая статистика
				body.Append(new Paragraph(new Run(new Bold(), new Text("Общая статистика"))));
				body.Append(new Paragraph(new Run(new Text($"Всего пользователей: {TotalUsersText.Text}"))));
				body.Append(new Paragraph(new Run(new Text($"Всего отзывов: {TotalReviewsText.Text}"))));
				body.Append(new Paragraph(new Run(new Text($"Средний рейтинг: {AvgRatingText.Text}"))));
				body.Append(new Paragraph(new Run(new Text($"Активных пользователей: {ActiveUsersText.Text}"))));
				body.Append(new Paragraph(new Run(new Text(""))));

				mainPart.Document.Append(body);
				mainPart.Document.Save();
			}

			MessageBox.Show("Отчёт успешно создан!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
	public class UserActivityModel
	{
		public string Username { get; set; }
		public int Registrations { get; set; }
		public int Activity { get; set; } // например, количество действий за период
	}

	public class PopularMovieModel
	{
		public string Title { get; set; }
		public int Views { get; set; }
		public int WatchlistAdds { get; set; }
	}

	public class SearchErrorModel
	{
		public string Query { get; set; }
		public int ResultsCount { get; set; }
	}
}