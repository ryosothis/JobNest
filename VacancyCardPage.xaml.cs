using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Npgsql;

namespace JobNest
{
    public partial class VacancyCardPage : Page
    {
        private readonly string connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";
        private Vacancy currentVacancy;

        public VacancyCardPage(int vacancyId)
        {
            InitializeComponent();
            LoadVacancy(vacancyId);
        }

        private void LoadVacancy(int vacancyId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_vacancy_details(@vacancy_id)", connection))
                {
                    cmd.Parameters.AddWithValue("@vacancy_id", vacancyId);
                    connection.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentVacancy = new Vacancy
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Description = reader.GetString(2),
                                Requirements = reader.GetString(3),
                                Salary = reader.GetInt32(4),
                                Region = reader.GetString(5),
                                Schedule = reader.GetString(6),
                                CreatedAt = reader.GetDateTime(7),
                                Education = reader.GetString(8),
                                ExperienceRequirement = reader.GetString(9),
                                EmployerId = reader.GetInt32(10),
                                EmployerFirstName = reader.GetString(11),
                                EmployerLastName = reader.GetString(12),
                                EmployerCompany = reader.GetString(13)
                            };

                            TitleText.Text = currentVacancy.Title.ToUpper();
                            SalaryText.Text = $"{currentVacancy.Salary:N0} ₽";
                            EmployerNameText.Text = $"{currentVacancy.EmployerFirstName} {currentVacancy.EmployerLastName}";
                            EmployerCompanyText.Text = currentVacancy.EmployerCompany;
                            ScheduleText.Text = currentVacancy.Schedule;
                            RegionText.Text = currentVacancy.Region;
                            DateText.Text = currentVacancy.CreatedAt.ToString("dd.MM.yyyy");
                            EducationText.Text = currentVacancy.Education;
                            ExperienceText.Text = currentVacancy.ExperienceRequirement;
                            DescriptionText.Text = currentVacancy.Description;
                            RequirementsText.Text = currentVacancy.Requirements;
                        }
                        else
                        {
                            MessageBox.Show("Вакансия не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            NavigationService.GoBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке вакансии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.GoBack();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ResponseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthManager.IsAuthenticated)
            {
                MessageBox.Show("Для отклика на вакансию необходимо войти в систему",
                              "Требуется авторизация",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            if (AuthManager.CurrentUser.UserType == "employer")
            {
                MessageBox.Show("Работодатели не могут откликаться на вакансии",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                return;
            }

            var dialog = new Comment();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(dialog.CommentText))
                {
                    MessageBox.Show("Комментарий не может быть пустым",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    return;
                }

                if (dialog.CommentText.Length > 1000)
                {
                    MessageBox.Show("Комментарий не должен превышать 1000 символов",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    return;
                }

                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    using (var cmd = new NpgsqlCommand("SELECT work.add_vacancy_response(@vacancyId, @userId, @comment)", connection))
                    {
                        cmd.Parameters.AddWithValue("@vacancyId", currentVacancy.Id);
                        cmd.Parameters.AddWithValue("@userId", AuthManager.CurrentUser.UserId);
                        cmd.Parameters.AddWithValue("@comment", dialog.CommentText.Trim());

                        connection.Open();
                        int responseId = Convert.ToInt32(cmd.ExecuteScalar());

                        ResponseButton.Content = "Отклик отправлен";
                        ResponseButton.IsEnabled = false;
                        ResponseButton.Background = Brushes.LightGray;

                        MessageBox.Show($"Ваш отклик #{responseId} успешно отправлен!",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отправке отклика: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private void EmployerName_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new UserPage(currentVacancy.EmployerId));
        }

        private void UserIcon_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new ProfilePage());
        }

        private void Home_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new MainPage());
        }

        private void Vacancies_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new VacanciesPage());
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