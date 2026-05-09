using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbContextUser
{
    enum Role
    {
        Guest,
        User,
        Admin
    }

    class UserLogin
    {
        public int PK_UserLoginId {get; set;}
        public string? EmailUser {get; set;}
        public string? PasswordUser {get; set;}

        public int FK_UserId {get; set;}
        public User User {get; set;} = null!;
    }

    class User
    {
        public int PK_UserId {get; set;}
        public int DownloadToday { get; set; }
        public DateTime LastDownloadDate { get; set; }

        public int CustomLimit { get; set; } = 100;

        public Role? Role {get; set;}
        public UserLogin? UserLogin {get; set;}

        public Profile? Profile {get; set;}
    }

    class Profile
    {
        public int Age {get; set;}
        public string? PhoneNumber {get; set;}
        public string? Name {get; set;}
        public string? SurName {get; set;}
        public string? NickName {get; set;}
        public DateTime? DateBirthday {get; set;}
    }

    class DbUser : DbContext
    {

        public DbSet<UserLogin> UserLogin {get; set;} = null!;
        public DbSet<User> User {get; set;} = null!;

        public DbUser(DbContextOptions<DbUser> db) : base(db)
        {
        }

        public DbUser()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        // конфиг для настройки БД
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite("Data Source=C:\\Users\\Limbo\\Desktop\\Diplom Web-Site — копия\\DB\\User\\DataBaseUser.db");

        // конфиг дял настрйоки параметров в БД, таких как Id, Name и так далее
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserLogin>(UserLobginModel);
            modelBuilder.Entity<User>(UserModel);

            base.OnModelCreating(modelBuilder);
        }

        private void UserModel(EntityTypeBuilder<User> BuilderUser)
        {
            BuilderUser.HasKey(u => u.PK_UserId);

            BuilderUser.HasOne(u => u.UserLogin)
                       .WithOne(w => w.User)
                       .HasForeignKey<UserLogin>(fk => fk.FK_UserId);

            BuilderUser.OwnsOne(p => p.Profile);
        }

        private void UserLobginModel(EntityTypeBuilder<UserLogin> BuilderUserLogin)
        {
            BuilderUserLogin.HasKey(userlogin => userlogin.PK_UserLoginId);
            BuilderUserLogin.HasIndex(userlogin => userlogin.EmailUser).IsUnique();
        }
    }
}