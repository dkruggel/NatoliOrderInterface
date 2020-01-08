using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NatoliOrderInterface.Models.NEC
{
    public partial class NECContext : DbContext
    {
        public NECContext()
        {
        }

        public NECContext(DbContextOptions<NECContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Rm00101> Rm00101 { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=" + App.Server + ";Database=NEC;Persist Security Info=" + App.PersistSecurityInfo + ";User ID=" + App.UserID + ";Password=" + App.Password + ";");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rm00101>(entity =>
            {
                entity.HasKey(e => e.Custnmbr)
                    .HasName("PKRM00101")
                    .IsClustered(false);

                entity.ToTable("RM00101");

                entity.HasIndex(e => new { e.Cprcstnm, e.Custnmbr })
                    .HasName("AK7RM00101")
                    .IsUnique();

                entity.HasIndex(e => new { e.Custclas, e.Custnmbr })
                    .HasName("AK3RM00101")
                    .IsUnique();

                entity.HasIndex(e => new { e.Custname, e.DexRowId })
                    .HasName("AK2RM00101")
                    .IsUnique();

                entity.HasIndex(e => new { e.Salsterr, e.Custnmbr })
                    .HasName("AK6RM00101")
                    .IsUnique();

                entity.HasIndex(e => new { e.Slprsnid, e.Custnmbr })
                    .HasName("AK5RM00101")
                    .IsUnique();

                entity.HasIndex(e => new { e.Txrgnnum, e.Custnmbr })
                    .HasName("AK8RM00101")
                    .IsUnique();

                entity.HasIndex(e => new { e.Userdef1, e.Custnmbr })
                    .HasName("AK4RM00101")
                    .IsUnique();

                entity.Property(e => e.Custnmbr)
                    .HasColumnName("CUSTNMBR")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Address1)
                    .IsRequired()
                    .HasColumnName("ADDRESS1")
                    .HasMaxLength(61)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Address2)
                    .IsRequired()
                    .HasColumnName("ADDRESS2")
                    .HasMaxLength(61)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Address3)
                    .IsRequired()
                    .HasColumnName("ADDRESS3")
                    .HasMaxLength(61)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Adrscode)
                    .IsRequired()
                    .HasColumnName("ADRSCODE")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Balnctyp).HasColumnName("BALNCTYP");

                entity.Property(e => e.Bankname)
                    .IsRequired()
                    .HasColumnName("BANKNAME")
                    .HasMaxLength(31)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Bnkbrnch)
                    .IsRequired()
                    .HasColumnName("BNKBRNCH")
                    .HasMaxLength(21)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Cbvat).HasColumnName("CBVAT");

                entity.Property(e => e.Ccode)
                    .IsRequired()
                    .HasColumnName("CCode")
                    .HasMaxLength(7)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Ccrdxpdt)
                    .HasColumnName("CCRDXPDT")
                    .HasColumnType("datetime");

                entity.Property(e => e.Chekbkid)
                    .IsRequired()
                    .HasColumnName("CHEKBKID")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasColumnName("CITY")
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Cntcprsn)
                    .IsRequired()
                    .HasColumnName("CNTCPRSN")
                    .HasMaxLength(61)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Comment1)
                    .IsRequired()
                    .HasColumnName("COMMENT1")
                    .HasMaxLength(31)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Comment2)
                    .IsRequired()
                    .HasColumnName("COMMENT2")
                    .HasMaxLength(31)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Country)
                    .IsRequired()
                    .HasColumnName("COUNTRY")
                    .HasMaxLength(61)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Cprcstnm)
                    .IsRequired()
                    .HasColumnName("CPRCSTNM")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Crcardid)
                    .IsRequired()
                    .HasColumnName("CRCARDID")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Crcrdnum)
                    .IsRequired()
                    .HasColumnName("CRCRDNUM")
                    .HasMaxLength(21)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Creatddt)
                    .HasColumnName("CREATDDT")
                    .HasColumnType("datetime");

                entity.Property(e => e.Crlmtamt)
                    .HasColumnName("CRLMTAMT")
                    .HasColumnType("numeric(19, 5)");

                entity.Property(e => e.Crlmtpam)
                    .HasColumnName("CRLMTPAM")
                    .HasColumnType("numeric(19, 5)");

                entity.Property(e => e.Crlmtper).HasColumnName("CRLMTPER");

                entity.Property(e => e.Crlmttyp).HasColumnName("CRLMTTYP");

                entity.Property(e => e.Curncyid)
                    .IsRequired()
                    .HasColumnName("CURNCYID")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Custclas)
                    .IsRequired()
                    .HasColumnName("CUSTCLAS")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Custdisc).HasColumnName("CUSTDISC");

                entity.Property(e => e.Custname)
                    .IsRequired()
                    .HasColumnName("CUSTNAME")
                    .HasMaxLength(65)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Custpriority).HasColumnName("CUSTPRIORITY");

                entity.Property(e => e.Declid)
                    .IsRequired()
                    .HasColumnName("DECLID")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Defcacty).HasColumnName("DEFCACTY");

                entity.Property(e => e.DexRowId)
                    .HasColumnName("DEX_ROW_ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.DexRowTs)
                    .HasColumnName("DEX_ROW_TS")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Disgrper).HasColumnName("DISGRPER");

                entity.Property(e => e.Docfmtid)
                    .IsRequired()
                    .HasColumnName("DOCFMTID")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Duegrper).HasColumnName("DUEGRPER");

                entity.Property(e => e.Fax)
                    .IsRequired()
                    .HasColumnName("FAX")
                    .HasMaxLength(21)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Finchdlr)
                    .HasColumnName("FINCHDLR")
                    .HasColumnType("numeric(19, 5)");

                entity.Property(e => e.Finchid)
                    .IsRequired()
                    .HasColumnName("FINCHID")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Fnchatyp).HasColumnName("FNCHATYP");

                entity.Property(e => e.Fnchpcnt).HasColumnName("FNCHPCNT");

                entity.Property(e => e.Frstindt)
                    .HasColumnName("FRSTINDT")
                    .HasColumnType("datetime");

                entity.Property(e => e.Govcrpid)
                    .IsRequired()
                    .HasColumnName("GOVCRPID")
                    .HasMaxLength(31)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Govindid)
                    .IsRequired()
                    .HasColumnName("GOVINDID")
                    .HasMaxLength(31)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Gpsfointegrationid)
                    .IsRequired()
                    .HasColumnName("GPSFOINTEGRATIONID")
                    .HasMaxLength(31)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Hold).HasColumnName("HOLD");

                entity.Property(e => e.Inactive).HasColumnName("INACTIVE");

                entity.Property(e => e.Includeindp).HasColumnName("INCLUDEINDP");

                entity.Property(e => e.Integrationid)
                    .IsRequired()
                    .HasColumnName("INTEGRATIONID")
                    .HasMaxLength(31)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Integrationsource).HasColumnName("INTEGRATIONSOURCE");

                entity.Property(e => e.Kpcalhst).HasColumnName("KPCALHST");

                entity.Property(e => e.Kpdsthst).HasColumnName("KPDSTHST");

                entity.Property(e => e.Kperhist).HasColumnName("KPERHIST");

                entity.Property(e => e.Kptrxhst).HasColumnName("KPTRXHST");

                entity.Property(e => e.Minpydlr)
                    .HasColumnName("MINPYDLR")
                    .HasColumnType("numeric(19, 5)");

                entity.Property(e => e.Minpypct).HasColumnName("MINPYPCT");

                entity.Property(e => e.Minpytyp).HasColumnName("MINPYTYP");

                entity.Property(e => e.Modifdt)
                    .HasColumnName("MODIFDT")
                    .HasColumnType("datetime");

                entity.Property(e => e.Mxwoftyp).HasColumnName("MXWOFTYP");

                entity.Property(e => e.Mxwrofam)
                    .HasColumnName("MXWROFAM")
                    .HasColumnType("numeric(19, 5)");

                entity.Property(e => e.Noteindx)
                    .HasColumnName("NOTEINDX")
                    .HasColumnType("numeric(19, 5)");

                entity.Property(e => e.Orderfulfilldefault).HasColumnName("ORDERFULFILLDEFAULT");

                entity.Property(e => e.Phone1)
                    .IsRequired()
                    .HasColumnName("PHONE1")
                    .HasMaxLength(21)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Phone2)
                    .IsRequired()
                    .HasColumnName("PHONE2")
                    .HasMaxLength(21)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Phone3)
                    .IsRequired()
                    .HasColumnName("PHONE3")
                    .HasMaxLength(21)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.PostResultsTo).HasColumnName("Post_Results_To");

                entity.Property(e => e.Prbtadcd)
                    .IsRequired()
                    .HasColumnName("PRBTADCD")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Prclevel)
                    .IsRequired()
                    .HasColumnName("PRCLEVEL")
                    .HasMaxLength(11)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Prstadcd)
                    .IsRequired()
                    .HasColumnName("PRSTADCD")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Pymtrmid)
                    .IsRequired()
                    .HasColumnName("PYMTRMID")
                    .HasMaxLength(21)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Ratetpid)
                    .IsRequired()
                    .HasColumnName("RATETPID")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.RevalueCustomer).HasColumnName("Revalue_Customer");

                entity.Property(e => e.Rmaracc).HasColumnName("RMARACC");

                entity.Property(e => e.Rmavacc).HasColumnName("RMAVACC");

                entity.Property(e => e.Rmcosacc).HasColumnName("RMCOSACC");

                entity.Property(e => e.Rmcshacc).HasColumnName("RMCSHACC");

                entity.Property(e => e.Rmfcgacc).HasColumnName("RMFCGACC");

                entity.Property(e => e.Rmivacc).HasColumnName("RMIVACC");

                entity.Property(e => e.RmovrpymtWrtoffAcctIdx).HasColumnName("RMOvrpymtWrtoffAcctIdx");

                entity.Property(e => e.Rmslsacc).HasColumnName("RMSLSACC");

                entity.Property(e => e.Rmsoracc).HasColumnName("RMSORACC");

                entity.Property(e => e.Rmtakacc).HasColumnName("RMTAKACC");

                entity.Property(e => e.Rmwracc).HasColumnName("RMWRACC");

                entity.Property(e => e.Salsterr)
                    .IsRequired()
                    .HasColumnName("SALSTERR")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SendEmailStatements).HasColumnName("Send_Email_Statements");

                entity.Property(e => e.Shipcomplete).HasColumnName("SHIPCOMPLETE");

                entity.Property(e => e.Shipmthd)
                    .IsRequired()
                    .HasColumnName("SHIPMTHD")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Shrtname)
                    .IsRequired()
                    .HasColumnName("SHRTNAME")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Slprsnid)
                    .IsRequired()
                    .HasColumnName("SLPRSNID")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Staddrcd)
                    .IsRequired()
                    .HasColumnName("STADDRCD")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.State)
                    .IsRequired()
                    .HasColumnName("STATE")
                    .HasMaxLength(29)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Stmtcycl).HasColumnName("STMTCYCL");

                entity.Property(e => e.Stmtname)
                    .IsRequired()
                    .HasColumnName("STMTNAME")
                    .HasMaxLength(65)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Taxexmt1)
                    .IsRequired()
                    .HasColumnName("TAXEXMT1")
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Taxexmt2)
                    .IsRequired()
                    .HasColumnName("TAXEXMT2")
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Taxschid)
                    .IsRequired()
                    .HasColumnName("TAXSCHID")
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Txrgnnum)
                    .IsRequired()
                    .HasColumnName("TXRGNNUM")
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Upszone)
                    .IsRequired()
                    .HasColumnName("UPSZONE")
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Userdef1)
                    .IsRequired()
                    .HasColumnName("USERDEF1")
                    .HasMaxLength(21)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Userdef2)
                    .IsRequired()
                    .HasColumnName("USERDEF2")
                    .HasMaxLength(21)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Userlang).HasColumnName("USERLANG");

                entity.Property(e => e.Zip)
                    .IsRequired()
                    .HasColumnName("ZIP")
                    .HasMaxLength(11)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
