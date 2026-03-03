using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MovieApp_Adminpanel
{
	public partial class LoginWindow : Window
	{
		private bool _isMfaStep = false;

		public LoginWindow()
		{
			InitializeComponent();
		}

		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(UsernameTextBox.Text) ||
				string.IsNullOrEmpty(PasswordBox.Password))
			{
				ShowStatus("⚠ Пожалуйста, заполните все поля", false);
				return;
			}

			if (UsernameTextBox.Text != "admin" || PasswordBox.Password != "admin123")
			{
				ShowStatus("❌ Неверный логин или пароль", false);
				UsernameTextBox.BorderBrush = Brushes.Red;
				PasswordBox.BorderBrush = Brushes.Red;
				return;
			}

			// Сбрасываем красную рамку при успешном логине
			UsernameTextBox.ClearValue(Border.BorderBrushProperty);
			PasswordBox.ClearValue(Border.BorderBrushProperty);

			ShowStatus("✅ Успешная авторизация! Переход к MFA...", true);

			Task.Delay(800).ContinueWith(_ =>
			{
				Dispatcher.Invoke(ShowMfaStep);
			});
		}

		private async void ShowStatus(string message, bool isSuccess)
		{
			PopupStatusText.Text = message;
			PopupStatus.Background = isSuccess
				? new SolidColorBrush(Color.FromRgb(39, 174, 96)) // зеленый
				: new SolidColorBrush(Color.FromRgb(192, 57, 43)); // красный

			PopupStatus.Visibility = Visibility.Visible;

			// Анимация появления
			var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(300)));
			PopupStatus.BeginAnimation(Border.OpacityProperty, fadeIn);

			// Автоматическое скрытие через 3 секунды
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

			if (MfaCodeTextBox.Text == "123456")
			{
				ShowStatus("✅ Успешная аутентификация! Загрузка панели...", true);

				Task.Delay(1000).ContinueWith(_ =>
				{
					Dispatcher.Invoke(() =>
					{
						var mainWindow = new MainWindow();
						mainWindow.Show();
						this.Close();
					});
				});
			}
			else
			{
				ShowStatus("❌ Неверный код. Тестовый код: 123456", false);
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