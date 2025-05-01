using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using Npgsql;

namespace JobNest
{
    public partial class VacanciesPage : Page
    {
        private readonly string connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";
        private readonly List<Vacancy> allVacancies = new List<Vacancy>();

        public VacanciesPage()
        {
            InitializeComponent();
            LoadFilters();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadVacancies();
        }

        private void LoadFilters()
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_regions()", connection))
                    {
                        var regions = new List<Region>();
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            regions.Add(new Region
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                        reader.Close();
                        RegionComboBox.ItemsSource = regions;
                        RegionComboBox.SelectedIndex = -1;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_workschedules()", connection))
                    {
                        var schedules = new List<WorkSchedule>();
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            schedules.Add(new WorkSchedule
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                        reader.Close();
                        ScheduleComboBox.ItemsSource = schedules;
                        ScheduleComboBox.SelectedIndex = -1;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_educations()", connection))
                    {
                        var educations = new List<Education>();
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            educations.Add(new Education
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                        reader.Close();
                        EducationComboBox.ItemsSource = educations;
                        EducationComboBox.SelectedIndex = -1;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_experiences()", connection))
                    {
                        var experiences = new List<Experience>();
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            experiences.Add(new Experience
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                        reader.Close();
                        ExperienceComboBox.ItemsSource = experiences;
                        ExperienceComboBox.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке фильтров: {ex.Message}");
            }
        }

        private void LoadVacancies(string searchTerm = null, int? regionId = null, int? scheduleId = null,
            int? salaryFrom = null, int? salaryTo = null, int? educationId = null, int? experienceId = null)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                using (var cmd = new NpgsqlCommand("SELECT * FROM work.search_vacancies(@search_term, @region_id, @schedule_id, @salary_from, @salary_to, @education_id, @exp_id)", connection))
                {
                    cmd.Parameters.AddWithValue("@search_term", searchTerm ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@region_id", regionId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@schedule_id", scheduleId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@salary_from", salaryFrom ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@salary_to", salaryTo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@education_id", educationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@exp_id", experienceId ?? (object)DBNull.Value);

                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        VacanciesContainer.Children.Clear();
                        bool hasResults = false;

                        while (reader.Read())
                        {
                            hasResults = true;
                            var vacancy = new Vacancy
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
                            AddVacancyCard(vacancy);
                        }

                        NoResultsText.Visibility = hasResults ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке вакансий: {ex.Message}");
            }
        }

        private void AddVacancyCard(Vacancy vacancy)
        {
            var card = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 15),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Effect = this.Resources["ShadowEffect"] as DropShadowEffect,
                Cursor = Cursors.Hand
            };

            card.MouseLeftButtonDown += (sender, e) =>
            {
                NavigationService?.Navigate(new VacancyCardPage(vacancy.Id));
            };

            var stackPanel = new StackPanel();

            var titlePanel = new DockPanel();

            var titleText = new TextBlock
            {
                Text = vacancy.Title.ToUpper(),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkSlateGray
            };
            DockPanel.SetDock(titleText, Dock.Left);
            titlePanel.Children.Add(titleText);

            var employerText = new TextBlock
            {
                Text = vacancy.EmployerCompany,
                FontSize = 14,
                Foreground = Brushes.Gray,
                Margin = new Thickness(10, 3, 0, 0)
            };
            DockPanel.SetDock(employerText, Dock.Right);
            titlePanel.Children.Add(employerText);

            stackPanel.Children.Add(titlePanel);

            var detailsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 0)
            };

            detailsPanel.Children.Add(new TextBlock
            {
                Text = vacancy.Schedule,
                Style = this.Resources["SecondaryText"] as Style
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = " / ",
                Style = this.Resources["SecondaryText"] as Style
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = vacancy.Region,
                Style = this.Resources["SecondaryText"] as Style
            });

            stackPanel.Children.Add(detailsPanel);

            var requirementsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 0)
            };

            requirementsPanel.Children.Add(new TextBlock
            {
                Text = vacancy.Education,
                Style = this.Resources["SecondaryText"] as Style
            });

            requirementsPanel.Children.Add(new TextBlock
            {
                Text = " / ",
                Style = this.Resources["SecondaryText"] as Style
            });

            requirementsPanel.Children.Add(new TextBlock
            {
                Text = vacancy.ExperienceRequirement,
                Style = this.Resources["SecondaryText"] as Style
            });

            stackPanel.Children.Add(requirementsPanel);

            var descriptionText = new TextBlock
            {
                Text = vacancy.Description.Length > 200 ? vacancy.Description.Substring(0, 200) + "..." : vacancy.Description,
                Margin = new Thickness(0, 10, 0, 0),
                Foreground = Brushes.DimGray,
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(descriptionText);

            var bottomPanel = new DockPanel
            {
                Margin = new Thickness(0, 15, 0, 0)
            };

            var salaryText = new TextBlock
            {
                Text = $"{vacancy.Salary:N0} ₽",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.LightSkyBlue
            };
            DockPanel.SetDock(salaryText, Dock.Left);
            bottomPanel.Children.Add(salaryText);

            var dateText = new TextBlock
            {
                Text = vacancy.CreatedAt.ToString("dd.MM.yyyy"),
                Style = this.Resources["SecondaryText"] as Style,
                Margin = new Thickness(10, 3, 0, 0)
            };
            DockPanel.SetDock(dateText, Dock.Left);
            bottomPanel.Children.Add(dateText);

            var responseButton = new Button
            {
                Content = "Откликнуться",
                Style = this.Resources["PrimaryButton"] as Style,
                HorizontalAlignment = HorizontalAlignment.Right,
                Tag = vacancy.Id
            };
            responseButton.Click += ResponseButton_Click;
            bottomPanel.Children.Add(responseButton);

            stackPanel.Children.Add(bottomPanel);

            card.Child = stackPanel;
            VacanciesContainer.Children.Add(card);
        }

        private void ResponseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int vacancyId)
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
                            cmd.Parameters.AddWithValue("@vacancyId", vacancyId);
                            cmd.Parameters.AddWithValue("@userId", AuthManager.CurrentUser.UserId);
                            cmd.Parameters.AddWithValue("@comment", dialog.CommentText.Trim());

                            connection.Open();
                            int responseId = Convert.ToInt32(cmd.ExecuteScalar());

                            button.Content = "Отклик отправлен";
                            button.IsEnabled = false;
                            button.Background = Brushes.LightGray;

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
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            ClearSearchButton.Visibility = Visibility.Collapsed;
            SearchTextBox.Focus();
            ApplyFilters();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearSearchButton.Visibility = string.IsNullOrEmpty(SearchTextBox.Text)
                ? Visibility.Collapsed
                : Visibility.Visible;

            ApplyFilters();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string searchTerm = string.IsNullOrWhiteSpace(SearchTextBox.Text) ? null : SearchTextBox.Text;
            int? regionId = (RegionComboBox.SelectedItem as Region)?.Id;
            int? scheduleId = (ScheduleComboBox.SelectedItem as WorkSchedule)?.Id;
            int? educationId = (EducationComboBox.SelectedItem as Education)?.Id;
            int? experienceId = (ExperienceComboBox.SelectedItem as Experience)?.Id;

            int? salaryFrom = null;
            if (!string.IsNullOrWhiteSpace(SalaryFromTextBox.Text) && int.TryParse(SalaryFromTextBox.Text, out int from))
                salaryFrom = from;

            int? salaryTo = null;
            if (!string.IsNullOrWhiteSpace(SalaryToTextBox.Text) && int.TryParse(SalaryToTextBox.Text, out int to))
                salaryTo = to;

            LoadVacancies(searchTerm, regionId, scheduleId, salaryFrom, salaryTo, educationId, experienceId);
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            ClearSearchButton.Visibility = Visibility.Collapsed;
            RegionComboBox.SelectedIndex = -1;
            ScheduleComboBox.SelectedIndex = -1;
            EducationComboBox.SelectedIndex = -1;
            ExperienceComboBox.SelectedIndex = -1;
            SalaryFromTextBox.Text = string.Empty;
            SalaryToTextBox.Text = string.Empty;
            ApplyFilters();
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
    }
}