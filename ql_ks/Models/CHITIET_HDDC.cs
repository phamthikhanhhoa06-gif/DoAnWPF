namespace ql_ks.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CHITIET_HDDC
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Ma_CTHDDC { get; set; }

        public DateTime? ThoiGianLap_CTHDDC { get; set; }

        public long? TriGia_CTHDDC { get; set; }

        public int? MA_HD { get; set; }

        public int? Ma_CD { get; set; }

        public virtual CHUYENDI CHUYENDI { get; set; }

        public virtual HOADON HOADON { get; set; }
    }
}
