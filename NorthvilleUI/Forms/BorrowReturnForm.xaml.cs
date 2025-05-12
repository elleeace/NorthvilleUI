using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NorthvilleUI
{
    public partial class BorrowReturnForm : Window
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        private bool isStudentLoaded = false;
        private bool isBookLoaded = false;
        private bool isStudentVerified = false;
        private bool isBookVerified = false;
        private bool isReturning = false;
        private Student studentBorrowing;
        private Borrow_Transaction preloadedTransaction;

        public BorrowReturnForm(Student student, Borrow_Transaction transaction)
            : this(student, returning: true)
        {
            preloadedTransaction = transaction;
            PreloadTransactionDetails();
        }


        public BorrowReturnForm(bool returning = false)
        {
            InitializeComponent();
            isReturning = returning;
            UpdateFormMode();
        }

        public BorrowReturnForm(Student student, bool returning = false)
        {
            InitializeComponent();
            isReturning = returning;
            studentBorrowing = student;
            ApplyStudentBorrowingContext(); 
            UpdateFormMode();
        }

        private void PreloadTransactionDetails()
        {
            if (preloadedTransaction == null) return;

            txtStudentId.Text = preloadedTransaction.student_id;
            txtStudentId.IsEnabled = false;
            txtBookId.Text = preloadedTransaction.copy_id;  

            isStudentVerified = true;
            isBookVerified = true;

            LoadDetails_Click(null, null);
        }


        private void ApplyStudentBorrowingContext()
        {
            if (studentBorrowing != null)
            {
                txtStudentId.Text = studentBorrowing.student_id.Trim();
                txtStudentId.IsEnabled = false;
                txtStudentId.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // light gray
                txtStudentId.ToolTip = "This field is locked for authenticated students.";
                btnVerifyStudent.IsEnabled = false;
                btnVerifyStudent.Visibility = Visibility.Collapsed;
                isStudentVerified = true;
            }
        }

        private void UpdateFormMode()
        {
            btnBorrowBook.Visibility = isReturning ? Visibility.Collapsed : Visibility.Visible;
            btnReturnBook.Visibility = isReturning ? Visibility.Visible : Visibility.Collapsed;
            tbBorrowReturn.Text = isReturning ? "Return Form" : "Borrow Form";
            this.Title = isReturning ? "Return Book" : "Borrow Book";
        }


        private void BorrowBook_Click(object sender, RoutedEventArgs e)
        {
            if (!isStudentLoaded || !isBookLoaded)
            {
                MessageBox.Show("Please load both student and book details first.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string studentId = txtStudentId.Text.Trim();
            string bookId = txtBookId.Text.Trim();

            if (string.IsNullOrWhiteSpace(bookId))
            {
                MessageBox.Show($"Book ID {bookId} is empty or invalid. Please check the book details.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int dueDays = 7;

            try
            {
                db.usp_ProcessBookCheckout(studentId, bookId, dueDays);
                MessageBox.Show("Book successfully borrowed via stored procedure.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void VerifyStudent_Click(object sender, RoutedEventArgs e)
        {

            if (studentBorrowing != null)
            {
                MessageBox.Show("You are logged in as a student. This field is locked.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string studentId = txtStudentId.Text.Trim();

            if (string.IsNullOrEmpty(studentId))
            {
                MessageBox.Show(
                    "Please enter a Student ID before proceeding.",
                    "Missing Input",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }


            var student = db.Students.FirstOrDefault(s => s.student_id == studentId);
            if (student == null)
            {
                MessageBox.Show(
                    $"No student found with the ID: {studentId}. Please check and try again.",
                    "Student Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                isStudentVerified = false;
                return;
            }


            MessageBox.Show("Student ID verified.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            isStudentVerified = true;
        }

        private void VerifyBook_Click(object sender, RoutedEventArgs e)
        {

            string bookId = txtBookId.Text.Trim();

            var availableCopy = db.Book_Copies.FirstOrDefault(bc => bc.book_id == bookId && bc.copy_status == "Available");

            if (availableCopy == null)
            {
                MessageBox.Show("No available copy found for this Book ID.");
                isBookVerified = false;
                return;
            }

            MessageBox.Show("Book ID verified with available copy.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            isBookVerified = true;
        }

        private void LoadDetails_Click(object sender, RoutedEventArgs e)
        {
            if (!isStudentVerified || !isBookVerified)
            {
                MessageBox.Show("Please verify both student and book first.", "Verification Required");
                return;
            }

            string studentId = txtStudentId.Text.Trim();
            string bookID = txtBookId.Text.Trim();

            isStudentLoaded = false;
            isBookLoaded = false;

            try
            {
                var student = db.Students.FirstOrDefault(s => s.student_id == studentId);
                var course = db.Courses.FirstOrDefault(c => c.course_id == student.course_id);

                if (student != null)
                {
                    txtStudentName.Text = student.student_name;
                    txtCourse.Text = course?.course_name ?? "N/A";
                    tbContactDetails.Text = student.student_email + " | " + student.student_phone;
                    txtCurrentlyBorrowed.Text = db.Borrow_Transactions
                        .Count(bt => bt.student_id == studentId && bt.return_date == null)
                        .ToString();
                    txtOutstandingFines.Text = db.FineBalances
                        .Where(f => f.student_id == studentId && f.payment_status == "Unpaid")
                        .Sum(f => f.balance_remaining)
                        .ToString();
                    isStudentLoaded = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading student info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
               
                Borrow_Transaction borrowed = null;
                Book_Copy copy = null;

                if (isReturning)
                {
                    borrowed = db.Borrow_Transactions
                        .Where(bt => bt.student_id == studentId &&
                                     bt.return_date == null &&
                                     db.Book_Copies.Any(bc => bc.copy_id == bt.copy_id && bc.book_id == bookID))
                        .OrderByDescending(bt => bt.borrow_date)
                        .FirstOrDefault();

                    if (borrowed != null)
                    {
                        copy = db.Book_Copies.FirstOrDefault(c => c.copy_id == borrowed.copy_id);
                    }
                }
                else
                {
                    
                    copy = db.Book_Copies
                        .FirstOrDefault(c => c.book_id == bookID && c.copy_status == "Available");
                }

                if (copy == null)
                {
                    MessageBox.Show("Book copy not found.");
                    return;
                }

                var book = db.Books.FirstOrDefault(b => b.book_id == copy.book_id);
                var genre = db.Genres.FirstOrDefault(g => g.genre_id == book.genre_id);
                var author = db.Authors.FirstOrDefault(a => a.author_id == book.author_id);

                txtBookTitle.Text = book.book_title;
                txtAuthor.Text = author?.author_name ?? "Unknown";
                txtGenre.Text = genre?.genre_name ?? "Unknown";
                txtCopyNumber.Text = copy.copy_id;
                txtStatus.Text = copy.copy_status;
                txtFirstAvailableCopy.Text = isReturning ? "N/A (Returning)" : copy.copy_id;

                isBookLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                "An error occurred while loading book details.\n\n" + ex.Message,
                "Load Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            }

            if (isStudentLoaded && isBookLoaded)
            {
                MessageBox.Show("Details successfully loaded.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnReturnBook_Click(object sender, RoutedEventArgs e)
        {
            string copyId = txtCopyNumber.Text.Trim();
            var transaction = db.Borrow_Transactions
                .Where(bt => bt.copy_id == copyId && bt.return_date == null)
                .OrderByDescending(bt => bt.borrow_date)
                .FirstOrDefault();

            if (transaction == null)
            {
                MessageBox.Show("No active borrow found for this copy.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                db.usp_ReturnBook(transaction.transaction_id, DateTime.Now);
                MessageBox.Show("Book successfully returned.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}