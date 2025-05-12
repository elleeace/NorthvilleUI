using NorthvilleUI.Forms;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NorthvilleUI.Pages
{
    /// <summary>
    /// Interaction logic for BookPage.xaml
    /// </summary>
    public partial class BookPage : Page
    {

        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        string userRole;

        public BookPage(string _role)
        {
            InitializeComponent();
            LoadStatistics();
            userRole = _role;
        }

        private void btnViewBooks_Click(object sender, RoutedEventArgs e)
        {
            if (userRole == "Clerical Assistant")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var bookList = (from b in db.Books
                            select new
                            {
                                BookID = b.book_id,
                                ISBN = b.book_isbn,
                                Title = b.book_title,
                                PublicationYear = b.book_publication_year,
                                GenreID = b.genre_id,
                                AuthorID = b.author_id,
                                Publisher = b.book_publisher
                            }).ToList();

            dgMainTable.ItemsSource = bookList;

        }

        private void LoadStatistics()
        {
            tbTotalBooks.Text = db.Books.Count().ToString();
            tbTotalGenres.Text = db.Genres.Count().ToString();
            tbTotalCopies.Text = db.Book_Copies.Count().ToString();
            tbTotalAuthors.Text = db.Authors.Count().ToString();
        }

        private void btnAddBook_Click(object sender, RoutedEventArgs e)
        {

            if (userRole != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var form = new BookForm();
            if (form.ShowDialog() == true)
            {
                btnViewBooks_Click(null, null); // Refresh
            }

        }

        private void btnEditBook_Click(object sender, RoutedEventArgs e)
        {
            if (userRole != "Admin" && userRole != "Librarian")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var selectedItem = dgMainTable.SelectedItem;

            if (selectedItem == null)
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var bookId = selectedItem.GetType().GetProperty("BookID")?.GetValue(selectedItem)?.ToString();

            if (string.IsNullOrWhiteSpace(bookId))
            {
                MessageBox.Show("Unable to get Book ID.");
                return;
            }

            var bookToEdit = db.Books.FirstOrDefault(b => b.book_id == bookId);
            if (bookToEdit != null)
            {
                var form = new BookForm(bookToEdit);
                if (form.ShowDialog() == true)
                {
                    btnViewBooks_Click(null, null); // Refresh
                }
            }

        }

        private void btnDeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (userRole != "Admin")
            {
                FunctionalityErrorMessage.DisplayFunctionLimitMessage();
                return;
            }

            var selectedItem = dgMainTable.SelectedItem;

            if (selectedItem == null)
            {
                            MessageBox.Show(
                "Please select a book to edit.",
                "No Book Selected",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

                return;
            }

            var bookId = selectedItem.GetType().GetProperty("BookID")?.GetValue(selectedItem)?.ToString();

            if (string.IsNullOrWhiteSpace(bookId))
            {
                        MessageBox.Show(
             "Unable to retrieve the Book ID. Please make sure a valid book is selected.",
             "Error Retrieving Book ID",
             MessageBoxButton.OK,
             MessageBoxImage.Error);

                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete Book ID '{bookId}' and its copies?",
                                         "Confirm Deletion",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var book = db.Books.FirstOrDefault(b => b.book_id == bookId);
                    var copies = db.Book_Copies.Where(c => c.book_id == bookId);

                    if (copies.Any())
                        db.Book_Copies.DeleteAllOnSubmit(copies);

                    if (book != null)
                    {
                        db.Books.DeleteOnSubmit(book);
                        db.SubmitChanges();
                        MessageBox.Show(
                            "The book and all associated copies have been successfully deleted.",
                            "Deletion Successful",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        btnViewBooks_Click(null, null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                    "An error occurred while attempting to delete the book:\n\n" + ex.Message,
                    "Deletion Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                }
            }



        }
    }
}
