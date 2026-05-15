using DbContextUser;
using Microsoft.EntityFrameworkCore;

class AuthorizePerson
{
    public async Task<IResult> Authorize(UserAuthorize userAuthorize, HttpContext context, DbUser db)
    {

        if (userAuthorize == null)
            return Results.BadRequest("Данные не получены");

        if (string.IsNullOrWhiteSpace(userAuthorize.Email) ||
            string.IsNullOrWhiteSpace(userAuthorize.Password) ||
            string.IsNullOrWhiteSpace(userAuthorize.Repeat_Password))
        {
            return Results.BadRequest("Все поля обязательны");
        }

        if (userAuthorize.Password.Length < 8)
            return Results.BadRequest("Пароль должен состоять из более 8 символов");

        var exists = await db.UserLogin
            .AnyAsync(u => u.EmailUser == userAuthorize.Email);

        if (exists)
            return Results.BadRequest("Пользователь уже существует");

        // 2. Потом создаём User
        var user = new User
        {
            Role = Role.User,
            Profile = new Profile
            {
                Age = 18,
                Name = "Новый пользователь",
                PhoneNumber = "0"
            },
            LastDownloadDate = DateTime.Today,
            DownloadToday = 0
        };

        await db.User.AddAsync(user);
        await db.SaveChangesAsync();

        // 1. Сначала создаём UserLogin
        var userLogin = new UserLogin
        {
            EmailUser = userAuthorize.Email,
            PasswordUser = userAuthorize.Password,
            FK_UserId = user.PK_UserId
        };

        await db.UserLogin.AddAsync(userLogin);
        await db.SaveChangesAsync();

        context.Session.SetInt32("userId", user.PK_UserId);
        System.Console.WriteLine("Пользователь добавлен в базу");

        return Results.Ok("Пользователь успешно добавлен");
    }
}
