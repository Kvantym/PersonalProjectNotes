using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalProjectNotes.Domain.Entities;
using System;

namespace PersonalProjectNotes.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<ListCart> ListCarts { get; set; }
        public DbSet<ActivityCart> ActivitiesCarts { get; set; }
        public DbSet<ActivityBoard> ActivitiesListBoards { get; set; }
        public DbSet<ActivityListCart> ActivityListCarts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // ПЕРШИМ ДІЛОМ викликаємо base метод для ініціалізації Identity
            base.OnModelCreating(builder);

            // --- Налаштування зв'язків (Relationships) ---

            builder.Entity<Cart>()
                .HasOne(c => c.ListCart)
                .WithMany(l => l.Carts)
                .HasForeignKey(c => c.ListCartId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Board>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ListCart>()
                .HasOne(lc => lc.Board)
                .WithMany(b => b.ListCart)
                .HasForeignKey(lc => lc.BoardId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ActivityBoard>()
                .HasOne(a => a.Board)
                .WithMany(b => b.ActivityBoards)
                .HasForeignKey(a => a.BoardId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ActivityCart>()
                .HasOne(a => a.Cart)
                .WithMany(c => c.ActivityCart)
                .HasForeignKey(a => a.CartId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ActivityListCart>()
                .HasOne(a => a.ListCart)
                .WithMany(lc => lc.ActivityListCarts)
                .HasForeignKey(a => a.ListCartId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // --- Налаштування конвертації GUID для MySQL (char(36)) ---

            var guidConverter = new ValueConverter<Guid, string>(
                v => v.ToString().ToLower(),
                v => Guid.Parse(v)
            );

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    // Отримуємо базовий тип (на випадок Guid?)
                    var underlyingType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

                    if (underlyingType == typeof(Guid))
                    {
                        // 1. Встановлюємо тип колонки
                        property.SetColumnType("char(36)");

                        // 2. Додаємо конвертер (Guid <-> string)
                        property.SetValueConverter(guidConverter);

                        // 3. Автогенерація UUID для первинних ключів
                        if (property.IsPrimaryKey())
                        {
                            property.SetDefaultValueSql("(UUID())");
                        }
                    }
                }
            }
        }
    }
}
