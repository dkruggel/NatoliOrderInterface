using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NatoliOrderInterface.NewModels
{
    public partial class NAT01Context : DbContext
    {
        public NAT01Context()
        {
        }

        public NAT01Context(DbContextOptions<NAT01Context> options)
            : base(options)
        {
        }

        public virtual DbSet<CupConfig> CupConfig { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=NSQL05;Initial Catalog=NAT01;Persist Security Info=True; User ID=BarcodeUser;Password=PrivateKey(0)");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CupConfig>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => e.CupId)
                    .HasName("CupID")
                    .IsUnique();

                entity.Property(e => e.CupId).HasColumnName("CupID");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Tm2id)
                    .IsRequired()
                    .HasColumnName("TM2ID")
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
