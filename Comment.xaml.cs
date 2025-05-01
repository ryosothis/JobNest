using System.Windows;

namespace JobNest
{
    public partial class Comment : Window
    {
        public string CommentText => CommentTextBox.Text;

        public Comment()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(CommentTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите комментарий",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return false;
            }

            if (CommentTextBox.Text.Length > 300)
            {
                MessageBox.Show("Комментарий не должен превышать 300 символов",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}