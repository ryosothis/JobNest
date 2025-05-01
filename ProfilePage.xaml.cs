using System;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Newtonsoft.Json.Linq;
using System.Windows.Input;

namespace JobNest
{
    public partial class ProfilePage : Page
    {
        private readonly string _connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";

        public ProfilePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthManager.IsAuthenticated)
            {
                NavigationService?.Navigate(new Authorization());
                return;
            }

            await LoadUserData();
            await LoadResumeData();
            UpdateActionButtons();
        }

        private async Task LoadUserData()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_user_profile(@user_id)", connection))
                    {
                        cmd.Parameters.AddWithValue("@user_id", AuthManager.CurrentUser.UserId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                UserNameText.Text = reader["username"].ToString();
                                UserEmailText.Text = reader["email"].ToString();
                                UserTypeText.Text = reader["usertype"] != DBNull.Value
                                    ? reader["usertype"].ToString() == "user" ? "Соискатель"
                                    : reader["usertype"].ToString() == "employer" ? "Работодатель"
                                    : reader["usertype"].ToString() == "admin" ? "Администратор"
                                    : reader["usertype"].ToString()
                                    : "Не указан";
                                FirstNameText.Text = reader["name"].ToString();
                                LastNameText.Text = reader["lastname"].ToString();
                                PhoneText.Text = reader["contactnumber"] != DBNull.Value
                                    ? reader["contactnumber"].ToString()
                                    : "Не указан";
                                CompanyText.Text = reader["company"] != DBNull.Value
                                    ? reader["company"].ToString()
                                    : "Не указана";

                                if (reader["rate"] != DBNull.Value)
                                {
                                    var rate = Convert.ToDecimal(reader["rate"]);
                                    UserRatingText.Text = $"{rate:0.0}";
                                    RatingPanel.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    RatingPanel.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных профиля: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadResumeData()
        {
            try
            {
                if (AuthManager.CurrentUser.UserType != "user")
                {
                    ResumePanel.Visibility = Visibility.Collapsed;
                    NoResumeText.Visibility = Visibility.Collapsed;
                    EditResumeButton.Visibility = Visibility.Collapsed;
                    CreateResumeButton.Visibility = Visibility.Collapsed;
                    return;
                }

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_user_resume(@user_id)", connection))
                    {
                        cmd.Parameters.AddWithValue("@user_id", AuthManager.CurrentUser.UserId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                bool resumeExists = reader.GetBoolean(0);

                                if (resumeExists)
                                {
                                    ResumePanel.Visibility = Visibility.Visible;
                                    NoResumeText.Visibility = Visibility.Collapsed;
                                    EditResumeButton.Visibility = Visibility.Visible;
                                    CreateResumeButton.Visibility = Visibility.Collapsed;

                                    ResumeTitle.Text = reader["title"].ToString();
                                    ResumeSkills.Text = reader["skills"].ToString();
                                    ResumeExperience.Text = reader["experience"].ToString();

                                    if (reader["additionalinfo"] != DBNull.Value)
                                    {
                                        var additionalInfo = JObject.Parse(reader["additionalinfo"].ToString());
                                        ResumeAdditionalInfo.Text = additionalInfo["additional_info"]?.ToString() ?? "";
                                    }
                                    else
                                    {
                                        ResumeAdditionalInfo.Text = "";
                                    }
                                }
                                else
                                {
                                    ResumePanel.Visibility = Visibility.Collapsed;
                                    NoResumeText.Visibility = Visibility.Visible;
                                    EditResumeButton.Visibility = Visibility.Collapsed;
                                    CreateResumeButton.Visibility = Visibility.Visible;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки резюме: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateActionButtons()
        {
            AdminPanelButton.Visibility = Visibility.Collapsed;
            CreateVacancyButton.Visibility = Visibility.Collapsed;
            CheckResponsesButton.Visibility = Visibility.Collapsed;

            if (AuthManager.CurrentUser.UserType == "admin")
            {
                AdminPanelButton.Visibility = Visibility.Visible;
            }
            else if (AuthManager.CurrentUser.UserType == "employer")
            {
                CreateVacancyButton.Visibility = Visibility.Visible;
                CheckEmpResponsesButton.Visibility = Visibility.Visible;
            }

            else if (AuthManager.CurrentUser.UserType == "user")
            {
                CheckResponsesButton.Visibility = Visibility.Visible;
            }
        }
        private void CheckResponsesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ResponsesPage());
        }

        private void CheckEmpResponsesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ResponsesEmployerPage());
        }

        private void EditResumeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CreateResumePage(
                ResumeTitle.Text,
                ResumeSkills.Text,
                ResumeExperience.Text,
                ResumeAdditionalInfo.Text
            ));
        }

        private void CreateResumeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CreateResumePage());
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfileEditPage());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            AuthManager.Logout();
            NavigationService?.Navigate(new Authorization());
        }

        private void AdminPanelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminPanelPage());
        }

        private void CreateVacancyButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CreateVacancyPage());
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