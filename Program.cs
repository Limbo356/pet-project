using Microsoft.EntityFrameworkCore;
using DbContextBook;
using DbContextUser;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DbBook>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DbBook")));
builder.Services.AddDbContext<DbUser>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DbUser")));
builder.Services.AddScoped<DownloadBook>();
builder.Services.AddScoped<AuthorizePerson>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<DownloadsLeft>();
builder.Services.AddTransient<HelperForUser>();
builder.Services.AddTransient<FilterBookForCategory>();
builder.Services.AddTransient<DeleteBookService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseStaticFiles();
app.UseDefaultFiles();
app.UseSession();

//  логины и пароли
List<UserLogin> userLogins = new List<UserLogin>
{
    new UserLogin{ EmailUser = "tom@gmail.com", PasswordUser = "A1234567"},
    new UserLogin{ EmailUser = "liza@gmail.com", PasswordUser = "12345678"},
    new UserLogin{ EmailUser = "alina@gmail.com", PasswordUser = "87654321"},
    new UserLogin{ EmailUser = "guzel@gmail.com", PasswordUser = "135791113"},
    new UserLogin{ EmailUser = "bob@gmail.com", PasswordUser = "24681012"}
};

List<Profile> profile = new List<Profile>{
    new Profile { Age = 24, Name = "Tom", SurName = "Anderson", NickName = "ShadowTom", DateBirthday = new DateOnly(2001, 5, 14), PhoneNumber = "+77011234567" },
    new Profile { Age = 21, Name = "Liza", SurName = "Morozova", NickName = "LizaMoon", DateBirthday = new DateOnly(2004, 2, 3), PhoneNumber = "+77019876543" },
    new Profile { Age = 27, Name = "Bob", SurName = "Carter", NickName = "BobbyX", DateBirthday = new DateOnly(1998, 11, 22), PhoneNumber = "+77015554433" },
    new Profile { Age = 30, Name = "Arthur", SurName = "Kingsley", NickName = "ArtLegend", DateBirthday = new DateOnly(1995, 7, 9), PhoneNumber = "+77017778899" },
    new Profile { Age = 19, Name = "Kuka", SurName = "Tanaka", NickName = "KukaChan", DateBirthday = new DateOnly(2006, 1, 28), PhoneNumber = "+77013332211" }
};

//  пользователи
List<User> users = new List<User>
{
    new User{Role = Role.Admin, LastDownloadDate = DateTime.Today, DownloadToday = 0, UserLogin = userLogins[0], Profile = profile[0]},
    new User{Role = Role.Admin, LastDownloadDate = DateTime.Today, DownloadToday = 0, UserLogin = userLogins[1], Profile = profile[1]},
    new User{Role = Role.User, LastDownloadDate = DateTime.Today, DownloadToday = 0, UserLogin = userLogins[2], Profile = profile[2]},
    new User{Role = Role.User, LastDownloadDate = DateTime.Today, DownloadToday = 0, UserLogin = userLogins[3], Profile = profile[3]},
    new User{Role = Role.Admin, LastDownloadDate = DateTime.Today, DownloadToday = 0, UserLogin = userLogins[4], Profile = profile[4]}
};
//==================================

app.Use(async (context, next) =>
{
    var userId = context.Session.GetInt32("userId");

    if (userId != null)
    {
        var db = context.RequestServices.GetRequiredService<DbUser>();

        var user = await db.User
            .Include(u => u.UserLogin)
            .FirstOrDefaultAsync(u => u.PK_UserId == userId);

        context.Items["User"] = user;
    }

    await next();
});

app.MapGet("/", () => Results.Redirect("/index.html"));

app.MapPut("/editProfileSecurity", async (HttpContext context, DbUser db) =>
{
    var userID = context.Session.GetInt32("userId");

    var userJsonResult = await context.Request.ReadFromJsonAsync<SecurityProfileUser>();

    if(userJsonResult is null)
    return Results.BadRequest("Данные не получены");

    var user = db.User.Include(i => i.UserLogin)
                      .FirstOrDefault(i => i.PK_UserId == userID);

    if(user is null)
    return Results.BadRequest("Книга не найдена");

    user.UserLogin!.EmailUser = userJsonResult.EmailUser;
    user.UserLogin.PasswordUser = userJsonResult.New_Password;

    await db.SaveChangesAsync();

    return Results.Ok("Дaнные о пользователе были изменены");
});

app.MapPut("/editProfileUser", async (HttpContext context, DbUser dbUser) =>
{
    var userID = context.Session.GetInt32("userId");

    var userJsonResult = await context.Request.ReadFromJsonAsync<PutUserProfileDto>();

    if(userJsonResult is null)
    return Results.BadRequest("Данные не получены");

    var user = dbUser.User.Include(i => i.UserLogin)
                      .FirstOrDefault(i => i.PK_UserId == userID);

    if(user is null)
    return Results.BadRequest("Книга не найдена");

    user.Profile!.Age = userJsonResult.Age;
    user.Profile!.PhoneNumber = userJsonResult.NumberPhone;
    user.Profile!.Name = userJsonResult.Name;
    user.Profile!.SurName = userJsonResult.SurName;
    user.Profile!.NickName = userJsonResult.NickName;
    user.Profile!.DateBirthday = userJsonResult.DateBirthday;

    await dbUser.SaveChangesAsync();

    return Results.Ok("Дaнные о пользователе были изменены");
});

app.MapGet("/getProfileUserInfo", async (HttpContext context, DbUser dbUser) =>
{
    var userID = context.Session.GetInt32("userId");

    var user = dbUser.User.Include(i => i.UserLogin)
                      .Select(u => new GetUserProfileDto
                      {
                          Id = u.PK_UserId,
                          Name = u.Profile!.Name,
                          SurName = u.Profile.SurName,
                          Age = u.Profile.Age,
                          PhoneNumber = u.Profile.PhoneNumber,
                          NickName = u.Profile.NickName,
                          DateBirthday = u.Profile.DateBirthday,
                          EmailUser = u.UserLogin!.EmailUser,
                          PasswordUser = u.UserLogin!.PasswordUser,
                      })
                      .FirstOrDefault(i => i.Id == userID);

    return Results.Ok(user);
});

app.MapGet("/api/count", async(DbBook dbook, DbUser dbuser) =>
{
    var book = dbook.BooKParametrs.Count();
    var user = dbuser.User.Count();

    return Results.Ok(new { countBook = book, countUser = user });
});

app.MapGet("/getUser/{id:int}", async (int id, DbUser db) =>
{
    var user = db.User.Include(i => i.UserLogin)
                      .Select(s => new PutUserDto
                      {
                        Id = s.PK_UserId,
                        Name = s.Profile!.Name,
                        SurName = s.Profile!.SurName,
                        Age = s.Profile!.Age,
                        NumberPhone = s.Profile.PhoneNumber,
                        DateBirthday = s.Profile.DateBirthday,
                        LastDownloadDate = s.LastDownloadDate,
                        DownloadToday = s.DownloadToday,
                        EmailUser = s.UserLogin != null ? s.UserLogin.EmailUser : null,
                        PasswordUser = s.UserLogin != null ? s.UserLogin.PasswordUser : null
                      }).FirstOrDefault(g => g.Id == id);

    if(user == null)
    {
        return Results.NotFound(new {message = "Пользователь не был найден"});
    }

    return Results.Ok(new {user});
});

app.MapPut("/editUser/{id:int}", async (int id, HttpContext context, DbUser db) =>
{
    var userJsonResult = await context.Request.ReadFromJsonAsync<GetUserDto>();

    if(userJsonResult is null)
    return Results.BadRequest("Данные не получены");

    var user = db.User.Include(i => i.UserLogin)
                      .FirstOrDefault(i => i.PK_UserId == id);

    if(user is null)
    return Results.BadRequest("Книга не найдена");

    user.Profile!.Name = userJsonResult.Name;
    user.Profile.SurName = userJsonResult.SurName;
    user.Profile!.Age = userJsonResult.Age;
    user.Profile.PhoneNumber = userJsonResult.NumberPhone;
    user.Profile.DateBirthday = userJsonResult.DateBirthday;
    user.DownloadToday = userJsonResult.DownloadToday;
    user.LastDownloadDate = userJsonResult.LastDownloadDate;
    user.UserLogin!.EmailUser = userJsonResult.EmailUser;
    user.UserLogin.PasswordUser = userJsonResult.PasswordUser;

    await db.SaveChangesAsync();

    return Results.Ok("Дaнные о пользователе были изменены");
});

app.MapGet("/getBook/{id:int}", async (int id, DbBook db) =>
{
    var book = db.BooKParametrs.Include(a => a.AuthorBooks)
                               .Include(p => p.PublishingHouse)
                               .Select(s => new GetBookDto
                               {
                                    BookId = s.PK_BookParametrsId,
                                    BookName = s.NameBook,
                                    PdfPathBook = s.PdfPathBook,
                                    EpubPathBook = s.EpubPathBook,
                                    ImagePathBook = s.ImagePathBook,
                                    Description = s.Description,
                                    AuthorId = s.AuthorBooks!.PK_AuthorBookId,
                                    FullName = $"{s.AuthorBooks.NameAuthor} {s.AuthorBooks.SurnameAuthor} {s.AuthorBooks.PatronimycAuthor}",
                                    PublishingId = s.PublishingHouse!.Id,
                                    NamePublishing = s.PublishingHouse.NamePublishing,
                                    Gentres = s.Gentres
                               }).FirstOrDefault(s => s.BookId == id);

    if(book == null)
    {
        return Results.NotFound(new {message = "Пользователь не был найден"});
    }

    return Results.Ok(new {book});
});

app.MapPut("/editBook/{id:int}", async (int id, HttpContext context, DbBook db) =>
{
    var bookJsonResolt = await context.Request.ReadFromJsonAsync<PutBookDto>();

    if(bookJsonResolt is null)
        return Results.BadRequest("Данные не получены");

    var book = db.BooKParametrs.Include(g => g.Gentres)
                               .Include(a => a.AuthorBooks)
                               .FirstOrDefault(b => b.PK_BookParametrsId == id);

    if(book is null)
        return Results.BadRequest("Книга не найдена");

    string[] splitName = bookJsonResolt.FullName!.Split();

    book.AuthorBooks!.NameAuthor = splitName[0];
    book.AuthorBooks.SurnameAuthor = splitName[1];
    book.AuthorBooks.PatronimycAuthor = splitName[2];
    book.NameBook = bookJsonResolt.BookName;
    book.PdfPathBook = bookJsonResolt.PdfPathBook;
    book.EpubPathBook = bookJsonResolt.EpubPathBook;
    book.ImagePathBook = bookJsonResolt.ImagePathBook!;

    if(bookJsonResolt.Gentres != null)
    {
        foreach(var gDto in bookJsonResolt.Gentres)
        {
            if(string.IsNullOrEmpty(gDto.NameGentre))
            continue;

            var norm = gDto.NameGentre.Trim();

            var existingGentre = db.Gentres.FirstOrDefault(g => g.NameGentre == gDto.NameGentre);
        
            if (existingGentre != null)
            {
                // Если жанр уже есть — просто привязываем
                book.Gentres!.Add(existingGentre);
            }
            else
            {
                // Если жанра нет — создаём
                var newGentre = new Gentre
                {
                    NameGentre = norm
                };

                db.Gentres.Add(newGentre);
                book.Gentres!.Add(newGentre);
            }
        }

        await db.SaveChangesAsync();
    }

    return Results.Ok("Данные о книги были изменены");
});

app.MapDelete("/deleteUser/{id:int}", async (int id, DbUser db) =>
{
    var user = await db.User.FirstOrDefaultAsync(w => w.PK_UserId == id);

    if (user == null)
    {
        return Results.NotFound(new { message = "Пользователь не найден" });
    }

    db.User.Remove(user);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


app.MapDelete("/deleteBook/{id:int}", async (int id, DeleteBookService bookService, DbBook db) =>
{
    var book = await db.BooKParametrs.FirstOrDefaultAsync(w => w.PK_BookParametrsId == id);

    var query = bookService.DeleteBook(book!);

    db.BooKParametrs.Remove(book!);

    await db.SaveChangesAsync();

    return Results.Ok("Книга была удалена");
});

// ------------------ Узнать, сколько скачиваний осталось ------------------
app.MapGet("/api/downloads-left", async (DownloadsLeft downloads, HttpContext context, DbUser db) =>
{
    var helper = app.Services.GetService<HelperForUser>();

    var user = context.Items["User"] as User;

    await downloads!.GetDownloadsToDay(db, user!, context, helper!);
});

// ------------------ Проверка пользователя по роли ------------------
app.MapGet("/api/current-user", async (CurrentUser currentUser, HttpContext context) =>
{
    var user = context.Items["User"] as User;

    return await currentUser.GetRoleUser(user!);
});

// ------------------ Получить всех пользователей ------------------
app.MapGet("/api/users", async (DbUser db) =>
{
    var users = await db.User
        .Include(u => u.UserLogin)
        .Select(u => new
        {
            Id = u.PK_UserId,
            Name = u.Profile!.Name,
            Age = u.Profile!.Age,
            NumberPhone = u.Profile.PhoneNumber,
            Role = u.Role.ToString(),
            Email = u.UserLogin != null ? u.UserLogin.EmailUser : null
        })
        .ToListAsync();

    return Results.Ok(users);
});

// ------------------ Получить все книги ------------------
app.MapGet("/api/books", async (DbBook db) =>
{
    var books = await db.BooKParametrs
        .Include(b => b.AuthorBooks)
        .Include(b => b.Gentres)
        .Select(b => new
        {
            PK_BookParametrsId = b.PK_BookParametrsId,
            NameBook = b.NameBook,
            AuthorBooks = b.AuthorBooks != null 
                ? $"{b.AuthorBooks.NameAuthor} {b.AuthorBooks.SurnameAuthor}" 
                : null,
            Gentres = b.Gentres!.Select(g => g.NameGentre).ToList()
        })
        .ToListAsync();

    return Results.Ok(books);
});


app.MapGet("/book", async (DbBook db) =>
{
   var getBook = await db.BooKParametrs.Include(g => g.Gentres)
   .Select(b => new
   {
        BookId = b.PK_BookParametrsId,
        Title = b.NameBook,
        Author = $"{b.AuthorBooks!.NameAuthor} {b.AuthorBooks.SurnameAuthor}",
        Gentre = b.Gentres,
        Img = b.ImagePathBook.Replace("/home/debian/Diplom Web-Site/wwwroot", "")
   }).ToListAsync();

   return Results.Ok(new {getBook});
});

app.MapGet("/api/book/{id:int}", async (DbBook db, int id) =>
{
    var book = await db.BooKParametrs.Include(g => g.Gentres)
    .Include(a => a.AuthorBooks)
    .Include(p => p.PublishingHouse)
    .Select(b => new GetBookDto
    {
        BookId = b.PK_BookParametrsId,
        BookName = b.NameBook,
        NamePublishing = b.PublishingHouse!.NamePublishing,
        FullName = $"{b.AuthorBooks!.NameAuthor} {b.AuthorBooks.SurnameAuthor}",
        Gentres = b.Gentres,
        ImagePathBook = b.ImagePathBook.Replace("/home/debian/Diplom Web-Site/wwwroot", ""),
        PdfPathBook = b.PdfPathBook!.Replace("/home/debian/Diplom Web-Site/wwwroot", ""),
        EpubPathBook = b.EpubPathBook!.Replace("/home/debian/Diplom Web-Site/wwwroot", ""),
        Description = b.Description
    }).FirstOrDefaultAsync(g => g.BookId == id);

    if(book == null)
        return Results.NotFound();

    return Results.Ok(book);

});

// Простой эндпоинт по ID
app.MapGet("/api/download-book/{id:int}/{type}", async (
    DownloadBook downloadBook,
    HttpContext context,
    DbUser dbUser,
    DbBook db,
    int id,
    string type) =>
{
    var book = await db.BooKParametrs
     .FirstOrDefaultAsync(b => b.PK_BookParametrsId == id);

    var helper = app.Services.GetService<HelperForUser>();

    // Проверка лимитов
    var user = context.Items["User"] as User;

    return await downloadBook.Download(book!, helper!, context, dbUser, user!, type, id);

});
// ------------------ Логин ------------------
app.MapPost("/api/logger", async (HttpContext context, DbUser db) =>
{
    try
    {
        var person = await context.Request.ReadFromJsonAsync<UserLoger>();

        if (person == null)
            return Results.BadRequest(new { text = "Данные не получены" });

        var user = await db.User
            .Include(u => u.UserLogin)
            .FirstOrDefaultAsync(u =>
                u.UserLogin!.EmailUser == person.Email &&
                u.UserLogin.PasswordUser == person.Password);

        if (user == null)
            return Results.BadRequest(new { text = "Неверный логин или пароль" });

        context.Session.SetInt32("userId", user.PK_UserId);

        return Results.Ok(new { text = "Успешный вход" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка логина: {ex.Message}");
        return Results.BadRequest(new { text = "Ошибка при входе" });
    }
});

// ------------------ Регистрация ------------------
app.MapPost("/api/authorize", async (AuthorizePerson registation, UserAuthorize userAuthorize, HttpContext context, DbUser db) =>
    await registation.Authorize(userAuthorize, context, db));

// ------------------ Фильтры книг ------------------
app.MapGet("/getBook_filter", async (DbBook db) =>
{
    var publishing = await db.PublishingHouse
        .Select(s => new {NamePublishing = s.NamePublishing})
        .ToListAsync();

    var authorBooks = await db.AuthorBook
        .Select(a => new { Id = a.PK_AuthorBookId, FullName = a.NameAuthor + " " + a.SurnameAuthor })
        .ToListAsync();

    var gentre = await db.Gentres
        .Select(g => new { Id = g.PK_Gentre, NameGentre = g.NameGentre })
        .ToListAsync();

    return Results.Ok(new { authorBooks, publishing, gentre });
});

// ------------------ Фильтр книг по категориям ------------------
app.MapPost("/category_book", async(FilterBookForCategory filterbook, FilterBook bookfilter, DbBook db) =>
{
    var book = db.BooKParametrs.AsQueryable();

    var result = filterbook.FilterBookAsync(book, bookfilter);

    var getBook = await result.Select(b => new
    {
        BookId = b.PK_BookParametrsId,
        Title = b.NameBook,
        Author = $"{b.AuthorBooks!.NameAuthor} {b.AuthorBooks.SurnameAuthor}",
        Gentre = b.Gentres!.Select(g => new { g.NameGentre }),
        Img = b.ImagePathBook
    }).ToListAsync();

    return Results.Ok(new { getBook });
});

app.MapPost("/api/logout", (HttpContext context) =>
{
    context.Session.Remove("userId");
    context.Session.Clear(); // можно полностью

    return Results.Ok(new { message = "logout" });
});

app.Run();