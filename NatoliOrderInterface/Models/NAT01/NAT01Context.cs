using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NatoliOrderInterface.Models.NAT01
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


        public virtual DbSet<CustomerInstructionTable> CustomerInstructionTable { get; set; }
        public virtual DbSet<HobList> HobList { get; set; }
        public virtual DbSet<MachineList> MachineList { get; set; }
        public virtual DbSet<OedetailType> OedetailType { get; set; }
        public virtual DbSet<OptionsList> OptionsList { get; set; }
        public virtual DbSet<OrderDetailOptions> OrderDetailOptions { get; set; }
        public virtual DbSet<OrderDetails> OrderDetails { get; set; }
        public virtual DbSet<OrderHeader> OrderHeader { get; set; }
        public virtual DbSet<OrdOptionValueASingleNum> OrdOptionValueASingleNum { get; set; }
        public virtual DbSet<OrdOptionValueBDoubleNum> OrdOptionValueBDoubleNum { get; set; }
        public virtual DbSet<OrdOptionValueCTolerance> OrdOptionValueCTolerance { get; set; }
        public virtual DbSet<OrdOptionValueDDegreeVal> OrdOptionValueDDegreeVal { get; set; }
        public virtual DbSet<OrdOptionValueESmallText> OrdOptionValueESmallText { get; set; }
        public virtual DbSet<OrdOptionValueFLargeText> OrdOptionValueFLargeText { get; set; }
        public virtual DbSet<OrdOptionValueGDegrees> OrdOptionValueGDegrees { get; set; }
        public virtual DbSet<OrdOptionValueHHardness> OrdOptionValueHHardness { get; set; }
        public virtual DbSet<OrdOptionValueIHardness2> OrdOptionValueIHardness2 { get; set; }
        public virtual DbSet<OrdOptionValueJOptionMult> OrdOptionValueJOptionMult { get; set; }
        public virtual DbSet<OrdOptionValueKVendor> OrdOptionValueKVendor { get; set; }
        public virtual DbSet<OrdOptionValueLSurfaceTreat> OrdOptionValueLSurfaceTreat { get; set; }
        public virtual DbSet<OrdOptionValueMScrew> OrdOptionValueMScrew { get; set; }
        public virtual DbSet<OrdOptionValueNColor> OrdOptionValueNColor { get; set; }
        public virtual DbSet<OrdOptionValueOInteger> OrdOptionValueOInteger { get; set; }
        public virtual DbSet<OrdOptionValuePDegDec> OrdOptionValuePDegDec { get; set; }
        public virtual DbSet<OrdOptionValueQDimensions> OrdOptionValueQDimensions { get; set; }
        public virtual DbSet<OrdOptionValueRIntegerText> OrdOptionValueRIntegerText { get; set; }
        public virtual DbSet<OrdOptionValueSText> OrdOptionValueSText { get; set; }
        public virtual DbSet<OrdOptionValueTDecText> OrdOptionValueTDecText { get; set; }
        public virtual DbSet<QuoteDetailOptions> QuoteDetailOptions { get; set; }
        public virtual DbSet<QuoteDetails> QuoteDetails { get; set; }
        public virtual DbSet<QuoteFreightDesc> QuoteFreightDesc { get; set; }
        public virtual DbSet<QuoteHeader> QuoteHeader { get; set; }
        public virtual DbSet<QuoteOptionValueASingleNum> QuoteOptionValueASingleNum { get; set; }
        public virtual DbSet<QuoteOptionValueBDoubleNum> QuoteOptionValueBDoubleNum { get; set; }
        public virtual DbSet<QuoteOptionValueCTolerance> QuoteOptionValueCTolerance { get; set; }
        public virtual DbSet<QuoteOptionValueDDegreeVal> QuoteOptionValueDDegreeVal { get; set; }
        public virtual DbSet<QuoteOptionValueESmallText> QuoteOptionValueESmallText { get; set; }
        public virtual DbSet<QuoteOptionValueFLargeText> QuoteOptionValueFLargeText { get; set; }
        public virtual DbSet<QuoteOptionValueGDegrees> QuoteOptionValueGDegrees { get; set; }
        public virtual DbSet<QuoteOptionValueHHardness> QuoteOptionValueHHardness { get; set; }
        public virtual DbSet<QuoteOptionValueIHardness2> QuoteOptionValueIHardness2 { get; set; }
        public virtual DbSet<QuoteOptionValueJOptionMult> QuoteOptionValueJOptionMult { get; set; }
        public virtual DbSet<QuoteOptionValueKVendor> QuoteOptionValueKVendor { get; set; }
        public virtual DbSet<QuoteOptionValueLSurfaceTreat> QuoteOptionValueLSurfaceTreat { get; set; }
        public virtual DbSet<QuoteOptionValueMScrew> QuoteOptionValueMScrew { get; set; }
        public virtual DbSet<QuoteOptionValueNColor> QuoteOptionValueNColor { get; set; }
        public virtual DbSet<QuoteOptionValueOInteger> QuoteOptionValueOInteger { get; set; }
        public virtual DbSet<QuoteOptionValuePDegDec> QuoteOptionValuePDegDec { get; set; }
        public virtual DbSet<QuoteOptionValueQDimensions> QuoteOptionValueQDimensions { get; set; }
        public virtual DbSet<QuoteOptionValueRIntegerTxt> QuoteOptionValueRIntegerTxt { get; set; }
        public virtual DbSet<QuoteOptionValueSSmallText> QuoteOptionValueSSmallText { get; set; }
        public virtual DbSet<QuoteOptionValueTDecText> QuoteOptionValueTDecText { get; set; }
        public virtual DbSet<QuoteRepresentative> QuoteRepresentative { get; set; }
        public virtual DbSet<SteelType> SteelType { get; set; }
        public virtual DbSet<ShapeFields> ShapeFields { get; set; }
        public virtual DbSet<DieList> DieList { get; set; }
        public virtual DbSet<CupConfig> CupConfig { get; set; }
        public virtual DbSet<CustomerMachines> CustomerMachines { get; set; }
        public virtual DbSet<Keys> Keys { get; set; }

        public virtual DbSet<BisectCodes> BisectCodes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder != null)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseSqlServer("Server="+App.Server+";Database=NAT01;Persist Security Info="+App.PersistSecurityInfo+";User ID="+App.UserID+";Password="+App.Password+";");
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                modelBuilder.Entity<CustomerInstructionTable>(entity =>
                {
                    entity.HasNoKey();

                    entity.HasIndex(e => new { e.CustomerId, e.Category, e.Sequence })
                        .HasName("CustCategory");

                    entity.HasIndex(e => new { e.CustomerId, e.Selected, e.Sequence })
                        .HasName("CustSelect");

                    entity.HasIndex(e => new { e.CustomerId, e.Smiapplied, e.Sequence })
                        .HasName("SMIApplied");

                    entity.HasIndex(e => new { e.Inactive, e.CustomerId, e.Sequence })
                        .HasName("CustInactive")
                        .IsUnique();

                    entity.Property(e => e.Category)
                        .IsRequired()
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.CustomerId)
                        .IsRequired()
                        .HasColumnName("CustomerID")
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DateStamp).HasColumnType("datetime");

                    entity.Property(e => e.EffectiveDate).HasColumnType("datetime");

                    entity.Property(e => e.Instruction)
                        .IsRequired()
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Smiapplied).HasColumnName("SMIApplied");

                    entity.Property(e => e.TimeStamp)
                        .IsRequired()
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.UserStamp)
                        .IsRequired()
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<HobList>(entity =>
                {
                    entity.HasNoKey();

                    entity.HasIndex(e => e.Nnumber)
                        .HasName("ByNNumber");

                    entity.HasIndex(e => e.Shape)
                        .HasName("ByShape");

                    entity.HasIndex(e => e.Size)
                        .HasName("BySize");

                    entity.HasIndex(e => e.TipQty)
                        .HasName("ByTipQty");

                    entity.HasIndex(e => new { e.HobNo, e.TipQty, e.BoreCircle })
                        .HasName("ByHobNo")
                        .IsUnique();

                    entity.HasIndex(e => new { e.DieId, e.HobNo, e.TipQty, e.BoreCircle })
                        .HasName("ByDieNo");

                    entity.Property(e => e.BisectCode)
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.BisectedCode)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.BoreCircle).HasDefaultValueSql("((0))");

                    entity.Property(e => e.Class)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.CopperYorN)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.CupDepthM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.CupRadius)
                        .HasColumnType("numeric(12, 4)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.CupRadiusM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.DateDesigned).HasColumnType("datetime");

                    entity.Property(e => e.DieId)
                        .HasColumnName("DieID")
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DrawingType)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DrawingYorN)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Embossed1)
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Embossed2)
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Embossing)
                        .HasMaxLength(40)
                        .IsUnicode(false);

                    entity.Property(e => e.Flush).HasDefaultValueSql("((1))");

                    entity.Property(e => e.HobNo)
                        .IsRequired()
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.HobYorNorD)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.LandBlendedYorN)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.LandM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.LandRangeM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.MeasurableCd).HasColumnName("MeasurableCD");

                    entity.Property(e => e.MeasurableCdm)
                        .HasColumnName("MeasurableCDM")
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.Nnumber).HasColumnName("NNumber");

                    entity.Property(e => e.Note1)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Note2)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Note3)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OwnerReservedFor)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Shape)
                        .IsRequired()
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShapeCode)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Size)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.TempDate)
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<MachineList>(entity =>
                {
                    entity.HasNoKey();

                    entity.HasIndex(e => e.Description)
                        .HasName("ByDescription");

                    entity.HasIndex(e => e.MachineNo)
                        .HasName("ByMachineNo")
                        .IsUnique();

                    entity.HasIndex(e => e.MachineTypePrCode)
                        .HasName("ByMachineType");

                    entity.Property(e => e.Ddiameter).HasColumnName("DDiameter");

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Dweight).HasColumnName("DWeight");

                    entity.Property(e => e.Ldiameter).HasColumnName("LDiameter");

                    entity.Property(e => e.LowerSize)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Lweight).HasColumnName("LWeight");

                    entity.Property(e => e.MachineTypePrCode)
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Od).HasColumnName("OD");

                    entity.Property(e => e.Ol).HasColumnName("OL");

                    entity.Property(e => e.SpecialInfo)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Udiameter).HasColumnName("UDiameter");

                    entity.Property(e => e.UpperSize)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Uweight).HasColumnName("UWeight");
                });

                modelBuilder.Entity<OedetailType>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OEDetailType");

                    entity.HasIndex(e => e.TypeId)
                        .HasName("ByTypeID")
                        .IsUnique()
                        .IsClustered();

                    entity.HasIndex(e => new { e.ParentTypeId, e.Sequence })
                        .HasName("ByParentIDType_Sequence");

                    entity.Property(e => e.CommodityCode)
                        .HasMaxLength(12)
                        .IsUnicode(false);

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(23)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.EndDate).HasColumnType("date");

                    entity.Property(e => e.ParentTypeId)
                        .HasColumnName("ParentTypeID")
                        .HasMaxLength(5)
                        .IsUnicode(false);

                    entity.Property(e => e.PartsPerManhourReportCategory)
                        .HasMaxLength(20)
                        .IsUnicode(false);

                    entity.Property(e => e.ShippingReportCategory)
                        .HasMaxLength(10)
                        .IsUnicode(false);

                    entity.Property(e => e.ShortDesc)
                        .HasMaxLength(20)
                        .IsUnicode(false);

                    entity.Property(e => e.StartDate).HasColumnType("date");

                    entity.Property(e => e.TypeId)
                        .HasColumnName("TypeID")
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OptionsList>(entity =>
                {
                    entity.HasNoKey();

                    entity.HasIndex(e => e.OptionCode)
                        .HasName("OptionCode")
                        .IsUnique();

                    entity.HasIndex(e => e.OptionDescription)
                        .HasName("OptionDescription");

                    entity.Property(e => e.Color)
                        .IsRequired()
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Edocs).HasColumnName("EDocs");

                    entity.Property(e => e.Label)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Large)
                        .IsRequired()
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionDescription)
                        .IsRequired()
                        .HasMaxLength(45)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionMultiplier)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionType)
                        .IsRequired()
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Pmisc).HasColumnName("PMisc");

                    entity.Property(e => e.Screw)
                        .IsRequired()
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Small)
                        .IsRequired()
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SurfaceTreatment)
                        .IsRequired()
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Tm2dataCd).HasColumnName("TM2DataCD");

                    entity.Property(e => e.ValueType)
                        .IsRequired()
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Vendor)
                        .IsRequired()
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Zdocs).HasColumnName("ZDocs");
                });

                modelBuilder.Entity<OrdOptionValueASingleNum>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueA_SingleNum");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("AOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.Number1Mm).HasColumnName("Number1MM");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueBDoubleNum>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueB_DoubleNum");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("BOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.Number1Mm).HasColumnName("Number1MM");

                    entity.Property(e => e.Number2Mm).HasColumnName("Number2MM");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text1)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text2)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueCTolerance>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueC_Tolerance");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("COrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.BottomMm).HasColumnName("BottomMM");

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.TopMm).HasColumnName("TopMM");
                });

                modelBuilder.Entity<OrdOptionValueDDegreeVal>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueD_DegreeVal");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("DOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ValueMm).HasColumnName("ValueMM");
                });

                modelBuilder.Entity<OrdOptionValueESmallText>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueE_SmallText");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("EOptionLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SmallText)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueFLargeText>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueF_LargeText");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("FOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.LargeText)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueGDegrees>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueG_Degrees");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("GOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueHHardness>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueH_Hardness");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("HOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueIHardness2>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueI_Hardness2");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("IOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueJOptionMult>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueJ_OptionMult");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("JOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionMult)
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueKVendor>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueK_Vendor");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("KOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.VendorId)
                        .HasColumnName("VendorID")
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueLSurfaceTreat>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueL_SurfaceTreat");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("LOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SurfaceTreatment)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.VendorId)
                        .HasColumnName("VendorID")
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueMScrew>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueM_Screw");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("MOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Screw)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueNColor>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueN_Color");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("NOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.Color)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueOInteger>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueO_Integer");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("OOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValuePDegDec>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueP_DegDec");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("POrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueQDimensions>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueQ_Dimensions");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("QOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.DepthMm).HasColumnName("DepthMM");

                    entity.Property(e => e.LengthMm).HasColumnName("LengthMM");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.WidthMm).HasColumnName("WidthMM");
                });

                modelBuilder.Entity<OrdOptionValueRIntegerText>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueR_IntegerText");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("ROrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueSText>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueS_Text");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("SOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrdOptionValueTDecText>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("OrdOptionValueT_DecText");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDetailType, e.OptionCode })
                        .HasName("TOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrderDetailOptions>(entity =>
                {
                    entity.HasNoKey();

                    entity.HasIndex(e => e.OptionCode)
                        .HasName("OptionCode");

                    entity.HasIndex(e => new { e.OptionCode, e.OrderNumber })
                        .HasName("CodeOrderReverse");

                    entity.HasIndex(e => new { e.OrderNumber, e.OrderDetailLineNo, e.OptionCode })
                        .HasName("WOLineOptCode")
                        .IsUnique();

                    entity.HasIndex(e => new { e.OrderNumber, e.OrderDetailLineNo, e.OptionLineNo })
                        .HasName("WOLineOptLineNo")
                        .IsUnique();

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionComments)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionText)
                        .HasMaxLength(45)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionType)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<OrderDetails>(entity =>
                {
                    entity.HasKey(e => new { e.OrderNo, e.LineNumber, e.RecordId});

                    entity.HasIndex(e => e.HobNoShapeId)
                        .HasName("ByHobNoShapeID");

                    entity.HasIndex(e => e.SteelId)
                        .HasName("BySteel_ID");

                    entity.HasIndex(e => e.TravellerNo)
                        .HasName("ByTravellerNo");

                    entity.HasIndex(e => new { e.OrderNo, e.DetailTypeId })
                        .HasName("ReverseType");

                    entity.HasIndex(e => new { e.OrderNo, e.LineNumber })
                        .HasName("OrderNo");

                    entity.HasIndex(e => new { e.OrderNo, e.Sequence })
                        .HasName("Sequence");

                    entity.HasIndex(e => new { e.OrderNo, e.DetailTypeId, e.HobNoShapeId })
                        .HasName("ByOrderNo_DetailTypeID_HobNoShapeID");

                    entity.HasIndex(e => new { e.PrintStatus, e.OrderNo, e.DetailTypeId })
                        .HasName("WorkOrderPrint");

                    entity.HasIndex(e => new { e.PrintStatus, e.SheetColor, e.OrderNo, e.DetailTypeId })
                        .HasName("PrintOrder");

                    entity.Property(e => e.CommodityCode)
                        .HasMaxLength(12)
                        .IsUnicode(false);

                    entity.Property(e => e.CompletedYn)
                        .HasColumnName("CompletedYN")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc1)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc2)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc3)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc4)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc5)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc6)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc7)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc8)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DetailTypeId)
                        .HasColumnName("DetailTypeID")
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DieShapeId).HasColumnName("DieShapeID");

                    entity.Property(e => e.HobNoShapeId)
                        .HasColumnName("HobNoShapeID")
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.HoldFlag)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.MachinePriceCode)
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PriceOptionsAdded)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintStatus)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.RecordId)
                        .HasColumnName("RecordID")
                        .ValueGeneratedOnAdd();

                    entity.Property(e => e.ShapePriceCode)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SheetColor)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SteelId)
                        .HasColumnName("SteelID")
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SteelPriceCode)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.TravellerNo)
                        .HasMaxLength(14)
                        .IsUnicode(false);
                });

                modelBuilder.Entity<OrderHeader>(entity =>
                {
                    entity.HasKey(e => e.OrderNo);

                    entity.HasIndex(e => e.CustomerNo)
                        .HasName("ByCustomerNo");

                    entity.HasIndex(e => e.OrderDate)
                        .HasName("ByOrderDate");

                    entity.HasIndex(e => e.OrderNo)
                        .HasName("ByOrderNo")
                        .IsUnique();

                    entity.HasIndex(e => e.ShipWithWono)
                        .HasName("ShipWithWONo");

                    entity.HasIndex(e => e.WorkOrderType)
                        .HasName("ByWorkOrderType");

                    entity.HasIndex(e => new { e.CustomerNo, e.OrderDate })
                        .HasName("ByCustDate");

                    entity.HasIndex(e => new { e.OrderDate, e.CustomerNo })
                        .HasName("ByDateCust");

                    entity.HasIndex(e => new { e.OrderNo, e.ShippedYn })
                        .HasName("ByOrderNo_ShippedYN");

                    entity.HasIndex(e => new { e.PostedtoGpasyn, e.OrderNo })
                        .HasName("ByPostedOrderNo")
                        .IsUnique();

                    entity.HasIndex(e => new { e.ShipToAccountNo, e.ShipToNo })
                        .HasName("ByShipTo");

                    entity.HasIndex(e => new { e.UserAcctNo, e.UserLocNo })
                        .HasName("ByEndUser");

                    entity.HasIndex(e => new { e.CustomerNo, e.OrderDate, e.OrderNo })
                        .HasName("ByCustReverseDate");

                    entity.HasIndex(e => new { e.CustomerNo, e.Ponumber, e.Poextension })
                        .HasName("ByCustomerPO")
                        .IsUnique();

                    entity.HasIndex(e => new { e.InternationalId, e.CustomerNo, e.OrderDate })
                        .HasName("InternationalID");

                    entity.HasIndex(e => new { e.OrderNo, e.OrderDate, e.UserAcctNo })
                        .HasName("_dta_index_OrderHeader_12_1505440437__K7_K131_1");

                    entity.HasIndex(e => new { e.ShipToAccountNo, e.OrderDate, e.OrderNo })
                        .HasName("ShipToAcctNoReverseDate");

                    entity.HasIndex(e => new { e.UserAcctNo, e.OrderDate, e.OrderNo })
                        .HasName("UserAcctNoReverseDate");

                    entity.HasIndex(e => new { e.ShipToAccountNo, e.ShipToNo, e.ShippedYn, e.RequestedShipDate })
                        .HasName("ByShipAcctShipDate");

                    entity.Property(e => e.Aetching1)
                        .HasColumnName("AEtching1")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching10)
                        .HasColumnName("AEtching10")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching2)
                        .HasColumnName("AEtching2")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching3)
                        .HasColumnName("AEtching3")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching4)
                        .HasColumnName("AEtching4")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching5)
                        .HasColumnName("AEtching5")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching6)
                        .HasColumnName("AEtching6")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching7)
                        .HasColumnName("AEtching7")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching8)
                        .HasColumnName("AEtching8")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching9)
                        .HasColumnName("AEtching9")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.AlreadyPrintedWo)
                        .HasColumnName("AlreadyPrintedWO")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.CommInvAddlComments)
                        .HasMaxLength(180)
                        .IsUnicode(false);

                    entity.Property(e => e.CommInvType)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ContactPerson)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ContactPhoneNo)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.CurrencyId)
                        .HasColumnName("CurrencyID")
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.CustomerNo)
                        .IsRequired()
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Detching1)
                        .HasColumnName("DEtching1")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching1B)
                        .HasColumnName("DEtching1B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching2)
                        .HasColumnName("DEtching2")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching2B)
                        .HasColumnName("DEtching2B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching3)
                        .HasColumnName("DEtching3")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching3B)
                        .HasColumnName("DEtching3B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching4)
                        .HasColumnName("DEtching4")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching4B)
                        .HasColumnName("DEtching4B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching5)
                        .HasColumnName("DEtching5")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching5B)
                        .HasColumnName("DEtching5B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching6)
                        .HasColumnName("DEtching6")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching6B)
                        .HasColumnName("DEtching6B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching7)
                        .HasColumnName("DEtching7")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching7B)
                        .HasColumnName("DEtching7B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.DrawingPo)
                        .HasColumnName("DrawingPO")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.DrawingSetNo)
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.DutiesTaxesAcctNo)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DutiesTaxesBilling)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email2)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email2Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email3)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email3Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email4)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email4Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email5)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email5Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email6)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email6Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email7)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email7Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email8)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email8Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.EmailName)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.EngineeringNote1)
                        .HasMaxLength(90)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.EngineeringNote2)
                        .HasMaxLength(90)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.EtchingNote)
                        .HasMaxLength(360)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ExportReason)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ExportYorN)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ExportedtoProE)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.HardSoft)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.HobRequired)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Incoterms)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.InspectionNote)
                        .HasMaxLength(90)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.InternationalId)
                        .HasColumnName("InternationalID")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.InternationalYorN)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Itnno)
                        .HasColumnName("ITNNo")
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Itnrequired)
                        .HasColumnName("ITNRequired")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.LastWonoteNo).HasColumnName("LastWONoteNo");

                    entity.Property(e => e.Letching1)
                        .HasColumnName("LEtching1")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching1B)
                        .HasColumnName("LEtching1B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching2)
                        .HasColumnName("LEtching2")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching2B)
                        .HasColumnName("LEtching2B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching3)
                        .HasColumnName("LEtching3")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching3B)
                        .HasColumnName("LEtching3B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching4)
                        .HasColumnName("LEtching4")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching4B)
                        .HasColumnName("LEtching4B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching5)
                        .HasColumnName("LEtching5")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching5B)
                        .HasColumnName("LEtching5B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching6)
                        .HasColumnName("LEtching6")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching6B)
                        .HasColumnName("LEtching6B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching7)
                        .HasColumnName("LEtching7")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching7B)
                        .HasColumnName("LEtching7B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.LogisticsContact)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.LogisticsEmail)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.LogisticsPhoneNo)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.MiscNote)
                        .HasMaxLength(90)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.NatoliContact)
                        .HasMaxLength(7)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.NewCustomer)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Note1)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Note2)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OnHold)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OrderDate).HasColumnType("datetime");

                    entity.Property(e => e.OrigShipDate).HasColumnType("datetime");

                    entity.Property(e => e.OriginalWo).HasColumnName("OriginalWO");

                    entity.Property(e => e.PaidRushFee)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Poextension)
                        .HasColumnName("POExtension")
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Ponumber)
                        .HasColumnName("PONumber")
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PostedtoGpasyn)
                        .HasColumnName("PostedtoGPASYN")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintComments1)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintComments2)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintHobTicketStatus)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintMiniWorkOrd)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintMisc2)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintOrderAckStat)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintOrderStatus)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintPkgSlipStat)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintVenPkSlip)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Priority)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ProductName)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Purpose)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.RefWo).HasColumnName("RefWO");

                    entity.Property(e => e.Reference)
                        .HasMaxLength(60)
                        .IsUnicode(false);

                    entity.Property(e => e.RequestedShipDate).HasColumnType("datetime");

                    entity.Property(e => e.RequestedShipDateRev).HasColumnType("datetime");

                    entity.Property(e => e.RestrictShipment)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.RestrictShipmentDesc)
                        .HasMaxLength(20)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching1)
                        .HasColumnName("REtching1")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching1B)
                        .HasColumnName("REtching1B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching2)
                        .HasColumnName("REtching2")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching2B)
                        .HasColumnName("REtching2B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching3)
                        .HasColumnName("REtching3")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching3B)
                        .HasColumnName("REtching3B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching4)
                        .HasColumnName("REtching4")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching4B)
                        .HasColumnName("REtching4B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching5)
                        .HasColumnName("REtching5")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching5B)
                        .HasColumnName("REtching5B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching6)
                        .HasColumnName("REtching6")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching6B)
                        .HasColumnName("REtching6B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching7)
                        .HasColumnName("REtching7")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching7B)
                        .HasColumnName("REtching7B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Revision)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.RushNatoliContact)
                        .HasMaxLength(7)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.RushYorN)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SalesRepId)
                        .HasColumnName("SalesRepID")
                        .HasMaxLength(7)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Shape)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShipToAccountNo)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShipToContact)
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.ShipToNo)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShipToTaxRegNo)
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    entity.Property(e => e.ShipWithWono)
                        .HasColumnName("ShipWithWONo")
                        .HasMaxLength(35)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShippedVia)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShippedYn)
                        .HasColumnName("ShippedYN")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShipperQuoteNo)
                        .HasMaxLength(15)
                        .IsUnicode(false);

                    entity.Property(e => e.ShippingAcctNo)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShippingAmountFixed)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShippingBillMethod)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShippingNote)
                        .HasMaxLength(270)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoAddr1)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoAddr2)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoAddr3)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoCity)
                        .HasMaxLength(35)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoCountry)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoFax)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoName)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoPhone)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoState)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoZip)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SoldToTaxRegNo)
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    entity.Property(e => e.TermsId)
                        .HasColumnName("TermsID")
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Tm2data).HasColumnName("TM2Data");

                    entity.Property(e => e.Uetching1)
                        .HasColumnName("UEtching1")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching1B)
                        .HasColumnName("UEtching1B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching2)
                        .HasColumnName("UEtching2")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching2B)
                        .HasColumnName("UEtching2B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching3)
                        .HasColumnName("UEtching3")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching3B)
                        .HasColumnName("UEtching3B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching4)
                        .HasColumnName("UEtching4")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching4B)
                        .HasColumnName("UEtching4B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching5)
                        .HasColumnName("UEtching5")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching5B)
                        .HasColumnName("UEtching5B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching6)
                        .HasColumnName("UEtching6")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching6B)
                        .HasColumnName("UEtching6B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching7)
                        .HasColumnName("UEtching7")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching7B)
                        .HasColumnName("UEtching7B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.UnitOfMeasure)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.UserAcctNo)
                        .HasMaxLength(15)
                        .IsUnicode(false);

                    entity.Property(e => e.UserLocNo)
                        .HasMaxLength(15)
                        .IsUnicode(false);

                    entity.Property(e => e.UserStampCreated)
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    entity.Property(e => e.UserStampModified)
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    entity.Property(e => e.UserTaxRegNo)
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    entity.Property(e => e.WorkOrderType)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteDetailOptions>(entity =>
                {
                    entity.HasKey(e => new { e.QuoteNumber, e.RevisionNo, e.QuoteDetailLineNo, e.OptionLineNo });

                    entity.HasIndex(e => e.OptionCode)
                        .HasName("Option");

                    entity.HasIndex(e => new { e.QuoteNumber, e.RevisionNo, e.QuoteDetailLineNo, e.OptionCode })
                        .HasName("OrdNoLineNoOptCode")
                        .IsUnique();

                    entity.HasIndex(e => new { e.QuoteNumber, e.RevisionNo, e.QuoteDetailLineNo, e.OptionLineNo })
                        .HasName("OrdNoLineNoOptNo")
                        .IsUnique();

                    entity.Property(e => e.OptionCode)
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionComments)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionText)
                        .HasMaxLength(45)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionType)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteDetails>(entity =>
                {
                    entity.HasKey(e => new { e.QuoteNo, e.LineNumber, e.Revision });

                    entity.HasIndex(e => new { e.QuoteNo, e.Revision, e.DetailTypeId })
                        .HasName("ReverseType");

                    entity.HasIndex(e => new { e.QuoteNo, e.Revision, e.LineNumber })
                        .HasName("ByQuoteNo")
                        .IsUnique();

                    entity.HasIndex(e => new { e.QuoteNo, e.Revision, e.Sequence })
                        .HasName("Sequence");

                    entity.Property(e => e.CompletedYn)
                        .HasColumnName("CompletedYN")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc1)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc2)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc3)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc4)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc5)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc6)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc7)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Desc8)
                        .HasMaxLength(55)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DetailTypeId)
                        .HasColumnName("DetailTypeID")
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DieShapeId).HasColumnName("DieShapeID");

                    entity.Property(e => e.HobNoShapeId)
                        .HasColumnName("HobNoShapeID")
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.HoldFlag)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.MachinePriceCode)
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PriceOptionsAdded)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintStatus)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShapePriceCode)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SheetColor)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SteelId)
                        .HasColumnName("SteelID")
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SteelPriceCode)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteFreightDesc>(entity =>
                {
                    entity.HasNoKey();

                    entity.HasIndex(e => e.FreightId)
                        .HasName("FreightID")
                        .IsUnique();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.FreightId)
                        .IsRequired()
                        .HasColumnName("FreightID")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteHeader>(entity =>
                {
                    entity.HasKey(e => new { e.QuoteNo, e.QuoteRevNo });

                    entity.HasIndex(e => new { e.QuoteNo, e.QuoteRevNo })
                        .HasName("QuoteNo")
                        .IsUnique()
                        .IsClustered();

                    entity.HasIndex(e => new { e.ShipToAccountNo, e.ShipToNo })
                        .HasName("ByShipTo");

                    entity.HasIndex(e => new { e.UserAcctNo, e.UserLocNo })
                        .HasName("ByEndUser");

                    entity.HasIndex(e => new { e.CustomerNo, e.QuoteDate, e.QuoteNo })
                        .HasName("CustomerReverseDate");

                    entity.HasIndex(e => new { e.CustomerNo, e.QuoteNo, e.QuoteRevNo })
                        .HasName("CustomerNo");

                    entity.HasIndex(e => new { e.InternationalId, e.CustomerNo, e.QuoteDate })
                        .HasName("InternationalID");

                    entity.HasIndex(e => new { e.QuoteDate, e.QuoteNo, e.QuoteRevNo })
                        .HasName("Date");

                    entity.HasIndex(e => new { e.ShipToAccountNo, e.QuoteDate, e.QuoteNo })
                        .HasName("ShipToAcctReverseDate");

                    entity.HasIndex(e => new { e.UserAcctNo, e.QuoteDate, e.QuoteNo })
                        .HasName("UserAcctNoReverseDate");

                    entity.Property(e => e.Aetching1)
                        .HasColumnName("AEtching1")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching10)
                        .HasColumnName("AEtching10")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching2)
                        .HasColumnName("AEtching2")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching3)
                        .HasColumnName("AEtching3")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching4)
                        .HasColumnName("AEtching4")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching5)
                        .HasColumnName("AEtching5")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching6)
                        .HasColumnName("AEtching6")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching7)
                        .HasColumnName("AEtching7")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching8)
                        .HasColumnName("AEtching8")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Aetching9)
                        .HasColumnName("AEtching9")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.BillToAddr1)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.BillToAddr2)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.BillToAddr3)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.BillToCity)
                        .HasMaxLength(35)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.BillToCountry)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.BillToName)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.BillToState)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.BillToZip)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.CommInvType)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ContactPerson)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ContactPhoneNo)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ConvertedtoOrder)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.CurrencyId)
                        .HasColumnName("CurrencyID")
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.CustomerNo)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Detching1)
                        .HasColumnName("DEtching1")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching1B)
                        .HasColumnName("DEtching1B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching2)
                        .HasColumnName("DEtching2")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching2B)
                        .HasColumnName("DEtching2B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching3)
                        .HasColumnName("DEtching3")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching3B)
                        .HasColumnName("DEtching3B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching4)
                        .HasColumnName("DEtching4")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching4B)
                        .HasColumnName("DEtching4B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching5)
                        .HasColumnName("DEtching5")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching5B)
                        .HasColumnName("DEtching5B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching6)
                        .HasColumnName("DEtching6")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching6B)
                        .HasColumnName("DEtching6B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching7)
                        .HasColumnName("DEtching7")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Detching7B)
                        .HasColumnName("DEtching7B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.DrawingPo)
                        .HasColumnName("DrawingPO")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.DrawingSetNo)
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.DutiesTaxesAcctNo)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DutiesTaxesBilling)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email2)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email2Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email3)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email3Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email4)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email4Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email5)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email5Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email6)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email6Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email7)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email7Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Email8)
                        .HasMaxLength(55)
                        .IsUnicode(false);

                    entity.Property(e => e.Email8Name)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.EmailName)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.EngineeringNote1)
                        .HasMaxLength(90)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.EngineeringNote2)
                        .HasMaxLength(90)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.EtchingNote)
                        .HasMaxLength(360)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ExportReason)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ExportYorN)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ExportedToEng)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.FreightDescId)
                        .HasColumnName("FreightDescID")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.HobRequired)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Incoterms)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.InspectionNote)
                        .HasMaxLength(90)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.InternationalId)
                        .HasColumnName("InternationalID")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.InternationalYorN)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Itnno)
                        .HasColumnName("ITNNo")
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Itnrequired)
                        .HasColumnName("ITNRequired")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.LastWonoteNo).HasColumnName("LastWONoteNo");

                    entity.Property(e => e.Letching1)
                        .HasColumnName("LEtching1")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching1B)
                        .HasColumnName("LEtching1B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching2)
                        .HasColumnName("LEtching2")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching2B)
                        .HasColumnName("LEtching2B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching3)
                        .HasColumnName("LEtching3")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching3B)
                        .HasColumnName("LEtching3B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching4)
                        .HasColumnName("LEtching4")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching4B)
                        .HasColumnName("LEtching4B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching5)
                        .HasColumnName("LEtching5")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching5B)
                        .HasColumnName("LEtching5B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching6)
                        .HasColumnName("LEtching6")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching6B)
                        .HasColumnName("LEtching6B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching7)
                        .HasColumnName("LEtching7")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Letching7B)
                        .HasColumnName("LEtching7B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.LogisticsContact)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.LogisticsEmail)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.LogisticsPhoneNo)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.MiscNote)
                        .HasMaxLength(90)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.NewCustomer)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Note1)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Note2)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Origination)
                        .HasMaxLength(12)
                        .IsUnicode(false);

                    entity.Property(e => e.Poextension)
                        .HasColumnName("POExtension")
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Ponumber)
                        .HasColumnName("PONumber")
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PostedtoGpasyn)
                        .HasColumnName("PostedtoGPASYN")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Pricing)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintMiniWorkOrd)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintMisc1)
                        .HasColumnName("Print_Misc1")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintMisc2)
                        .HasColumnName("Print_Misc2")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintMisc3)
                        .HasColumnName("Print_Misc3")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintMisc4)
                        .HasColumnName("Print_Misc4")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintMisc5)
                        .HasColumnName("Print_Misc5")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintPkgSlipStat)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintQuoteStatus)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PrintStatus)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ProductName)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDate).HasColumnType("datetime");

                    entity.Property(e => e.QuoteRepId)
                        .IsRequired()
                        .HasColumnName("QuoteRepID")
                        .HasMaxLength(7)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.RefWo).HasColumnName("RefWO");

                    entity.Property(e => e.Reference)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Requested)
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.RequestedShipDate).HasColumnType("datetime");

                    entity.Property(e => e.Retching1)
                        .HasColumnName("REtching1")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching1B)
                        .HasColumnName("REtching1B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching2)
                        .HasColumnName("REtching2")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching2B)
                        .HasColumnName("REtching2B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching3)
                        .HasColumnName("REtching3")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching3B)
                        .HasColumnName("REtching3B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching4)
                        .HasColumnName("REtching4")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching4B)
                        .HasColumnName("REtching4B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching5)
                        .HasColumnName("REtching5")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching5B)
                        .HasColumnName("REtching5B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching6)
                        .HasColumnName("REtching6")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching6B)
                        .HasColumnName("REtching6B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching7)
                        .HasColumnName("REtching7")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Retching7B)
                        .HasColumnName("REtching7B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.RevisedYn)
                        .HasColumnName("RevisedYN")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.RushNatoliContact)
                        .HasMaxLength(7)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.RushYorN)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShipToAccountNo)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShipToContact)
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.ShipToFax)
                        .HasMaxLength(45)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShipToNo)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShipToPhone)
                        .HasMaxLength(35)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShipToTaxRegNo)
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    entity.Property(e => e.Shipment)
                        .HasMaxLength(25)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShippedVia)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShippedYn)
                        .HasColumnName("ShippedYN")
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShipperQuoteNo)
                        .HasMaxLength(15)
                        .IsUnicode(false);

                    entity.Property(e => e.ShippingAcctNo)
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShippingBillMethod)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShippingNote)
                        .HasMaxLength(270)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoAddr1)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoAddr2)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoAddr3)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoCity)
                        .HasMaxLength(35)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoCountry)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoName)
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoState)
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShiptoZip)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SoldToTaxRegNo)
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    entity.Property(e => e.TermsId)
                        .HasColumnName("TermsID")
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Tm2data).HasColumnName("TM2Data");

                    entity.Property(e => e.Uetching1)
                        .HasColumnName("UEtching1")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching1B)
                        .HasColumnName("UEtching1B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching2)
                        .HasColumnName("UEtching2")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching2B)
                        .HasColumnName("UEtching2B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching3)
                        .HasColumnName("UEtching3")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching3B)
                        .HasColumnName("UEtching3B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching4)
                        .HasColumnName("UEtching4")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching4B)
                        .HasColumnName("UEtching4B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching5)
                        .HasColumnName("UEtching5")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching5B)
                        .HasColumnName("UEtching5B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching6)
                        .HasColumnName("UEtching6")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching6B)
                        .HasColumnName("UEtching6B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching7)
                        .HasColumnName("UEtching7")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.Uetching7B)
                        .HasColumnName("UEtching7B")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    entity.Property(e => e.UnitOfMeasure)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.UserAcctNo)
                        .HasMaxLength(15)
                        .IsUnicode(false);

                    entity.Property(e => e.UserLocNo)
                        .HasMaxLength(15)
                        .IsUnicode(false);

                    entity.Property(e => e.UserTaxRegNo)
                        .HasMaxLength(25)
                        .IsUnicode(false);

                    entity.Property(e => e.WorkOrderType)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueASingleNum>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueA_SingleNum");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("AOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.Number1Mm).HasColumnName("Number1MM");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueBDoubleNum>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueB_DoubleNum");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("BOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.Number1Mm).HasColumnName("Number1MM");

                    entity.Property(e => e.Number2Mm).HasColumnName("Number2MM");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text1)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text2)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueCTolerance>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueC_Tolerance");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("COrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.BottomMm).HasColumnName("BottomMM");

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.TopMm).HasColumnName("TopMM");
                });

                modelBuilder.Entity<QuoteOptionValueDDegreeVal>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueD_DegreeVal");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("DOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ValueMm).HasColumnName("ValueMM");
                });

                modelBuilder.Entity<QuoteOptionValueESmallText>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueE_SmallText");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("EOptionLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SmallText)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueFLargeText>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueF_LargeText");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("FOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.LargeText)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueGDegrees>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueG_Degrees");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("GOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueHHardness>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueH_Hardness");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("HOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueIHardness2>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueI_Hardness2");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("IOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueJOptionMult>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueJ_OptionMult");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("JOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OptionMult)
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueKVendor>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueK_Vendor");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("KOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.VendorId)
                        .HasColumnName("VendorID")
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueLSurfaceTreat>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueL_SurfaceTreat");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("LOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SurfaceTreatment)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.VendorId)
                        .HasColumnName("VendorID")
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueMScrew>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueM_Screw");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("MOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Screw)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueNColor>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueN_Color");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("NOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.Color)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueOInteger>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueO_Integer");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("OOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValuePDegDec>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueP_DegDec");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("POrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueQDimensions>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueQ_Dimensions");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("QOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.DepthMm).HasColumnName("DepthMM");

                    entity.Property(e => e.LengthMm).HasColumnName("LengthMM");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.WidthMm).HasColumnName("WidthMM");
                });

                modelBuilder.Entity<QuoteOptionValueRIntegerTxt>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueR_IntegerTxt");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("ROrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueSSmallText>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueS_SmallText");

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteOptionValueTDecText>(entity =>
                {
                    entity.HasNoKey();

                    entity.ToTable("QuoteOptionValueT_DecText");

                    entity.HasIndex(e => e.DateVerified)
                        .HasName("DateVerified");

                    entity.HasIndex(e => new { e.QuoteNo, e.RevNo, e.QuoteDetailType, e.OptionCode })
                        .HasName("TOrderLineOption")
                        .IsUnique();

                    entity.Property(e => e.DateVerified).HasColumnType("datetime");

                    entity.Property(e => e.OptionCode)
                        .IsRequired()
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.QuoteDetailType)
                        .HasMaxLength(5)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Text)
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<QuoteRepresentative>(entity =>
                {
                    entity.HasNoKey();

                    entity.HasIndex(e => e.Name)
                        .HasName("ByName");

                    entity.HasIndex(e => e.RepId)
                        .HasName("RepID")
                        .IsUnique();

                    entity.Property(e => e.EmailAddress)
                        .IsRequired()
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Name)
                        .IsRequired()
                        .HasMaxLength(25)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.RepId)
                        .IsRequired()
                        .HasColumnName("RepID")
                        .HasMaxLength(7)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SignatureFile)
                        .IsRequired()
                        .HasMaxLength(60)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<SteelType>(entity =>
                {
                    entity.HasNoKey();

                    entity.HasIndex(e => e.Description)
                        .HasName("ByDesc");

                    entity.HasIndex(e => e.SteelPriceCode)
                        .HasName("BySteelPriceCode");

                    entity.HasIndex(e => e.TypeId)
                        .HasName("ByTypeID")
                        .IsUnique();

                    entity.Property(e => e.BarcodeHeatTreatDisplayGroup)
                        .HasMaxLength(10)
                        .IsUnicode(false);

                    entity.Property(e => e.CryoYesOrNo)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DiaMaxRc)
                        .IsRequired()
                        .HasColumnName("DiaMaxRC")
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DieMinRc)
                        .IsRequired()
                        .HasColumnName("DieMinRC")
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DrawingDescription)
                        .IsRequired()
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.HeatTreatPattern)
                        .HasMaxLength(15)
                        .IsUnicode(false);

                    entity.Property(e => e.PunchMaxRc)
                        .IsRequired()
                        .HasColumnName("PunchMaxRC")
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PunchMinRc)
                        .IsRequired()
                        .HasColumnName("PunchMinRC")
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.SteelPriceCode)
                        .IsRequired()
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.TypeId)
                        .IsRequired()
                        .HasColumnName("TypeID")
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.UnitofMeasure)
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<ShapeFields>(entity =>
                {
                    entity.HasKey(e => new { e.ShapeDescription });
                });

                modelBuilder.Entity<DieList>(entity =>
                {
                    entity.HasNoKey();

                    entity.HasIndex(e => e.DieId)
                        .HasName("ByShapeID")
                        .IsUnique();

                    entity.HasIndex(e => e.Size)
                        .HasName("BySizeField");

                    entity.Property(e => e.BisectedCode)
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.BlendingRadiusM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.CornerRadiusM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.CupDepth)
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.DateDesigned).HasColumnType("datetime");

                    entity.Property(e => e.DieId)
                        .IsRequired()
                        .HasColumnName("DieID")
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Embossed1)
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Embossed2)
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.EndRadiusM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.Land)
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.LengthMajorAxisM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.MasterDieStatus)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Note1)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Note2)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.Note3)
                        .HasMaxLength(40)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.OutsideDiameterM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.OwnerReservedFor)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.PlugGaugeStatus)
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.RadiusM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.RefOutsideDiameterM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.Shape)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShapeCode)
                        .HasMaxLength(2)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.ShapeId).HasColumnName("ShapeID");

                    entity.Property(e => e.SideRadiusM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");

                    entity.Property(e => e.Size)
                        .HasMaxLength(15)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.TempDate)
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.WidthMinorAxisM)
                        .HasColumnType("numeric(12, 3)")
                        .HasDefaultValueSql("((0))");
                });

                modelBuilder.Entity<CupConfig>(entity =>
                {
                    entity.HasNoKey();

                    entity.HasIndex(e => e.CupID)
                        .IsUnique();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .IsFixedLength();

                    entity.Property(e => e.TM2ID)
                        .IsRequired()
                        .HasMaxLength(6)
                        .IsUnicode(false)
                        .IsFixedLength();
                });

                modelBuilder.Entity<CustomerMachines>(entity =>
                {
                    entity.HasKey(e => e.CustomerMachineID);
                });

                modelBuilder.Entity<Keys>(entity =>
                {
                    entity.HasKey(e => e.DrawingNo);
                });

                modelBuilder.Entity<BisectCodes>(entity =>
                {
                    entity.HasKey(e => new { e.ID });
                });

                OnModelCreatingPartial(modelBuilder);

            }

        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}