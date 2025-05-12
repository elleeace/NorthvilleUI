using NorthvilleUI.Pages;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NorthvilleUI
{
    /// <summary>
    /// Interaction logic for LibraryAdminAssistantDashboard.xaml
    /// </summary>
    public partial class LibraryAdminAssistantDashboard : Window
    {
        string _role;
        string _name;
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);


        public LibraryAdminAssistantDashboard(string name, string role)
        {
            InitializeComponent();
            _name = name;
            _role = role;
            InitializeProgram();

        }
        private void InitializeProgram()
        {
            string welcomeMessage = $"{_role} {_name}'s Dashboard";
            tbPageTitle.Text = welcomeMessage;
            tbUsername.Text = _name;
            tbPortalName.Text = $"{_role} Portal";
            tbRole.Text = _role;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to log out?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow mw = new MainWindow();
                mw.Show();
                this.Close();
            }
        }


        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            frmContent.Navigate(new DashboardPage(_role));
            tbPageTitle.Text = "My Dashboard";
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Admin")
            {
                frmContent.Navigate(new UsersPage(_role));
                tbPageTitle.Text = "Users";
            }
            else
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
            }
        }

        private void btnStudents_Click(object sender, RoutedEventArgs e)
        {
            frmContent.Navigate(new StudentsPage(_role));
            tbPageTitle.Text = "Students";
        }

        private void btnBooks_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Clerical Assistant")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            frmContent.Navigate(new BookPage(_role));
            tbPageTitle.Text = "Books";
        }

        private void btnCourse_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Clerical Assistant")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            frmContent.Navigate(new CoursePage(_role));
            tbPageTitle.Text = "Courses";
        }

        private void btnBorrows_Click(object sender, RoutedEventArgs e)
        {
            frmContent.Navigate(new BorrowPage(_role));
            tbPageTitle.Text = "Borrows";
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            frmContent.Navigate(new ReportsPage(_role));
            tbPageTitle.Text = "Reports";
        }
    }
}
