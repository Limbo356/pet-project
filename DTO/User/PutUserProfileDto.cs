class PutUserProfileDto
{
    public int Age { get; set; }
    public string? NumberPhone{get; set;}
    public string? Name {get; set;}
    public string? SurName {get; set;}
    public string? NickName {get; set;}
    public DateOnly? DateBirthday { get; set;}
}