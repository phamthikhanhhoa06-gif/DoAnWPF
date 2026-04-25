using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class TraCuuHoaDonViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private bool _isAddingNew = false;
        public bool IsAddingNew
        {
            get => _isAddingNew;
            set { _isAddingNew = value; OnPropertyChanged(); }
        }

        private List<HOADON> _allHoaDons;
        public ObservableCollection<HOADON_Display> DanhSachHienThi { get; set; }

        public ObservableCollection<string> TinhTrangList { get; set; }
        public ObservableCollection<NHANVIEN> DanhSachNhanVien { get; set; }
        public ObservableCollection<KHACHHANG> DanhSachKhachHang { get; set; }

        private string _tuKhoaTimKiem = "";
        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set { _tuKhoaTimKiem = value; OnPropertyChanged(); LocTheoDieuKien(); }
        }

        private string _locTinhTrang = "Tất cả";
        public string LocTinhTrang
        {
            get => _locTinhTrang;
            set { _locTinhTrang = value; OnPropertyChanged(); LocTheoDieuKien(); }
        }

        private DateTime? _ngayBatDau;
        public DateTime? NgayBatDau
        {
            get => _ngayBatDau;
            set { _ngayBatDau = value; OnPropertyChanged(); LocTheoDieuKien(); }
        }

        private DateTime? _ngayKetThuc;
        public DateTime? NgayKetThuc
        {
            get => _ngayKetThuc;
            set { _ngayKetThuc = value; OnPropertyChanged(); LocTheoDieuKien(); }
        }

        private string _thongBao = "";
        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        private HOADON_Display _selectedHoaDon;
        public HOADON_Display SelectedHoaDon
        {
            get => _selectedHoaDon;
            set { _selectedHoaDon = value; OnPropertyChanged(); }
        }

        public ICommand ThemCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }

        public TraCuuHoaDonViewModel()
        {
            DanhSachHienThi = new ObservableCollection<HOADON_Display>();
            DanhSachNhanVien = new ObservableCollection<NHANVIEN>();
            DanhSachKhachHang = new ObservableCollection<KHACHHANG>();
            TinhTrangList = new ObservableCollection<string>
            {
                "Tất cả",
                "Chưa thanh toán",
                "Đã thanh toán",
                "Đã hủy"
            };
            LocTinhTrang = "Tất cả";
            SelectedHoaDon = new HOADON_Display();

            ThemCommand = new TCHoaDon_RelayCommand(_ => Them());
            LuuCommand = new TCHoaDon_RelayCommand(_ => Luu());
            SuaCommand = new TCHoaDon_RelayCommand(_ => Sua());
            XoaCommand = new TCHoaDon_RelayCommand(_ => Xoa());
            LamMoiCommand = new TCHoaDon_RelayCommand(_ => LamMoi());

            TaiDanhSachNhanVien();
            TaiDanhSachKhachHang();
            TaiDuLieu();
        }

        private void TaiDanhSachNhanVien()
        {
            try
            {
                var list = _db.NHANVIENs.OrderBy(nv => nv.HoTen_NV).ToList();
                DanhSachNhanVien.Clear();
                foreach (var nv in list) DanhSachNhanVien.Add(nv);
            }
            catch (Exception ex) { /* bỏ qua */ }
        }

        private void TaiDanhSachKhachHang()
        {
            try
            {
                var list = _db.KHACHHANGs.OrderBy(kh => kh.HoTen_KH).ToList();
                DanhSachKhachHang.Clear();
                foreach (var kh in list) DanhSachKhachHang.Add(kh);
            }
            catch (Exception ex) { /* bỏ qua */ }
        }

        private void TaiDuLieu()
        {
            try
            {
                _allHoaDons = _db.HOADONs
                    .Include(hd => hd.NHANVIEN)
                    .Include(hd => hd.KHACHHANG)
                    .OrderByDescending(hd => hd.ThoiGianLap_HD)
                    .ToList();

                LocTheoDieuKien();
                ThongBao = $"Tổng số hóa đơn: {_allHoaDons.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void LocTheoDieuKien()
        {
            if (_allHoaDons == null) return;

            var result = _allHoaDons.AsEnumerable();

            // Tìm kiếm theo từ khóa
            var keyword = (_tuKhoaTimKiem ?? "").Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                result = result.Where(hd =>
                    hd.MA_HD.ToString().Contains(keyword) ||
                    (hd.NHANVIEN?.HoTen_NV ?? "").ToLower().Contains(keyword) ||
                    (hd.KHACHHANG?.HoTen_KH ?? "").ToLower().Contains(keyword));
            }

            // Lọc tình trạng
            if (!string.IsNullOrEmpty(_locTinhTrang) && _locTinhTrang != "Tất cả")
            {
                result = result.Where(hd => (hd.TinhTrang_HD ?? "") == _locTinhTrang);
            }

            // Lọc theo ngày lập
            if (_ngayBatDau.HasValue && !_ngayKetThuc.HasValue)
                result = result.Where(hd => hd.ThoiGianLap_HD >= _ngayBatDau);
            else if (!_ngayBatDau.HasValue && _ngayKetThuc.HasValue)
                result = result.Where(hd => hd.ThoiGianLap_HD <= _ngayKetThuc);
            else if (_ngayBatDau.HasValue && _ngayKetThuc.HasValue)
                result = result.Where(hd => hd.ThoiGianLap_HD >= _ngayBatDau && hd.ThoiGianLap_HD <= _ngayKetThuc);

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả: {DanhSachHienThi.Count} / {_allHoaDons.Count} hóa đơn";
        }

        private void CapNhatDanhSach(List<HOADON> list)
        {
            DanhSachHienThi.Clear();
            foreach (var hd in list)
            {
                DanhSachHienThi.Add(new HOADON_Display
                {
                    MA_HD = hd.MA_HD,
                    TenNhanVien = hd.NHANVIEN?.HoTen_NV ?? "",
                    TenKhachHang = hd.KHACHHANG?.HoTen_KH ?? "",
                    ThoiGianLap_HD = hd.ThoiGianLap_HD,
                    TinhTrang_HD = hd.TinhTrang_HD ?? "",
                    TriGia_HD = hd.TriGia_HD ?? 0,
                    MA_NV = hd.MA_NV,
                    MA_KH = hd.MA_KH
                });
            }
        }

        public void Them()
        {
            IsAddingNew = true;
            int nextId = _allHoaDons.Any() ? _allHoaDons.Max(x => x.MA_HD) + 1 : 1;
            SelectedHoaDon = new HOADON_Display
            {
                MA_HD = nextId,
                ThoiGianLap_HD = DateTime.Now,
                TinhTrang_HD = "Chưa thanh toán",
                TriGia_HD = 0,
                MA_NV = DanhSachNhanVien.FirstOrDefault()?.MA_NV,
                MA_KH = DanhSachKhachHang.FirstOrDefault()?.MA_KH,
                TenNhanVien = "",
                TenKhachHang = ""
            };
            ThongBao = "Nhập thông tin hóa đơn vào form bên trái, sau đó bấm LƯU";
        }

        public void Luu()
        {
            if (SelectedHoaDon == null || SelectedHoaDon.MA_HD <= 0)
            {
                MessageBox.Show("Mã hóa đơn không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var exists = _db.HOADONs.Any(hd => hd.MA_HD == SelectedHoaDon.MA_HD);

                if (IsAddingNew && exists)
                {
                    MessageBox.Show("Mã hóa đơn đã tồn tại!", "Lỗi");
                    return;
                }

                if (IsAddingNew || !exists)
                {
                    // THÊM MỚI
                    var hd = new HOADON
                    {
                        MA_HD = SelectedHoaDon.MA_HD,
                        ThoiGianLap_HD = SelectedHoaDon.ThoiGianLap_HD ?? DateTime.Now,
                        TinhTrang_HD = SelectedHoaDon.TinhTrang_HD,
                        TriGia_HD = SelectedHoaDon.TriGia_HD,
                        MA_NV = SelectedHoaDon.MA_NV,
                        MA_KH = SelectedHoaDon.MA_KH
                    };

                    _db.HOADONs.Add(hd);
                    _db.SaveChanges();

                    MessageBox.Show("Thêm hóa đơn thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAddingNew = false;
                    TaiDuLieu();
                }
                else
                {
                    // CẬP NHẬT
                    var hd = _db.HOADONs.Find(SelectedHoaDon.MA_HD);
                    if (hd != null)
                    {
                        hd.ThoiGianLap_HD = SelectedHoaDon.ThoiGianLap_HD;
                        hd.TinhTrang_HD = SelectedHoaDon.TinhTrang_HD;
                        hd.TriGia_HD = SelectedHoaDon.TriGia_HD;
                        hd.MA_NV = SelectedHoaDon.MA_NV;
                        hd.MA_KH = SelectedHoaDon.MA_KH;

                        _db.SaveChanges();
                        MessageBox.Show("Cập nhật thành công!", "Thành công");
                        TaiDuLieu();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Sua()
        {
            if (SelectedHoaDon == null || SelectedHoaDon.MA_HD <= 0)
            {
                ThongBao = "Vui lòng chọn 1 hóa đơn để sửa!";
                return;
            }
            IsAddingNew = false;
            ThongBao = "Chỉnh sửa thông tin bên trái, sau đó bấm LƯU";
        }

        public void Xoa()
        {
            if (SelectedHoaDon == null || SelectedHoaDon.MA_HD <= 0)
            {
                ThongBao = "Vui lòng chọn 1 hóa đơn để xóa!";
                return;
            }

            var rs = MessageBox.Show(
                $"Xóa hóa đơn:\nMã: {SelectedHoaDon.MA_HD}\nKhách: {SelectedHoaDon.TenKhachHang}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (rs == MessageBoxResult.Yes)
            {
                try
                {
                    var hd = _db.HOADONs.Find(SelectedHoaDon.MA_HD);
                    if (hd != null)
                    {
                        _db.HOADONs.Remove(hd);
                        _db.SaveChanges();
                        TaiDuLieu();
                        SelectedHoaDon = new HOADON_Display();
                        ThongBao = "Đã xóa thành công!";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa (có thể do ràng buộc dữ liệu): " + ex.Message, "Lỗi");
                }
            }
        }

        public void LamMoi()
        {
            TuKhoaTimKiem = "";
            LocTinhTrang = "Tất cả";
            NgayBatDau = null;
            NgayKetThuc = null;
            SelectedHoaDon = new HOADON_Display();
            IsAddingNew = false;
            TaiDuLieu();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HOADON_Display : INotifyPropertyChanged
    {
        private int _maHD;
        public int MA_HD
        {
            get => _maHD;
            set { _maHD = value; OnPropertyChanged(); }
        }

        public string TenNhanVien { get; set; }
        public string TenKhachHang { get; set; }

        private DateTime? _thoiGianLap;
        public DateTime? ThoiGianLap_HD
        {
            get => _thoiGianLap;
            set { _thoiGianLap = value; OnPropertyChanged(); }
        }

        private string _tinhTrang;
        public string TinhTrang_HD
        {
            get => _tinhTrang;
            set { _tinhTrang = value; OnPropertyChanged(); }
        }

        private long? _triGia;
        public long? TriGia_HD
        {
            get => _triGia;
            set { _triGia = value; OnPropertyChanged(); }
        }

        public int? MA_NV { get; set; }
        public int? MA_KH { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}