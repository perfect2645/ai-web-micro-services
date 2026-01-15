using Microsoft.EntityFrameworkCore;
using repository.file.Repositories.Entities;

namespace repository.file.Repositories
{
    public class FileDbContext :DbContext
    {

        public DbSet<UploadedItem> UploadedItems { get; set; }
        public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UploadedItem>(entity =>
            {
                // Because GUID is not sequential by default, we set the primary key as non-clustered to optimize performance.
                entity.HasKey(entity => entity.Id).IsClustered(false);
                entity.Property(entity => entity.FileName).IsRequired().HasMaxLength(512);
                entity.Property(entity => entity.FileHash).IsRequired().HasMaxLength(64);
                entity.HasIndex(entity => new { entity.FileHash, entity.FileSize }).IsUnique();

                entity.Property(x => x.FileSize).IsRequired();
                entity.Property(x => x.FileHash).IsRequired();
                entity.Property(x => x.BackupUrl).IsRequired();
                entity.Property(x => x.RemoteUrl).IsRequired();
                entity.Property(x => x.CreateTime).IsRequired();
            });
        }
    }
}
