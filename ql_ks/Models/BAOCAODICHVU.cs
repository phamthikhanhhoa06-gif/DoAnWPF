namespace ql_ks.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BAOCAODICHVU")]
    public partial class BAOCAODICHVU
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MA_BCDV { get; set; }

        public DateTime? THOIGIANLAP_BCDV { get; set; }

        [Column(TypeName = "money")]
        public decimal? TONGDOANHTHU_BCDV { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHULUUUTRU_BCDV { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUANUONG_BCDV { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUGIATUI_BCDV { get; set; }

        [Column(TypeName = "money")]
        public decimal? DOANHTHUDICHUYEN_BCDV { get; set; }

        public DateTime? NGAYBATDAU_BCDV { get; set; }

        public DateTime? NGAYKETTHUC_BCDV { get; set; }
    }
}
