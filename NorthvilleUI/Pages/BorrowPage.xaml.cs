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
    /// Interaction logic for BorrowPage.xaml
    /// </summary>
    public partial class BorrowPage : Page
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        string _role;
        public BorrowPage(string role)
        {
            InitializeComponent();
            _role = role;
        }

        private void btnViewBorrows_Click(object sender, RoutedEventArgs e)
        {
            var borrowView = (from bt in db.Borrow_Transactions
                              join s in db.Students on bt.student_id equals s.student_id
                              join bc in db.Book_Copies on bt.copy_id equals bc.copy_id
                              join b in db.Books on bc.book_id equals b.book_id
                              select new
                              {
                                  TransactionID = bt.transaction_id,
                                  StudentName = s.student_name,
                                  BookTitle = b.book_title,
                                  BorrowDate = bt.borrow_date,
                                  DueDate = bt.due_date,
                                  ReturnDate = bt.return_date
                              }).OrderByDescending(x => x.BorrowDate).ToList();

            dgMainTable.ItemsSource = borrowView;
        }

        private void btnEditBorrow_Click(object sender, RoutedEventArgs e)
        {
            if (_role != "Admin" && _role != "Librarian")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var selectedItem = dgMainTable.SelectedItem;

            if (selectedItem == null)
            {
                MessageBox.Show("Please select a borrow transaction to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var transactionIdProp = selectedItem.GetType().GetProperty("TransactionID");
            if (transactionIdProp == null)
            {
                MessageBox.Show("Unable to retrieve transaction ID from the selected item.");
                return;
            }

            string transactionId = transactionIdProp.GetValue(selectedItem)?.ToString();

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                MessageBox.Show("Transaction ID is missing.");
                return;
            }

            var transactionToEdit = db.Borrow_Transactions.FirstOrDefault(t => t.transaction_id == transactionId);
            if (transactionToEdit == null)
            {
                MessageBox.Show("Selected transaction not found in the database.");
                return;
            }

            var form = new BorrowTransactionEditForm(transactionToEdit);
            if (form.ShowDialog() == true)
            {
                btnViewBorrows_Click(null, null); // Refresh the datagrid after editing
            }
        }


        private void btnDeleteBorrow_Click(object sender, RoutedEventArgs e)
        {

            if (_role != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var selectedItem = dgMainTable.SelectedItem;

            if (selectedItem == null)
            {
                MessageBox.Show("Please select a transaction to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var transactionIdProp = selectedItem.GetType().GetProperty("TransactionID");
            string transactionId = transactionIdProp?.GetValue(selectedItem)?.ToString();

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                MessageBox.Show("Unable to determine selected transaction's ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete transaction '{transactionId}'?",
                                         "Confirm Deletion",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var transactionToDelete = db.Borrow_Transactions.FirstOrDefault(bt => bt.transaction_id == transactionId);

                    if (transactionToDelete != null)
                    {
                        db.Borrow_Transactions.DeleteOnSubmit(transactionToDelete);
                        db.SubmitChanges();

                        MessageBox.Show("Transaction successfully deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                        btnViewBorrows_Click(null, null); // Refresh DataGrid
                    }
                    else
                    {
                        MessageBox.Show("Transaction not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting transaction: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
