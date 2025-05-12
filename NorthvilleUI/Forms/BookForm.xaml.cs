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

namespace NorthvilleUI.Forms
{
    /// <summary>
    /// Interaction logic for BookForm.xaml
    /// </summary>
    public partial class BookForm : Window
    {
        private NorthvilleLibDataContext db = new NorthvilleLibDataContext(Properties.Settings.Default.NorthvilleConnectionString);
        private bool isEditMode;
        private Book bookToEdit;

        public BookForm()
        {
            InitializeComponent();
            isEditMode = false;
            LoadDropdowns();
        }

        public BookForm(Book book)
        {
            InitializeComponent();
            isEditMode = true;
            bookToEdit = book;

            LoadDropdowns(); // Load before setting selected values

            if (book != null)
            {
                txtBookId.Text = book.book_id;
                txtBookTitle.Text = book.book_title;
                txtISBN.Text = book.book_isbn;
                txtPublisher.Text = book.book_publisher;
                txtPublicationYear.Text = book.book_publication_year.ToString();
                cbGenre.SelectedValue = book.genre_id;
                cbAuthor.SelectedValue = book.author_id;

                txtBookId.IsReadOnly = true;
                txtCopies.IsEnabled = false; // Disable copies input when editing
            }
        }
        private void LoadDropdowns()
        {
            cbGenre.ItemsSource = db.Genres.ToList();
            cbGenre.DisplayMemberPath = "genre_name";
            cbGenre.SelectedValuePath = "genre_id";

            cbAuthor.ItemsSource = db.Authors.ToList();
            cbAuthor.DisplayMemberPath = "author_name";
            cbAuthor.SelectedValuePath = "author_id";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSaveBook_Click(object sender, RoutedEventArgs e)
        {
            string bookId = txtBookId.Text.Trim();
            string title = txtBookTitle.Text.Trim();
            string isbn = txtISBN.Text.Trim();
            string publisher = txtPublisher.Text.Trim();
            int year = -1;
            int copies = -1;

            if (string.IsNullOrWhiteSpace(bookId) || string.IsNullOrWhiteSpace(title) ||
                cbAuthor.SelectedValue == null || cbGenre.SelectedValue == null ||
                !int.TryParse(txtPublicationYear.Text.Trim(), out year) ||
                (!isEditMode && !int.TryParse(txtCopies.Text.Trim(), out copies)))
            {
                MessageBox.Show(
                "Please complete all required fields correctly before proceeding.",
                "Incomplete Form",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

                return;
            }

            if (isEditMode)
            {
                var book = db.Books.FirstOrDefault(b => b.book_id == bookId);
                if (book != null)
                {
                    book.book_title = title;
                    book.book_isbn = isbn;
                    book.book_publisher = publisher;
                    book.book_publication_year = year;
                    book.genre_id = cbGenre.SelectedValue.ToString();
                    book.author_id = cbAuthor.SelectedValue.ToString();
                }
            }
            else
            {
                var newBook = new Book
                {
                    book_id = bookId,
                    book_title = title,
                    book_isbn = isbn,
                    book_publisher = publisher,
                    book_publication_year = year,
                    genre_id = cbGenre.SelectedValue.ToString(),
                    author_id = cbAuthor.SelectedValue.ToString()
                };

                db.Books.InsertOnSubmit(newBook);

             
                int bookNum = int.Parse(bookId.Substring(1)); 

                for (int i = 1; i <= copies; i++)
                {
                    var copy = new Book_Copy
                    {
                        copy_id = $"BC{bookNum:D2}{i:D2}",
                        book_id = bookId,
                        copy_status = "Available",
                        date_added = DateTime.Now
                    };
                    db.Book_Copies.InsertOnSubmit(copy);
                }

            }

            try
            {
                db.SubmitChanges();
                MessageBox.Show(
                    "The book has been saved successfully.",
                    "Save Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                 "An error occurred while saving the book:\n\n" + ex.Message,
                 "Save Error",
                 MessageBoxButton.OK,
                 MessageBoxImage.Error);


            }
        }
    }
}