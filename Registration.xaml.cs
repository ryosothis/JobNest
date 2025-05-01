using System;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Net.Mail;
using System.Windows.Input;

namespace JobNest
{
    public partial class Registration : Page
    {
        private readonly string _connectionString = "Host=172.20.7.53;Port=5432;Database=db2991_08;Username=st2991;Password=pwd2991";
        private Brush _originalBorderBrush = Brushes.Gray;

        public Registration()
        {
            InitializeComponent();
            _originalBorderBrush = txtRegisterEmail.BorderBrush;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateRegistrationData())
                    return;

                RegisterButton.IsEnabled = false;
                RegisterButton.Content = "Регистрация...";

                int? userId = await RegisterUserAsync();

                if (userId.HasValue)
                {
                    MessageBox.Show("Регистрация прошла успешно!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService?.Navigate(new Authorization());
                }
            }
            catch (PostgresException ex)
            {
                HandlePgError(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                RegisterButton.IsEnabled = true;
                RegisterButton.Content = "Зарегистрироваться";
            }
        }

        private async Task<int?> RegisterUserAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new NpgsqlCommand("SELECT work.register_user(@email, @name, @lastname, @username, @password, @usertype, @company, @contactnumber)", connection))
                {
                    cmd.Parameters.AddWithValue("email", txtRegisterEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("name", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("lastname", txtLastName.Text.Trim());
                    cmd.Parameters.AddWithValue("username", txtRegisterUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("password", GetPassword());
                    cmd.Parameters.AddWithValue("usertype", "user");
                    cmd.Parameters.AddWithValue("company", "");
                    cmd.Parameters.AddWithValue("contactnumber", DBNull.Value);

                    var userId = await cmd.ExecuteScalarAsync();
                    return userId != null ? Convert.ToInt32(userId) : (int?)null;
                }
            }
        }

        private string GetPassword()
        {
            return txtRegisterPassword.Visibility == Visibility.Visible
                ? txtRegisterPassword.Password
                : txtVisiblePassword1.Text;
        }

        private bool ValidateRegistrationData()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                SetError(txtName, "Введите имя");
                isValid = false;
            }
            else
            {
                ClearError(txtName);
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                SetError(txtLastName, "Введите фамилию");
                isValid = false;
            }
            else
            {
                ClearError(txtLastName);
            }

            if (string.IsNullOrWhiteSpace(txtRegisterUsername.Text))
            {
                SetError(txtRegisterUsername, "Введите имя пользователя");
                isValid = false;
            }
            else if (txtRegisterUsername.Text.Length < 3)
            {
                SetError(txtRegisterUsername, "Имя пользователя должно содержать минимум 3 символа");
                isValid = false;
            }
            else
            {
                ClearError(txtRegisterUsername);
            }

            if (string.IsNullOrWhiteSpace(txtRegisterEmail.Text))
            {
                SetError(txtRegisterEmail, "Введите email");
                isValid = false;
            }
            else if (!IsValidEmail(txtRegisterEmail.Text))
            {
                SetError(txtRegisterEmail, "Неверный формат email");
                isValid = false;
            }
            else
            {
                ClearError(txtRegisterEmail);
            }

            string password = GetPassword();
            if (string.IsNullOrWhiteSpace(password))
            {
                SetError(txtRegisterPassword, "Введите пароль");
                isValid = false;
            }
            else if (password.Length < 8)
            {
                SetError(txtRegisterPassword, "Пароль должен содержать минимум 8 символов");
                isValid = false;
            }
            else
            {
                ClearError(txtRegisterPassword);
            }

            string confirmPassword = txtConfirmPassword.Visibility == Visibility.Visible
                ? txtConfirmPassword.Password
                : txtVisiblePassword2.Text;

            if (password != confirmPassword)
            {
                SetError(txtConfirmPassword, "Пароли не совпадают");
                isValid = false;
            }
            else
            {
                ClearError(txtConfirmPassword);
            }

            return isValid;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void HandlePgError(PostgresException ex)
        {
            string errorMessage = ex.Message.Contains("имя пользователя уже занято")
                ? "Это имя пользователя уже занято"
                : ex.Message.Contains("email уже используется")
                    ? "Этот email уже зарегистрирован"
                    : ex.Message.Contains("неверный формат email")
                        ? "Неверный формат email"
                        : ex.Message;

            MessageBox.Show(errorMessage, "Ошибка регистрации",
                MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag.ToString() == "1")
                {
                    TogglePassword(txtRegisterPassword, txtVisiblePassword1, btnTogglePassword1);
                }
                else if (button.Tag.ToString() == "2")
                {
                    TogglePassword(txtConfirmPassword, txtVisiblePassword2, btnTogglePassword2);
                }
            }
        }

        private void TogglePassword(PasswordBox passwordBox, TextBox textBox, Button toggleButton)
        {
            if (passwordBox.Visibility == Visibility.Visible)
            {
                textBox.Text = passwordBox.Password;
                textBox.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Collapsed;
                toggleButton.Content = "🙈";
            }
            else
            {
                passwordBox.Password = textBox.Text;
                passwordBox.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Collapsed;
                toggleButton.Content = "👁️";
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Authorization());
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