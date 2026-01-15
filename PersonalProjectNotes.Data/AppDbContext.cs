using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // Додано для конвертера
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
// 1. Створюємо конвертер
var guidConverter = new ValueConverter<Guid, string>(
    v => v.ToString().ToLower(),
    v => Guid.Parse(v)
);

// 2. Явно перераховуємо ваші сутності
var entities = new[] { 
    typeof(Board), typeof(Cart), typeof(ListCart), 
    typeof(ActivityCart), typeof(ActivityBoard), typeof(ActivityListCart),
    typeof(ApplicationUser), typeof(ApplicationRole) 
};

foreach (var type in entities)
{
    var mutableEntityType = builder.Entity(type);
    
    foreach (var property in mutableEntityType.Metadata.GetProperties())
    {
        var underlyingType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

        if (underlyingType == typeof(Guid))
        {
            property.SetColumnType("char(36)");
            property.SetValueConverter(guidConverter);

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
