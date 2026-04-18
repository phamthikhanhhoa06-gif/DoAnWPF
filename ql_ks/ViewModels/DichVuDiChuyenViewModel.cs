using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class DichVuDiChuyenViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private int _maPhong = 0;
        private string _thongBao = "";

        // Danh sách để hiển thị lên ComboBox điểm đến
        public ObservableCollection<CHUYENDI> DanhSachDiemDen { get; set; }

        // Danh sách phòng chọn được
        public ObservableCollection<PhongChonVM> DanhSachPhong { get; set; }

        // Danh sách đơn đã tạo (chưa thanh toán)
        public ObservableCollection<DonDiChuyenVM> DanhSachDon { get; set; }

        private CHUYENDI _selectedDiemDen;
        private decimal _tongTien = 0;

        public int MaPhong
        {
            get => _maPhong;
            set { _maPhong = value; OnPropertyChanged(); }
        }

        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        public decimal TongTien
        {
            get => _tongTien;
            set { _tongTien = value; OnPropertyChanged(); }
        }

        public CHUYENDI SelectedDiemDen
        {
            get => _selectedDiemDen;
            set
            {
                _selectedDiemDen = value;
                OnPropertyChanged();
                if (value != null) CapNhatThongBao();
            }
        }

        public ICommand LapHoaDonCommand { get; }
        public ICommand LamMoiCommand { get; }
        public ICommand ThemDonCommand { get; } // Thêm đơn mới

        public DichVuDiChuyenViewModel()
        {
            DanhSachDiemDen = new ObservableCollection<CHUYENDI>();
            DanhSachPhong = new ObservableCollection<PhongChonVM>();
            DanhSachDon = new ObservableCollection<DonDiChuyenVM>();

            LapHoaDonCommand = new DiChuyen_RelayCommand(_ => LapHoaDon());
            LamMoiCommand = new DiChuyen_RelayCommand(_ => LamMoi());
            ThemDonCommand = new DiChuyen_RelayCommand(_ => ThemDon());

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                // Load danh sách điểm đến từ DB (Sân bay, Ga Sài Gòn, v.v.)
                var dsDiem = _db.CHUYENDIs.OrderBy(x => x.Ma_CD).ToList();
                DanhSachDiemDen = new ObservableCollection<CHUYENDI>(dsDiem);

                // Load phòng
                var dsPhong = from p in _db.PHONGs
                              join lp in _db.LOAIPHONGs on p.Ma_LP equals lp.Ma_LP
                              select new PhongChonVM
                              {
                                  MaPhong = p.Ma_Phong,
                                  HienThi = p.Ma_Phong + " - " + (lp.Ten_TP ?? "")
                              };

                DanhSachPhong = new ObservableCollection<PhongChonVM>(dsPhong.ToList());

                if (DanhSachDiemDen.Count > 0)
                    SelectedDiemDen = DanhSachDiemDen.First();

                ThongBao = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu di chuyển: " + ex.Message);
            }
        }

        private void ThemDon()
        {
            if (_maPhong == 0)
            {
                ThongBao = "Vui lòng nhập số phòng!";
                return;
            }

            if (_selectedDiemDen == null)
            {
                ThongBao = "Vui lòng chọn điểm đến!";
                return;
            }

            // Kiểm tra đã có đơn này chưa (cùng phòng + cùng điểm đến)?
            var exist = DanhSachDon.FirstOrDefault(x => x.MaPhong == _maPhong && x.Ma_CD == SelectedDiemDen.Ma_CD);

            if (exist != null)
            {
                exist.SoLuong++;
            }
            else
            {
                DanhSachDon.Add(new DonDiChuyenVM
                {
                    MaPhong = _maPhong,
                    Ma_CD = SelectedDiemDen.Ma_CD,
                    DiemDen_CD = SelectedDiemDen.DiemDen_CD,
                    DonGia_CD = SelectedDiemDen.DonGia_CD ?? 0,
                    SoLuong = 1,
                    NgayDat = DateTime.Now,
                    TrangThai = "Chờ xử lý"
                });
            }

            CapNhatTongTien();
            ThongBao = $"Đã thêm dịch vụ đi đến: {SelectedDiemDen.DiemDen_CD} cho phòng {_maPhong}";
        }

        private void LapHoaDon()
        {
            if (_maPhong == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DanhSachDon.Count == 0)
            {
                MessageBox.Show("Chưa có đơn nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Tạo CHITIET_HDDC cho từng đơn trong danh sách
                foreach (var don in DanhSachDon)
                {
                    CHITIET_HDDC ct = new CHITIET_HDDC
                    {
                        Ma_CD = don.Ma_CD,
                        TriGia_CTHDDC = don.DonGia_CD * don.SoLuong,
                        ThoiGianLap_CTHDDC = DateTime.Now
                    };

                    _db.CHITIET_HDDC.Add(ct);
                }

                _db.SaveChanges();

                MessageBox.Show(
                    $"Đã tạo hóa đơn di chuyển thành công!\n\n" +
                    $"Tổng số đơn: {DanhSachDon.Count}\n" +
                    $"Tổng tiền: {TongTien:N0} đ",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LamMoi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu hóa đơn di chuyển: " + ex.Message);
            }
        }

        private void LamMoi()
        {
            MaPhong = 0;
            ThongBao = "Đã làm mới dữ liệu.";
            DanhSachDon.Clear();
            TongTien = 0;

            if (DanhSachDiemDen.Count > 0)
                SelectedDiemDen = DanhSachDiemDen.First();
        }

        public void CapNhatTongTien()
        {
            TongTien = DanhSachDon.Sum(x => x.ThanhTien);
        }

        private void CapNhatThongBao()
        {
            if (_selectedDiemDen != null)
            {
                ThongBao = $"Giá: {_selectedDiemDen.DonGia_CD:N0} đ";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Class hiển thị đơn tạm thời
    public class DonDiChuyenVM : INotifyPropertyChanged
    {
        private int _soLuong = 1;

        public int MaPhong { get; set; }
        public int Ma_CD { get; set; }
        public string DiemDen_CD { get; set; }
        public long DonGia_CD { get; set; }
        public DateTime NgayDat { get; set; }
        public string TrangThai { get; set; }

        public int SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value < 1 ? 1 : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public long ThanhTien => DonGia_CD * SoLuong;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper cho phòng
    public class PhongChonDC_VM
    {
        public int MaPhong { get; set; }
        public string HienThi { get; set; }
    }
}
