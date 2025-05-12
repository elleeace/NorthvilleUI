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
    /// Interaction logic for ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext();
        string _role;

        public ReportsPage(string role)
        {
            InitializeComponent();
            _role = role;
        }

    
        private void btnViewAttendance_Click(object sender, RoutedEventArgs e)
        {
            var visits = from visit in db.Library_Visits
                         join student in db.Students on visit.student_id equals student.student_id
                         select new
                         {
                             VisitID = visit.visit_id,
                             StudentID = student.student_id,
                             StudentName = student.student_name,
                             VisitDate = visit.visit_date
                         };

            dgReportsTable.ItemsSource = visits.ToList();
        }

        private void btnViewStudentFines_Click(object sender, RoutedEventArgs e)
        {
            var fines = from fine in db.FineBalances
                        select new
                        {
                            FineID = fine.fine_id,
                            TransactionID = fine.transaction_id,
                            StudentID = fine.student_id,
                            StudentName = fine.student_name,
                            BookTitle = fine.book_title,
                            OriginalFine = fine.original_fine,
                            AmountPaid = fine.amount_paid,
                            BalanceRemaining = fine.balance_remaining,
                            PaymentStatus = fine.payment_status
                        };

            dgReportsTable.ItemsSource = fines.ToList();
        }

        private void btnViewFineSummary_Click(object sender, RoutedEventArgs e)
        {
            var summary = from s in db.StudentFinesSummaries
                          select new
                          {
                              StudentID = s.student_id,
                              StudentName = s.student_name,
                              Email = s.student_email,
                              Phone = s.student_phone,
                              Course = s.course_name,
                              OutstandingBalance = s.outstanding_balance,
                              ActiveFines = s.active_fines
                          };

            dgReportsTable.ItemsSource = summary.ToList();
        }

        private void btnViewPayments_Click(object sender, RoutedEventArgs e)
        {
            var payments = from p in db.Payments
                           join f in db.Fines on p.fine_id equals f.fine_id
                           join s in db.Students on p.student_id equals s.student_id
                           select new
                           {
                               PaymentID = p.payment_id,
                               StudentID = p.student_id,
                               StudentName = s.student_name,
                               PaymentDate = p.payment_date,
                               Amount = p.payment_amount,
                               Method = p.payment_method,
                               Receipt = p.receipt_number,
                               FineAmount = f.fine_amount,
                               FineStatus = f.fine_status
                           };

            dgReportsTable.ItemsSource = payments.ToList();
        }
    }
}
