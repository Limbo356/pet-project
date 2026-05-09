class PutBookDto
{
    public string? BookName {get; set;}
    public string? PdfPathBook {get; set;}
    public string? EpubPathBook {get; set; }
    public string? ImagePathBook {get; set;}

    public int AuthorId {get; set;}
    public string? FullName {get; set;}
    public List<GentreDto>? Gentres {get; set;}
}

class GentreDto
{
    public string? NameGentre {get; set;}   
}