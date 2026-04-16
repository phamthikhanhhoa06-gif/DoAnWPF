namespace ql_ks.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CHITIET_HDGU
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Ma_CTHDGU { get; set; }

        public DateTime? ThoiGianLap_CTHDGU { get; set; }

        public long? TriGia_CTHDGU { get; set; }

        public int? MA_HD { get; set; }

        public int? Ma_LuotGU { get; set; }

        public virtual HOADON HOADON { get; set; }

        public virtual LUOTGIATUI LUOTGIATUI { get; set; }
    }
}
