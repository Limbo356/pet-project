class GetUserDto
{
    public int Age {get; set;}
    public int DownloadToday {get; set;}
    public string? NumberPhone{get; set;}
    public string? Name {get; set;}
    public string? SurName {get; set;}
    public DateTime LastDownloadDate {get; set;}
    public DateOnly? DateBirthday { get; set;}

    public string? EmailUser {get; set;}
    public string? PasswordUser {get; set;}
}