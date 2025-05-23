﻿using System;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;

namespace JobNest
{
    public partial class ResponsesPage : Page
    {
        private readonly string _connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";

        public ResponsesPage()
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

            await LoadResponses();
        }

        private async Task LoadResponses()
        {
            try
            {
                ResponsesContainer.Children.Clear();
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_user_responses(@user_id)", connection))
                    {
                        cmd.Parameters.AddWithValue("@user_id", AuthManager.CurrentUser.UserId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            bool hasResponses = false;

                            while (await reader.ReadAsync())
                            {
                                hasResponses = true;
                                AddResponseCard(
                                    reader.GetInt32(0), 
                                    reader.GetInt32(1),
                                    reader.GetString(2),
                                    reader.GetInt32(3),
                                    reader.GetString(4),
                                    reader.GetString(5),
                                    reader.GetDateTime(6),
                                    reader.GetString(7)
                                );
                            }

                            NoResponsesText.Visibility = hasResponses ? Visibility.Collapsed : Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки откликов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddResponseCard(int responseId, int vacancyId, string vacancyTitle,
                                   int employerId, string employerName, string employerLastname,
                                   DateTime responseDate, string comment)
        {
            var card = new Border
            {
                Style = (Style)FindResource("ResponseCard")
            };

            var stackPanel = new StackPanel();

            var vacancyTitleBlock = new TextBlock
            {
                Text = vacancyTitle.ToUpper(),
                Style = (Style)FindResource("VacancyTitle")
            };
            stackPanel.Children.Add(vacancyTitleBlock);

            var employerNameBlock = new TextBlock
            {
                Text = $"{employerName} {employerLastname}",
                Style = (Style)FindResource("EmployerName"),
                Tag = employerId
            };
            employerNameBlock.MouseLeftButtonDown += EmployerName_Click;
            stackPanel.Children.Add(employerNameBlock);

            var dateBlock = new TextBlock
            {
                Text = $"Отправлено: {responseDate:dd.MM.yyyy HH:mm}",
                Style = (Style)FindResource("ResponseDate")
            };
            stackPanel.Children.Add(dateBlock);

            if (!string.IsNullOrWhiteSpace(comment))
            {
                var commentBlock = new TextBlock
                {
                    Text = $"Комментарий: {comment}",
                    Style = (Style)FindResource("MessageText")
                };
                stackPanel.Children.Add(commentBlock);
            }

            card.Child = stackPanel;
            ResponsesContainer.Children.Add(card);
        }

        private void EmployerName_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Tag is int employerId)
            {
                NavigationService?.Navigate(new UserPage(employerId));
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