namespace ql_ks.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BAOCAONAM")]
    public partial class BAOCAONAM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MA_BCN { get; set; }

        public DateTime? THOIGIANLAP_BCN { get; set; }

        public int? NAM_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? TONGDOANHTHU_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG1_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG2_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG3_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG4_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG5_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG6_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG7_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG8_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG9_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG10_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG11_BCN { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUTHANG12_BCN { get; set; }
    }
}
