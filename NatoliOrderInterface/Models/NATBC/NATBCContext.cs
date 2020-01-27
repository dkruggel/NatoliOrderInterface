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
        public virtual DbSet<TravellerScansAudit> TravellerScansAudit { get; set; }

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

            modelBuilder.Entity<TravellerScansAudit>(entity =>
            {
                entity.HasKey(e => e.TsaId);

                entity.HasIndex(e => e.DepartmentCode)
                    .HasName("ByDepartment");

                entity.HasIndex(e => e.MoeCardCode)
                    .HasName("ByMOECardCode");

                entity.HasIndex(e => e.OffNcmincidentNo)
                    .HasName("ByOffNCMIncidentNo");

                entity.HasIndex(e => e.OnNcmincidentNo)
                    .HasName("ByOnNCMIncidentNo");

                entity.HasIndex(e => e.OrderNumber)
                    .HasName("By_OrderNumber");

                entity.HasIndex(e => e.ScanTimeStamp)
                    .HasName("IX_TravellerScanAudit_ScanTime");

                entity.HasIndex(e => e.TravellerNo)
                    .HasName("TSA_TravellerNo");

                entity.HasIndex(e => new { e.PairingId, e.MoeCardCode })
                    .HasName("ByMOECardCodePairingID");

                entity.HasIndex(e => new { e.TravellerNo, e.DeletedFlag })
                    .HasName("ByTravellerNo_DeletedFlag");

                entity.HasIndex(e => new { e.MoeCardCode, e.CurrentGoal, e.NextLevelGoal })
                    .HasName("ByCardCode_CurrentGoal_NextLevelGoal");

                entity.HasIndex(e => new { e.OrderNumber, e.OrderDetailTypeId, e.ScanTimeStamp })
                    .HasName("IX_TravellerScansAudit");

                entity.HasIndex(e => new { e.EmployeeCode, e.DepartmentCode, e.MachineCode, e.OperationCode })
                    .HasName("ByEDMO");

                entity.Property(e => e.TsaId).HasColumnName("TSA_ID");

                entity.Property(e => e.BatchId).HasColumnName("BatchID");

                entity.Property(e => e.CurrentGoal).HasColumnName("Current_Goal");

                entity.Property(e => e.DepartmentCode)
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.DepartmentDesc)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeCode)
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeType)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.FpiPairingId).HasColumnName("FPI_PairingID");

                entity.Property(e => e.FpiPassed).HasColumnName("FPI_Passed");

                entity.Property(e => e.FpiTimestamp)
                    .HasColumnName("FPI_Timestamp")
                    .HasColumnType("datetime");

                entity.Property(e => e.LeadEmployeeCode)
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.LeadEmployeeName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.MachineCode)
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.MachineDesc)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.ManagerEmployeeCode)
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.ManagerEmployeeName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.MoeCardCode)
                    .HasColumnName("MOE_CardCode")
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.NextLevelGoal).HasColumnName("NextLevel_Goal");

                entity.Property(e => e.NtuserId)
                    .IsRequired()
                    .HasColumnName("NTUserID")
                    .HasMaxLength(35)
                    .IsUnicode(false);

                entity.Property(e => e.OffAdcsyncDateTime)
                    .HasColumnName("OffADCSyncDateTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.OffAdcsyncFlag).HasColumnName("OffADCSyncFlag");

                entity.Property(e => e.OffNcmincidentNo).HasColumnName("OffNCMIncidentNo");

                entity.Property(e => e.OffScrapReasonCode)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.OffScrapReasonDesc)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.OnAdcsyncDateTime)
                    .HasColumnName("OnADCSyncDateTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.OnAdcsyncFlag).HasColumnName("OnADCSyncFlag");

                entity.Property(e => e.OnNcmincidentNo).HasColumnName("OnNCMIncidentNo");

                entity.Property(e => e.OnScrapReasonCode)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.OnScrapReasonDesc)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.OperationCode)
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.OperationDesc)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.OrderDetailTypeDescription)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.OrderDetailTypeId)
                    .HasColumnName("OrderDetailTypeID")
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.PairingId).HasColumnName("PairingID");

                entity.Property(e => e.PayrollDate).HasColumnType("date");

                entity.Property(e => e.PayrollShiftName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.ReworkCode)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ReworkDesc)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.ScanOff).HasColumnType("datetime");

                entity.Property(e => e.ScanOffDate).HasColumnType("date");

                entity.Property(e => e.ScanOn).HasColumnType("datetime");

                entity.Property(e => e.ScanOnDate).HasColumnType("date");

                entity.Property(e => e.ScanTimeStamp).HasColumnType("datetime");

                entity.Property(e => e.SupervisorEmployeeCode)
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.SupervisorEmployeeName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.TerminalId)
                    .IsRequired()
                    .HasColumnName("TerminalID")
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionType)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.TravellerNo)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.WorkOrderNumber)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.WorkOrderToDisplay)
                    .IsRequired()
                    .HasMaxLength(75)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
