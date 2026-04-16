using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace ql_ks.Models
{
    public partial class QLKhachSan_Model : DbContext
    {
        public QLKhachSan_Model()
            : base("name=QLKhachSan_Model2")
        {
        }

        public virtual DbSet<BAOCAODICHVU> BAOCAODICHVUs { get; set; }
        public virtual DbSet<BAOCAONAM> BAOCAONAMs { get; set; }
        public virtual DbSet<CHITIET_HDAU> CHITIET_HDAU { get; set; }
        public virtual DbSet<CHITIET_HDDC> CHITIET_HDDC { get; set; }
        public virtual DbSet<CHITIET_HDGU> CHITIET_HDGU { get; set; }
        public virtual DbSet<CHITIET_HDLT> CHITIET_HDLT { get; set; }
        public virtual DbSet<CHUYENDI> CHUYENDIs { get; set; }
        public virtual DbSet<HOADON> HOADONs { get; set; }
        public virtual DbSet<KHACHHANG> KHACHHANGs { get; set; }
        public virtual DbSet<LOAIGIATUI> LOAIGIATUIs { get; set; }
        public virtual DbSet<LOAIPHONG> LOAIPHONGs { get; set; }
        public virtual DbSet<LUOTGIATUI> LUOTGIATUIs { get; set; }
        public virtual DbSet<MATHANG> MATHANGs { get; set; }
        public virtual DbSet<NHANVIEN> NHANVIENs { get; set; }
        public virtual DbSet<PHONG> PHONGs { get; set; }
        public virtual DbSet<TAIKHOAN> TAIKHOANs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BAOCAODICHVU>()
                .Property(e => e.TONGDOANHTHU_BCDV)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAODICHVU>()
                .Property(e => e.DOANHTHULUUUTRU_BCDV)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAODICHVU>()
                .Property(e => e.DOANHTHUANUONG_BCDV)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAODICHVU>()
                .Property(e => e.DOANHTHUGIATUI_BCDV)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAODICHVU>()
                .Property(e => e.DOANHTHUDICHUYEN_BCDV)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.TONGDOANHTHU_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG1_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG2_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG3_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG4_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG5_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG6_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG7_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG8_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG9_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG10_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG11_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BAOCAONAM>()
                .Property(e => e.DOANHTHUTHANG12_BCN)
                .HasPrecision(19, 4);

            modelBuilder.Entity<LOAIGIATUI>()
                .Property(e => e.DonGia_LoaiGU)
                .HasPrecision(19, 4);
        }
    }
}
