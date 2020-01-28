using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NatoliOrderInterface.NewModels
{
    public partial class NAT02Context : DbContext
    {
        public NAT02Context()
        {
        }

        public NAT02Context(DbContextOptions<NAT02Context> options)
            : base(options)
        {
        }

        public virtual DbSet<EoiTrackedDocuments> EoiTrackedDocuments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=NSQL05;Database=NAT02;Persist Security Info=True;User ID=BarcodeUser;Password=PrivateKey(0);");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EoiTrackedDocuments>(entity =>
            {
                entity.ToTable("EOI_TrackedDocuments");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.MovementId).HasColumnName("Movement_ID");

                entity.Property(e => e.Number)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Timestamp)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
