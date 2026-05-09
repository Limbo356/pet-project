using DbContextUser;

class HelperForUser
{
    public int GetDailyLimit(User? user)
{
    if (user == null)
        return 3; // Guest

    return user.Role switch
    {
        Role.Admin => int.MaxValue,
        Role.User => 10,
        _ => 3
    };
}

public void ResetUserDownloadsIfNeeded(User user)
{
    if (user.LastDownloadDate == default || user.LastDownloadDate.Date < DateTime.Today)
    {
        user.DownloadToday = 0;
        user.LastDownloadDate = DateTime.Today;
    }
}

public int GetGuestDownloadsToday(HttpContext context)
{
    var lastDate = context.Session.GetString("guestLastDownloadDate");
    var today = DateTime.Today.ToString("yyyy-MM-dd");

    if (lastDate != today)
    {
        context.Session.SetInt32("guestDownloadToday", 0);
        context.Session.SetString("guestLastDownloadDate", today);
    }

    return context.Session.GetInt32("guestDownloadToday") ?? 0;
}

// Определение MIME типа
public string GetMimeType(string filePath)
{
    var ext = Path.GetExtension(filePath).ToLower();
    return ext switch
    {
        ".pdf" => "application/pdf",
        ".epub" => "application/epub+zip",
        ".fb2" => "application/fb2",
        ".mobi" => "application/x-mobipocket-ebook",
        _ => "application/octet-stream"
    };
}
}