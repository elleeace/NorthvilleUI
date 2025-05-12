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
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        string _role;
        public DashboardPage(string role)
        {
            InitializeComponent();
            LoadDashboardStats();
            _role = role;
        }
   
        private void LoadDashboardStats()
        {
            tbTotalBooks.Text = db.Books.Count().ToString();
            tbTotalAvailableBooks.Text = db.Book_Copies.Count(bc => bc.copy_status == "Available").ToString(); 

            int totalBorrowed = db.Borrow_Transactions.Count(bt => bt.return_date == null);
            tbTotalBorrowed.Text = totalBorrowed.ToString();

            tbOverdueCount.Text = db.OverdueBooks.Count().ToString();

            tbStudentsCount.Text = db.Students.Count(s => s.student_status == "Active").ToString();

         
            DateTime today = DateTime.Today;
            tbTotalVisits.Text = db.Library_Visits.Count(v => v.visit_date == today).ToString();

            decimal pendingFines = db.FineBalances
                .Where(f => f.payment_status == "Unpaid")
                .Sum(f => (decimal?)f.balance_remaining) ?? 0;
            tbPendingFines.Text = $"₱{pendingFines:N2}";

           
            decimal collectedToday = db.Payments
                .Where(p => p.payment_date == today)
                .Sum(p => (decimal?)p.payment_amount) ?? 0;
            tbCollectedFinesToday.Text = $"₱{collectedToday:N2}";
        }


        private void ResetTabButtons()
        {
            btnOverdue.BorderBrush = Brushes.Transparent;
            btnAvailable.BorderBrush = Brushes.Transparent;
            btnBorrowed.BorderBrush = Brushes.Transparent;
        }

        private void BtnOverdue_Click(object sender, RoutedEventArgs e)
        {
           
            ResetTabButtons();
            btnOverdue.BorderBrush = (Brush)FindResource("AccentBrush2");
            var overdueBooks = db.OverdueBooks
    .Select(o => new
    {
        StudentID = o.student_id,
        StudentName = o.student_name,
        BookTitle = o.book_title,
        BorrowDate = o.borrow_date,
        DueDate = o.due_date,
        DaysOverdue = o.days_overdue,
        CalculatedFine = o.calculated_fine,
        CurrentFine = o.current_fine,
        FineStatus = o.fine_status
    }).ToList();

            dgMainTable.ItemsSource = overdueBooks;
            tbTableName.Text = "Overdue Books";


        }

        private void BtnAvailable_Click(object sender, RoutedEventArgs e)
        {
          

            ResetTabButtons();
            btnAvailable.BorderBrush = (Brush)FindResource("AccentBrush2");
            var availableBooks = db.AvailableBooks
    .Select(a => new
    {
        BookID = a.book_id,
        Title = a.book_title,
        ISBN = a.book_isbn,
        Author = a.author_name,
        Genre = a.genre_name,
        Publisher = a.book_publisher,
        PublicationYear = a.book_publication_year,
        AvailableCopies = a.available_copies,
        CopyIDs = a.copy_ids
    }).ToList();

            dgMainTable.ItemsSource = availableBooks;
            tbTableName.Text = "Available Books";
        }

        private void BtnBorrowed_Click(object sender, RoutedEventArgs e)
        {
           

            ResetTabButtons();
            btnBorrowed.BorderBrush = (Brush)FindResource("AccentBrush2");
            var borrowRecords = (from bt in db.Borrow_Transactions
                                 join s in db.Students
                                 on bt.student_id equals s.student_id
                                 select new
                                 {
                                     TransactionID = bt.transaction_id,
                                     StudentID = bt.student_id,
                                     StudentName = s.student_name,
                                     CopyID = bt.copy_id,
                                     BorrowDate = bt.borrow_date,
                                     DueDate = bt.due_date,
                                     ReturnDate = bt.return_date
                                 }).ToList();

            dgMainTable.ItemsSource = borrowRecords;
            tbTableName.Text = "Borrwed Books";
        }

        private void btnBorrowBook_Click(object sender, RoutedEventArgs e)
        {
            BorrowReturnForm borrowReturnForm = new BorrowReturnForm();
            borrowReturnForm.ShowDialog();
            
        }

        private void btnReturnBook_Click(object sender, RoutedEventArgs e)
        {
            BorrowReturnForm borrowReturnForm = new BorrowReturnForm(true);
            borrowReturnForm.ShowDialog();
        }
    }
}
