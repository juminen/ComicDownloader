using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace ComicDownloader.Repo
{
    class ComicContext : DbContext
    {
        public ComicContext(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new ArgumentException($"File '{filePath}' does not exists.");
            }
            else
            {
                databasePath = filePath;
            }
        }

        private string databasePath;
        public DbSet<ComicDto> Comics { get; set; }
        public DbSet<ComicPhotoDto> Photos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            string connectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = databasePath
            }
            .ToString();

            SqliteConnection connection = new SqliteConnection(connectionString);
            optionsBuilder.UseSqlite(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region comic
            modelBuilder.Entity<ComicDto>()
                .ToTable(TableNames.ComicTable.Name);

            modelBuilder.Entity<ComicDto>()
                .Property(x => x.UniqueIdentifier)
                .HasColumnName(TableNames.ComicTable.Columns.Guid)
                .HasConversion<string>();

            modelBuilder.Entity<ComicDto>()
                .HasKey(x => x.UniqueIdentifier);

            modelBuilder.Entity<ComicDto>()
                .Property(x => x.Name)
                .HasColumnName(TableNames.ComicTable.Columns.Name);

            modelBuilder.Entity<ComicDto>()
                .Property(x => x.StartUrl)
                .HasColumnName(TableNames.ComicTable.Columns.Url);

            modelBuilder.Entity<ComicDto>()
                .Property(x => x.LastDownloadDate)
                .HasColumnName(TableNames.ComicTable.Columns.LastDownloadDate);

            modelBuilder.Entity<ComicDto>()
                .Property(x => x.SavingLocation)
                .HasColumnName(TableNames.ComicTable.Columns.SavingLocation);
            #endregion

            #region Photo
            modelBuilder.Entity<ComicPhotoDto>()
                .ToTable(TableNames.ComicPhotoTable.Name);

            modelBuilder.Entity<ComicPhotoDto>()
                .Property(x => x.UniqueIdentifier)
                .HasColumnName(TableNames.ComicPhotoTable.Columns.Guid)
                .HasConversion<string>();

            modelBuilder.Entity<ComicPhotoDto>()
                .HasKey(x => x.UniqueIdentifier);

            modelBuilder.Entity<ComicPhotoDto>()
                .Property(x => x.ReleaseDate)
                .HasColumnName(TableNames.ComicPhotoTable.Columns.ReleaseDate);

            modelBuilder.Entity<ComicPhotoDto>()
                .Property(x => x.DownloadDate)
                .HasColumnName(TableNames.ComicPhotoTable.Columns.DownloadDate);

            modelBuilder.Entity<ComicPhotoDto>()
                .Property(x => x.Status)
                .HasColumnName(TableNames.ComicPhotoTable.Columns.Status);

            modelBuilder.Entity<ComicPhotoDto>()
                .Property(x => x.FilePath)
                .HasColumnName(TableNames.ComicPhotoTable.Columns.FilePath);

            modelBuilder.Entity<ComicPhotoDto>()
                .Property(x => x.Url)
                .HasColumnName(TableNames.ComicPhotoTable.Columns.Url);

            modelBuilder.Entity<ComicPhotoDto>()
                .Property(x => x.ComicGuid)
                .HasColumnName(TableNames.ComicPhotoTable.Columns.ComicGuid)
                .HasConversion<string>();

            modelBuilder.Entity<ComicPhotoDto>()
                .HasOne(photo => photo.Comic)
                .WithMany(comic => comic.Photos)
                .HasForeignKey(photo => photo.ComicGuid)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
