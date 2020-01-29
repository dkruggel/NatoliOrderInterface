using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;


namespace NatoliOrderInterface.Models.DriveWorks
{
    public partial class Driveworks_USRContext : DbContext
    {
        public Driveworks_USRContext()
        {
        }

        public Driveworks_USRContext(DbContextOptions<Driveworks_USRContext> options)
            : base(options)
        {
        }

        public virtual DbSet<UserCustomizations> UserCustomizations { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server="+App.Server+";Database=DriveWorks;Persist Security Info=True;User ID="+App.UserID+";Password="+App.Password+"");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("USR");
            modelBuilder.Entity<UserCustomizations>(entity =>
            {
                entity.HasKey(e => e.User);
            });
            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
