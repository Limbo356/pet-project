using DbContextBook;

class GetBookDto
{
    public int BookId {get; set;}
    public string? BookName {get; set;}
    public string? PdfPathBook { get; set; }
    public string? EpubPathBook { get; set; }
    public string? ImagePathBook { get; set; }
    public string? Description {get; set;}

    public int AuthorId {get; set;}
    public string? FullName {get; set;}

    public int PublishingId {get; set;}
    public string? NamePublishing {get; set;}
    public List<Gentre>? Gentres {get; set;}
}