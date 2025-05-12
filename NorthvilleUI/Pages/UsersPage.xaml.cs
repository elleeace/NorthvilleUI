using NorthvilleUI.Forms;
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

namespace NorthvilleUI.Pages
{
    /// <summary>
    /// Interaction logic for UsersPage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {

        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        string _role;
        public UsersPage(string role)
        {
            InitializeComponent();
            _role = role;
        }

        private void btnAddUser(object sender, RoutedEventArgs e)
        {
            if (_role != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var form = new UserForm();
            if (form.ShowDialog() == true)
            {
                btnViewUsers_Click(null, null); // reload datagrid
            }
        }

        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var selectedItem = dgMainTable.SelectedItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Please select a user to edit.");
                return;
            }

            string selectedUserId = (string)selectedItem.GetType().GetProperty("UserID").GetValue(selectedItem, null);
            var userToEdit = db.Users.FirstOrDefault(u => u.user_id == selectedUserId);

            if (userToEdit != null)
            {
                var form = new UserForm(userToEdit);
                if (form.ShowDialog() == true)
                {
                    btnViewUsers_Click(null, null); 
                }
            }
        }


        private void btnViewUsers_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var users = (from ut in db.Users
                         select new
                         {
                             UserID = ut.user_id,
                             Username = ut.username,
                             Password = ut.password,
                             Role = ut.user_role
                         });

            dgMainTable.ItemsSource = users; 
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var selectedUser = dgMainTable.SelectedItem;

            if (selectedUser == null)
            {
                MessageBox.Show("Please select a user to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var userIdProperty = selectedUser.GetType().GetProperty("UserID");
            string userId = userIdProperty?.GetValue(selectedUser)?.ToString();

            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("Unable to determine selected user's ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete user ID '{userId}'?",
                                         "Confirm Deletion",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var userToDelete = db.Users.FirstOrDefault(u => u.user_id == userId);

                    if (userToDelete != null)
                    {
                        db.Users.DeleteOnSubmit(userToDelete);
                        db.SubmitChanges();
                        MessageBox.Show("User successfully deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                        btnViewUsers_Click(null, null);
                    }
                    else
                    {
                        MessageBox.Show("User not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
