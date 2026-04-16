namespace ql_ks.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CHITIET_HDAU
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Ma_CTHDAU { get; set; }

        public int? SoLuong_MH { get; set; }

        public DateTime? ThoiGianLap_CTHDAU { get; set; }

        public long? TriGia_CTHDAU { get; set; }

        public int? MA_HD { get; set; }

        public int? Ma_MH { get; set; }

        public virtual HOADON HOADON { get; set; }

        public virtual MATHANG MATHANG { get; set; }
    }
}
