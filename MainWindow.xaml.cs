using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MovieApp_Adminpanel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadMockData();
        }

        private void LoadMockData()
        {
            // Mock данные для пользователей
            var users = new List<User>
            {
                new User { Id = 1, Username = "admin", Email = "admin@movies.com", Role = "Admin", CreatedAt = "2024-01-15" },
                new User { Id = 2, Username = "user1", Email = "user1@mail.com", Role = "User", CreatedAt = "2024-02-20" },
                new User { Id = 3, Username = "moderator", Email = "mod@movies.com", Role = "Moderator", CreatedAt = "2024-01-10" },
                new User { Id = 4, Username = "user2", Email = "user2@mail.com", Role = "User", CreatedAt = "2024-03-05" }
            };

            UsersDataGrid.ItemsSource = users;

            // Mock данные для отзывов
            var reviews = new List<Review>
            {
                new Review { Id = 1, MovieTitle = "Интерстеллар", User = "user1", Rating = 9, Date = "2024-03-10", Status = "На модерации" },
                new Review { Id = 2, MovieTitle = "Начало", User = "user2", Rating = 8, Date = "2024-03-09", Status = "На модерации" },
                new Review { Id = 3, MovieTitle = "Побег из Шоушенка", User = "user3", Rating = 10, Date = "2024-03-08", Status = "На модерации" }
            };

            ReviewsDataGrid.ItemsSource = reviews;

            // Mock данные для пользовательских списков
            var userLists = new List<UserList>
            {
                new UserList { Username = "user1", ListType = "Watchlist", ListName = "Хочу посмотреть", MovieCount = 15, CreatedAt = "2024-02-25" },
                new UserList { Username = "user2", ListType = "Favorites", ListName = "Любимые фильмы", MovieCount = 8, CreatedAt = "2024-03-01" },
                new UserList { Username = "user1", ListType = "Watched", ListName = "Просмотренные", MovieCount = 42, CreatedAt = "2024-01-20" }
            };

            UserListsDataGrid.ItemsSource = userLists;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void SaveDbSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Настройки базы данных успешно сохранены", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TestDbConnection_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Подключение к базе данных успешно проверено", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция редактирования пользователя", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления пользователя", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewReview_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Просмотр полного текста отзыва", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApproveReview_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Отзыв одобрен", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RejectReview_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Отзыв отклонен", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Поиск пользователя", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewUserList_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Просмотр пользовательского списка", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Отчет успешно сгенерирован", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string CreatedAt { get; set; }
    }

    public class Review
    {
        public int Id { get; set; }
        public string MovieTitle { get; set; }
        public string User { get; set; }
        public int Rating { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
    }

    public class UserList
    {
        public string Username { get; set; }
        public string ListType { get; set; }
        public string ListName { get; set; }
        public int MovieCount { get; set; }
        public string CreatedAt { get; set; }
    }
}