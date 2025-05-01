using Npgsql;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace JobNest
{
    public partial class Authorization : Page
    {
        private readonly string _connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";
        private Brush _originalBorderBrush = Brushes.Gray;

        public Authorization()
        {
            InitializeComponent();
            _originalBorderBrush = txtLoginUsername.BorderBrush;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLoginUsername.Text))
                {
                    SetError(txtLoginUsername, "Введите имя пользователя");
                    return;
                }

                if (string.IsNullOrWhiteSpace(GetLoginPassword()))
                {
                    SetError(txtLoginPassword, "Введите пароль");
                    return;
                }

                var user = await AuthenticateUserAsync(txtLoginUsername.Text, GetLoginPassword());

                if (user != null)
                {
                    AuthManager.Login(user);
                    MessageBox.Show($"Добро пожаловать, {user.Name}!", "Успешный вход",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService?.Navigate(new MainPage());
                }
            }
            catch (PostgresException ex)
            {
                MessageBox.Show("Неверное имя пользователя или пароль", "Ошибка входа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void UserIcon_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new ProfilePage());
        }

        private string GetLoginPassword()
        {
            return txtLoginPassword.Visibility == Visibility.Visible
                ? txtLoginPassword.Password
                : txtVisibleLoginPassword.Text;
        }

        private void ToggleLoginPasswordVisibility(object sender, RoutedEventArgs e)
        {
            if (txtLoginPassword.Visibility == Visibility.Visible)
            {
                txtVisibleLoginPassword.Text = txtLoginPassword.Password;
                txtVisibleLoginPassword.Visibility = Visibility.Visible;
                txtLoginPassword.Visibility = Visibility.Collapsed;
                btnToggleLoginPassword.Content = "🙈";
            }
            else
            {
                txtLoginPassword.Password = txtVisibleLoginPassword.Text;
                txtLoginPassword.Visibility = Visibility.Visible;
                txtVisibleLoginPassword.Visibility = Visibility.Collapsed;
                btnToggleLoginPassword.Content = "👁️";
            }
        }

        private async Task<User> AuthenticateUserAsync(string username, string password)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new NpgsqlCommand("SELECT * FROM work.authenticate_user(@username, @password)", connection))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("password", password);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                UserId = reader.GetInt32(0),
                                Email = reader.GetString(1),
                                Name = reader.GetString(2),
                                LastName = reader.GetString(3),
                                UserType = reader.GetString(4),
                                Company = reader.GetString(5),
                                Rate = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6)
                            };
                        }
                    }
                }
            }
            return null;
        }

        private void SetError(Control control, string message)
        {
            control.BorderBrush = Brushes.Red;
            control.ToolTip = message;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Registration());
        }

        private void NavigateToMainPage(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new MainPage());
        }

        private void NavigateToVacanciesPage(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new VacanciesPage());
        }

        private void NavigateToContactPage(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new ContactPage());
        }

        private void NavigateToProfilePage(object sender, MouseButtonEventArgs e)
        {
            if (AuthManager.IsAuthenticated)
            {
                NavigationService?.Navigate(new ProfilePage());
            }
            else
            {
                NavigationService?.Navigate(new Authorization());
            }
        }
    }
}