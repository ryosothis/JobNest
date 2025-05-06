using System.Windows;

namespace JobNest
{
    public partial class InputDialog : Window
    {
        public string Answer { get; private set; }

        public InputDialog(string title, string question)
        {
            InitializeComponent();
            Title = title;
            QuestionTextBlock.Text = question;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Answer = AnswerTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}