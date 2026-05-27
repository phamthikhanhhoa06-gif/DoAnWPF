using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class BaoCaoThongKeKhachHangViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        // === PROPERTIES ===
        private DateTime _ngayBatDau;
        public DateTime NgayBatDau
        {
            get => _ngayBatDau;
            set { _ngayBatDau = value; OnPropertyChanged(); }
        }

        private DateTime _ngayKetThuc;
        public DateTime NgayKetThuc
        {
            get => _ngayKetThuc;
            set { _ngayKetThuc = value; OnPropertyChanged(); }
        }

        private decimal _tongDoanhThu;
        public decimal TongDoanhThu
        {
            get => _tongDoanhThu;
            set { _tongDoanhThu = value; OnPropertyChanged(); }
        }

        private int _tongSoKhach;
        public int TongSoKhach
        {
            get => _tongSoKhach;
            set { _tongSoKhach = value; OnPropertyChanged(); }
        }

        private string _thongBao = "";
        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        public ObservableCollection<KhachHangDoanhThuItem> ChiTietKhachHangList { get; set; }
        public ObservableCollection<BieuDoKhachHangItem> BieuDoKhachHangList { get; set; }

        public ICommand TimKiemCommand { get; }

        // === CONSTRUCTOR ===
        public BaoCaoThongKeKhachHangViewModel()
        {
            ChiTietKhachHangList = new ObservableCollection<KhachHangDoanhThuItem>();
            BieuDoKhachHangList = new ObservableCollection<BieuDoKhachHangItem>();

            TimKiemCommand = new RelayCommand(_ => LoadData());

            NgayBatDau = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            NgayKetThuc = DateTime.Now;

            LoadData();
        }

        // === LOAD DATA ===
        private void LoadData()
        {
            try
            {
                if (NgayBatDau > NgayKetThuc)
                {
                    ThongBao = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc!";
                    return;
                }

                DateTime tuNgay = NgayBatDau.Date;
                DateTime denNgay = NgayKetThuc.Date.AddDays(1);

                // Truy vấn doanh thu theo khách hàng
                // Dùng ThoiGianLap_HD và TriGia_HD (long?)
                var query = _db.HOADONs
                    .Include(hd => hd.KHACHHANG)
                    .Where(hd => hd.ThoiGianLap_HD >= tuNgay
                              && hd.ThoiGianLap_HD < denNgay
                              && hd.MA_KH != null
                              && hd.TriGia_HD != null)
                    .GroupBy(hd => new
                    {
                        hd.MA_KH,
                        hd.KHACHHANG.HoTen_KH,
                        hd.KHACHHANG.SoDienThoai_KH
                    })
                    .Select(g => new
                    {
                        MaKH = g.Key.MA_KH,
                        HoTen = g.Key.HoTen_KH,
                        SoDienThoai = g.Key.SoDienThoai_KH,
                        TongTien = g.Sum(x => x.TriGia_HD ?? 0),
                        SoHoaDon = g.Count()
                    })
                    .OrderByDescending(x => x.TongTien)
                    .ToList();

                if (!query.Any())
                {
                    ChiTietKhachHangList.Clear();
                    BieuDoKhachHangList.Clear();
                    TongDoanhThu = 0;
                    TongSoKhach = 0;
                    ThongBao = "Không có dữ liệu trong khoảng thời gian đã chọn!";
                    return;
                }

                // Tính tổng (TriGia_HD là long, ép sang decimal)
                decimal tongDoanhThu = query.Sum(x => (decimal)x.TongTien);
                TongDoanhThu = tongDoanhThu;
                TongSoKhach = query.Count;

                // Màu sắc cho biểu đồ
                Brush[] colors = new Brush[]
                {
                    new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                    new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                    new SolidColorBrush(Color.FromRgb(241, 196, 15)),
                    new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                    new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                    new SolidColorBrush(Color.FromRgb(26, 188, 156)),
                    new SolidColorBrush(Color.FromRgb(230, 126, 34)),
                    new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                    new SolidColorBrush(Color.FromRgb(22, 160, 133)),
                    new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                    new SolidColorBrush(Color.FromRgb(41, 128, 185)),
                    new SolidColorBrush(Color.FromRgb(142, 68, 173)),
                    new SolidColorBrush(Color.FromRgb(39, 174, 96)),
                    new SolidColorBrush(Color.FromRgb(211, 84, 0)),
                    new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                };

                ChiTietKhachHangList.Clear();
                BieuDoKhachHangList.Clear();

                // Bảng chi tiết
                int stt = 0;
                foreach (var item in query)
                {
                    stt++;
                    decimal doanhThu = (decimal)item.TongTien;
                    decimal tyLe = tongDoanhThu > 0 ? doanhThu / tongDoanhThu : 0;

                    ChiTietKhachHangList.Add(new KhachHangDoanhThuItem
                    {
                        STT = stt,
                        MaKH = item.MaKH ?? 0,
                        HoTen = item.HoTen ?? "N/A",
                        SoDienThoai = item.SoDienThoai ?? "N/A",
                        SoHoaDon = item.SoHoaDon,
                        DoanhThu = doanhThu,
                        TyLe = tyLe
                    });
                }

                // Biểu đồ tròn - Top 10 + Khác
                var topItems = query.Take(10).ToList();
                decimal tongTopDoanhThu = topItems.Sum(x => (decimal)x.TongTien);
                decimal doanhThuKhac = tongDoanhThu - tongTopDoanhThu;

                double gocHienTai = 0;

                for (int i = 0; i < topItems.Count; i++)
                {
                    decimal doanhThu = (decimal)topItems[i].TongTien;
                    decimal tyLe = tongDoanhThu > 0 ? doanhThu / tongDoanhThu : 0;
                    double goc = (double)tyLe * 360;

                    BieuDoKhachHangList.Add(new BieuDoKhachHangItem
                    {
                        TenKhachHang = topItems[i].HoTen ?? "N/A",
                        MaKH = topItems[i].MaKH ?? 0,
                        DoanhThu = doanhThu,
                        TyLe = tyLe,
                        Mau = colors[i % colors.Length],
                        GocBatDau = gocHienTai,
                        GocKet = goc
                    });

                    gocHienTai += goc;
                }

                // Phần "Khác"
                if (doanhThuKhac > 0)
                {
                    decimal tyLeKhac = tongDoanhThu > 0
                        ? doanhThuKhac / tongDoanhThu : 0;
                    double gocKhac = (double)tyLeKhac * 360;

                    BieuDoKhachHangList.Add(new BieuDoKhachHangItem
                    {
                        TenKhachHang = "Khác",
                        MaKH = 0,
                        DoanhThu = doanhThuKhac,
                        TyLe = tyLeKhac,
                        Mau = new SolidColorBrush(Color.FromRgb(189, 195, 199)),
                        GocBatDau = gocHienTai,
                        GocKet = gocKhac
                    });
                }

                ThongBao = $"Báo cáo từ {NgayBatDau:dd/MM/yyyy} đến " +
                           $"{NgayKetThuc:dd/MM/yyyy} - {TongSoKhach} khách hàng";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class KhachHangDoanhThuItem
    {
        public int STT { get; set; }
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public int SoHoaDon { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyLe { get; set; }
    }

    public class BieuDoKhachHangItem
    {
        public string TenKhachHang { get; set; }
        public int MaKH { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyLe { get; set; }
        public Brush Mau { get; set; }
        public double GocBatDau { get; set; }
        public double GocKet { get; set; }
    }
}