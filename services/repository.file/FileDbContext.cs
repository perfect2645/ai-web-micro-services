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

            var builder = modelBuilder.Entity<UploadedItem>();
            // Because GUID is not sequential by default, we set the primary key as non-clustered to optimize performance.
            builder.HasKey(entity => entity.Id).IsClustered(false);
            builder.Property(entity => entity.FileName).IsRequired().HasMaxLength(512);
            builder.Property(entity => entity.FileHash).IsRequired().HasMaxLength(64);
            builder.HasIndex(entity => new { entity.FileHash, entity.FileSize }).IsUnique();
        }
    }
}
