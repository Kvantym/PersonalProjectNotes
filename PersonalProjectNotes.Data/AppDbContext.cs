using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

            // Cart -> ListCart (один до багатьох)
            builder.Entity<Cart>()
                .HasOne(c => c.ListCart)
                .WithMany(l => l.Carts)
                .HasForeignKey(c => c.ListCartId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // каскадне видалення

            // Cart -> User (один до багатьох)
            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // користувач не видаляється разом із картами

            // Board -> User (один до багатьох)
            builder.Entity<Board>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // ListCart -> Board (один до багатьох)
            builder.Entity<ListCart>()
                .HasOne(lc => lc.Board)
                .WithMany(b => b.ListCart)
                .HasForeignKey(lc => lc.BoardId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // каскадне видалення

            // ActivityBoard -> Board
            builder.Entity<ActivityBoard>()
                .HasOne(a => a.Board)
                .WithMany(b => b.ActivityBoards)
                .HasForeignKey(a => a.BoardId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // каскадне видалення

            // ActivityCart -> Cart
            builder.Entity<ActivityCart>()
                .HasOne(a => a.Cart)
                .WithMany(c => c.ActivityCart)
                .HasForeignKey(a => a.CartId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // каскадне видалення

            // ActivityListCart -> ListCart
            builder.Entity<ActivityListCart>()
                .HasOne(a => a.ListCart)
                .WithMany(lc => lc.ActivityListCarts)
                .HasForeignKey(a => a.ListCartId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // каскадне видалення

                foreach (var entityType in builder.Model.GetEntityTypes())
    {
        foreach (var property in entityType.GetProperties())
        {
            if (property.ClrType == typeof(Guid) || property.ClrType == typeof(Guid?))
            {
                property.SetColumnType("char(36)");
            }
        }
    }
        }
    }
}
