using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class BaoCaoThongKeNamViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private int _namChon;
        public int NamChon
        {
            get => _namChon;
            set { _namChon = value; OnPropertyChanged(); LoadData(); }
        }

        private decimal _tongDoanhThuNam;
        public decimal TongDoanhThuNam
        {
            get => _tongDoanhThuNam;
            set { _tongDoanhThuNam = value; OnPropertyChanged(); }
        }

        private string _thongBao = "";
        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        public ObservableCollection<int> NamList { get; set; }
        public ObservableCollection<ThangNamItem> ChiTietThangList { get; set; }
        public ObservableCollection<BieuDoNamItem> BieuDoNamList { get; set; }

        public ICommand TimKiemCommand { get; }

        public BaoCaoThongKeNamViewModel()
        {
            NamList = new ObservableCollection<int>();
            ChiTietThangList = new ObservableCollection<ThangNamItem>();
            BieuDoNamList = new ObservableCollection<BieuDoNamItem>();

            TimKiemCommand = new RelayCommand(_ => LoadData());

            for (int y = DateTime.Now.Year - 5; y <= DateTime.Now.Year + 1; y++)
                NamList.Add(y);

            NamChon = DateTime.Now.Year;
        }

        private void LoadData()
        {
            try
            {
                var bcn = _db.BAOCAONAMs.FirstOrDefault(x => x.NAM_BCN == NamChon);

                if (bcn == null)
                {
                    ChiTietThangList.Clear();
                    BieuDoNamList.Clear();
                    TongDoanhThuNam = 0;
                    ThongBao = "Không tìm thấy báo cáo năm " + NamChon;
                    return;
                }

                TongDoanhThuNam = bcn.TONGDOANHTHU_BCN ?? 0;

                Brush[] colors = new Brush[]
                {
                    new SolidColorBrush(Color.FromRgb(52, 152, 219)),   // Jan
                    new SolidColorBrush(Color.FromRgb(46, 204, 113)),   // Feb
                    new SolidColorBrush(Color.FromRgb(241, 196, 15)),   // Mar
                    new SolidColorBrush(Color.FromRgb(231, 76, 60)),    // Apr
                    new SolidColorBrush(Color.FromRgb(155, 89, 182)),   // May
                    new SolidColorBrush(Color.FromRgb(26, 188, 156)),   // Jun
                    new SolidColorBrush(Color.FromRgb(230, 126, 34)),   // Jul
                    new SolidColorBrush(Color.FromRgb(52, 73, 94)),     // Aug
                    new SolidColorBrush(Color.FromRgb(22, 160, 133)),   // Sep
                    new SolidColorBrush(Color.FromRgb(192, 57, 43)),    // Oct
                    new SolidColorBrush(Color.FromRgb(41, 128, 185)),   // Nov
                    new SolidColorBrush(Color.FromRgb(142, 68, 173))    // Dec
                };

                ChiTietThangList.Clear();
                BieuDoNamList.Clear();

                // 1. Thêm dữ liệu vào List trước
                for (int thang = 1; thang <= 12; thang++)
                {
                    decimal doanhThuThang = GetDoanhThuThang(bcn, thang);
                    decimal tyLe = TongDoanhThuNam > 0 ? doanhThuThang / TongDoanhThuNam : 0;

                    ChiTietThangList.Add(new ThangNamItem
                    {
                        Thang = thang,
                        TenThang = "Tháng " + thang,
                        DoanhThu = doanhThuThang,
                        TyLe = tyLe
                    });
                }

                // 2. Tính Max SAU KHI đã có dữ liệu
                decimal maxDoanhThu = ChiTietThangList.Any()
                    ? ChiTietThangList.Max(t => t.DoanhThu)
                    : 1;

                double maxBarHeight = 220;

                // 3. Tạo biểu đồ dựa trên max đã tính
                for (int thang = 1; thang <= 12; thang++)
                {
                    var item = ChiTietThangList[thang - 1]; // Lấy từ list đã thêm
                    double chieuCao = maxDoanhThu > 0
                        ? (double)(item.DoanhThu / maxDoanhThu) * maxBarHeight
                        : 15;

                    BieuDoNamList.Add(new BieuDoNamItem
                    {
                        Thang = "T" + thang,
                        TenThangDayDu = "Tháng " + thang,
                        DoanhThu = item.DoanhThu,
                        TyLe = item.TyLe,           // Thêm TyLe vào đây
                        Mau = colors[thang - 1],
                        ChieuCaoBar = chieuCao > 15 ? chieuCao : 15,
                        ShowValue = chieuCao > 40   // Hiện số nếu cột đủ cao
                    });
                }

                ThongBao = "Báo cáo năm " + NamChon;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private decimal GetDoanhThuThang(BAOCAONAM bcn, int thang)
        {
            if (bcn == null) return 0;

            switch (thang)
            {
                case 1: return bcn.DOANHTHUTHANG1_BCN ?? 0;
                case 2: return bcn.DOANHTHUTHANG2_BCN ?? 0;
                case 3: return bcn.DOANHTHUTHANG3_BCN ?? 0;
                case 4: return bcn.DOANHTHUTHANG4_BCN ?? 0;
                case 5: return bcn.DOANHTHUTHANG5_BCN ?? 0;
                case 6: return bcn.DOANHTHUTHANG6_BCN ?? 0;
                case 7: return bcn.DOANHTHUTHANG7_BCN ?? 0;
                case 8: return bcn.DOANHTHUTHANG8_BCN ?? 0;
                case 9: return bcn.DOANHTHUTHANG9_BCN ?? 0;
                case 10: return bcn.DOANHTHUTHANG10_BCN ?? 0;
                case 11: return bcn.DOANHTHUTHANG11_BCN ?? 0;
                case 12: return bcn.DOANHTHUTHANG12_BCN ?? 0;
                default: return 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThangNamItem
    {
        public int Thang { get; set; }
        public string TenThang { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyLe { get; set; }
    }

    // Giữ lại class này ở cuối, xóa class trùng ở đầu
    public class BieuDoNamItem
    {
        public string Thang { get; set; }
        public string TenThangDayDu { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyLe { get; set; }        // Thêm dòng này
        public Brush Mau { get; set; }
        public double ChieuCaoBar { get; set; }
        public bool ShowValue { get; set; }
    }
}