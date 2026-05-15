using DbContextUser;

class DownloadsLeft
{
    public async Task<IResult> GetDownloadsToDay(DbUser dbUser, User user, HttpContext context, HelperForUser helper)
    {
        var limit = helper!.GetDailyLimit(user);

        // ---------------- Гость ----------------
        if (user == null)
        {
            var used = helper.GetGuestDownloadsToday(context);

            return Results.Ok(new
            {
                role = "Guest",
                limit,
                used,
                left = Math.Max(0, limit - used)
            });
        }

        // ---------------- Пользователь ----------------
        helper.ResetUserDownloadsIfNeeded(user);
        await dbUser.SaveChangesAsync();

        return Results.Ok(new
        {
            role = user.Role!.ToString(),
            limit = user.Role == Role.Admin ? -1 : limit,
            used = user.Role == Role.Admin ? 0 : user.DownloadToday,
            left = user.Role == Role.Admin ? -1 : Math.Max(0, limit - user.DownloadToday)
        });
    }
}