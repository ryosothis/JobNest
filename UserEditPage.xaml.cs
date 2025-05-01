using System;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JobNest
{
    public partial class UserEditPage : Page
    {
        private readonly string _connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";
        private Brush _originalBorderBrush;
        private int _userId = -1;

        public UserEditPage()
        {
            InitializeComponent();
            Title = "Добавление пользователя";
            Loaded += Page_Loaded;
        }

        public UserEditPage(int userId) : this()
        {
            _userId = userId;
            Title = "Редактирование пользователя";
            txtPassword.Visibility = Visibility.Collapsed;
            txtConfirmPassword.Visibility = Visibility.Collapsed;
            lblPasswordError.Visibility = Visibility.Collapsed;
            lblConfirmPasswordError.Visibility = Visibility.Collapsed;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AuthManager.IsAuthenticated || AuthManager.CurrentUser.UserType != "admin")
            {
                NavigationService?.Navigate(new MainPage());
                return;
            }

            _originalBorderBrush = txtEmail.BorderBrush;

            if (_userId > 0)
            {
                await LoadUserDataAsync(_userId);
            }
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

                                    string userType = reader["usertype"].ToString();
                                    foreach (ComboBoxItem item in cmbUserType.Items)
                                    {
                                        if (item.Content.ToString() == GetUserType(userType))
                                        {
                                            cmbUserType.SelectedItem = item;
                                            break;
                                        }
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных пользователя: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetUserType(string userType)
        {
            switch (userType.ToLower())
            {
                case "user": return "Соискатель";
                case "employer": return "Работодатель";
                case "admin": return "Администратор";
                default: return userType;
            }
        }

        private string GetUserTypeValue(string displayName)
        {
            switch (displayName)
            {
                case "Соискатель": return "user";
                case "Работодатель": return "employer";
                case "Администратор": return "admin";
                default: return displayName.ToLower();
            }
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

        private void SetError(Control control, TextBlock errorLabel, string message)
        {
            control.BorderBrush = Brushes.Red;
            errorLabel.Text = message;
            errorLabel.Visibility = Visibility.Visible;
        }

        private void ClearError(Control control, TextBlock errorLabel)
        {
            control.BorderBrush = _originalBorderBrush;
            errorLabel.Visibility = Visibility.Collapsed;
        }

        private bool ValidateInput()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                SetError(txtUsername, lblUsernameError, "Логин обязателен для заполнения");
                isValid = false;
            }
            else
            {
                ClearError(txtUsername, lblUsernameError);
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                SetError(txtEmail, lblEmailError, "Email обязателен для заполнения");
                isValid = false;
            }
            else if (!IsValidEmail(txtEmail.Text))
            {
                SetError(txtEmail, lblEmailError, "Некорректный формат email");
                isValid = false;
            }
            else
            {
                ClearError(txtEmail, lblEmailError);
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                SetError(txtName, lblNameError, "Имя обязательно для заполнения");
                isValid = false;
            }
            else
            {
                ClearError(txtName, lblNameError);
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                SetError(txtLastName, lblLastNameError, "Фамилия обязательна для заполнения");
                isValid = false;
            }
            else
            {
                ClearError(txtLastName, lblLastNameError);
            }

            if (_userId <= 0)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    SetError(txtPassword, lblPasswordError, "Пароль обязателен для заполнения");
                    isValid = false;
                }
                else if (txtPassword.Password.Length < 6)
                {
                    SetError(txtPassword, lblPasswordError, "Пароль должен содержать минимум 6 символов");
                    isValid = false;
                }
                else
                {
                    ClearError(txtPassword, lblPasswordError);
                }

                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    SetError(txtConfirmPassword, lblConfirmPasswordError, "Пароли не совпадают");
                    isValid = false;
                }
                else
                {
                    ClearError(txtConfirmPassword, lblConfirmPasswordError);
                }
            }

            if (!string.IsNullOrEmpty(txtPhone.Text) && !IsValidPhone(txtPhone.Text))
            {
                SetError(txtPhone, lblPhoneError, "Некорректный формат телефона");
                isValid = false;
            }
            else
            {
                ClearError(txtPhone, lblPhoneError);
            }

            return isValid;
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

                    string userType = cmbUserType.SelectedItem != null
                        ? GetUserTypeValue(((ComboBoxItem)cmbUserType.SelectedItem).Content.ToString())
                        : "user";

                    if (_userId > 0)
                    {
                        using (var cmd = new NpgsqlCommand("SELECT work.update_user(@user_id, @email::work.chemail, @name::work.chn, @lastname::work.chn, @username, @contactnumber, @company, @usertype)", connection))
                        {
                            cmd.Parameters.AddWithValue("user_id", _userId);
                            cmd.Parameters.AddWithValue("email", txtEmail.Text);
                            cmd.Parameters.AddWithValue("name", txtName.Text);
                            cmd.Parameters.AddWithValue("lastname", txtLastName.Text);
                            cmd.Parameters.AddWithValue("username", txtUsername.Text);
                            cmd.Parameters.AddWithValue("contactnumber",
                                string.IsNullOrWhiteSpace(txtPhone.Text) ? DBNull.Value : (object)txtPhone.Text);
                            cmd.Parameters.AddWithValue("company", txtCompany.Text);
                            cmd.Parameters.AddWithValue("usertype", userType);

                            bool success = (bool)await cmd.ExecuteScalarAsync();
                            if (success)
                            {
                                MessageBox.Show("Пользователь успешно обновлен", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                NavigationService?.Navigate(new AdminPanelPage());
                            }
                            else
                            {
                                MessageBox.Show("Не удалось обновить пользователя", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        using (var cmd = new NpgsqlCommand("SELECT work.create_user(@email::work.chemail, @name::work.chn, @lastname::work.chn, @username, @password, @contactnumber, @company, @usertype)", connection))
                        {
                            cmd.Parameters.AddWithValue("email", txtEmail.Text);
                            cmd.Parameters.AddWithValue("name", txtName.Text);
                            cmd.Parameters.AddWithValue("lastname", txtLastName.Text);
                            cmd.Parameters.AddWithValue("username", txtUsername.Text);
                            cmd.Parameters.AddWithValue("password", txtPassword.Password);
                            cmd.Parameters.AddWithValue("contactnumber",
                                string.IsNullOrWhiteSpace(txtPhone.Text) ? DBNull.Value : (object)txtPhone.Text);
                            cmd.Parameters.AddWithValue("company", txtCompany.Text);
                            cmd.Parameters.AddWithValue("usertype", userType);

                            int newUserId = (int)await cmd.ExecuteScalarAsync();
                            if (newUserId > 0)
                            {
                                MessageBox.Show($"Пользователь успешно создан с ID: {newUserId}", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                NavigationService?.Navigate(new AdminPanelPage());
                            }
                            else
                            {
                                MessageBox.Show("Не удалось создать пользователя", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (PostgresException ex)
            {
                string errorMessage = ex.Message.Contains("email")
                    ? "Email уже используется другим пользователем"
                    : ex.Message.Contains("username")
                        ? "Логин уже занят"
                        : $"Ошибка базы данных: {ex.Message}";

                if (ex.Message.Contains("email"))
                {
                    SetError(txtEmail, lblEmailError, errorMessage);
                }
                else if (ex.Message.Contains("username"))
                {
                    SetError(txtUsername, lblUsernameError, errorMessage);
                }
                else
                {
                    MessageBox.Show(errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении пользователя: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminPanelPage());
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