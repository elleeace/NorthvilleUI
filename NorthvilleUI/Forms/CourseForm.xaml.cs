using System;
using System.Linq;
using System.Windows;

namespace NorthvilleUI.Forms
{
    public partial class CourseForm : Window
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        private bool isEditMode;
        private Course courseToEdit;

        public CourseForm()
        {
            InitializeComponent();
            isEditMode = false;
        }

        public CourseForm(Course course)
        {
            InitializeComponent();
            isEditMode = true;
            courseToEdit = course;

            if (course != null)
            {
                txtCourseId.Text = course.course_id;
                txtCourseName.Text = course.course_name;
                txtDepartment.Text = course.department;

                txtCourseId.IsReadOnly = true;
                lblFormTitle.Text = "Edit Course";
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSaveCourse_Click(object sender, RoutedEventArgs e)
        {
            string id = txtCourseId.Text.Trim();
            string name = txtCourseName.Text.Trim();
            string dept = txtDepartment.Text.Trim();

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(dept))
            {
                            MessageBox.Show(
                 "Please fill in all required fields: Course ID, Course Name, and Department.",
                 "Missing Input",
                 MessageBoxButton.OK,
                 MessageBoxImage.Warning);

                return;
            }

            if (isEditMode)
            {
                var existing = db.Courses.FirstOrDefault(c => c.course_id == id);
                if (existing != null)
                {
                    existing.course_name = name;
                    existing.department = dept;
                }
            }
            else
            {
                Course newCourse = new Course
                {
                    course_id = id,
                    course_name = name,
                    department = dept
                };
                db.Courses.InsertOnSubmit(newCourse);
            }

            try
            {
                db.SubmitChanges();
                MessageBox.Show(
                  "The course has been saved successfully.",
                  "Save Successful",
                  MessageBoxButton.OK,
                  MessageBoxImage.Information);

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                 "An error occurred while saving the course.\n\n" + ex.Message,
                 "Save Error",
                 MessageBoxButton.OK,
                 MessageBoxImage.Error);

            }
        }
    }
}
