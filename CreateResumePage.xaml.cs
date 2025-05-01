using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Npgsql;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Newtonsoft.Json.Linq;
using System.Windows.Input;

namespace JobNest
{
    public partial class CreateResumePage : Page
    {
        private readonly string _connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";
        private readonly bool _isEditMode;
        private Brush _originalBorderBrush;

        public CreateResumePage(string title = null, string skills = null, string experience = null, string additionalInfo = null)
        {
            InitializeComponent();
            _isEditMode = title != null;
            _originalBorderBrush = TitleTextBox.BorderBrush;

            if (_isEditMode)
            {
                PageTitle.Text = "Редактирование резюме";
                TitleTextBox.Text = title;
                SkillsTextBox.Text = skills;
                ExperienceTextBox.Text = experience;
                AdditionalInfoTextBox.Text = additionalInfo;
                SubmitButton.Content = "Обновить";
            }

            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthManager.IsAuthenticated)
            {
                NavigationService?.Navigate(new Authorization());
            }
        }

        private void SetError(Control control, TextBlock errorBlock, string message)
        {
            control.BorderBrush = Brushes.Red;
            errorBlock.Text = message;
            errorBlock.Visibility = Visibility.Visible;
        }

        private void ClearError(Control control, TextBlock errorBlock)
        {
            control.BorderBrush = _originalBorderBrush;
            errorBlock.Visibility = Visibility.Collapsed;
        }

        private bool ValidateInput()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                SetError(TitleTextBox, TitleError, "Введите название резюме");
                isValid = false;
            }
            else
            {
                ClearError(TitleTextBox, TitleError);
            }

            if (string.IsNullOrWhiteSpace(SkillsTextBox.Text))
            {
                SetError(SkillsTextBox, SkillsError, "Укажите ключевые навыки");
                isValid = false;
            }
            else
            {
                ClearError(SkillsTextBox, SkillsError);
            }

            if (string.IsNullOrWhiteSpace(ExperienceTextBox.Text))
            {
                SetError(ExperienceTextBox, ExperienceError, "Опишите опыт работы");
                isValid = false;
            }
            else
            {
                ClearError(ExperienceTextBox, ExperienceError);
            }

            return isValid;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                object additionalInfoParam = DBNull.Value;
                if (!string.IsNullOrWhiteSpace(AdditionalInfoTextBox.Text))
                {
                    var additionalInfo = new JObject
                    {
                        ["additional_info"] = AdditionalInfoTextBox.Text.Trim()
                    };
                    additionalInfoParam = additionalInfo.ToString();
                }

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    if (_isEditMode)
                    {
                        using (var cmd = new NpgsqlCommand("SELECT work.update_user_resume(@user_id, @title, @skills, @experience, @additional_info::jsonb)", connection))
                        {
                            cmd.Parameters.AddWithValue("@user_id", AuthManager.CurrentUser.UserId);
                            cmd.Parameters.AddWithValue("@title", TitleTextBox.Text.Trim());
                            cmd.Parameters.AddWithValue("@skills", SkillsTextBox.Text.Trim());
                            cmd.Parameters.AddWithValue("@experience", ExperienceTextBox.Text.Trim());
                            cmd.Parameters.AddWithValue("@additional_info", additionalInfoParam);

                            bool success = (bool)await cmd.ExecuteScalarAsync();
                            if (success)
                            {
                                MessageBox.Show("Резюме успешно обновлено!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                NavigationService?.Navigate(new ProfilePage());
                            }
                            else
                            {
                                MessageBox.Show("Не удалось обновить резюме", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        using (var cmd = new NpgsqlCommand("SELECT work.create_resume(@title, @skills, @experience, @user_id, @additional_info::jsonb)", connection))
                        {
                            cmd.Parameters.AddWithValue("@title", TitleTextBox.Text.Trim());
                            cmd.Parameters.AddWithValue("@skills", SkillsTextBox.Text.Trim());
                            cmd.Parameters.AddWithValue("@experience", ExperienceTextBox.Text.Trim());
                            cmd.Parameters.AddWithValue("@user_id", AuthManager.CurrentUser.UserId);
                            cmd.Parameters.AddWithValue("@additional_info", additionalInfoParam);

                            int resumeId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            MessageBox.Show($"Резюме успешно создано! ID: {resumeId}", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            NavigationService?.Navigate(new ProfilePage());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfilePage());
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