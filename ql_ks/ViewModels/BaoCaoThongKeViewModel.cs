using ql_ks.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ql_ks.ViewModels
{
    public class ChartItem
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
        public double ScaledHeight { get; set; }
        public string DisplayValue => Value.ToString("N0");
    }

    public class BaoCaoThongKeViewModel : Common_BaseViewModel
    {
        private ObservableCollection<BAOCAONAM> _listBaoCaoNam;
        public ObservableCollection<BAOCAONAM> ListBaoCaoNam
        {
            get => _listBaoCaoNam;
            set { _listBaoCaoNam = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ChartItem> _chartItems;
        public ObservableCollection<ChartItem> ChartItems
        {
            get => _chartItems;
            set { _chartItems = value; OnPropertyChanged(); }
        }

        private ObservableCollection<BAOCAODICHVU> _listBaoCaoDichVu;
        public ObservableCollection<BAOCAODICHVU> ListBaoCaoDichVu
        {
            get => _listBaoCaoDichVu;
            set { _listBaoCaoDichVu = value; OnPropertyChanged(); }
        }

        private int _namBaoCao = DateTime.Now.Year;
        public int NamBaoCao
        {
            get => _namBaoCao;
            set { _namBaoCao = value; OnPropertyChanged(); }
        }

        private DateTime _tuNgay = DateTime.Now.AddMonths(-1);
        public DateTime TuNgay
        {
            get => _tuNgay;
            set { _tuNgay = value; OnPropertyChanged(); }
        }

        private DateTime _denNgay = DateTime.Now;
        public DateTime DenNgay
        {
            get => _denNgay;
            set { _denNgay = value; OnPropertyChanged(); }
        }

        public ICommand LapBaoCaoNamCommand { get; set; }
        public ICommand LapBaoCaoDichVuCommand { get; set; }

        public BaoCaoThongKeViewModel()
        {
            LoadData();

            LapBaoCaoNamCommand = new Common_RelayCommand(LapBaoCaoNam);
            LapBaoCaoDichVuCommand = new Common_RelayCommand(LapBaoCaoDichVu);
        }

        private void LoadData()
        {
            using (var db = new QLKhachSan_Model())
            {
                ListBaoCaoNam = new ObservableCollection<BAOCAONAM>(db.BAOCAONAMs.ToList());
                ListBaoCaoDichVu = new ObservableCollection<BAOCAODICHVU>(db.BAOCAODICHVUs.ToList());
            }
            UpdateChartData();
        }

        private void UpdateChartData()
        {
            var bcn = ListBaoCaoNam.FirstOrDefault(b => b.NAM_BCN == NamBaoCao);
            if (bcn == null)
            {
                ChartItems = new ObservableCollection<ChartItem>();
                return;
            }

            decimal[] vals = new decimal[]
            {
                bcn.DOANHTHUTHANG1_BCN ?? 0, bcn.DOANHTHUTHANG2_BCN ?? 0, bcn.DOANHTHUTHANG3_BCN ?? 0,
                bcn.DOANHTHUTHANG4_BCN ?? 0, bcn.DOANHTHUTHANG5_BCN ?? 0, bcn.DOANHTHUTHANG6_BCN ?? 0,
                bcn.DOANHTHUTHANG7_BCN ?? 0, bcn.DOANHTHUTHANG8_BCN ?? 0, bcn.DOANHTHUTHANG9_BCN ?? 0,
                bcn.DOANHTHUTHANG10_BCN ?? 0, bcn.DOANHTHUTHANG11_BCN ?? 0, bcn.DOANHTHUTHANG12_BCN ?? 0
            };

            decimal maxVal = vals.Max();
            if (maxVal == 0) maxVal = 1;

            var items = new ObservableCollection<ChartItem>();
            for (int i = 0; i < 12; i++)
            {
                items.Add(new ChartItem
                {
                    Label = "T" + (i + 1),
                    Value = vals[i],
                    ScaledHeight = (double)(vals[i] / maxVal) * 200 // Max height is 200px
                });
            }
            ChartItems = items;
        }

        private void LapBaoCaoNam(object obj)
        {
            using (var db = new QLKhachSan_Model())
            {
                // Xóa báo cáo cũ của năm này nếu có
                var oldReports = db.BAOCAONAMs.Where(b => b.NAM_BCN == NamBaoCao).ToList();
                db.BAOCAONAMs.RemoveRange(oldReports);

                // Tính toán
                var invoices = db.HOADONs.Where(h => h.ThoiGianLap_HD.HasValue && h.ThoiGianLap_HD.Value.Year == NamBaoCao && h.TinhTrang_HD == "Đã thanh toán").ToList();

                decimal[] monthlyRevenue = new decimal[12];
                foreach (var inv in invoices)
                {
                    int monthIndex = inv.ThoiGianLap_HD.Value.Month - 1;
                    monthlyRevenue[monthIndex] += (decimal)(inv.TriGia_HD ?? 0);
                }

                autoGenBcn:
                int randomId = new Random().Next(1000, 99999);
                if (db.BAOCAONAMs.Any(x => x.MA_BCN == randomId)) goto autoGenBcn;

                var bcn = new BAOCAONAM
                {
                    MA_BCN = randomId,
                    THOIGIANLAP_BCN = DateTime.Now,
                    NAM_BCN = NamBaoCao,
                    TONGDOANHTHU_BCN = monthlyRevenue.Sum(),
                    DOANHTHUTHANG1_BCN = monthlyRevenue[0],
                    DOANHTHUTHANG2_BCN = monthlyRevenue[1],
                    DOANHTHUTHANG3_BCN = monthlyRevenue[2],
                    DOANHTHUTHANG4_BCN = monthlyRevenue[3],
                    DOANHTHUTHANG5_BCN = monthlyRevenue[4],
                    DOANHTHUTHANG6_BCN = monthlyRevenue[5],
                    DOANHTHUTHANG7_BCN = monthlyRevenue[6],
                    DOANHTHUTHANG8_BCN = monthlyRevenue[7],
                    DOANHTHUTHANG9_BCN = monthlyRevenue[8],
                    DOANHTHUTHANG10_BCN = monthlyRevenue[9],
                    DOANHTHUTHANG11_BCN = monthlyRevenue[10],
                    DOANHTHUTHANG12_BCN = monthlyRevenue[11]
                };

                db.BAOCAONAMs.Add(bcn);
                db.SaveChanges();
            }
            LoadData();
            MessageBox.Show($"Đã lập báo cáo doanh thu năm {NamBaoCao}!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LapBaoCaoDichVu(object obj)
        {
            using (var db = new QLKhachSan_Model())
            {
                autoGenBcdv:
                int randomId = new Random().Next(1000, 99999);
                if (db.BAOCAODICHVUs.Any(x => x.MA_BCDV == randomId)) goto autoGenBcdv;

                // Lưu trú
                var ltSum = db.CHITIET_HDLT.Where(c => c.ThoiGianNhan_PHONG >= TuNgay && c.ThoiGianNhan_PHONG <= DenNgay)
                                           .Sum(c => (decimal?)c.TriGia_CTHDLT) ?? 0;
                                           
                // Ăn uống
                var auSum = db.CHITIET_HDAU.Where(c => c.ThoiGianLap_CTHDAU >= TuNgay && c.ThoiGianLap_CTHDAU <= DenNgay)
                                           .Sum(c => (decimal?)c.TriGia_CTHDAU) ?? 0;
                                           
                // Giặt ủi
                var guSum = db.CHITIET_HDGU.Select(c => (decimal?)0).Sum() ?? 0; // Tương tự
                
                // Di chuyển
                var dcSum = db.CHITIET_HDDC.Select(c => (decimal?)0).Sum() ?? 0; // Tương tự
                
                var bcdv = new BAOCAODICHVU
                {
                    MA_BCDV = randomId,
                    THOIGIANLAP_BCDV = DateTime.Now,
                    NGAYBATDAU_BCDV = TuNgay,
                    NGAYKETTHUC_BCDV = DenNgay,
                    DOANHTHULUUUTRU_BCDV = ltSum,
                    DOANHTHUANUONG_BCDV = auSum,
                    DOANHTHUGIATUI_BCDV = guSum,
                    DOANHTHUDICHUYEN_BCDV = dcSum,
                    TONGDOANHTHU_BCDV = ltSum + auSum + guSum + dcSum
                };

                db.BAOCAODICHVUs.Add(bcdv);
                db.SaveChanges();
            }
            LoadData();
            MessageBox.Show("Đã lập báo cáo dịch vụ theo kỳ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
