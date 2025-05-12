using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NorthvilleUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);

        public MainWindow()
        {
            InitializeComponent();
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb == tbUsername && tb.Text == "Enter your username")
                tb.Text = "";
            else if (tb == tbPassword && tb.Text == "Enter your password")
                tb.Text = "";
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb == tbUsername && string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = "Enter your username";
            else if (tb == tbPassword && string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = "Enter your password";
        }


        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text.Trim();
            string password = tbPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = db.Users
                         .FirstOrDefault(u => u.username == username && u.password == password);

            if (user == null)
            {
                MessageBox.Show("Invalid credentials or inactive account.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Redirect based on role
            Window nextWindow = null;
            switch (user.user_role.ToLower())
            {
                case "student":
                    nextWindow = new StudentDashboard(user.username);
                    break;
                case "clerical assistant":
                    nextWindow = new LibraryAdminAssistantDashboard(username, user.user_role);
                    break;
                case "librarian":
                    nextWindow = new LibraryAdminAssistantDashboard(username, user.user_role);
                    break;
                case "admin":
                    nextWindow = new LibraryAdminAssistantDashboard(username, user.user_role);
                    break;
                default:
                    MessageBox.Show("Unrecognized user role.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            nextWindow.Show();
            this.Close();
        }
    }
}
