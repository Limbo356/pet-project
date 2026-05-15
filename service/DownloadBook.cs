using DbContextBook;
using DbContextUser;

class DownloadBook
{
    public async Task<IResult> Download(BooKParametrs book, HelperForUser helper, HttpContext context, 
                                        DbUser dbUser, User user, string type, int id)
    {
        if (book == null)
        {
            return Results.NotFound(new
            {
                message = "Книга не найдена"
            });
        }

        string filePath;

        // Выбор Windows пути
        switch (type.ToLower())
        {
            case ".pdf":
                filePath = book.PdfPathBook!;
                break;

            case ".epub":
                filePath = book.EpubPathBook!;
                break;

            default:

                return Results.BadRequest(new
                {
                    message = "Неизвестный тип файла"
                });
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Results.NotFound(new
            {
                message = "Путь к файлу отсутствует"
            });
        }

        // Проверка Windows файла
        if (!File.Exists(filePath))
        {
            return Results.NotFound(new
            {
                message = "Файл не найден на сервере"
            });
        }

        var limit = helper!.GetDailyLimit(user);

        if (user == null)
        {
            var used = helper.GetGuestDownloadsToday(context);
            if (used >= limit)
            {
                return Results.BadRequest(new
                {
                    message = "Вы исчерпали лимит скачиваний"
                });
            }
            used++;
            context.Session.SetInt32("guestDownloadToday", used);
        }
        else
        {
            helper.ResetUserDownloadsIfNeeded(user);

            if (user.Role != Role.Admin &&
                user.DownloadToday >= limit)
            {
                return Results.BadRequest(new
                {
                    message = "Вы исчерпали лимит скачиваний"
                });
            }

            if (user.Role != Role.Admin)
            {
                user.DownloadToday++;

                user.LastDownloadDate = DateTime.Today;

                await dbUser.SaveChangesAsync();
            }
        }

        // Отправка файла
        var extension = Path.GetExtension(filePath);

        var mimeType = helper.GetMimeType(filePath);

        var safeFileName = $"{book.NameBook}{extension}";

        var fileBytes = await File.ReadAllBytesAsync(filePath);

        return Results.File(
        fileBytes,
        mimeType,
        safeFileName
    );
    }
}
