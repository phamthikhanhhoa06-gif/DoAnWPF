using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity; // Thêm dòng này cho .Include()
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class TraCuuNhanVienViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        // Flag để biết đang thêm mới hay sửa
        private bool _isAddingNew = false;
        public bool IsAddingNew
        {
            get => _isAddingNew;
            set { _isAddingNew = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> LocChucVuList { get; set; }
        // Danh sách gốc từ DB
        private List<NHANVIEN> _allNhanViens;

        // Danh sách hiển thị lên UI (sau khi lọc/tìm)
        public ObservableCollection<NHANVIEN_Display> DanhSachHienThi { get; set; }

        // Control Filter
        private string _tuKhoaTimKiem = "";
        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set { _tuKhoaTimKiem = value; OnPropertyChanged(); TimKiem(); }
        }

        private string _locChucVu = "Tất cả";
        public string LocChucVu
        {
            get => _locChucVu;
            set { _locChucVu = value; OnPropertyChanged(); LocTheoDieuKien(); }
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

        // Item đang được chọn (để sửa/xóa)
        private NHANVIEN_Display _selectedNhanVien;
        public NHANVIEN_Display SelectedNhanVien
        {
            get => _selectedNhanVien;
            set { _selectedNhanVien = value; OnPropertyChanged(); }
        }

        public ICommand ThemCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }
        public ICommand LuuCommand { get; }

        public TraCuuNhanVienViewModel()
        {
            DanhSachHienThi = new ObservableCollection<NHANVIEN_Display>();
            // ✅ Khởi tạo để không bị null binding
            SelectedNhanVien = new NHANVIEN_Display();

            LuuCommand = new TCNhanVien_RelayCommand((Action<object>)(_ => Luu()));
            // Khởi tạo danh sách chức vụ
            LocChucVuList = new ObservableCollection<string>
    {
        "Tất cả",
        "Quản lý",
        "Lễ tân",
        "Kế toán",
        "Bảo vệ",
        "Phục vụ",
        "Thông tin"
    };

            LocChucVu = "Tất cả"; // Giá trị mặc định

            ThemCommand = new TCNhanVien_RelayCommand((Action<object>)(_ => Them()));
            SuaCommand = new TCNhanVien_RelayCommand((Action<object>)(_ => Sua()));
            XoaCommand = new TCNhanVien_RelayCommand((Action<object>)(_ => Xoa()));
            LamMoiCommand = new TCNhanVien_RelayCommand((Action<object>)(_ => LamMoi()));

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                // Load toàn bộ nhân viên từ DB
                _allNhanViens = _db.NHANVIENs
                    .Include(nv => nv.TAIKHOAN)
                    .OrderByDescending(nv => nv.NgayVaoLam_NV)
                    .ToList();

                LocTheoDieuKien();
                ThongBao = $"Tổng số nhân viên: {_allNhanViens.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu nhân viên: " + ex.Message);
            }
        }

        private void TimKiem()
        {
            if (_allNhanViens == null) return;

            var keyword = (_tuKhoaTimKiem ?? "").Trim().ToLower();

            var result = string.IsNullOrWhiteSpace(keyword)
                ? _allNhanViens
                : _allNhanViens.Where(nv =>
                    (nv.HoTen_NV ?? "").ToLower().Contains(keyword) ||
                    nv.MA_NV.ToString().Contains(keyword) ||
                    (nv.SoDienThoai_NV ?? "").Contains(keyword));

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả tìm kiếm: {result.Count()} nhân viên";
        }

        private void LocTheoDieuKien()
        {
            if (_allNhanViens == null) return;

            var result = _allNhanViens.AsEnumerable();

            // Lọc theo chức vụ
            if (!string.IsNullOrEmpty(_locChucVu) && _locChucVu != "Tất cả")
            {
                result = result.Where(nv => (nv.ChucVu_NV ?? "") == _locChucVu);
            }

            // Lọc theo ngày
            if (_ngayBatDau.HasValue && !_ngayKetThuc.HasValue)
            {
                result = result.Where(nv => nv.NgayVaoLam_NV >= _ngayBatDau);
            }
            else if (!_ngayBatDau.HasValue && _ngayKetThuc.HasValue)
            {
                result = result.Where(nv => nv.NgayVaoLam_NV <= _ngayKetThuc);
            }
            else if (_ngayBatDau.HasValue && _ngayKetThuc.HasValue)
            {
                result = result.Where(nv => nv.NgayVaoLam_NV >= _ngayBatDau && nv.NgayVaoLam_NV <= _ngayKetThuc);
            }

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả lọc: {result.Count()} nhân viên";
        }

        private void CapNhatDanhSach(List<NHANVIEN> list)
        {
            var displayList = list.Select(nv => new NHANVIEN_Display
            {
                MA_NV = nv.MA_NV,
                HoTen_NV = nv.HoTen_NV,
                SoDienThoai_NV = nv.SoDienThoai_NV,
                GioiTinh_NV = nv.GioiTinh_NV == true ? "Nam" : "Nữ",
                MatKhau = "****", // Ẩn bảo mật
                NgaySinh_NV = nv.NgaySinh_NV,
                ChucVu_NV = nv.ChucVu_NV,
                DiaChi_NV = nv.DiaChi_NV,
                NgayVaoLam_NV = nv.NgayVaoLam_NV,
                Email = nv.TAIKHOAN?.TenDangNhap_TK ?? "",
                IsChecked = false
            }).ToList();

            DanhSachHienThi.Clear();
            foreach (var item in displayList)
                DanhSachHienThi.Add(item);

            TongTien = DanhSachHienThi.Count;
        }

        private int _tongTien;
        private int TongTien
        {
            get { return _tongTien; }
            set { _tongTien = value; OnPropertyChanged(); }
        }

        #region CRUD Operations

        public void Them()
        {
            IsAddingNew = true;
            SelectedNhanVien = new NHANVIEN_Display
            {
                MA_NV = 0,
                HoTen_NV = "",
                SoDienThoai_NV = "",
                GioiTinh_NV = "Nam",
                NgaySinh_NV = null,
                ChucVu_NV = "Lễ tân",
                DiaChi_NV = "",
                NgayVaoLam_NV = DateTime.Now,
                Email = "",
                IsChecked = false
            };
            ThongBao = "Nhập thông tin vào form bên trái, sau đó bấm LƯU";
        }
        public void Luu()
        {
            if (SelectedNhanVien == null || string.IsNullOrWhiteSpace(SelectedNhanVien.HoTen_NV))
            {
                MessageBox.Show("Vui lòng nhập họ tên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (IsAddingNew || SelectedNhanVien.MA_NV == 0)
                {
                    // THÊM MỚI
                    int newMa = _db.NHANVIENs.Any() ? _db.NHANVIENs.Max(x => x.MA_NV) + 1 : 1;

                    var nv = new NHANVIEN
                    {
                        MA_NV = newMa,
                        HoTen_NV = SelectedNhanVien.HoTen_NV,
                        GioiTinh_NV = SelectedNhanVien.GioiTinh_NV == "Nam",
                        SoDienThoai_NV = SelectedNhanVien.SoDienThoai_NV,
                        NgaySinh_NV = SelectedNhanVien.NgaySinh_NV,
                        ChucVu_NV = SelectedNhanVien.ChucVu_NV,
                        DiaChi_NV = SelectedNhanVien.DiaChi_NV,
                        NgayVaoLam_NV = DateTime.Now,
                        Ma_TK = null
                    };

                    _db.NHANVIENs.Add(nv);
                    _db.SaveChanges();

                    MessageBox.Show("✅ Thêm nhân viên thành công!", "Thành công");
                    IsAddingNew = false;
                    TaiDuLieu();
                }
                else
                {
                    // CẬP NHẬT (nếu cần)
                    var nv = _db.NHANVIENs.Find(SelectedNhanVien.MA_NV);
                    if (nv != null)
                    {
                        nv.HoTen_NV = SelectedNhanVien.HoTen_NV;
                        nv.SoDienThoai_NV = SelectedNhanVien.SoDienThoai_NV;
                        nv.GioiTinh_NV = SelectedNhanVien.GioiTinh_NV == "Nam";
                        nv.NgaySinh_NV = SelectedNhanVien.NgaySinh_NV;
                        nv.ChucVu_NV = SelectedNhanVien.ChucVu_NV;
                        nv.DiaChi_NV = SelectedNhanVien.DiaChi_NV;
                        _db.SaveChanges();
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
            if (_selectedNhanVien == null)
            {
                ThongBao = "Vui lòng chọn 1 nhân viên để sửa!";
                return;
            }

            MessageBox.Show($"Sửa nhân viên: {_selectedNhanVien.HoTen_NV}",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Xoa()
        {
            if (_selectedNhanVien == null)
            {
                ThongBao = "Vui lòng chọn 1 nhân viên để xóa!";
                return;
            }

            var rs = MessageBox.Show(
                $"Bạn có chắc muốn xóa nhân viên:\n" +
                $"Mã: {_selectedNhanVien.MA_NV}\n" +
                $"Tên: {_selectedNhanVien.HoTen_NV}?\n" +
                $"(Thao tác không thể đảo ngược)",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (rs == MessageBoxResult.Yes)
            {
                try
                {
                    var nvToDelete = _db.NHANVIENs.Find(_selectedNhanVien.MA_NV);
                    if (nvToDelete != null)
                    {
                        _db.NHANVIENs.Remove(nvToDelete);
                        _db.SaveChanges();

                        TaiDuLieu();
                        ThongBao = "Đã xóa thành công!";
                    }
                }
                catch (Exception ex)
                {
                    // SỬA LỖI: MessageBoxButtons -> MessageBoxButton
                    MessageBox.Show("Không thể xóa nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void LamMoi()
        {
            TuKhoaTimKiem = "";
            LocChucVu = "Tất cả";
            NgayBatDau = null;
            NgayKetThuc = null;
            SelectedNhanVien = null;

            TaiDuLieu();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Class Display (ẩn sensitive data)
    public class NHANVIEN_Display : INotifyPropertyChanged
    {
        private string _hoTen_NV;
        public string HoTen_NV
        {
            get => _hoTen_NV;
            set { _hoTen_NV = value; OnPropertyChanged(); }
        }

        private string _soDienThoai_NV;
        public string SoDienThoai_NV
        {
            get => _soDienThoai_NV;
            set { _soDienThoai_NV = value; OnPropertyChanged(); }
        }

        private string _gioiTinh_NV;
        public string GioiTinh_NV
        {
            get => _gioiTinh_NV;
            set { _gioiTinh_NV = value; OnPropertyChanged(); }
        }

        private string _chucVu_NV;
        public string ChucVu_NV
        {
            get => _chucVu_NV;
            set { _chucVu_NV = value; OnPropertyChanged(); }
        }

        private string _diaChi_NV;
        public string DiaChi_NV
        {
            get => _diaChi_NV;
            set { _diaChi_NV = value; OnPropertyChanged(); }
        }

        public int MA_NV { get; set; }
        public DateTime? NgaySinh_NV { get; set; }
        public DateTime? NgayVaoLam_NV { get; set; }
        public string Email { get; set; }
        public string MatKhau { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}