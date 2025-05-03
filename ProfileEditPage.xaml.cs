using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Npgsql;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows.Input;

namespace JobNest
{
    public partial class ProfileEditPage : Page
    {
        private readonly string _connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";
        private Brush _originalBorderBrush;

        public ProfileEditPage()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthManager.IsAuthenticated)
            {
                NavigationService?.Navigate(new Authorization());
                return;
            }

            _originalBorderBrush = txtEmail.BorderBrush;
            await LoadUserDataAsync(AuthManager.CurrentUser.UserId);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return true;

            if (phone[0] == '+')
            {
                if (phone.Length < 12) return false;
                phone = phone.Substring(1);
            }
            else if (phone[0] == '8' && phone.Length == 11)
            {
                phone = phone.Substring(1);
            }

            foreach (char c in phone)
            {
                if (!char.IsDigit(c)) return false;
            }

            return phone.Length >= 10 && phone.Length <= 15;
        }

        private void SetError(Control control, string message)
        {
            control.BorderBrush = Brushes.Red;
            control.ToolTip = message;
        }

        private void ClearError(Control control)
        {
            control.BorderBrush = _originalBorderBrush;
            control.ToolTip = null;
        }

        private bool ValidateInput()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                SetError(txtUsername, "Логин обязателен для заполнения");
                isValid = false;
            }
            else
            {
                ClearError(txtUsername);
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                SetError(txtEmail, "Email обязателен для заполнения");
                lblEmailError.Text = "Email обязателен для заполнения";
                lblEmailError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!IsValidEmail(txtEmail.Text))
            {
                SetError(txtEmail, "Некорректный формат email");
                lblEmailError.Text = "Некорректный формат email";
                lblEmailError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                ClearError(txtEmail);
                lblEmailError.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                SetError(txtName, "Имя обязательно для заполнения");
                isValid = false;
            }
            else
            {
                ClearError(txtName);
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                SetError(txtLastName, "Фамилия обязательна для заполнения");
                isValid = false;
            }
            else
            {
                ClearError(txtLastName);
            }

            if (!string.IsNullOrEmpty(txtPhone.Text) && !IsValidPhone(txtPhone.Text))
            {
                SetError(txtPhone, "Некорректный формат телефона");
                lblPhoneError.Text = "Используйте +79991234567 или 89991234567";
                lblPhoneError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                ClearError(txtPhone);
                lblPhoneError.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrWhiteSpace(txtName.Text) && ContainsNumbers(txtName.Text))
            {
                SetError(txtName, "Имя не должно содержать цифры");
                isValid = false;
            }

            if (!string.IsNullOrWhiteSpace(txtLastName.Text) && ContainsNumbers(txtLastName.Text))
            {
                SetError(txtLastName, "Фамилия не должна содержать цифры");
                isValid = false;
            }

            return isValid;
        }

        private async Task LoadUserDataAsync(int userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand("SELECT * FROM work.get_user_profile(@user_id)", connection))
                    {
                        cmd.Parameters.AddWithValue("user_id", userId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    txtUsername.Text = reader["username"].ToString();
                                    txtEmail.Text = reader["email"].ToString();
                                    txtName.Text = reader["name"].ToString();
                                    txtLastName.Text = reader["lastname"].ToString();
                                    txtPhone.Text = reader["contactnumber"] != DBNull.Value
                                        ? reader["contactnumber"].ToString()
                                        : string.Empty;
                                    txtCompany.Text = reader["company"].ToString();
                                });
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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand("SELECT work.update_user_profile(@user_id, @email::work.chemail, @name::work.chn, @lastname::work.chn, @username, @contactnumber, @company)", connection))
                    {
                        cmd.Parameters.AddWithValue("user_id", AuthManager.CurrentUser.UserId);
                        cmd.Parameters.AddWithValue("email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("name", txtName.Text);
                        cmd.Parameters.AddWithValue("lastname", txtLastName.Text);
                        cmd.Parameters.AddWithValue("username", txtUsername.Text);
                        cmd.Parameters.AddWithValue("contactnumber",
                            string.IsNullOrWhiteSpace(txtPhone.Text) ? DBNull.Value : (object)txtPhone.Text);
                        cmd.Parameters.AddWithValue("company", txtCompany.Text);

                        bool success = (bool)await cmd.ExecuteScalarAsync();
                        if (success)
                        {
                            MessageBox.Show("Профиль успешно обновлен", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            NavigationService?.Navigate(new ProfilePage());
                        }
                    }
                }
            }
            catch (PostgresException ex)
            {
                string errorMessage = ex.Message.Contains("email")
                    ? "Email уже используется другим пользователем"
                    : $"Ошибка базы данных: {ex.Message}";

                if (ex.Message.Contains("email"))
                {
                    SetError(txtEmail, errorMessage);
                    lblEmailError.Text = errorMessage;
                    lblEmailError.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show(errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении профиля: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ContainsNumbers(string text)
        {
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                    return true;
            }
            return false;
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
