using System;
using System.Linq;
using System.Windows;

namespace NorthvilleUI.Forms
{
    public partial class BorrowTransactionEditForm : Window
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        private Borrow_Transaction transactionToEdit;

        public BorrowTransactionEditForm(Borrow_Transaction transaction)
        {
            InitializeComponent();

            if (transaction != null)
            {
                transactionToEdit = db.Borrow_Transactions
        .FirstOrDefault(bt => bt.transaction_id == transaction.transaction_id);
                txtTransactionId.Text = transaction.transaction_id;
                txtStudentId.Text = transaction.student_id;
                txtCopyId.Text = transaction.copy_id;

                dpBorrowDate.SelectedDate = transaction.borrow_date;
                dpDueDate.SelectedDate = transaction.due_date;
                dpReturnDate.SelectedDate = transaction.return_date;
            }
            else
            {
                            MessageBox.Show(
                "Please select a valid transaction before proceeding.",
                "Invalid Selection",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

                this.Close();
            }

            txtTransactionId.IsReadOnly = false;
            txtStudentId.IsReadOnly = true;
            txtCopyId.IsReadOnly = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (transactionToEdit == null)
            {
                MessageBox.Show(
                    "No transaction is currently loaded. Please select or load a transaction to edit.",
                    "Transaction Not Loaded",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }


            if (dpBorrowDate.SelectedDate == null || dpDueDate.SelectedDate == null)
            {
                MessageBox.Show(
                    "Both Borrow Date and Due Date are required to proceed.",
                    "Missing Date Fields",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }


            string transactionID = txtTransactionId.Text;
            DateTime borrowDate = dpBorrowDate.SelectedDate.Value;
            DateTime dueDate = dpDueDate.SelectedDate.Value;
            DateTime? returnDate = dpReturnDate.SelectedDate;

            if (dueDate < borrowDate)
            {
                MessageBox.Show(
                    "The Due Date cannot be earlier than the Borrow Date. Please correct the dates.",
                    "Invalid Date Range",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }


            transactionToEdit.transaction_id = transactionID;
            transactionToEdit.borrow_date = borrowDate;
            transactionToEdit.due_date = dueDate;
            transactionToEdit.return_date = returnDate;

            try
            {
                db.SubmitChanges();
                MessageBox.Show(
                    $"The borrow transaction has been updated successfully.\n\nBorrow Date: {borrowDate:MMMM dd, yyyy}",
                    "Update Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.DialogResult = true;
            }

            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while updating the transaction.\n\n" + ex.Message,
                    "Update Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

        }

    }
}
