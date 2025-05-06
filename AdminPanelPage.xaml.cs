using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Data;
using System.Windows.Navigation;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JobNest
{
    public partial class AdminPanelPage : Page
    {
        private readonly string _connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";
        private int _selectedUserId = -1;
        private int _selectedVacancyId = -1;
        private int _selectedResumeId = -1;
        private int _selectedRegionId = -1;

        public AdminPanelPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthManager.IsAuthenticated || AuthManager.CurrentUser.UserType != "admin")
            {
                NavigationService?.Navigate(new MainPage());
                return;
            }

            await LoadUsersAsync();
            await LoadVacanciesAsync();
            await LoadResumesAsync();
            await LoadRegionsAsync();
        }

        private async Task LoadUsersAsync(string searchTerm = null)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    NpgsqlCommand cmd;
                    if (string.IsNullOrEmpty(searchTerm))
                    {
                        cmd = new NpgsqlCommand("SELECT * FROM work.get_all_users()", connection);
                    }
                    else
                    {
                        cmd = new NpgsqlCommand("SELECT * FROM work.search_users(@search_term)", connection);
                        cmd.Parameters.AddWithValue("search_term", searchTerm);
                    }

                    var dataTable = new DataTable();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        UsersGrid.ItemsSource = dataTable.DefaultView;
                        UserSearchTextBox.Text = searchTerm ?? "";
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadVacanciesAsync(string searchTerm = null)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    NpgsqlCommand cmd;
                    if (string.IsNullOrEmpty(searchTerm))
                    {
                        cmd = new NpgsqlCommand("SELECT * FROM work.get_all_vacancies()", connection);
                    }
                    else
                    {
                        cmd = new NpgsqlCommand("SELECT * FROM work.search_vacancies_adm(@search_term)", connection);
                        cmd.Parameters.AddWithValue("search_term", searchTerm);
                    }

                    var dataTable = new DataTable();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        VacanciesGrid.ItemsSource = dataTable.DefaultView;
                        VacancySearchTextBox.Text = searchTerm ?? "";
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки вакансий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadResumesAsync(string searchTerm = null)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    NpgsqlCommand cmd;
                    if (string.IsNullOrEmpty(searchTerm))
                    {
                        cmd = new NpgsqlCommand("SELECT * FROM work.get_all_resumes()", connection);
                    }
                    else
                    {
                        cmd = new NpgsqlCommand("SELECT * FROM work.search_resumes(@search_term)", connection);
                        cmd.Parameters.AddWithValue("search_term", searchTerm);
                    }

                    var dataTable = new DataTable();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ResumesGrid.ItemsSource = dataTable.DefaultView;
                        ResumeSearchTextBox.Text = searchTerm ?? "";
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки резюме: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersGrid.SelectedItem is DataRowView row)
            {
                _selectedUserId = Convert.ToInt32(row["userid"]);
            }
        }
        private void RegionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RegionsGrid.SelectedItem is DataRowView row)
            {
                _selectedRegionId = Convert.ToInt32(row["regionid"]);
            }
        }
        private void VacanciesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VacanciesGrid.SelectedItem is DataRowView row)
            {
                _selectedVacancyId = Convert.ToInt32(row["vacancyid"]);
            }
        }

        private void ResumesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResumesGrid.SelectedItem is DataRowView row)
            {
                _selectedResumeId = Convert.ToInt32(row["resumeid"]);
            }
        }

        private async void SearchUserButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync(UserSearchTextBox.Text);
        }

        private async void SearchVacancyButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadVacanciesAsync(VacancySearchTextBox.Text);
        }

        private async void SearchResumeButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadResumesAsync(ResumeSearchTextBox.Text);
        }

        private async void RefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }

        private async void RefreshVacancies_Click(object sender, RoutedEventArgs e)
        {
            await LoadVacanciesAsync();
        }

        private async void RefreshResumes_Click(object sender, RoutedEventArgs e)
        {
            await LoadResumesAsync();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UserEditPage());
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId <= 0)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NavigationService?.Navigate(new UserEditPage(_selectedUserId));
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId <= 0)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        using (var cmd = new NpgsqlCommand("SELECT work.delete_user(@user_id)", connection))
                        {
                            cmd.Parameters.AddWithValue("user_id", _selectedUserId);
                            bool success = (bool)await cmd.ExecuteScalarAsync();

                            if (success)
                            {
                                MessageBox.Show("Пользователь успешно удален", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                await LoadUsersAsync();
                            }
                            else
                            {
                                MessageBox.Show("Пользователь не найден", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteVacancy_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVacancyId <= 0)
            {
                MessageBox.Show("Пожалуйста, выберите вакансию для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту вакансию?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        using (var cmd = new NpgsqlCommand("SELECT work.delete_vacancy(@vacancy_id)", connection))
                        {
                            cmd.Parameters.AddWithValue("vacancy_id", _selectedVacancyId);
                            bool success = (bool)await cmd.ExecuteScalarAsync();

                            if (success)
                            {
                                MessageBox.Show("Вакансия успешно удалена", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                await LoadVacanciesAsync();
                            }
                            else
                            {
                                MessageBox.Show("Вакансия не найдена", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления вакансии: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteResume_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedResumeId <= 0)
            {
                MessageBox.Show("Пожалуйста, выберите резюме для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить это резюме?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        using (var cmd = new NpgsqlCommand("SELECT work.delete_resume(@resume_id)", connection))
                        {
                            cmd.Parameters.AddWithValue("resume_id", _selectedResumeId);
                            bool success = (bool)await cmd.ExecuteScalarAsync();

                            if (success)
                            {
                                MessageBox.Show("Резюме успешно удалено", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                await LoadResumesAsync();
                            }
                            else
                            {
                                MessageBox.Show("Резюме не найдено", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления резюме: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task LoadRegionsAsync(string searchTerm = "")
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.search_regions(@search_term)", connection))
                    {
                        cmd.Parameters.AddWithValue("search_term", string.IsNullOrEmpty(searchTerm) ? "" : searchTerm);

                        var dataTable = new DataTable();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }

                        Dispatcher.Invoke(() =>
                        {
                            RegionsGrid.ItemsSource = dataTable.DefaultView;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки регионов: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchRegionButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadRegionsAsync(RegionSearchTextBox.Text);
        }

        private async void RefreshRegions_Click(object sender, RoutedEventArgs e)
        {
            RegionSearchTextBox.Text = "";
            await LoadRegionsAsync();
        }

        private async void AddRegion_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Добавление региона", "Введите название региона:");
            if (dialog.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(dialog.Answer))
                {
                    try
                    {
                        using (var connection = new NpgsqlConnection(_connectionString))
                        {
                            await connection.OpenAsync();

                            using (var cmd = new NpgsqlCommand("SELECT work.add_region(@region_name)", connection))
                            {
                                cmd.Parameters.AddWithValue("region_name", dialog.Answer);

                                int newRegionId = (int)await cmd.ExecuteScalarAsync();
                                if (newRegionId > 0)
                                {
                                    MessageBox.Show($"Регион успешно добавлен с ID: {newRegionId}", "Успех",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                    await LoadRegionsAsync();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении региона: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void DeleteRegion_Click(object sender, RoutedEventArgs e)
        {
            if (RegionsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите регион для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранный регион?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DataRowView row = (DataRowView)RegionsGrid.SelectedItem;
                    int regionId = Convert.ToInt32(row["regionid"]);

                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        using (var cmd = new NpgsqlCommand("SELECT work.delete_region(@region_id)", connection))
                        {
                            cmd.Parameters.AddWithValue("region_id", regionId);

                            bool success = (bool)await cmd.ExecuteScalarAsync();
                            if (success)
                            {
                                MessageBox.Show("Регион успешно удален", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                await LoadRegionsAsync();
                            }
                            else
                            {
                                MessageBox.Show("Не удалось удалить регион. Возможно, он используется в вакансиях.", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении региона: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
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