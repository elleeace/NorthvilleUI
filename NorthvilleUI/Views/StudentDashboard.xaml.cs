using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NorthvilleUI
{
    public partial class StudentDashboard : Window
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        private string currentStudentId;

        private Student loggedInStudent;
      
        public StudentDashboard(string studentId)
        {
            InitializeComponent();
            currentStudentId = studentId;

            loggedInStudent = db.Students.FirstOrDefault(s => s.student_id == currentStudentId);

            LoadStudentInfo();
            LoadStatistics();
            LoadBorrowHistory(); 
        }


        private void LoadStudentInfo()
        {
            var student = db.Students.FirstOrDefault(s => s.student_id == currentStudentId);
            if (student != null)
            {
                tbStudentId.Text = student.student_id;
                tbStudentName.Text = student.student_name;
                tbCourse.Text = student.Course.course_name;
                tbContact.Text = student.student_email ?? student.student_phone;
            }
        }

        private void LoadStatistics()
        {
            int borrowed = db.Borrow_Transactions.Count(bt => bt.student_id == currentStudentId);
            int active = db.Borrow_Transactions.Count(bt => bt.student_id == currentStudentId && bt.return_date == null);
            int penalties = db.Borrow_Transactions
                            .Where(bt => bt.student_id == currentStudentId && bt.return_date == null && bt.due_date < DateTime.Now)
                            .Count();

            tbBorrowedBooks.Text = borrowed.ToString() + " Borrowed Books";
            tbActiveBorrows.Text = active.ToString() + " Active Borrows";
            tbPenaltyAmount.Text = penalties.ToString() + " Total Penalty Incurred";
        }

        private void LoadBorrowHistory()
        {
            tbTableTitle.Text = "My Borrow History";
            var history = from bt in db.Borrow_Transactions
                          join bc in db.Book_Copies on bt.copy_id equals bc.copy_id
                          join b in db.Books on bc.book_id equals b.book_id
                          where bt.student_id == currentStudentId
                          select new
                          {
                              BookId = bc.book_id,
                              Title = b.book_title,
                              BorrowDate = bt.borrow_date,
                              DueDate = bt.due_date,
                              ReturnDate = bt.return_date,
                              Status = bt.return_date == null ? "Borrowed" : "Returned"
                          };

            dgStudentBorrows.ItemsSource = history.ToList();
        }

        private void LoadActiveBorrows()
        {
            tbTableTitle.Text = "Active Borrows";

            var active = from bt in db.Borrow_Transactions
                         join bc in db.Book_Copies on bt.copy_id equals bc.copy_id
                         join b in db.Books on bc.book_id equals b.book_id
                         where bt.student_id == currentStudentId && bt.return_date == null
                         select new
                         {
                             TransactionId = bt.transaction_id, // <-- Add this
                             BookId = bc.book_id,
                             Title = b.book_title,
                             BorrowDate = bt.borrow_date,
                             DueDate = bt.due_date,
                             Status = "Active"
                         };

            dgStudentBorrows.ItemsSource = active.ToList();
        }

        private void LoadAvailableBooks()
        {
            var books = db.AvailableBooks
                          .Select(b => new
                          {
                              BookID = b.book_id,
                              Title = b.book_title,
                              Author = b.author_name,
                              Genre = b.genre_name,
                              Copies = b.available_copies,
                              CopyIDs = b.copy_ids
                          }).ToList();

            dgStudentBorrows.ItemsSource = books;
            tbTableTitle.Text = "Available Books";
        }

        private void LoadOverdueBooks()
        {
            var overdue = db.OverdueBooks
                            .Where(o => o.student_id == currentStudentId)
                            .Select(o => new
                            {
                                TransactionID = o.transaction_id,
                                Title = o.book_title,
                                CopyID = o.copy_id,
                                DueDate = o.due_date,
                                DaysOverdue = o.days_overdue,
                                CurrentFine = o.current_fine,
                                Status = o.fine_status
                            }).ToList();

            dgStudentBorrows.ItemsSource = overdue;
            tbTableTitle.Text = "Overdue Books";
        }

        private void MyHistory_Click(object sender, RoutedEventArgs e)
        {
            LoadBorrowHistory();
        }

        private void ActiveBorrows_Click(object sender, RoutedEventArgs e)
        {
            LoadActiveBorrows();
        }

        private void Overdue_Click(object sender, RoutedEventArgs e)
        {
            LoadOverdueBooks();
        }

        private void BorrowBook_Click(object sender, RoutedEventArgs e)
        {
            var form = new BorrowReturnForm(loggedInStudent, returning: false);
            form.ShowDialog();
            LoadAvailableBooks();
            LoadStatistics();
        }

        private void ReturnBook_Click(object sender, RoutedEventArgs e)
        {
            LoadActiveBorrows();
            var selectedTransaction = dgStudentBorrows.SelectedItem;
            if (selectedTransaction == null)
            {
                MessageBox.Show("Please select a transaction to return.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string transactionId = selectedTransaction.GetType().GetProperty("TransactionId")?.GetValue(selectedTransaction)?.ToString();

            if (string.IsNullOrEmpty(transactionId))
            {
                MessageBox.Show("Unable to determine transaction ID. Please select from 'Active Borrows'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Confirm returning this book?", "Return Book", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                db.usp_ReturnBook(transactionId, DateTime.Now);
                MessageBox.Show("Book successfully returned.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadBorrowHistory(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error returning book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadStatistics();
        }



        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void btnViewAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentStudentId))
            {
                MessageBox.Show("No student selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var visitList = db.Library_Visits
                    .Where(v => v.student_id == currentStudentId)
                    .OrderByDescending(v => v.visit_date)
                    .ToList();

                var attendanceRecords = visitList
                    .Select((v, index) => new
                    {
                        VisitID = index + 1,
                        VisitDate = v.visit_date.ToString("MMMM dd, yyyy")
                    }).ToList();

                dgStudentBorrows.ItemsSource = attendanceRecords;
                tbTableTitle.Text = "Visit History";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
