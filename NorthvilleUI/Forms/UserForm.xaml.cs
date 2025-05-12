using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace NorthvilleUI.Forms
{
    public partial class UserForm : Window
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        private bool isEditMode;
        private User userToEdit;

        public UserForm()
        {
            InitializeComponent();
            LoadUserRoles();
            isEditMode = false;
        }

        public UserForm(User user)
        {
            InitializeComponent();
            LoadUserRoles();

            if (user != null)
            {
                isEditMode = true;
                userToEdit = user;

                txtUserId.Text = user.user_id;
                txtUsername.Text = user.username;
                txtPassword.Text = user.password;
                cbUserRole.SelectedItem = user.user_role;
                txtUserId.IsReadOnly = true;
            }
        }

        private void LoadUserRoles()
        {
            cbUserRole.ItemsSource = new ObservableCollection<string>
        {
            "Admin", "Librarian", "Clerical Assistant", "Student"
        };
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSaveUser_Click(object sender, RoutedEventArgs e)
        {
            string userId = txtUserId.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string role = cbUserRole.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                            MessageBox.Show(
                "Please complete all required fields: User ID, Username, Password, and Role.",
                "Incomplete Form",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

                return;
            }

            if (isEditMode)
            {
                var existingUser = db.Users.FirstOrDefault(u => u.user_id == userId);
                if (existingUser != null)
                {
                    existingUser.username = username;
                    existingUser.password = password;
                    existingUser.user_role = role;
                }
            }
            else
            {
                var newUser = new User
                {
                    user_id = userId,
                    username = username,
                    password = password,
                    user_role = role
                };
                db.Users.InsertOnSubmit(newUser);
            }

            try
            {
                db.SubmitChanges();
                MessageBox.Show(
                "The user has been saved successfully.",
                "Save Successful",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                 "An error occurred while saving the user record.\n\n" + ex.Message,
                 "Save Error",
                 MessageBoxButton.OK,
                 MessageBoxImage.Error);

            }
        }
    }
}
