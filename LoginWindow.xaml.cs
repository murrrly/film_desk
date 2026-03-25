using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MovieApp_Adminpanel.Pages;
using MovieApp_Adminpanel.Services;
using Npgsql;

namespace MovieApp_Adminpanel
{
	public partial class LoginWindow : Window
	{
		private bool _isMfaStep = false;
		private long _currentAdminId;

		private readonly DatabaseService db = new DatabaseService();

		public LoginWindow()
		{
			InitializeComponent();
			LoadRememberMe();
		}

		private void LoadRememberMe()
		{
			var data = RememberMeService.Load();
			UsernameTextBox.Text = data.Username;
			PasswordBox.Password = data.Password;
			RememberMeCheckBox.IsChecked = data.RememberMe;
		}

		private void SaveRememberMe()
		{
			var data = new RememberMeData
			{
				Username = RememberMeCheckBox.IsChecked == true ? UsernameTextBox.Text : "",
				Password = RememberMeCheckBox.IsChecked == true ? PasswordBox.Password : "",
				RememberMe = RememberMeCheckBox.IsChecked == true
			};
			RememberMeService.Save(data);
		}

		private async void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(UsernameTextBox.Text) || string.IsNullOrEmpty(PasswordBox.Password))
			{
				ShowStatus("⚠ Заполните все поля", false);
				return;
			}

			try
			{
				using (var conn = db.GetConnection())
				{
					await conn.OpenAsync();

					string sql = @"
                        SELECT id 
                        FROM administrators 
                        WHERE username = @username 
                        AND is_deleted = false 
                        AND is_active = true
                        AND (
                            password_hash = @password
                            OR password_hash = crypt(@password, password_hash)
                        )";

					using (var cmd = new NpgsqlCommand(sql, conn))
					{
						cmd.Parameters.AddWithValue("username", UsernameTextBox.Text);
						cmd.Parameters.AddWithValue("password", PasswordBox.Password);

						var result = await cmd.ExecuteScalarAsync();

						if (result == null)
						{
							ShowStatus("❌ Неверный логин или пароль", false);
							return;
						}

						_currentAdminId = Convert.ToInt64(result);

						// 🔥 авто-хеширование (если пароль был plain)
						string hashSql = @"
                            UPDATE administrators 
                            SET password_hash = crypt(@password, gen_salt('bf'))
                            WHERE id = @id AND password_hash = @password";

						using (var hashCmd = new NpgsqlCommand(hashSql, conn))
						{
							hashCmd.Parameters.AddWithValue("password", PasswordBox.Password);
							hashCmd.Parameters.AddWithValue("id", _currentAdminId);
							await hashCmd.ExecuteNonQueryAsync();
						}

						// ✅ обновим last_login
						string updateSql = "UPDATE administrators SET last_login = NOW() WHERE id = @id";
						using (var updateCmd = new NpgsqlCommand(updateSql, conn))
						{
							updateCmd.Parameters.AddWithValue("id", _currentAdminId);
							await updateCmd.ExecuteNonQueryAsync();
						}

						// Сохраняем логин/пароль в JSON
						SaveRememberMe();

						AdminSession.CurrentAdminId = _currentAdminId;
						AdminSession.Username = UsernameTextBox.Text;

						ShowStatus("✅ Успешный вход!", true);
						await Task.Delay(800);
						ShowMfaStep();
					}
				}
			}
			catch (Exception ex)
			{
				ShowStatus("Ошибка подключения к БД", false);
				MessageBox.Show(ex.Message);
			}
		}

		private async void ShowStatus(string message, bool isSuccess)
		{
			PopupStatusText.Text = message;
			PopupStatus.Background = isSuccess
				? new SolidColorBrush(Color.FromRgb(39, 174, 96))
				: new SolidColorBrush(Color.FromRgb(192, 57, 43));

			PopupStatus.Visibility = Visibility.Visible;

			var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(300)));
			PopupStatus.BeginAnimation(Border.OpacityProperty, fadeIn);

			await Task.Delay(3000);

			var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(300)));
			fadeOut.Completed += (s, e) => PopupStatus.Visibility = Visibility.Collapsed;
			PopupStatus.BeginAnimation(Border.OpacityProperty, fadeOut);
		}

		private void ShowMfaStep()
		{
			_isMfaStep = true;
			Step1Panel.Visibility = Visibility.Collapsed;
			Step2Panel.Visibility = Visibility.Visible;
			StatusBorder.Visibility = Visibility.Collapsed;
			MfaCodeTextBox.Focus();
			MfaCodeTextBox.SelectAll();
		}

		private void BackButton_Click(object sender, RoutedEventArgs e)
		{
			_isMfaStep = false;
			Step2Panel.Visibility = Visibility.Collapsed;
			Step1Panel.Visibility = Visibility.Visible;
			StatusBorder.Visibility = Visibility.Collapsed;
		}

		private void VerifyMfaButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(MfaCodeTextBox.Text) || MfaCodeTextBox.Text.Length != 6)
			{
				ShowStatus("Введите 6-значный код", false);
				return;
			}

			if (MfaCodeTextBox.Text == "123456") // тест
			{
				var profilePage = new DashboardPage();
				var mainWindow = new MainWindow();
				mainWindow.MainFrame.Content = profilePage;
				mainWindow.Show();
				this.Close();
			}
			else
			{
				ShowStatus("❌ Неверный код", false);
				MfaCodeTextBox.Clear();
				MfaCodeTextBox.Focus();
			}
		}

		private void MfaCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			StatusBorder.Visibility = Visibility.Collapsed;
		}
	}
}