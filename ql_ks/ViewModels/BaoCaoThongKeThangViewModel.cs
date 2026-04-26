using System;
using System.Collections.Generic;
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
    public class BaoCaoThongKeThangViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private int _namChon;
        public int NamChon
        {
            get => _namChon;
            set { _namChon = value; OnPropertyChanged(); }
        }

        private int _thangChon;
        public int ThangChon
        {
            get => _thangChon;
            set { _thangChon = value; OnPropertyChanged(); }
        }

        private decimal _tongDoanhThuThang;
        public decimal TongDoanhThuThang
        {
            get => _tongDoanhThuThang;
            set { _tongDoanhThuThang = value; OnPropertyChanged(); }
        }

        private string _thongBao = "";
        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        public ObservableCollection<int> NamList { get; set; }
        public ObservableCollection<int> ThangList { get; set; }
        public ObservableCollection<NgayThangItem> ChiTietNgayList { get; set; }
        public ObservableCollection<BieuDoNgayItem> BieuDoNgayList { get; set; }

        public ICommand TimKiemCommand { get; }

        public BaoCaoThongKeThangViewModel()
        {
            NamList = new ObservableCollection<int>();
            for (int y = DateTime.Now.Year - 5; y <= DateTime.Now.Year + 1; y++)
                NamList.Add(y);

            ThangList = new ObservableCollection<int>();
            for (int m = 1; m <= 12; m++)
                ThangList.Add(m);

            NamChon = DateTime.Now.Year;
            ThangChon = DateTime.Now.Month;

            ChiTietNgayList = new ObservableCollection<NgayThangItem>();
            BieuDoNgayList = new ObservableCollection<BieuDoNgayItem>();

            TimKiemCommand = new RelayCommand(_ => LoadData());

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var bcn = _db.BAOCAONAMs.FirstOrDefault(x => x.NAM_BCN == NamChon);

                if (bcn == null)
                {
                    ChiTietNgayList.Clear();
                    BieuDoNgayList.Clear();
                    TongDoanhThuThang = 0;
                    ThongBao = "Không tìm thấy báo cáo năm " + NamChon;
                    return;
                }

                decimal doanhThuThang = GetDoanhThuThang(bcn, ThangChon);
                TongDoanhThuThang = doanhThuThang;

                int soNgay = DateTime.DaysInMonth(NamChon, ThangChon);

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
                    new SolidColorBrush(Color.FromRgb(0, 121, 107)),
                    new SolidColorBrush(Color.FromRgb(183, 49, 49)),
                    new SolidColorBrush(Color.FromRgb(0, 98, 204)),
                    new SolidColorBrush(Color.FromRgb(163, 120, 0)),
                    new SolidColorBrush(Color.FromRgb(0, 151, 167)),
                    new SolidColorBrush(Color.FromRgb(136, 23, 152)),
                    new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                    new SolidColorBrush(Color.FromRgb(96, 125, 139)),
                    new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                    new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                    new SolidColorBrush(Color.FromRgb(103, 58, 183)),
                    new SolidColorBrush(Color.FromRgb(0, 188, 212)),
                    new SolidColorBrush(Color.FromRgb(121, 85, 72)),
                    new SolidColorBrush(Color.FromRgb(63, 81, 181)),
                    new SolidColorBrush(Color.FromRgb(139, 195, 74)),
                    new SolidColorBrush(Color.FromRgb(255, 87, 34)),
                };

                ChiTietNgayList.Clear();
                BieuDoNgayList.Clear();

                for (int day = 1; day <= soNgay; day++)
                {
                    decimal doanhThuNgay = doanhThuThang > 0
                        ? Math.Round(doanhThuThang / soNgay, 0)
                        : 0;

                    // ✅ Thêm SoThuTu để sort đúng thứ tự
                    ChiTietNgayList.Add(new NgayThangItem
                    {
                        SoThuTu = day,
                        Ngay = "Ngày " + day,
                        DoanhThu = doanhThuNgay,
                        TyLe = doanhThuThang > 0
                            ? doanhThuNgay / doanhThuThang
                            : 0
                    });

                    BieuDoNgayList.Add(new BieuDoNgayItem
                    {
                        Ngay = "Ngày " + day,
                        DoanhThu = doanhThuNgay,
                        Mau = colors[(day - 1) % colors.Length],
                        ChieuCaoBar = doanhThuThang > 0 ? 20 : 0
                    });
                }

                ThongBao = "Báo cáo tháng " + ThangChon + "/" + NamChon;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private decimal GetDoanhThuThang(BAOCAONAM bcn, int thang)
        {
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

    // ✅ Thêm SoThuTu
    public class NgayThangItem
    {
        public int SoThuTu { get; set; }
        public string Ngay { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyLe { get; set; }
    }

    public class BieuDoNgayItem
    {
        public string Ngay { get; set; }
        public decimal DoanhThu { get; set; }
        public Brush Mau { get; set; }
        public double ChieuCaoBar { get; set; }
    }
}