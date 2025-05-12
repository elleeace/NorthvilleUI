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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NorthvilleUI.Pages
{
    /// <summary>
    /// Interaction logic for CoursePage.xaml
    /// </summary>
    public partial class CoursePage : Page
    {

        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        string _role;
        public CoursePage(string role)
        {
            InitializeComponent();
            _role = role;
        }
        private void btnViewCourses_Click(object sender, RoutedEventArgs e)
        {

            if (_role == "Clerical Assistant")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var courseList = from c in db.Courses
                             select new
                             {
                                 CourseID = c.course_id,
                                 CourseName = c.course_name,
                                 Department = c.department
                             };

            dgMainTable.ItemsSource = courseList.ToList();
            tbTableName.Text = "Courses Table";
        }

        private void btnAddCourse_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var form = new CourseForm();
            if (form.ShowDialog() == true)
            {
                btnViewCourses_Click(null, null);
            }
        }

        private void btnEditCourse_Click(object sender, RoutedEventArgs e)
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
    "Please select a course to edit.",
    "No Course Selected",
    MessageBoxButton.OK,
    MessageBoxImage.Information);

                return;
            }

            string selectedCourseId = (string)selectedItem.GetType().GetProperty("CourseID").GetValue(selectedItem, null);
            var courseToEdit = db.Courses.FirstOrDefault(c => c.course_id == selectedCourseId);

            if (courseToEdit != null)
            {
                var form = new CourseForm(courseToEdit);
                if (form.ShowDialog() == true)
                {
                    btnViewCourses_Click(null, null);
                }
            }
        }


        private void btnDeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var selectedItem = dgMainTable.SelectedItem;

            if (selectedItem == null)
            {
                MessageBox.Show("Please select a course to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var courseIdProperty = selectedItem.GetType().GetProperty("CourseID");
            string courseId = courseIdProperty?.GetValue(selectedItem)?.ToString();

            if (string.IsNullOrWhiteSpace(courseId))
            {
                MessageBox.Show("Unable to determine selected course ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete course '{courseId}'?",
                                         "Confirm Deletion",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var courseToDelete = db.Courses.FirstOrDefault(c => c.course_id == courseId);

                    if (courseToDelete != null)
                    {
                        db.Courses.DeleteOnSubmit(courseToDelete);
                        db.SubmitChanges();
                        MessageBox.Show("Course successfully deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                        btnViewCourses_Click(null, null);
                    }
                    else
                    {
                        MessageBox.Show("Course not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the course:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
