using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MovieApp_Adminpanel.Services;
using Npgsql;

namespace MovieApp_Adminpanel.Pages
{
	public partial class ModerationPage : Page
	{
		private DatabaseService db = new DatabaseService();

		public ModerationPage()
		{
			InitializeComponent();
			LoadModerationQueue();
		}

		private void LoadModerationQueue()
		{
			var reviews = new List<dynamic>();

			try
			{
				using var conn = db.GetConnection();
				conn.Open();

				var cmd = new NpgsqlCommand(@"
                    SELECT ur.id,
                           u.username,
                           ur.tmdb_id,
                           ur.review_text,
                           ur.created_at
                    FROM user_reviews ur
                    JOIN users u ON u.id = ur.user_id
                    WHERE ur.review_status='pending'
                    ORDER BY ur.created_at DESC", conn);

				using var reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					reviews.Add(new
					{
						ReviewId = reader.GetInt64(0),
						Username = reader.GetString(1),
						MovieTitle = reader.GetInt32(2),
						ReviewText = reader.GetString(3),
						CreatedAt = reader.GetDateTime(4).ToString("yyyy-MM-dd")
					});
				}

				ModerationGrid.ItemsSource = reviews;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		// кнопка поиска
		private void Search_Click(object sender, RoutedEventArgs e)
		{
			LoadModerationQueue();
		}

		private void Approve_Click(object sender, RoutedEventArgs e)
		{
			dynamic row = (sender as Button).DataContext;

			ApproveReview(row.ReviewId);

			MessageBox.Show("Рецензия одобрена");

			LoadModerationQueue();
		}

		private void Reject_Click(object sender, RoutedEventArgs e)
		{
			dynamic row = (sender as Button).DataContext;

			RejectReview(row.ReviewId);

			MessageBox.Show("Рецензия отклонена");

			LoadModerationQueue();
		}

		private void ApproveReview(long reviewId)
		{
			using var conn = db.GetConnection();
			conn.Open();

			var cmd = new NpgsqlCommand(@"
                UPDATE user_reviews
                SET review_status='approved',
                    moderated_at=NOW()
                WHERE id=@id", conn);

			cmd.Parameters.AddWithValue("id", reviewId);

			cmd.ExecuteNonQuery();
		}

		private void RejectReview(long reviewId)
		{
			using var conn = db.GetConnection();
			conn.Open();

			var cmd = new NpgsqlCommand(@"
                UPDATE user_reviews
                SET review_status='rejected',
                    moderated_at=NOW()
                WHERE id=@id", conn);

			cmd.Parameters.AddWithValue("id", reviewId);

			cmd.ExecuteNonQuery();
		}
	}
}