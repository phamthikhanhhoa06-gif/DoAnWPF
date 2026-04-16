namespace ql_ks.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("NHANVIEN")]
    public partial class NHANVIEN
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NHANVIEN()
        {
            HOADONs = new HashSet<HOADON>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MA_NV { get; set; }

        [Required]
        [StringLength(255)]
        public string HoTen_NV { get; set; }

        public bool? GioiTinh_NV { get; set; }

        public DateTime? NgaySinh_NV { get; set; }

        [StringLength(20)]
        public string SoDienThoai_NV { get; set; }

        [StringLength(100)]
        public string ChucVu_NV { get; set; }

        [StringLength(255)]
        public string DiaChi_NV { get; set; }

        public DateTime? NgayVaoLam_NV { get; set; }

        public int? Ma_TK { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HOADON> HOADONs { get; set; }

        public virtual TAIKHOAN TAIKHOAN { get; set; }
    }
}
