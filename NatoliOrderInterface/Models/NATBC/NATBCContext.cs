using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NatoliOrderInterface.Models
{
    public partial class NATBCContext : DbContext
    {
        public NATBCContext()
        {
        }

        public NATBCContext(DbContextOptions<NATBCContext> options)
            : base(options)
        {
        }

        public virtual DbSet<MoeEmployees> MoeEmployees { get; set; }
        public virtual DbSet<NatoliOrderList> NatoliOrderList { get; set; }
        public virtual DbSet<LineItemLastScan> LineItemLastScan { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=" + App.Server + ";Database=NATBC;Persist Security Info=" + App.PersistSecurityInfo + ";User ID=" + App.UserID + ";Password=" + App.Password + ";");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MoeEmployees>(entity =>
            {
                entity.HasKey(e => e.MoeEmployeeCode);

                entity.ToTable("MOE_Employees");

                entity.Property(e => e.MoeEmployeeCode)
                    .HasColumnName("MOE_EmployeeCode")
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.EmpType)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.MasterScheduleId).HasColumnName("MasterScheduleID");

                entity.Property(e => e.MoeDepartmentCode)
                    .HasColumnName("MOE_DepartmentCode")
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.MoeEmployeeName)
                    .HasColumnName("MOE_EmployeeName")
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.MoeFirstName)
                    .HasColumnName("MOE_FirstName")
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.MoeJobTitleCode)
                    .HasColumnName("MOE_JobTitleCode")
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.MoeLastName)
                    .HasColumnName("MOE_LastName")
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.MoeMiddleInitial)
                    .HasColumnName("MOE_MiddleInitial")
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.PayrollFullName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.ReportsTo)
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.Schedule)
                    .IsRequired()
                    .HasMaxLength(75)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<NatoliOrderList>(entity =>
            {
                entity.HasKey(e => e.OrderNo);

                entity.ToTable("NatoliOrderList");
            });

            modelBuilder.Entity<LineItemLastScan>(entity =>
            {
                entity.HasKey(e => new { e.OrderDetailTypeDescription, e.ScanTimeStamp });
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
