using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ProjectsContext : DbContext
    {
        public ProjectsContext()
        {
        }

        public ProjectsContext(DbContextOptions<ProjectsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AllProjectDataTop> AllProjectDataTop { get; set; }
        public virtual DbSet<AllProjectsData> AllProjectsData { get; set; }
        public virtual DbSet<AllProjectsView> AllProjectsView { get; set; }
        public virtual DbSet<ChangeLog> ChangeLog { get; set; }
        public virtual DbSet<DelayedProjects> DelayedProjects { get; set; }
        public virtual DbSet<HoldStatus> HoldStatus { get; set; }
        public virtual DbSet<JUnk> JUnk { get; set; }
        public virtual DbSet<MarkedPriority> MarkedPriority { get; set; }
        public virtual DbSet<PriorityProjects> PriorityProjects { get; set; }
        public virtual DbSet<ProjectSpecSheet> ProjectSpecSheet { get; set; }
        public virtual DbSet<ProjectSpecSheetTest> ProjectSpecSheetTest { get; set; }
        public virtual DbSet<ProjectStartedTablet> ProjectStartedTablet { get; set; }
        public virtual DbSet<ProjectStartedTool> ProjectStartedTool { get; set; }
        public virtual DbSet<ProjectsToClear> ProjectsToClear { get; set; }
        public virtual DbSet<ReserveProjectNumber> ReserveProjectNumber { get; set; }
        public virtual DbSet<SpecialReport> SpecialReport { get; set; }
        public virtual DbSet<SubmitDrawingNumber> SubmitDrawingNumber { get; set; }
        public virtual DbSet<TabletCheckedBy> TabletCheckedBy { get; set; }
        public virtual DbSet<TabletDrawnBy> TabletDrawnBy { get; set; }
        public virtual DbSet<TabletProjectDifficultyTimeData> TabletProjectDifficultyTimeData { get; set; }
        public virtual DbSet<TabletProjects> TabletProjects { get; set; }
        public virtual DbSet<TabletProjectsCompleted> TabletProjectsCompleted { get; set; }
        public virtual DbSet<TabletProjectsHistory> TabletProjectsHistory { get; set; }
        public virtual DbSet<TabletProjectsIn> TabletProjectsIn { get; set; }
        public virtual DbSet<TabletProjectsSummaryView> TabletProjectsSummaryView { get; set; }
        public virtual DbSet<TabletProjectsThisMonth> TabletProjectsThisMonth { get; set; }
        public virtual DbSet<TabletProjectsThisWeek> TabletProjectsThisWeek { get; set; }
        public virtual DbSet<TabletProjectsToday> TabletProjectsToday { get; set; }
        public virtual DbSet<TabletSubmittedBy> TabletSubmittedBy { get; set; }
        public virtual DbSet<Test> Test { get; set; }
        public virtual DbSet<ToolCheckedBy> ToolCheckedBy { get; set; }
        public virtual DbSet<ToolDrawnBy> ToolDrawnBy { get; set; }
        public virtual DbSet<ToolProjectDifficultyTimeData> ToolProjectDifficultyTimeData { get; set; }
        public virtual DbSet<ToolProjects> ToolProjects { get; set; }
        public virtual DbSet<ToolProjectsCompleted> ToolProjectsCompleted { get; set; }
        public virtual DbSet<ToolProjectsHistory> ToolProjectsHistory { get; set; }
        public virtual DbSet<ToolProjectsIn> ToolProjectsIn { get; set; }
        public virtual DbSet<ToolProjectsSummaryView> ToolProjectsSummaryView { get; set; }
        public virtual DbSet<ToolProjectsThisMonth> ToolProjectsThisMonth { get; set; }
        public virtual DbSet<ToolProjectsThisWeek> ToolProjectsThisWeek { get; set; }
        public virtual DbSet<ToolProjectsToday> ToolProjectsToday { get; set; }
        public virtual DbSet<ToolSubmittedBy> ToolSubmittedBy { get; set; }
        public virtual DbSet<EngineeringProjects> EngineeringProjects { get; set; }
        public virtual DbSet<EngineeringTabletProjects> EngineeringTabletProjects { get; set; }
        public virtual DbSet<EngineeringToolProjects> EngineeringToolProjects { get; set; }
        public virtual DbSet<EngineeringArchivedProjects> EngineeringArchivedProjects { get; set; }
        public virtual DbSet<OrdersAndProjectsReport> OrdersAndProjectsReport { get; set; }
        public virtual DbSet<OrdersReport> OrdersReport { get; set; }
        public virtual DbSet<ProjectsReport> ProjectsReport { get; set; }
        public virtual DbSet<TabletProjectsReport> TabletProjectsReport { get; set; }
        public virtual DbSet<TabletProjectsReportStartEnd> TabletProjectsReportStartEnd { get; set; }
        public virtual DbSet<ToolProjectsReport> ToolProjectsReport { get; set; }
        public virtual DbSet<ToolProjectsReportStartEnd> ToolProjectsReportStartEnd { get; set; }
        public virtual DbSet<ToolProjectsCheckedReport> ToolProjectsCheckedReport { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=" + App.Server + ";Database=Projects;Persist Security Info=" + App.PersistSecurityInfo + ";User ID=" + App.UserID + ";Password=" + App.Password + ";");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AllProjectDataTop>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("AllProjectDataTop");

                entity.Property(e => e.Attention)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("Customer Name")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerNumber)
                    .IsRequired()
                    .HasColumnName("Customer Number")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnName("Date Created")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DrawingNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due Date")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.EndUser)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LowerHobNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MachName)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.MachineNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MiscNotes)
                    .HasColumnName("Misc Notes")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Proj).HasColumnName("Proj #");

                entity.Property(e => e.TabletCheckedBy)
                    .HasColumnName("Tablet Checked By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletCompletionDate)
                    .HasColumnName("Tablet Completion Date")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ToolCheckedBy)
                    .HasColumnName("Tool Checked By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolCompletionDate)
                    .HasColumnName("Tool Completion Date")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpperHobNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AllProjectsData>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("AllProjectsData");

                entity.Property(e => e.Attention)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("Customer Name")
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerNumber)
                    .HasColumnName("Customer Number")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnName("Date Created")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DrawingNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due Date")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.EndUser)
                    .HasColumnName("End User")
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.LowerHobNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MachName)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.MachineNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MiscNotes)
                    .HasColumnName("Misc Notes")
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Proj).HasColumnName("Proj #");

                entity.Property(e => e.TabletCheckedBy)
                    .HasColumnName("Tablet Checked By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletCompletionDate)
                    .HasColumnName("Tablet Completion Date")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TabletDrawnBy)
                    .HasColumnName("Tablet Drawn By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletSubmittedBy)
                    .HasColumnName("Tablet Submitted By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletSubmittedDate)
                    .HasColumnName("Tablet Submitted Date")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ToolCheckedBy)
                    .HasColumnName("Tool Checked By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolCompletionDate)
                    .HasColumnName("Tool Completion Date")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ToolDrawnBy)
                    .HasColumnName("Tool Drawn By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpperHobNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AllProjectsView>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevNumber });
            });

            modelBuilder.Entity<ChangeLog>(entity =>
            {
                entity.HasKey(e => e.LogId);

                entity.Property(e => e.LogId).HasColumnName("Log_ID");

                entity.Property(e => e.FieldName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.NewValue)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.OldValue)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryKey1Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryKey1Value)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryKey2Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryKey2Value)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TableName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDateTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedByStation)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DelayedProjects>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Comments)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<HoldStatus>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber });

                entity.Property(e => e.HoldStatus1)
                    .HasColumnName("HoldStatus")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.OnHoldComment)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.RevisionNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TimeSubmitted)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<JUnk>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("jUNK");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("Customer Name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerNumber)
                    .IsRequired()
                    .HasColumnName("Customer Number")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnName("Date Created")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DrawingNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due Date")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.HoldStatus)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MiscNotes)
                    .HasColumnName("Misc Notes")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectNumber).HasColumnName("Project Number");

                entity.Property(e => e.RevNumber).HasColumnName("Rev Number");
            });

            modelBuilder.Entity<MarkedPriority>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.MarkedPriority1)
                    .HasColumnName("MarkedPriority")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TimeSubmitted)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PriorityProjects>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("PriorityProjects");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("Customer Name")
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerNumber)
                    .HasColumnName("Customer Number")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnName("Date Created")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due Date")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LowerHobNumber)
                    .HasColumnName("Lower Hob Number")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Priority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Proj).HasColumnName("Proj #");

                entity.Property(e => e.TabletCheckedBy)
                    .HasColumnName("Tablet Checked By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletDrawnBy)
                    .HasColumnName("Tablet Drawn By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolCheckedBy)
                    .HasColumnName("Tool Checked By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolDrawnBy)
                    .HasColumnName("Tool Drawn By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpperHobNumber)
                    .HasColumnName("Upper Hob Number")
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ProjectSpecSheet>(entity =>
            {
                entity.HasKey(e => e.ProjectsId);

                entity.HasIndex(e => new { e.ProjectsId, e.ProjectNumber })
                    .HasName("IX_ProjectSpecSheet_1")
                    .IsUnique();

                entity.HasIndex(e => new { e.TimeSubmitted, e.ProjectNumber })
                    .HasName("IX_ProjectSpecSheet");

                entity.HasIndex(e => new { e.CustomerNumber, e.ShipToLocation, e.ProjectNumber, e.Tablet, e.Tools })
                    .HasName("IX_ProjectSpecSheet_2");

                entity.Property(e => e.ProjectsId).HasColumnName("ProjectsID");

                entity.Property(e => e.AlignPrint).HasDefaultValueSql("('')");

                entity.Property(e => e.Attention)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Ccwangle)
                    .HasColumnName("CCWAngle")
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.CupDepth).HasDefaultValueSql("('')");

                entity.Property(e => e.CupType)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(65)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.CustomerNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Cwangle)
                    .HasColumnName("CWAngle")
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.DieDiameter).HasDefaultValueSql("('')");

                entity.Property(e => e.DieHeight).HasDefaultValueSql("('')");

                entity.Property(e => e.DieInsert)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.DiePrint).HasDefaultValueSql("('')");

                entity.Property(e => e.DieSteel)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.DrawingNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.DueDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.DwgPicLoc)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.EndUser)
                    .HasMaxLength(65)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.EnglastMod)
                    .HasColumnName("ENGLastMod")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.EstimatedCompletion).HasColumnType("date");

                entity.Property(e => e.FilmCoated)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.HoldStatus)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.InsertState).HasDefaultValueSql("((0))");

                entity.Property(e => e.InternationalId)
                    .HasColumnName("InternationalID")
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Land).HasDefaultValueSql("('')");

                entity.Property(e => e.LowerGroove)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.LowerHobDescription)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.LowerHobNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.LowerKey).HasDefaultValueSql("('')");

                entity.Property(e => e.LowerPrint).HasDefaultValueSql("('')");

                entity.Property(e => e.MachName)
                    .HasMaxLength(45)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MachineNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MachineStyle)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MachineType)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.MiscNotes)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.OnHoldComment)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.OtherShape)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ProjectStartedTablet)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ProjectStartedTool)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.PunchSteel)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.QuoteOrOrderNumber).HasDefaultValueSql("('')");

                entity.Property(e => e.QuoteRevNum).HasDefaultValueSql("('')");

                entity.Property(e => e.RejectKey).HasDefaultValueSql("('')");

                entity.Property(e => e.RejectPrint).HasDefaultValueSql("('')");

                entity.Property(e => e.Shape)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ShipToLocation)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.SpecialMachineInstructions)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.SpecificationId).HasDefaultValueSql("('')");

                entity.Property(e => e.StationId)
                    .HasColumnName("StationID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Tablet).HasDefaultValueSql("('')");

                entity.Property(e => e.TabletCheckedBy)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TabletCompletionDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("('01/01/3000')");

                entity.Property(e => e.TabletDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TabletLength).HasDefaultValueSql("('')");

                entity.Property(e => e.TabletSubmittedBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletSubmittedDate).HasColumnType("datetime");

                entity.Property(e => e.TabletWidth).HasDefaultValueSql("('')");

                entity.Property(e => e.TimeSubmitted)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TipReliefForUpper).HasDefaultValueSql("('')");

                entity.Property(e => e.ToolAssignedTo)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolCheckedBy)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ToolCompletionDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("('01/01/3000')");

                entity.Property(e => e.ToolDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ToolSubmittedBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolSubmittedDate).HasColumnType("datetime");

                entity.Property(e => e.Tools).HasDefaultValueSql("('')");

                entity.Property(e => e.TypeOfKey)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.UpperGroove)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.UpperHobDescription)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.UpperHobNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.UpperKey).HasDefaultValueSql("('')");

                entity.Property(e => e.UpperPrint).HasDefaultValueSql("('')");

                entity.Property(e => e.ReturnToCsr)
                    .HasColumnName("ReturnToCSR")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<ProjectSpecSheetTest>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.AlignPrint)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Attention)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Ccwangle)
                    .HasColumnName("CCWAngle")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CupType)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerNumber)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.Cwangle)
                    .HasColumnName("CWAngle")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DieInsert)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DiePrint)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DieSteel)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DrawingNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.EndUser)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FilmCoated)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.HoldStatus)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.InternationalId)
                    .HasColumnName("InternationalID")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.LowerGroove)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LowerHobDescription)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LowerHobNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LowerKey)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LowerPrint)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MachName)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.MachineNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MachineStyle)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MachineType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MarkedPriority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MiscNotes)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.OtherShape)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectStartedTablet)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectStartedTool)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectsId)
                    .HasColumnName("ProjectsID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.PunchSteel)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.RejectKey)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.RejectPrint)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Shape)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.SpecialMachineInstructions)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Tablet)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletCheckedBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletCompletionDate).HasColumnType("datetime");

                entity.Property(e => e.TabletDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TimeSubmitted).HasColumnType("datetime");

                entity.Property(e => e.TipReliefForUpper)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolCheckedBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolCompletionDate).HasColumnType("datetime");

                entity.Property(e => e.ToolDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Tools)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TypeOfKey)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpperGroove)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UpperHobDescription)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UpperHobNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpperKey)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpperPrint)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ProjectStartedTablet>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber, e.ProjectStartedTablet1 });

                entity.Property(e => e.ProjectStartedTablet1)
                    .HasColumnName("ProjectStartedTablet")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TimeSubmitted).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProjectStartedTool>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber, e.ProjectStartedTool1 });

                entity.Property(e => e.ProjectStartedTool1)
                    .HasColumnName("ProjectStartedTool")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TimeSubmitted).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProjectsToClear>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("ProjectsToClear");

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectNumber).HasColumnName("Project Number");

                entity.Property(e => e.TabletCompletionDate).HasColumnType("datetime");

                entity.Property(e => e.ToolCompletionDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<ReserveProjectNumber>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.DwspecificationId)
                    .HasColumnName("DWSpecificationID")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ReserveProjectNumber1).HasColumnName("ReserveProjectNumber");

                entity.Property(e => e.SubmittedBy)
                    .HasMaxLength(35)
                    .IsUnicode(false);

                entity.Property(e => e.TimeSubmitted).HasColumnType("datetime");
            });

            modelBuilder.Entity<SpecialReport>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("SpecialReport");

                entity.Property(e => e.TabletCompleted)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.TabletDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolCompleted)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ToolDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SubmitDrawingNumber>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.DrawingNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.RevisionNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TabletCheckedBy>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber, e.TabletCheckedBy1 });

                entity.Property(e => e.TabletCheckedBy1)
                    .HasColumnName("TabletCheckedBy")
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.TimeSubmitted)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<TabletDrawnBy>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber, e.TabletDrawnBy1 });

                entity.Property(e => e.TabletDrawnBy1)
                    .HasColumnName("TabletDrawnBy")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TimeSubmitted)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<TabletProjectDifficultyTimeData>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.ActualTime).HasColumnType("time(0)");

                entity.Property(e => e.EstimatedTime).HasColumnType("time(0)");
            });

            modelBuilder.Entity<TabletProjects>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("TabletProjects");

                entity.Property(e => e.Comments)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("Customer Name")
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnName("Date Created")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DaysDelayed).HasColumnName("Days Delayed");

                entity.Property(e => e.DrawnBy)
                    .HasColumnName("Drawn By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due Date")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.EndUser)
                    .HasColumnName("End User")
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.HoldStatus)
                    .HasColumnName("Hold Status")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LowHob)
                    .HasColumnName("Low Hob #")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MiscNotes)
                    .HasColumnName("Misc Notes")
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.Priority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Proj).HasColumnName("Proj #");

                entity.Property(e => e.ProjStarted)
                    .HasColumnName("Proj Started")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.QtyOfProjectsDelaying).HasColumnName("Qty of Projects Delaying");

                entity.Property(e => e.SubmittedBy)
                    .HasColumnName("Submitted By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TimeCreated)
                    .HasColumnName("Time Created")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ToolProject).HasColumnName("Tool Project");

                entity.Property(e => e.UpHob)
                    .HasColumnName("Up Hob #")
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TabletProjectsCompleted>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("TabletProjectsCompleted");

                entity.Property(e => e.DateCompleted)
                    .HasColumnName("Date Completed")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectCompleted).HasColumnName("Project Completed");
            });

            modelBuilder.Entity<TabletProjectsHistory>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("TabletProjectsHistory");

                entity.Property(e => e.CheckedBy)
                    .HasColumnName("Checked By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("Customer Name")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.DateCompleted)
                    .HasColumnName("Date Completed")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnName("Date Created")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DrawnBy)
                    .HasColumnName("Drawn By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due Date")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LowHob)
                    .HasColumnName("Low Hob #")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MiscNotes)
                    .HasColumnName("Misc Notes")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Priority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Proj).HasColumnName("Proj #");

                entity.Property(e => e.ProjStarted)
                    .HasColumnName("Proj Started")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpHob)
                    .HasColumnName("Up Hob #")
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TabletProjectsIn>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("TabletProjectsIn");

                entity.Property(e => e.DateIn)
                    .HasColumnName("Date In")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectCompleted).HasColumnName("Project Completed");
            });

            modelBuilder.Entity<TabletProjectsSummaryView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("TabletProjectsSummaryView");

                entity.Property(e => e.Designer)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TabletProjectsThisMonth).HasColumnName("Tablet Projects This Month");

                entity.Property(e => e.TabletProjectsThisWeek).HasColumnName("Tablet Projects This Week");

                entity.Property(e => e.TabletProjectsToday).HasColumnName("Tablet Projects Today");
            });

            modelBuilder.Entity<TabletProjectsThisMonth>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("TabletProjectsThisMonth");

                entity.Property(e => e.ProjectsThisMonth).HasColumnName("Projects This Month");

                entity.Property(e => e.TabletDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TabletProjectsThisWeek>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("TabletProjectsThisWeek");

                entity.Property(e => e.ProjectsThisWeek).HasColumnName("Projects This Week");

                entity.Property(e => e.TabletDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TabletProjectsToday>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("TabletProjectsToday");

                entity.Property(e => e.ProjectsToday).HasColumnName("Projects Today");

                entity.Property(e => e.TabletDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TabletSubmittedBy>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber, e.TabletSubmittedBy1 });

                entity.Property(e => e.TabletSubmittedBy1)
                    .HasColumnName("TabletSubmittedBy")
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.TimeSubmitted).HasColumnType("datetime");
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Test");

                entity.Property(e => e.CheckedBy)
                    .HasColumnName("Checked By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("Customer Name")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.DateCompleted)
                    .HasColumnName("Date Completed")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnName("Date Created")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DrawnBy)
                    .HasColumnName("Drawn By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due Date")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.HoldStatus)
                    .HasColumnName("Hold Status")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LowHob)
                    .HasColumnName("Low Hob #")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MiscNotes)
                    .HasColumnName("Misc Notes")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Priority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Proj).HasColumnName("Proj #");

                entity.Property(e => e.ProjStarted)
                    .HasColumnName("Proj Started")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Rev).HasColumnName("Rev #");

                entity.Property(e => e.UpHob)
                    .HasColumnName("Up Hob #")
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ToolCheckedBy>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber, e.ToolCheckedBy1 });

                entity.Property(e => e.TimeSubmitted).HasColumnType("datetime");

                entity.Property(e => e.ToolCheckedBy1)
                    .HasColumnName("ToolCheckedBy")
                    .HasMaxLength(10)
                    .IsFixedLength();
            });

            modelBuilder.Entity<ToolDrawnBy>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevisionNumber, e.ToolDrawnBy1 });

                entity.Property(e => e.TimeSubmitted)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ToolDrawnBy1)
                    .HasColumnName("ToolDrawnBy")
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ToolProjectDifficultyTimeData>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.ActualTime).HasColumnType("time(0)");

                entity.Property(e => e.EstimatedTime).HasColumnType("time(0)");
            });

            modelBuilder.Entity<ToolProjects>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("ToolProjects");

                entity.Property(e => e.Comments)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("Customer Name")
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnName("Date Created")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DaysDelayed).HasColumnName("Days Delayed");

                entity.Property(e => e.DrawnBy)
                    .HasColumnName("Drawn By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due Date")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.EndUser)
                    .HasColumnName("End User")
                    .HasMaxLength(65)
                    .IsUnicode(false);

                entity.Property(e => e.HoldStatus)
                    .HasColumnName("Hold Status")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LowHob)
                    .HasColumnName("Low Hob #")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MachineNumber)
                    .HasColumnName("Machine Number")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MiscNotes)
                    .HasColumnName("Misc Notes")
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.Priority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Proj).HasColumnName("Proj #");

                entity.Property(e => e.ProjStarted)
                    .HasColumnName("Proj Started")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.QtyOfProjectsDelaying).HasColumnName("Qty of Projects Delaying");

                entity.Property(e => e.SubmittedBy)
                    .HasColumnName("Submitted By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TimeCreated)
                    .HasColumnName("Time Created")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ToolAssignedTo)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpHob)
                    .HasColumnName("Up Hob #")
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ToolProjectsCompleted>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("ToolProjectsCompleted");

                entity.Property(e => e.DateCompleted)
                    .HasColumnName("Date Completed")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectCompleted).HasColumnName("Project Completed");
            });

            modelBuilder.Entity<ToolProjectsHistory>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("ToolProjectsHistory");

                entity.Property(e => e.CheckedBy)
                    .HasColumnName("Checked By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Csr)
                    .HasColumnName("CSR")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasColumnName("Customer Name")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.DateCompleted)
                    .HasColumnName("Date Completed")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnName("Date Created")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DrawingNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DrawnBy)
                    .HasColumnName("Drawn By")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnName("Due Date")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LowHob)
                    .HasColumnName("Low Hob #")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Mach)
                    .HasColumnName("Mach #")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MiscNotes)
                    .HasColumnName("Misc Notes")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Priority)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Product)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Proj).HasColumnName("Proj #");

                entity.Property(e => e.ProjStarted)
                    .HasColumnName("Proj Started")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpHob)
                    .HasColumnName("Up Hob #")
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ToolProjectsIn>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("ToolProjectsIn");

                entity.Property(e => e.DateIn)
                    .HasColumnName("Date In")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectCompleted).HasColumnName("Project Completed");
            });

            modelBuilder.Entity<ToolProjectsSummaryView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("ToolProjectsSummaryView");

                entity.Property(e => e.Designer)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ToolProjectsThisMonth).HasColumnName("Tool Projects This Month");

                entity.Property(e => e.ToolProjectsThisWeek).HasColumnName("Tool Projects This Week");

                entity.Property(e => e.ToolProjectsToday).HasColumnName("Tool Projects Today");
            });

            modelBuilder.Entity<ToolProjectsThisMonth>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("ToolProjectsThisMonth");

                entity.Property(e => e.ProjectsThisMonth).HasColumnName("Projects This Month");

                entity.Property(e => e.ToolDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ToolProjectsThisWeek>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("ToolProjectsThisWeek");

                entity.Property(e => e.ProjectsThisWeek).HasColumnName("Projects This Week");

                entity.Property(e => e.ToolDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ToolProjectsToday>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("ToolProjectsToday");

                entity.Property(e => e.ProjectsToday).HasColumnName("Projects Today");

                entity.Property(e => e.ToolDrawnBy)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ToolSubmittedBy>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.TimeSubmitted).HasColumnType("datetime");

                entity.Property(e => e.ToolSubmittedBy1)
                    .HasColumnName("ToolSubmittedBy")
                    .HasMaxLength(10)
                    .IsFixedLength();
            });

            modelBuilder.Entity<EngineeringProjects>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevNumber });
            });

            modelBuilder.Entity<EngineeringTabletProjects>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevNumber });
            });

            modelBuilder.Entity<EngineeringToolProjects>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevNumber });
            });

            modelBuilder.Entity<EngineeringArchivedProjects>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevNumber });
            });

            modelBuilder.Entity<OrdersAndProjectsReport>(entity =>
            {
                entity.HasKey(e => e.Employee);
            });

            modelBuilder.Entity<OrdersReport>(entity =>
            {
                entity.HasKey(e => e.Employee);
            });

            modelBuilder.Entity<ProjectsReport>(entity =>
            {
                entity.HasKey(e => e.Employee);
            });

            modelBuilder.Entity<TabletProjectsReport>(entity =>
            {
                entity.HasKey(e => e.Employee);
            });
            modelBuilder.Entity<TabletProjectsReportStartEnd>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevNumber });
            });

            modelBuilder.Entity<ToolProjectsReport>(entity =>
            {
                entity.HasKey(e => e.Employee);
            });
            modelBuilder.Entity<ToolProjectsReportStartEnd>(entity =>
            {
                entity.HasKey(e => new { e.ProjectNumber, e.RevNumber });
            });
            modelBuilder.Entity<ToolProjectsCheckedReport>(entity =>
            {
                entity.HasKey(e => e.ToolCheckedBy);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
