namespace ql_ks.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CHITIET_HDLT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Ma_CTHDLT { get; set; }

        public DateTime? ThoiGianNhan_PHONG { get; set; }

        public DateTime? ThoiGianTra_PHONG { get; set; }

        public long? TriGia_CTHDLT { get; set; }

        public int? MA_HD { get; set; }

        public int? Ma_Phong { get; set; }

        public virtual HOADON HOADON { get; set; }

        public virtual PHONG PHONG { get; set; }
    }
}
