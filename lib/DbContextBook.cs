using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbContextBook
{
     class BooKParametrs
    {
        public int PK_BookParametrsId {get; set;}
        public string? NameBook {get; set;}
        public string? PdfPathBook { get; set; }
        public string? EpubPathBook { get; set; }
        public string ImagePathBook { get; set; } = null!;
        public string? Description {get; set;}

        public int FK_AuthorId {get; set;}
        public AuthorBook? AuthorBooks {get; set;}

        public int FK_PublishingId {get; set;}
        public PublishingHouse? PublishingHouse {get; set;}
        
        public List<Gentre>? Gentres {get; set;}
    }

    class AuthorBook
    {
        public int PK_AuthorBookId {get; set;}
        public string? NameAuthor {get; set;}
        public string? SurnameAuthor {get; set;}
        public string? PatronimycAuthor {get; set;}
        public List<BooKParametrs>? BooKParametrs {get; set;}
    }

    class Gentre
    {
        public int PK_Gentre {get; set;}
        public string? NameGentre {get; set;}
        public List<BooKParametrs>? BooKParametrs {get; set;}
    }

    class PublishingHouse
    {
        public int Id {get; set;}
        public string? NamePublishing {get; set;}
        public List<BooKParametrs>? BooKParametrs {get ;set;}
    }

    class DbBook : DbContext
    {
        public DbSet<AuthorBook> AuthorBook {get; set;} = null!;
        public DbSet<BooKParametrs> BooKParametrs {get; set;} = null!;
        public DbSet<Gentre> Gentre {get; set;} = null!;
        public DbSet<PublishingHouse> PublishingHouse {get; set;} = null!;

        public DbBook(DbContextOptions<DbBook> db) : base(db)
        {
        }

        public DbBook()
        {
            // Database.EnsureDeleted();
            // Database.EnsureCreated();
        }

        // конфиг дял настрйоки параметров в БД, таких как Id, Name и так далее
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BooKParametrs>(BooKParametrsModel);
            modelBuilder.Entity<AuthorBook>(AuthorBookModel);
            modelBuilder.Entity<Gentre>(GentreModel);

            base.OnModelCreating(modelBuilder);
        }

        private void AuthorBookModel(EntityTypeBuilder<AuthorBook> BuilderAuthorBook)
        {
            BuilderAuthorBook.HasKey(builderauthorbook => builderauthorbook.PK_AuthorBookId);
        }

        private void BooKParametrsModel(EntityTypeBuilder<BooKParametrs> BuilderBooKParametrs)
        {
            BuilderBooKParametrs.HasKey(booKparametrs => booKparametrs.PK_BookParametrsId);

            BuilderBooKParametrs.HasOne(b => b.AuthorBooks)
                                .WithMany(w => w.BooKParametrs)
                                .HasForeignKey(fk => fk.FK_AuthorId);

            BuilderBooKParametrs.HasOne(b => b.PublishingHouse)
                                .WithMany(w => w.BooKParametrs)
                                .HasForeignKey(fk => fk.FK_PublishingId);

            BuilderBooKParametrs.HasMany(g => g.Gentres)
                                .WithMany(b => b.BooKParametrs);
        }

        private void GentreModel(EntityTypeBuilder<Gentre> BuilderGentre)
        {
            BuilderGentre.HasKey(gentre => gentre.PK_Gentre);
        }
    }
}