using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Npgsql;

namespace JobNest
{
    public partial class CreateVacancyPage : Page
    {
        private readonly string _connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";
        private Brush _originalBorderBrush;

        public class ComboBoxItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public CreateVacancyPage()
        {
            InitializeComponent();
            _originalBorderBrush = TitleTextBox.BorderBrush;
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthManager.IsAuthenticated)
            {
                NavigationService?.Navigate(new Authorization());
                return;
            }

            if (AuthManager.CurrentUser.UserType != "employer")
            {
                MessageBox.Show("Только работодатели могут создавать вакансии", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService?.Navigate(new MainPage());
                return;
            }

            LoadComboBoxData();
        }

        private async void LoadComboBoxData()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var regions = new List<ComboBoxItem>();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_regions()", connection))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            regions.Add(new ComboBoxItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                        }
                    }
                    RegionComboBox.ItemsSource = regions;
                    RegionComboBox.DisplayMemberPath = "Name";
                    RegionComboBox.SelectedValuePath = "Id";

                    var schedules = new List<ComboBoxItem>();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_workschedules()", connection))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            schedules.Add(new ComboBoxItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                        }
                    }
                    ScheduleComboBox.ItemsSource = schedules;
                    ScheduleComboBox.DisplayMemberPath = "Name";
                    ScheduleComboBox.SelectedValuePath = "Id";

                    var educations = new List<ComboBoxItem>();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_educations()", connection))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            educations.Add(new ComboBoxItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                        }
                    }
                    EducationComboBox.ItemsSource = educations;
                    EducationComboBox.DisplayMemberPath = "Name";
                    EducationComboBox.SelectedValuePath = "Id";

                    var experiences = new List<ComboBoxItem>();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_experiences()", connection))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            experiences.Add(new ComboBoxItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                        }
                    }
                    ExperienceComboBox.ItemsSource = experiences;
                    ExperienceComboBox.DisplayMemberPath = "Name";
                    ExperienceComboBox.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                SetError(TitleTextBox, TitleError, "Введите название вакансии");
                isValid = false;
            }
            else
            {
                ClearError(TitleTextBox, TitleError);
            }

            if (RegionComboBox.SelectedItem == null)
            {
                SetError(RegionComboBox, RegionError, "Выберите регион");
                isValid = false;
            }
            else
            {
                ClearError(RegionComboBox, RegionError);
            }

            if (ScheduleComboBox.SelectedItem == null)
            {
                SetError(ScheduleComboBox, ScheduleError, "Выберите график работы");
                isValid = false;
            }
            else
            {
                ClearError(ScheduleComboBox, ScheduleError);
            }

            if (!int.TryParse(SalaryTextBox.Text, out int salary) || salary <= 0)
            {
                SetError(SalaryTextBox, SalaryError, "Введите корректную зарплату");
                isValid = false;
            }
            else
            {
                ClearError(SalaryTextBox, SalaryError);
            }

            if (EducationComboBox.SelectedItem == null)
            {
                SetError(EducationComboBox, EducationError, "Выберите уровень образования");
                isValid = false;
            }
            else
            {
                ClearError(EducationComboBox, EducationError);
            }

            if (ExperienceComboBox.SelectedItem == null)
            {
                SetError(ExperienceComboBox, ExperienceError, "Выберите требуемый опыт");
                isValid = false;
            }
            else
            {
                ClearError(ExperienceComboBox, ExperienceError);
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                SetError(DescriptionTextBox, DescriptionError, "Введите описание вакансии");
                isValid = false;
            }
            else
            {
                ClearError(DescriptionTextBox, DescriptionError);
            }

            if (string.IsNullOrWhiteSpace(RequirementsTextBox.Text))
            {
                SetError(RequirementsTextBox, RequirementsError, "Введите требования к кандидату");
                isValid = false;
            }
            else
            {
                ClearError(RequirementsTextBox, RequirementsError);
            }

            return isValid;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var region = (ComboBoxItem)RegionComboBox.SelectedItem;
                var schedule = (ComboBoxItem)ScheduleComboBox.SelectedItem;
                var education = (ComboBoxItem)EducationComboBox.SelectedItem;
                var experience = (ComboBoxItem)ExperienceComboBox.SelectedItem;

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand("SELECT work.create_vacancy(@title, @description, @requirements, @region_id, @salary, @schedule_id, @user_id, @education_id, @exp_id)", connection))
                    {
                        cmd.Parameters.AddWithValue("@title", TitleTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@description", DescriptionTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@requirements", RequirementsTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@region_id", region.Id);
                        cmd.Parameters.AddWithValue("@salary", int.Parse(SalaryTextBox.Text));
                        cmd.Parameters.AddWithValue("@schedule_id", schedule.Id);
                        cmd.Parameters.AddWithValue("@user_id", AuthManager.CurrentUser.UserId);
                        cmd.Parameters.AddWithValue("@education_id", education.Id);
                        cmd.Parameters.AddWithValue("@exp_id", experience.Id);

                        int vacancyId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        MessageBox.Show($"Вакансия успешно создана! ID: {vacancyId}", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.Navigate(new ProfilePage());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании вакансии: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfilePage());
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
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