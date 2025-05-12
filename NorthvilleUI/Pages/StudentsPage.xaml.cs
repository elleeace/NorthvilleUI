using NorthvilleUI.Forms;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NorthvilleUI.Pages
{
    /// <summary>
    /// Interaction logic for StudentsPage.xaml
    /// </summary>
    public partial class StudentsPage : Page
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        string _role;
        public StudentsPage(string role)
        {
            InitializeComponent();
            _role = role;
        }

        private void btnViewStudents_Click(object sender, RoutedEventArgs e)
        {


            var studentTable = (from st in db.Students
                                select new
                                {
                                    StudentID = st.student_id,
                                    Name = st.student_name,
                                    Email = st.student_email,
                                    ContactNumber = st.student_phone,
                                    Course = st.Course.course_id,
                                    Status = st.student_status
                                });

            dgMainTable.ItemsSource = studentTable;
        }


        private void btnEditStudent_Click(object sender, RoutedEventArgs e)
        {

            if (_role != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var selectedItem = dgMainTable.SelectedItem;

            if (selectedItem == null)
            {
                            MessageBox.Show(
                 "Please select a student to edit.",
                 "No Student Selected",
                 MessageBoxButton.OK,
                 MessageBoxImage.Information);

                return;
            }

            string selectedStudentId = selectedItem.GetType().GetProperty("StudentID").GetValue(selectedItem, null)?.ToString();
            var studentToEdit = db.Students.FirstOrDefault(s => s.student_id == selectedStudentId);

            if (studentToEdit != null)
            {
                var form = new StudentForm(studentToEdit);
                if (form.ShowDialog() == true)
                {
                    btnViewStudents_Click(null, null);
                }
            }
        }

        private void btnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {

            if (_role != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var selectedItem = dgMainTable.SelectedItem;

            if (selectedItem == null)
            {
                MessageBox.Show("Please select a student to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string studentId = selectedItem.GetType().GetProperty("StudentID").GetValue(selectedItem, null)?.ToString();

            if (string.IsNullOrWhiteSpace(studentId))
            {
                MessageBox.Show("Unable to identify the selected student ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to delete student ID '{studentId}'?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    var studentToDelete = db.Students.FirstOrDefault(s => s.student_id == studentId);

                    if (studentToDelete != null)
                    {
                        db.Students.DeleteOnSubmit(studentToDelete);
                        db.SubmitChanges();

                        MessageBox.Show("Student deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        btnViewStudents_Click(null, null);
                    }
                    else
                    {
                        MessageBox.Show("Student not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting student: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Admin" && _role != "Clerical Assistant")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var form = new StudentForm();
            if (form.ShowDialog() == true)
            {
                btnViewStudents_Click(null, null); // refresh
            }
        }
    }
}