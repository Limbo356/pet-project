using DbContextBook;

class FilterBookForCategory
{
    public FilterBookForCategory() {}

    public IQueryable<BooKParametrs> FilterBookAsync(IQueryable<BooKParametrs> book, FilterBook bookfilter)
    {

        if (!string.IsNullOrEmpty(bookfilter!.Author) && bookfilter.Author != "-")
        {
            var author = bookfilter.Author.Trim();

            book = book.Where(b =>
                (b.AuthorBooks!.NameAuthor + " " + b.AuthorBooks.SurnameAuthor) == bookfilter.Author);
        }

        if (!string.IsNullOrEmpty(bookfilter!.Publishing_House) && bookfilter.Publishing_House != "-")
        {
            var author = bookfilter.Publishing_House.Trim();

            book = book.Where(b =>
                b.PublishingHouse!.NamePublishing == bookfilter.Publishing_House);
        }

        if (!string.IsNullOrEmpty(bookfilter!.Gentres) && bookfilter.Gentres != "-")
        {
            book = book.Where(g => g.Gentres!.Any(b => b.NameGentre == bookfilter.Gentres));
        }

        return book;
    }
}
