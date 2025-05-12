using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NorthvilleUI.Forms
{
    public partial class StudentForm : Window
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        private bool isEditMode;
        private Student studentToEdit;

        public StudentForm()
        {
            InitializeComponent();
            LoadDropdowns();
            isEditMode = false;
        }

        public StudentForm(Student student)
        {
            InitializeComponent();
            LoadDropdowns();

            if (student != null)
            {
                isEditMode = true;
                studentToEdit = student;

                txtStudentId.Text = student.student_id;
                txtStudentId.IsReadOnly = true;

                txtStudentName.Text = student.student_name;
                txtStudentEmail.Text = student.student_email;
                txtStudentPhone.Text = student.student_phone;
                cbCourse.SelectedValue = student.course_id;
                cbStudentStatus.SelectedItem = student.student_status;

                lblFormTitle.Text = "Edit Student";
            }
        }
        private void LoadDropdowns()
        {
            cbCourse.ItemsSource = db.Courses.ToList();
            cbCourse.DisplayMemberPath = "course_name";
            cbCourse.SelectedValuePath = "course_id";

            cbStudentStatus.ItemsSource = new List<string> { "Active", "Inactive" };
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSaveStudent_Click(object sender, RoutedEventArgs e)
        {
            string id = txtStudentId.Text.Trim();
            string name = txtStudentName.Text.Trim();
            string email = txtStudentEmail.Text.Trim();
            string phone = txtStudentPhone.Text.Trim();
            string courseId = cbCourse.SelectedValue?.ToString();
            string status = cbStudentStatus.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(courseId) || string.IsNullOrWhiteSpace(status))
            {
                            MessageBox.Show(
                "Please complete all required fields: Student ID, Name, Course, and Status.",
                "Incomplete Form",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

                return;
            }

            if (isEditMode)
            {
                var existing = db.Students.FirstOrDefault(s => s.student_id == id);
                if (existing != null)
                {
                    existing.student_name = name;
                    existing.student_email = email;
                    existing.student_phone = phone;
                    existing.course_id = courseId;
                    existing.student_status = status;
                }
            }
            else
            {
                var newStudent = new Student
                {
                    student_id = id,
                    student_name = name,
                    student_email = email,
                    student_phone = phone,
                    course_id = courseId,
                    student_status = status
                };

                db.Students.InsertOnSubmit(newStudent);
            }

            try
            {
                db.SubmitChanges();
                            MessageBox.Show(
                 "The student has been saved successfully.",
                 "Save Successful",
                 MessageBoxButton.OK,
                 MessageBoxImage.Information);

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
     "An error occurred while saving the student record.\n\n" + ex.Message,
     "Save Error",
     MessageBoxButton.OK,
     MessageBoxImage.Error);

            }
        }
    }
}
