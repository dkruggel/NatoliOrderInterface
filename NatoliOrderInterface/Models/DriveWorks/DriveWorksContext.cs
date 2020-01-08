using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NatoliOrderInterface.Models.DriveWorks
{
    public partial class DriveWorksContext : DbContext
    {
        public DriveWorksContext()
        {
        }

        public DriveWorksContext(DbContextOptions<DriveWorksContext> options)
            : base(options)
        {
        }

        public virtual DbSet<QueueView> QueueView { get; set; }
        public virtual DbSet<Specifications> Specifications { get; set; }
        public virtual DbSet<SecurityUsers> SecurityUsers { get; set; }
        //public virtual DbSet<UserCustomizations> UserCustomizations { get; set; }
        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=" + App.Server + ";Database=Driveworks;Persist Security Info=" + App.PersistSecurityInfo + ";User ID=" + App.UserID + ";Password=" + App.Password + ";");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueueView>(entity =>
            {
                entity.HasKey(e => e.TargetName);

                entity.ToView("QueueView");

                entity.Property(e => e.DateReleased).HasColumnType("datetime");

                entity.Property(e => e.Tags).HasMaxLength(4000);

                entity.Property(e => e.TargetName).HasMaxLength(259);
            });

            modelBuilder.Entity<Specifications>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .HasName("IX_DW_S_ID");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateEdited).HasColumnType("datetime");

                entity.Property(e => e.Directory)
                    .IsRequired()
                    .HasMaxLength(259);

                entity.Property(e => e.MetadataDirectory).HasMaxLength(259);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(259);

                entity.Property(e => e.OriginalProjectExtension)
                    .IsRequired()
                    .HasMaxLength(259);

                entity.Property(e => e.OriginalProjectName)
                    .IsRequired()
                    .HasMaxLength(259);

                entity.Property(e => e.SpecificationProjectExtension)
                    .IsRequired()
                    .HasMaxLength(259);

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(259);

                entity.Property(e => e.Tags).HasMaxLength(4000);
            });

            modelBuilder.Entity<SecurityUsers>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ProviderName).HasMaxLength(2000);

                entity.Property(e => e.PrincipalId).HasMaxLength(2000);

                entity.Property(e => e.PrincipalToken).HasMaxLength(2000);

                entity.Property(e => e.DisplayName).HasMaxLength(259);

                entity.Property(e => e.EmailAddress).HasMaxLength(255);
            });

            //modelBuilder.Entity<UserCustomizations>(entity =>
            //{
            //    entity.HasKey(e => e.User);
            //});
            
            //OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
