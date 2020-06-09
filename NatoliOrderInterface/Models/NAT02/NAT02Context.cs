using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NatoliOrderInterface.Models
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

        public virtual DbSet<EoiAllTabletProjectsView> EoiAllTabletProjectsView { get; set; }
        public virtual DbSet<EoiAllToolProjectsView> EoiAllToolProjectsView { get; set; }
        public virtual DbSet<DwAutoRun> DwAutoRun { get; set; }
        public virtual DbSet<EoiBasePriceList> EoiBasePriceList { get; set; }
        public virtual DbSet<EoiMissingAutomationVariablesView> EoiMissingAutomationVariablesView { get; set; }
        public virtual DbSet<EoiNotificationsActive> EoiNotificationsActive { get; set; }
        public virtual DbSet<EoiNotificationsArchived> EoiNotificationsArchived { get; set; }
        public virtual DbSet<EoiNotificationsViewed> EoiNotificationsViewed { get; set; }
        public virtual DbSet<EoiOrdersBeingChecked> EoiOrdersBeingChecked { get; set; }
        public virtual DbSet<EoiOrdersBeingEnteredView> EoiOrdersBeingEnteredView { get; set; }
        public virtual DbSet<EoiOrdersCheckedBy> EoiOrdersCheckedBy { get; set; }
        public virtual DbSet<EoiOrdersDoNotProcess> EoiOrdersDoNotProcess { get; set; }
        public virtual DbSet<EoiOrdersEnteredAndUnscannedView> EoiOrdersEnteredAndUnscannedView { get; set; }
        public virtual DbSet<EoiOrdersInEngineeringUnprintedView> EoiOrdersInEngineeringUnprintedView { get; set; }
        public virtual DbSet<EoiOrdersInOfficeView> EoiOrdersInOfficeView { get; set; }
        public virtual DbSet<EoiOrdersMarkedForChecking> EoiOrdersMarkedForChecking { get; set; }
        public virtual DbSet<EoiOrdersPrintedInEngineeringView> EoiOrdersPrintedInEngineeringView { get; set; }
        public virtual DbSet<EoiOrdersReadyToPrintView> EoiOrdersReadyToPrintView { get; set; }
        public virtual DbSet<EoiProjectsFinished> EoiProjectsFinished { get; set; }
        public virtual DbSet<EoiProjectsOnHold> EoiProjectsOnHold { get; set; }
        public virtual DbSet<EoiQuotesMarkedForConversion> EoiQuotesMarkedForConversion { get; set; }
        public virtual DbSet<EoiQuotesMarkedForConversionView> EoiQuotesMarkedForConversionView { get; set; }
        public virtual DbSet<EoiQuotesNotConvertedView> EoiQuotesNotConvertedView { get; set; }
        public virtual DbSet<EoiSettings> EoiSettings { get; set; }
        public virtual DbSet<EoiTabletProjectsDrawn> EoiTabletProjectsDrawn { get; set; }
        public virtual DbSet<EoiTabletProjectsNotStarted> EoiTabletProjectsNotStarted { get; set; }
        public virtual DbSet<EoiTabletProjectsStarted> EoiTabletProjectsStarted { get; set; }
        public virtual DbSet<EoiTabletProjectsSubmitted> EoiTabletProjectsSubmitted { get; set; }
        public virtual DbSet<EoiToolProjectsDrawn> EoiToolProjectsDrawn { get; set; }
        public virtual DbSet<EoiToolProjectsNotStarted> EoiToolProjectsNotStarted { get; set; }
        public virtual DbSet<EoiToolProjectsStarted> EoiToolProjectsStarted { get; set; }
        public virtual DbSet<MaMachineVariables> MaMachineVariables { get; set; }
        public virtual DbSet<EoiQuoteScratchPad> EoiQuoteScratchPad { get; set; }
        public virtual DbSet<EoiQuotesOneWeekCompleted> EoiQuotesOneWeekCompleted { get; set; }
        public virtual DbSet<EoiQuoteSMICheck> EoiQuoteSMICheck { get; set; }
        public virtual DbSet<EoiOrderEntryInstructions> EoiOrderEntryInstructions { get; set; }
        public virtual DbSet<EoiTrackedDocuments> EoiTrackedDocuments { get; set; }
        public virtual DbSet<MultiTipSketchInformation> MultiTipSketchInformation { get; set; }
        public virtual DbSet<PartAllocation> PartAllocation { get; set; }
        public virtual DbSet<VwQuoteConversion> VwQuoteConversion { get; set; }
        public virtual DbSet<QuotePercentage> QuotePercentage { get; set; }
        public virtual DbSet<EoiAllOrdersView> EoiAllOrdersView { get; set; }
        public virtual DbSet<EoiCustomerNotes> EoiCustomerNotes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=" + App.Server + ";Database=NAT02;Persist Security Info=" + App.PersistSecurityInfo + ";User ID=" + App.UserID + ";Password=" + App.Password + ";");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EoiAllOrdersView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("EOI_AllOrdersView");

                entity.Property(e => e.CheckedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(52)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.PaidRushFee)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ProcessState)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.RushYorN)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Tm2).HasColumnName("TM2");

                entity.Property(e => e.TransitionName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                
            });

            modelBuilder.Entity<EoiAllTabletProjectsView>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber });

                entity.ToTable("EOI_AllTabletProjectsView");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectStartedTablet)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletSubmittedBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ReturnToCsr)
                    .HasColumnName("ReturnToCSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiAllToolProjectsView>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber });

                entity.ToTable("EOI_AllToolProjectsView");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectStartedTool)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ReturnToCsr)
                    .HasColumnName("ReturnToCSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DwAutoRun>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("DW_AutoRun");

                entity.Property(e => e.ProcessState)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.SpecificationId)
                    .HasColumnName("SpecificationID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.TransitionName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });
            
            modelBuilder.Entity<EoiBasePriceList>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("EOI_BasePriceList");

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.InternationalYorN)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.MachineType)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.PunchType)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Shape)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SteelPriceCode)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<EoiMissingAutomationVariablesView>(entity =>
            {
                entity.HasKey(e => e.OrderNumber);

                entity.ToView("EOI_MissingAutomationVariablesView");

                entity.Property(e => e.OrderNumber)
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiNotificationsActive>(entity =>
            {
                entity.ToTable("EOI_Notifications_Active");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Number)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiNotificationsArchived>(entity =>
            {
                entity.ToTable("EOI_Notifications_Archived");

                entity.HasKey(e => e.NotificationId);

                entity.Property(e => e.NotificationId).HasColumnName("NotificationID");

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Number)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiNotificationsViewed>(entity =>
            {
                entity.ToTable("EOI_Notifications_Viewed");

                entity.HasKey(e => e.NotificationId);

                entity.Property(e => e.NotificationId).HasColumnName("NotificationID");

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Number)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiOrdersBeingChecked>(entity =>
            {
                entity.HasKey(e => new { e.OrderNo, e.User });

                entity.ToTable("EOI_OrdersBeingChecked");

                entity.Property(e => e.User)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiOrdersBeingEnteredView>(entity =>
            {
                entity.HasKey(e => e.OrderNo);

                entity.ToView("EOI_OrdersBeingEnteredView");

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(64)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiOrdersCheckedBy>(entity =>
            {
                entity.HasKey(e => e.OrderNo);

                entity.ToTable("EOI_OrdersCheckedBy");

                entity.Property(e => e.CheckedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiOrdersDoNotProcess>(entity =>
            {
                entity.HasKey(e => e.OrderNo);

                entity.ToTable("EOI_OrdersDoNotProcess");

                entity.Property(e => e.UserName)
                    .HasMaxLength(75)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiOrdersEnteredAndUnscannedView>(entity =>
            {
                entity.HasKey(e => e.OrderNo);

                entity.ToView("EOI_OrdersEnteredAndUnscannedView");

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(64)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiOrdersInEngineeringUnprintedView>(entity =>
            {
                entity.HasKey(e => e.OrderNo);

                entity.ToView("EOI_OrdersInEngineeringUnprintedView");

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeName)
                    .HasMaxLength(75)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiOrdersInOfficeView>(entity =>
            {
                entity.HasKey(e => e.OrderNo);

                entity.ToView("EOI_OrdersInOfficeView");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(52)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeName)
                    .HasMaxLength(75)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiOrdersMarkedForChecking>(entity =>
            {
                entity.HasKey(e => e.OrderNo);

                entity.ToTable("EOI_OrdersMarkedForChecking");
            });

            modelBuilder.Entity<EoiOrdersPrintedInEngineeringView>(entity =>
            {
                entity.HasKey(e => e.OrderNo);

                entity.ToView("EOI_OrdersPrintedInEngineeringView");

                entity.Property(e => e.CheckedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeName)
                    .HasMaxLength(75)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiOrdersReadyToPrintView>(entity =>
            {
                entity.HasKey(e => e.OrderNo);

                entity.ToView("EOI_OrdersReadyToPrintView");

                entity.Property(e => e.CheckedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.DepartmentDesc)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeName)
                    .HasMaxLength(75)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiProjectsFinished>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber });

                entity.ToView("EOI_ProjectsFinished");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiProjectsOnHold>(entity =>
            {
                entity.HasKey(e=>new { e.ProjectNumber, e.RevisionNumber });

                entity.ToView("EOI_ProjectsOnHold");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiQuotesMarkedForConversion>(entity =>
            {
                entity.HasKey(e => new { e.QuoteNo, e.QuoteRevNo });

                entity.ToTable("EOI_QuotesMarkedForConversion");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(52)
                    .IsUnicode(false);

                entity.Property(e => e.CsrMarked)
                    .HasColumnName("CSR_Marked")
                    .HasMaxLength(52)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Rush)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.TimeSubmitted)
                    .HasColumnName("Time_Submitted")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<EoiQuotesMarkedForConversionView>(entity =>
            {
                entity.HasKey(e => new { e.QuoteNo, e.QuoteRevNo });

                entity.ToTable("EOI_QuotesMarkedForConversionView");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(52)
                    .IsUnicode(false);

                entity.Property(e => e.CsrMarked)
                    .HasColumnName("CSR_Marked")
                    .HasMaxLength(52)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Rush)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.TimeSubmitted)
                    .HasColumnName("Time_Submitted")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<EoiQuotesNotConvertedView>(entity =>
            {
                entity.HasKey(e => new { e.QuoteNo, e.QuoteRevNo });

                entity.ToView("EOI_QuotesNotConvertedView");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(52)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.RushYorN)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<EoiQuotesOneWeekCompleted>(entity =>
            {
                entity.HasKey(e => new { e.QuoteNo, e.QuoteRevNo });

                entity.ToView("EOI_QuotesOneWeekCompleted");
            });

            modelBuilder.Entity<EoiSettings>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);

                entity.ToView("EOI_Settings");
            });

            modelBuilder.Entity<EoiTrackedDocuments>(entity =>
            {
                entity.ToTable("EOI_TrackedDocuments");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.MovementId).HasColumnName("Movement_ID");

                entity.Property(e => e.Number)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiTabletProjectsDrawn>(entity =>
            {
                entity.HasKey(e => e.ProjectNumber);

                entity.ToView("EOI_TabletProjectsDrawn");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectStartedTablet)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiTabletProjectsNotStarted>(entity =>
            {
                entity.HasKey(e => e.ProjectNumber);

                entity.ToView("EOI_TabletProjectsNotStarted");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiTabletProjectsStarted>(entity =>
            {
                entity.HasKey(e => e.ProjectNumber);

                entity.ToView("EOI_TabletProjectsStarted");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectStartedTablet)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiTabletProjectsSubmitted>(entity =>
            {
                entity.HasKey(e => e.ProjectNumber);

                entity.ToView("EOI_TabletProjectsSubmitted");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectStartedTablet)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletSubmittedBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiToolProjectsDrawn>(entity =>
            {
                entity.HasKey(e => e.ProjectNumber);

                entity.ToView("EOI_ToolProjectsDrawn");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectStartedTool)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiToolProjectsNotStarted>(entity =>
            {
                entity.HasKey(e => e.ProjectNumber);

                entity.ToView("EOI_ToolProjectsNotStarted");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TabletCheckedBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EoiToolProjectsStarted>(entity =>
            {
                entity.HasKey(e => e.ProjectNumber);

                entity.ToView("EOI_ToolProjectsStarted");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate).HasColumnType("date");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectStartedTool)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MaMachineVariables>(entity =>
            {
                entity.HasKey(e => new { e.WorkOrderNumber, e.LineNumber });

                entity.ToTable("MA_MachineVariables");

                entity.Property(e => e.ChamferAngleHead).HasColumnType("decimal(11, 8)");

                entity.Property(e => e.ChamferAngleTip).HasColumnType("decimal(11, 8)");

                entity.Property(e => e.ChamferLength).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.ChamferLengthHead).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.ColletSize).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.CutBarrelDiameter).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.CutTipDiameter).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.DomeHeightTopAngle).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.FinishBarrelDiameter).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.GrooveProgram).HasColumnType("decimal(5, 0)");

                entity.Property(e => e.GrooveTipReliefYn)
                    .HasColumnName("GrooveTipReliefYN")
                    .HasColumnType("decimal(1, 0)");

                entity.Property(e => e.HeadFlat).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.HeadGaugeNumber)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.HeadOd)
                    .HasColumnName("HeadOD")
                    .HasColumnType("decimal(10, 8)");

                entity.Property(e => e.HeadRadius).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.HeadThickness).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.HeadType).HasColumnType("decimal(1, 0)");

                entity.Property(e => e.HoldLowBarrelYn).HasColumnName("HoldLowBarrelYN");

                entity.Property(e => e.KeywayLength).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.KeywayPosition).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.KeywayYn).HasColumnName("KeywayYN");

                entity.Property(e => e.LineType)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.MultiTipSketchId).HasColumnName("MultiTipSketchID");

                entity.Property(e => e.NeckAngle).HasColumnType("decimal(11, 8)");

                entity.Property(e => e.NeckDiameter).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.NeckLength).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.NeckRadius).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.NeckRadiusAtBarrel).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.NominalTipDiameter).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.OverallLength).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.ProbeCupDepth).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.SteelType).HasColumnType("decimal(5, 0)");

                entity.Property(e => e.StockDiameter).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.TipAngle).HasColumnType("decimal(11, 8)");

                entity.Property(e => e.TipRadius).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.TipReliefDiameter).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.TipReliefYn)
                    .HasColumnName("TipReliefYN")
                    .HasColumnType("decimal(1, 0)");

                entity.Property(e => e.TipStraight).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.TipWidth).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.UndercutDepth).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.UndercutYn)
                    .HasColumnName("UndercutYN")
                    .HasColumnType("decimal(1, 0)");

                entity.Property(e => e.WaycenterYn)
                    .HasColumnName("WaycenterYN")
                    .HasColumnType("decimal(1, 0)");

                entity.Property(e => e.WorkOrderNumber)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.WorkingLength).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.WorkingLengthMax).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.WorkingLengthMin).HasColumnType("decimal(10, 8)");
            });

            modelBuilder.Entity<EoiQuoteScratchPad>(entity =>
            {
                entity.HasKey(e => new { e.QuoteNo, e.RevNo, e.LineNo, e.LineType });

                entity.ToTable("EOI_QuoteScratchPad");
            });

            modelBuilder.Entity<EoiQuoteSMICheck>(entity =>
            {
                entity.HasKey(e => new { e.QuoteNo, e.RevNo, e.CustomerID, e.Sequence });

                entity.ToTable("EOI_QuoteSMICheck");
            });

            modelBuilder.Entity<EoiOrderEntryInstructions>(entity =>
            {
                entity.HasKey(e => new { e.QuoteNo, e.RevNo, e.TimeEntered });

                entity.ToTable("EOI_OrderEntryInstructions");
            });

            modelBuilder.Entity<MultiTipSketchInformation>(entity =>
            {
                entity.HasKey(e => new { e.ID });
            });

            modelBuilder.Entity<PartAllocation>(entity =>
            {
                entity.HasKey(e => new { e.QuoteNumber, e.QuoteRevNo, });
                entity.Property(e => e.EnteredDate).HasColumnType("date");
                entity.Property(e => e.ShipDate).HasColumnType("date");
            });

            modelBuilder.Entity<VwQuoteConversion>(entity =>
            {
                entity.ToView("vw_QuoteConversion");

                entity.HasKey(e => e.Rep);
            });

            modelBuilder.Entity<QuotePercentage>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<EoiCustomerNotes>(entity =>
            {
                entity.HasKey(e => new { e.ID });

                entity.ToTable("EOI_CustomerNotes");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
