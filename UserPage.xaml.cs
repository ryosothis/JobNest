using System;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JobNest
{
    public partial class UserPage : Page
    {
        private readonly string _connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";
        private readonly int _userId;

        public UserPage(int userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUserData();
        }

        private async Task LoadUserData()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_public_user_profile(@user_id)", connection))
                    {
                        cmd.Parameters.AddWithValue("@user_id", _userId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                UserNameText.Text = reader["username"].ToString();
                                FirstNameText.Text = reader["first_name"].ToString();
                                LastNameText.Text = reader["last_name"].ToString();

                                string userType = reader["user_type"].ToString();
                                UserTypeText.Text = userType == "user" ? "Соискатель" :
                                                  userType == "employer" ? "Работодатель" :
                                                  userType == "admin" ? "Администратор" :
                                                  userType;

                                CompanyText.Text = reader["company"].ToString();
                                PhoneText.Text = reader["contact_number"].ToString();

                                if (reader["rating_value"] != DBNull.Value)
                                {
                                    var rate = Convert.ToDecimal(reader["rating_value"]);
                                    UserRatingText.Text = $"{rate:0.0}";
                                }
                            }

                            else
                            {
                                MessageBox.Show("Пользователь не найден", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                                NavigationService?.GoBack();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки профиля пользователя: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService?.GoBack();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void NavigateToMainPage(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new MainPage());
        }

        private void NavigateToContactPage(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new ContactPage());
        }

        private void NavigateToVacanciesPage(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new VacanciesPage());
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