namespace ql_ks.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LOAIGIATUI")]
    public partial class LOAIGIATUI
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LOAIGIATUI()
        {
            LUOTGIATUIs = new HashSet<LUOTGIATUI>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Ma_LoaiGU { get; set; }

        [StringLength(255)]
        public string Ten_LoaiGU { get; set; }

        [Column(TypeName = "money")]
        public decimal? DonGia_LoaiGU { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LUOTGIATUI> LUOTGIATUIs { get; set; }
    }
}
