namespace ql_ks.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LUOTGIATUI")]
    public partial class LUOTGIATUI
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LUOTGIATUI()
        {
            CHITIET_HDGU = new HashSet<CHITIET_HDGU>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Ma_LuotGU { get; set; }

        public int? SoKilogram_LuotGU { get; set; }

        public DateTime? NgayBatDau_LuotGU { get; set; }

        public DateTime? NgayKetThuc_LuotGU { get; set; }

        public int? Ma_LoaiGU { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CHITIET_HDGU> CHITIET_HDGU { get; set; }

        public virtual LOAIGIATUI LOAIGIATUI { get; set; }
    }
}
