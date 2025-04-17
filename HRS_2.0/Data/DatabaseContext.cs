
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using HRS_2.Data;
namespace HRS_2.Models.Domain
{
    public class DatabaseContext : DbContext // Inherit from DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("TeKarConn") // 
                              .LogTo(Console.WriteLine, LogLevel.Information); // Log SQL queries
            }
        }

      
        public DbSet<User> User { get; set; }
        

        public DbSet<v_stafflist> v_stafflist { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>()
               .HasKey(u => u.NoPekerja);

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired();

       
            modelBuilder.Entity<v_stafflist>().HasNoKey().ToView("v_stafflist");

        }

    }
}
