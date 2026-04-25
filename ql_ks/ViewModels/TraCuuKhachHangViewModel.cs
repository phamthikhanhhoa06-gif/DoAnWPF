using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;
using System.Data.Entity; // Thêm dòng này cho .Include()

namespace ql_ks.ViewModels
{
    public class TraCuuKhachHangViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private bool _isAddingNew = false;
        public bool IsAddingNew
        {
            get => _isAddingNew;
            set { _isAddingNew = value; OnPropertyChanged(); }
        }

        private List<KHACHHANG> _allKhachHangs;
        public ObservableCollection<KHACHHANG_Display> DanhSachHienThi { get; set; }

        private string _tuKhoaTimKiem = "";
        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set { _tuKhoaTimKiem = value; OnPropertyChanged(); TimKiem(); }
        }

        private string _thongBao = "";
        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        private KHACHHANG_Display _selectedKhachHang;
        public KHACHHANG_Display SelectedKhachHang
        {
            get => _selectedKhachHang;
            set { _selectedKhachHang = value; OnPropertyChanged(); }
        }

        public ICommand ThemCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }

        public TraCuuKhachHangViewModel()
        {
            DanhSachHienThi = new ObservableCollection<KHACHHANG_Display>();
            SelectedKhachHang = new KHACHHANG_Display();

            ThemCommand = new TCNhanVien_RelayCommand((Action<object>)(_ => Them()));
            LuuCommand = new TCNhanVien_RelayCommand((Action<object>)(_ => Luu()));
            SuaCommand = new TCNhanVien_RelayCommand((Action<object>)(_ => Sua()));
            XoaCommand = new TCNhanVien_RelayCommand((Action<object>)(_ => Xoa()));
            LamMoiCommand = new TCNhanVien_RelayCommand((Action<object>)(_ => LamMoi()));

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                _allKhachHangs = _db.KHACHHANGs
                    .Include(kh => kh.TAIKHOAN)
                    .OrderByDescending(kh => kh.MA_KH)
                    .ToList();

                CapNhatDanhSach(_allKhachHangs);
                ThongBao = $"Tổng số khách hàng: {_allKhachHangs.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void TimKiem()
        {
            if (_allKhachHangs == null) return;

            var keyword = (_tuKhoaTimKiem ?? "").Trim().ToLower();

            var result = string.IsNullOrWhiteSpace(keyword)
                ? _allKhachHangs
                : _allKhachHangs.Where(kh =>
                    (kh.HoTen_KH ?? "").ToLower().Contains(keyword) ||
                    kh.MA_KH.ToString().Contains(keyword) ||
                    (kh.SoDienThoai_KH ?? "").Contains(keyword) ||
                    (kh.CMND_KH ?? "").Contains(keyword));

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả: {result.Count()} khách hàng";
        }

        private void CapNhatDanhSach(List<KHACHHANG> list)
        {
            DanhSachHienThi.Clear();
            foreach (var kh in list)
            {
                DanhSachHienThi.Add(new KHACHHANG_Display
                {
                    MA_KH = kh.MA_KH,
                    HoTen_KH = kh.HoTen_KH,
                    SoDienThoai_KH = kh.SoDienThoai_KH,
                    CMND_KH = kh.CMND_KH,
                    Email = kh.TAIKHOAN?.TenDangNhap_TK ?? ""
                });
            }
        }

        public void Them()
        {
            IsAddingNew = true;
            SelectedKhachHang = new KHACHHANG_Display
            {
                MA_KH = 0,
                HoTen_KH = "",
                SoDienThoai_KH = "",
                CMND_KH = ""
            };
            ThongBao = "Nhập thông tin khách hàng vào form bên trái, sau đó bấm LƯU";
        }

        public void Luu()
        {
            if (SelectedKhachHang == null || string.IsNullOrWhiteSpace(SelectedKhachHang.HoTen_KH))
            {
                MessageBox.Show("Họ tên không được để trống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (IsAddingNew || SelectedKhachHang.MA_KH == 0)
                {
                    // THÊM MỚI
                    int newMa = _db.KHACHHANGs.Any() ? _db.KHACHHANGs.Max(x => x.MA_KH) + 1 : 1;

                    if (_db.KHACHHANGs.Any(x => x.MA_KH == newMa))
                    {
                        MessageBox.Show("Mã khách hàng đã tồn tại!", "Lỗi");
                        return;
                    }

                    var kh = new KHACHHANG
                    {
                        MA_KH = newMa,
                        HoTen_KH = SelectedKhachHang.HoTen_KH,
                        SoDienThoai_KH = SelectedKhachHang.SoDienThoai_KH,
                        CMND_KH = SelectedKhachHang.CMND_KH,
                        Ma_TK = null
                    };

                    _db.KHACHHANGs.Add(kh);
                    _db.SaveChanges();

                    MessageBox.Show("Thêm khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAddingNew = false;
                    TaiDuLieu();
                }
                else
                {
                    // CẬP NHẬT
                    var kh = _db.KHACHHANGs.Find(SelectedKhachHang.MA_KH);
                    if (kh != null)
                    {
                        kh.HoTen_KH = SelectedKhachHang.HoTen_KH;
                        kh.SoDienThoai_KH = SelectedKhachHang.SoDienThoai_KH;
                        kh.CMND_KH = SelectedKhachHang.CMND_KH;

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
            if (SelectedKhachHang == null || SelectedKhachHang.MA_KH == 0)
            {
                ThongBao = "Vui lòng chọn 1 khách hàng để sửa!";
                return;
            }
            IsAddingNew = false;
            ThongBao = "Chỉnh sửa thông tin bên trái, sau đó bấm LƯU";
        }

        public void Xoa()
        {
            if (SelectedKhachHang == null || SelectedKhachHang.MA_KH == 0)
            {
                ThongBao = "Vui lòng chọn 1 khách hàng để xóa!";
                return;
            }

            var rs = MessageBox.Show(
                $"Xóa khách hàng:\nMã: {SelectedKhachHang.MA_KH}\nTên: {SelectedKhachHang.HoTen_KH}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (rs == MessageBoxResult.Yes)
            {
                try
                {
                    var kh = _db.KHACHHANGs.Find(SelectedKhachHang.MA_KH);
                    if (kh != null)
                    {
                        _db.KHACHHANGs.Remove(kh);
                        _db.SaveChanges();
                        TaiDuLieu();
                        SelectedKhachHang = new KHACHHANG_Display();
                        ThongBao = "Đã xóa thành công!";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa: " + ex.Message, "Lỗi");
                }
            }
        }

        public void LamMoi()
        {
            TuKhoaTimKiem = "";
            SelectedKhachHang = new KHACHHANG_Display();
            IsAddingNew = false;
            TaiDuLieu();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class KHACHHANG_Display : INotifyPropertyChanged
    {
        private int _maKH;
        public int MA_KH
        {
            get => _maKH;
            set { _maKH = value; OnPropertyChanged(); }
        }

        private string _hoTen_KH;
        public string HoTen_KH
        {
            get => _hoTen_KH;
            set { _hoTen_KH = value; OnPropertyChanged(); }
        }

        private string _soDienThoai_KH;
        public string SoDienThoai_KH
        {
            get => _soDienThoai_KH;
            set { _soDienThoai_KH = value; OnPropertyChanged(); }
        }

        private string _cmnd_KH;
        public string CMND_KH
        {
            get => _cmnd_KH;
            set { _cmnd_KH = value; OnPropertyChanged(); }
        }

        public string Email { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}