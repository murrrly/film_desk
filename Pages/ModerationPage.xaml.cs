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

				using var cmd = new MySqlCommand(@"
                    SELECT ur.id, u.username, ur.tmdb_id, ur.review_text, ur.created_at
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
						MovieTitle = reader["tmdb_id"]?.ToString(),
						ReviewText = reader["review_text"]?.ToString(),
						CreatedAt = Convert.ToDateTime(reader["created_at"]).ToString("dd.MM HH:mm")
					});
				}

				ModerationGrid.ItemsSource = reviews;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
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
                    SELECT u.username, ur.review_text, ur.review_status, ur.created_at
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

					allReviews.Add(new
					{
						Username = reader["username"]?.ToString(),
						ReviewText = reader["review_text"]?.ToString(),
						StatusText = status.ToUpper(),
						StatusColor = status == "approved" ? Brushes.LightGreen :
									 status == "rejected" ? Brushes.Tomato :
									 Brushes.Gray,
						CreatedAt = Convert.ToDateTime(reader["created_at"]).ToString("dd.MM.yyyy")
					});
				}

				AllReviewsGrid.ItemsSource = allReviews;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
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
			dynamic row = (sender as Button)?.DataContext;

			if (row != null)
			{
				UpdateStatus(row.ReviewId, "approved");
				LoadModerationQueue();
				LoadAllReviews("");
			}
		}

		private void Reject_Click(object sender, RoutedEventArgs e)
		{
			dynamic row = (sender as Button)?.DataContext;

			if (row != null)
			{
				UpdateStatus(row.ReviewId, "rejected");
				LoadModerationQueue();
				LoadAllReviews("");
			}
		}

		private void UpdateStatus(long id, string status)
		{
			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				using var cmd = new MySqlCommand(@"
                    UPDATE user_reviews
                    SET review_status = @s,
                        moderated_at = NOW()
                    WHERE id = @id", conn);

				cmd.Parameters.AddWithValue("@s", status);
				cmd.Parameters.AddWithValue("@id", id);

				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void AllReviewsTab_Selected(object sender, RoutedEventArgs e)
		{
			LoadAllReviews("");
		}
	}
}