using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MovieApp_Adminpanel.Services;
using MySqlConnector;

namespace MovieApp_Adminpanel.Pages
{
	public partial class ModerationPage : Page
	{
		private DatabaseService db = new DatabaseService();

		public ModerationPage()
		{
			InitializeComponent();
			LoadModerationQueue();
			LoadAllReviews("");
		}

		// 1. Очередь модерации
		private void LoadModerationQueue()
		{
			var reviews = new List<object>();

			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				// ДОБАВЛЕНО: ur.media_type в SELECT
				using var cmd = new MySqlCommand(@"
                    SELECT ur.id, u.username, ur.tmdb_id, ur.media_type, ur.review_text, ur.created_at
                    FROM user_reviews ur
                    JOIN users u ON u.id = ur.user_id
                    WHERE ur.review_status = 'pending'
                    ORDER BY ur.created_at DESC", conn);

				using var reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					reviews.Add(new
					{
						ReviewId = Convert.ToInt64(reader["id"]),
						Username = reader["username"]?.ToString(),
						MovieTitle = GetMediaTitle(reader["tmdb_id"]?.ToString(), reader["media_type"]?.ToString()),
						ReviewText = reader["review_text"]?.ToString(),
						CreatedAt = Convert.ToDateTime(reader["created_at"]).ToString("dd.MM HH:mm")
					});
				}

				ModerationGrid.ItemsSource = reviews;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка загрузки очереди: " + ex.Message);
			}
		}

		// 2. Архив отзывов
		private void LoadAllReviews(string search)
		{
			var allReviews = new List<object>();

			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				using var cmd = new MySqlCommand(@"
                    SELECT u.username, ur.review_text, ur.review_status, ur.created_at, ur.media_type, ur.tmdb_id
                    FROM user_reviews ur
                    JOIN users u ON u.id = ur.user_id
                    WHERE ur.review_text LIKE @s
                    ORDER BY ur.created_at DESC
                    LIMIT 100", conn);

				cmd.Parameters.AddWithValue("@s", $"%{search}%");

				using var reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					string status = reader["review_status"]?.ToString() ?? "";
					string mediaType = reader["media_type"]?.ToString() ?? "movie";
					string tmdbId = reader["tmdb_id"]?.ToString() ?? "";

					allReviews.Add(new
					{
						Username = reader["username"]?.ToString(),
						ReviewText = reader["review_text"]?.ToString(),
						MediaTitle = GetMediaTitle(tmdbId, mediaType),
						StatusText = GetStatusText(status),
						StatusColor = status == "approved" ? Brushes.LightGreen :
									 status == "rejected" ? Brushes.Tomato :
									 status == "pending" ? Brushes.Gray : Brushes.LightGray,
						CreatedAt = Convert.ToDateTime(reader["created_at"]).ToString("dd.MM.yyyy")
					});
				}

				AllReviewsGrid.ItemsSource = allReviews;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка загрузки архива: " + ex.Message);
			}
		}

		// Получение названия фильма/сериала по TMDB ID
		private string GetMediaTitle(string tmdbId, string mediaType)
		{
			if (string.IsNullOrEmpty(tmdbId))
				return "Неизвестно";

			string typeText = mediaType == "tv" ? "📺 Сериал" : "🎬 Фильм";
			return $"{typeText} #{tmdbId}";
		}

		// Получение текста статуса на русском
		private string GetStatusText(string status)
		{
			switch (status)
			{
				case "approved": return "ОДОБРЕН";
				case "rejected": return "ОТКЛОНЕН";
				case "pending": return "НА РАССМОТРЕНИИ";
				default: return status.ToUpper();
			}
		}

		private void SearchAll_Click(object sender, RoutedEventArgs e)
		{
			LoadAllReviews(SearchBoxAll.Text);
		}

		private void SearchBoxAll_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (SearchBoxAll.Text.Length >= 2 || SearchBoxAll.Text.Length == 0)
			{
				LoadAllReviews(SearchBoxAll.Text);
			}
		}

		private void Approve_Click(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			if (button == null) return;

			dynamic row = button.DataContext;

			if (row != null)
			{
				UpdateStatus(row.ReviewId, "approved");
				LoadModerationQueue();
				LoadAllReviews("");
			}
			else
			{
				MessageBox.Show("Не удалось определить отзыв", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void Reject_Click(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			if (button == null) return;

			dynamic row = button.DataContext;

			if (row != null)
			{
				string moderationNotes = Microsoft.VisualBasic.Interaction.InputBox(
					"Укажите причину отклонения (необязательно):",
					"Отклонение отзыва",
					"");

				UpdateStatus(row.ReviewId, "rejected", moderationNotes);
				LoadModerationQueue();
				LoadAllReviews("");
			}
			else
			{
				MessageBox.Show("Не удалось определить отзыв", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void UpdateStatus(long id, string status, string moderationNotes = null)
		{
			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				string query = @"
                    UPDATE user_reviews
                    SET review_status = @s,
                        moderated_at = NOW(),
                        moderation_notes = @notes
                    WHERE id = @id";

				using var cmd = new MySqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@s", status);
				cmd.Parameters.AddWithValue("@notes", moderationNotes ?? (object)DBNull.Value);
				cmd.Parameters.AddWithValue("@id", id);

				cmd.ExecuteNonQuery();

				MessageBox.Show($"Отзыв {status}!", "Успех", MessageBoxButton.OK,
					status == "approved" ? MessageBoxImage.Information : MessageBoxImage.Warning);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при обновлении: " + ex.Message);
			}
		}

		private void AllReviewsTab_Selected(object sender, RoutedEventArgs e)
		{
			LoadAllReviews("");
		}
	}
}