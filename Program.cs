using Microsoft.EntityFrameworkCore;
using DbContextBook;
using DbContextUser;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DbBook>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DbBook")));
builder.Services.AddDbContext<DbUser>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DbUser")));
builder.Services.AddTransient<HelperForUser>();

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
    new Profile { Age = 24, Name = "Tom", SurName = "Anderson", NickName = "ShadowTom", DateBirthday = new DateTime(2001, 5, 14), PhoneNumber = "+77011234567" },
    new Profile { Age = 21, Name = "Liza", SurName = "Morozova", NickName = "LizaMoon", DateBirthday = new DateTime(2004, 2, 3), PhoneNumber = "+77019876543" },
    new Profile { Age = 27, Name = "Bob", SurName = "Carter", NickName = "BobbyX", DateBirthday = new DateTime(1998, 11, 22), PhoneNumber = "+77015554433" },
    new Profile { Age = 30, Name = "Arthur", SurName = "Kingsley", NickName = "ArtLegend", DateBirthday = new DateTime(1995, 7, 9), PhoneNumber = "+77017778899" },
    new Profile { Age = 19, Name = "Kuka", SurName = "Tanaka", NickName = "KukaChan", DateBirthday = new DateTime(2006, 1, 28), PhoneNumber = "+77013332211" }
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

//using(DbUser db = new DbUser())
//{
//    await db.AddRangeAsync(users);
//    db.SaveChanges();
//}

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

    user.Profile!.PhoneNumber = userJsonResult.NumberPhone;
    user.Profile!.Name = userJsonResult.Name;
    user.Profile!.SurName = userJsonResult.SurName;
    user.Profile!.NickName = userJsonResult.NickName;
    user.Profile!.DateBirthday = userJsonResult.BirthdayDate;

    await dbUser.SaveChangesAsync();

    return Results.Ok("Дaнные о пользователе были изменены");
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
                        BirthdayDate = s.Profile.DateBirthday,
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
    user.Profile.DateBirthday = userJsonResult.BirthdayDate;
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

            var existingGentre = db.Gentre.FirstOrDefault(g => g.NameGentre == gDto.NameGentre);
        
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

                db.Gentre.Add(newGentre);
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


app.MapDelete("/deleteBook/{id:int}", async (int id, DbBook db) =>
{
    var book = await db.BooKParametrs.FirstOrDefaultAsync(w => w.PK_BookParametrsId == id);

    if (book == null)
    {
        return Results.NotFound(new { message = "Пользователь не найден" });
    }

    if (!string.IsNullOrEmpty(book.PdfPathBook) && System.IO.File.Exists(book.PdfPathBook))
    {
        System.IO.File.Delete(book.PdfPathBook);
    }

    if (!string.IsNullOrEmpty(book.EpubPathBook) && System.IO.File.Exists(book.EpubPathBook))
    {
        System.IO.File.Delete(book.EpubPathBook);
    }

    if (!string.IsNullOrEmpty(book.ImagePathBook) && System.IO.File.Exists(book.ImagePathBook))
    {
        System.IO.File.Delete(book.ImagePathBook);
    }
    
    db.BooKParametrs.Remove(book);

    await db.SaveChangesAsync();

    return Results.NoContent();
});

// ------------------ Узнать, сколько скачиваний осталось ------------------
app.MapGet("/api/downloads-left", async (HttpContext context, DbUser db) =>
{
    var helper = app.Services.GetService<HelperForUser>();

    var user = context.Items["User"] as User;
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
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        role = user.Role!.ToString(),
        limit = user.Role == Role.Admin ? -1 : limit,
        used = user.Role == Role.Admin ? 0 : user.DownloadToday,
        left = user.Role == Role.Admin ? -1 : Math.Max(0, limit - user.DownloadToday)
    });
});

// ------------------ Проверка пользователя по роли ------------------
app.MapGet("/api/current-user", (HttpContext context) =>
{
    var user = context.Items["User"] as User;

    if (user == null)
    {
        return Results.Ok(new
        {
            isAuth = false,
            role = "Guest",
            name = "Гость",
            email = ""
        });
    }

    return Results.Ok(new
    {
        isAuth = true,
        role = user.Role.ToString(),
        name = user.Profile!.Name,
        email = user.UserLogin?.EmailUser
    });
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
    HttpContext context,
    DbUser dbUser,
    DbBook db,
    int id,
    string type) =>
{
    var book = await db.BooKParametrs
        .FirstOrDefaultAsync(b => b.PK_BookParametrsId == id);

    if (book == null)
    {
        return Results.NotFound(new
        {
            message = "Книга не найдена"
        });
    }

    string filePath;

    // =========================
    // Выбор Windows пути
    // =========================
    switch (type.ToLower())
    {
        case "pdf":

            filePath = book.PdfPathBook!;

            break;

        case "epub":

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

    // =========================
    // Проверка Windows файла
    // =========================
    if (!File.Exists(filePath))
    {
        return Results.NotFound(new
        {
            message = "Файл не найден на сервере"
        });
    }

    var helper = app.Services.GetService<HelperForUser>();

    // =========================
    // Проверка лимитов
    // =========================
    var user = context.Items["User"] as User;

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

    // =========================
    // Отправка файла
    // =========================
    var extension = Path.GetExtension(filePath);

    var mimeType = helper.GetMimeType(filePath);

    var safeFileName = $"{book.NameBook}{extension}";

    var fileBytes = await File.ReadAllBytesAsync(filePath);

    return Results.File(
        fileBytes,
        mimeType,
        safeFileName
    );
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
app.MapPost("/api/authorize", async (HttpContext context, DbUser db) =>
{
    try
    {
        var person = await context.Request.ReadFromJsonAsync<UserAuthorize>();

        if (person == null)
            return Results.BadRequest("Данные не получены");

        if (string.IsNullOrWhiteSpace(person.Email) ||
            string.IsNullOrWhiteSpace(person.Password) ||
            string.IsNullOrWhiteSpace(person.Repeat_Password))
        {
            return Results.BadRequest("Все поля обязательны");
        }

        if (person.Password != person.Repeat_Password)
            return Results.BadRequest("Пароли не совпадают");

        if (person.Password.Length < 8)
            return Results.BadRequest("Пароль должен состоять из более 8 символов");

        var exists = await db.UserLogin
            .AnyAsync(u => u.EmailUser == person.Email);

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
            EmailUser = person.Email,
            PasswordUser = person.Password,
            FK_UserId = user.PK_UserId
        };

        await db.UserLogin.AddAsync(userLogin);
        await db.SaveChangesAsync();

        context.Session.SetInt32("userId", user.PK_UserId);
        System.Console.WriteLine("Пользователь добавлен в базу");

        return Results.Ok(new { message = "Пользователь успешно добавлен" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка регистрации: {ex.Message}");
        return Results.BadRequest($"Ошибка при регистрации: {ex.Message}");
    }
});

// ------------------ Фильтры книг ------------------
app.MapGet("/getBook_filter", async (DbBook db) =>
{
    var publishing = await db.PublishingHouse
        .Select(s => new {NamePublishing = s.NamePublishing})
        .ToListAsync();

    var authorBooks = await db.AuthorBook
        .Select(a => new { Id = a.PK_AuthorBookId, FullName = a.NameAuthor + " " + a.SurnameAuthor })
        .ToListAsync();

    var gentre = await db.Gentre
        .Select(g => new { Id = g.PK_Gentre, NameGentre = g.NameGentre })
        .ToListAsync();

    return Results.Ok(new { authorBooks, publishing, gentre });
});

// ------------------ Фильтр книг по категориям ------------------
app.MapPost("/category_book", async(DbBook db, HttpContext context) =>
{
    try
    {
        var bookfilter = await context.Request.ReadFromJsonAsync<FilterBook>();

        var query = db.BooKParametrs
                    .Include(a => a.AuthorBooks)
                    .Include(g => g.Gentres)
                    .Include(b => b.PublishingHouse)
                    .AsQueryable();

        if(!string.IsNullOrEmpty(bookfilter!.Author) && bookfilter.Author != "-")
        {
            var author = bookfilter.Author.Trim();

            query = query.Where(b =>
                (b.AuthorBooks!.NameAuthor + " " + b.AuthorBooks.SurnameAuthor) == bookfilter.Author);
        }

        if(!string.IsNullOrEmpty(bookfilter!.Publishing_House) && bookfilter.Publishing_House != "-")
        {
            var author = bookfilter.Publishing_House.Trim();

            query = query.Where(b =>
                b.PublishingHouse!.NamePublishing == bookfilter.Publishing_House);
        }

        if(!string.IsNullOrEmpty(bookfilter!.Gentres) && bookfilter.Gentres != "-")
        {
            query = query.Where(g => g.Gentres!.Any(b => b.NameGentre == bookfilter.Gentres));
        }

        var getBook = await query.Select(b => new
        {
            BookId = b.PK_BookParametrsId,
            Title = b.NameBook,
            Author = $"{b.AuthorBooks!.NameAuthor} {b.AuthorBooks.SurnameAuthor}",
            Gentre = b.Gentres!.Select(g => new { g.NameGentre }),
            Img = b.ImagePathBook
        }).ToListAsync();

        return Results.Ok(new { getBook });
    }
    
    catch(Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
        return Results.BadRequest(new {error = "Ошибка при фильтрации книги"});
    }
});

app.MapPost("/api/logout", (HttpContext context) =>
{
    context.Session.Remove("userId");
    context.Session.Clear(); // можно полностью

    return Results.Ok(new { message = "logout" });
});

app.Run();