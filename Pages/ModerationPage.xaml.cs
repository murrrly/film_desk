using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
			LoadAllReviews(""); // Загружаем историю сразу или при переключении таба
		}

		// 1. Загрузка очереди (только pending)
		private void LoadModerationQueue()
		{
			var reviews = new List<object>();
			try
			{
				using var conn = db.GetConnection();
				conn.Open();
				var cmd = new NpgsqlCommand(@"
                    SELECT ur.id, u.username, ur.tmdb_id, ur.review_text, ur.created_at
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
						CreatedAt = reader.GetDateTime(4).ToString("dd.MM HH:mm")
					});
				}
				ModerationGrid.ItemsSource = reviews;
			}
			catch (Exception ex) { MessageBox.Show(ex.Message); }
		}

		// 2. Загрузка ВСЕХ отзывов (Архив)
		private void LoadAllReviews(string search)
		{
			var allReviews = new List<object>();
			try
			{
				using var conn = db.GetConnection();
				conn.Open();
				var cmd = new NpgsqlCommand(@"
                    SELECT u.username, ur.review_text, ur.review_status, ur.created_at
                    FROM user_reviews ur
                    JOIN users u ON u.id = ur.user_id
                    WHERE ur.review_text ILIKE @s
                    ORDER BY ur.created_at DESC LIMIT 100", conn);
				cmd.Parameters.AddWithValue("s", $"%{search}%");

				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					string status = reader.GetString(2);
					allReviews.Add(new
					{
						Username = reader.GetString(0),
						ReviewText = reader.GetString(1),
						StatusText = status.ToUpper(),
						StatusColor = status == "approved" ? Brushes.LightGreen :
									 status == "rejected" ? Brushes.Tomato : Brushes.Gray,
						CreatedAt = reader.GetDateTime(3).ToString("dd.MM.yyyy")
					});
				}
				AllReviewsGrid.ItemsSource = allReviews;
			}
			catch (Exception ex) { MessageBox.Show(ex.Message); }
		}

		private void SearchAll_Click(object sender, RoutedEventArgs e) => LoadAllReviews(SearchBoxAll.Text);

		private void Approve_Click(object sender, RoutedEventArgs e)
		{
			var row = (sender as Button).DataContext as dynamic;
			UpdateStatus(row.ReviewId, "approved");
			LoadModerationQueue();
			LoadAllReviews(""); // Обновляем историю
		}

		// Поиск в реальном времени для истории отзывов
		private void SearchBoxAll_TextChanged(object sender, TextChangedEventArgs e)
		{
			// Начинаем поиск, только если введено хотя бы 2 символа (чтобы не грузить БД пустыми запросами)
			// Либо если строка пустая (чтобы вернуть полный список)
			if (SearchBoxAll.Text.Length >= 2 || SearchBoxAll.Text.Length == 0)
			{
				LoadAllReviews(SearchBoxAll.Text);
			}
		}

		private void Reject_Click(object sender, RoutedEventArgs e)
		{
			var row = (sender as Button).DataContext as dynamic;
			UpdateStatus(row.ReviewId, "rejected");
			LoadModerationQueue();
			LoadAllReviews(""); // Обновляем историю
		}

		private void UpdateStatus(long id, string status)
		{
			try
			{
				using var conn = db.GetConnection();
				conn.Open();
				var cmd = new NpgsqlCommand("UPDATE user_reviews SET review_status=@s, moderated_at=NOW() WHERE id=@id", conn);
				cmd.Parameters.AddWithValue("s", status);
				cmd.Parameters.AddWithValue("id", id);
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex) { MessageBox.Show(ex.Message); }
		}

		// Если хочешь подгружать данные только при нажатии на вкладку
		private void AllReviewsTab_Selected(object sender, RoutedEventArgs e) => LoadAllReviews("");
	}
}