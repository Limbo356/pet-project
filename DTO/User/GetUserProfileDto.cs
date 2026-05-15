public class GetUserProfileDto
{
    public int Id { get; set; }
    public int Age { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Name { get; set; }
    public string? SurName { get; set; }
    public string? NickName { get; set; }
    public DateOnly? DateBirthday { get; set; }


    public string? EmailUser { get; set; }
    public string? PasswordUser { get; set; }
}