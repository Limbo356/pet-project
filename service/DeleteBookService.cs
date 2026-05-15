using DbContextBook;

class DeleteBookService
{
    public BooKParametrs DeleteBook(BooKParametrs book)
    {
        if (book == null)
        {
            return (BooKParametrs)Results.NotFound(new { message = "Пользователь не найден" });
        }

        if (!string.IsNullOrEmpty(book.PdfPathBook) && System.IO.File.Exists(book.PdfPathBook))
        {
            System.IO.File.Delete(book.PdfPathBook);
        }

        if (!string.IsNullOrEmpty(book.EpubPathBook) && System.IO.File.Exists(book.EpubPathBook))
        {
            System.IO.File.Delete(book.EpubPathBook);
        }

        if (!string.IsNullOrEmpty(book.ImagePathBook) && System.IO.File.Exists(book.ImagePathBook))
        {
            System.IO.File.Delete(book.ImagePathBook);
        }

        return book;
    }
}