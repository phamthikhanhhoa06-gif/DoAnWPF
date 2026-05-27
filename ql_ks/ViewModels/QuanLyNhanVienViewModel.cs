using ql_ks.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ql_ks.ViewModels
{
    public class QuanLyNhanVienViewModel : Common_BaseViewModel
    {
        private ObservableCollection<NHANVIEN> _listNhanVien;
        public ObservableCollection<NHANVIEN> ListNhanVien
        {
            get => _listNhanVien;
            set { _listNhanVien = value; OnPropertyChanged(); }
        }

        private NHANVIEN _selectedNhanVien;
        public NHANVIEN SelectedNhanVien
        {
            get => _selectedNhanVien;
            set
            {
                _selectedNhanVien = value;
                OnPropertyChanged();
                if (_selectedNhanVien != null)
                {
                    MaNV = _selectedNhanVien.MA_NV;
                    HoTenNV = _selectedNhanVien.HoTen_NV;
                    GioiTinhNV = _selectedNhanVien.GioiTinh_NV.HasValue && _selectedNhanVien.GioiTinh_NV.Value ? "Nam" : "Nữ";
                    NgaySinhNV = _selectedNhanVien.NgaySinh_NV ?? DateTime.Now.AddYears(-20);
                    SdtNV = _selectedNhanVien.SoDienThoai_NV;
                    ChucVuNV = _selectedNhanVien.ChucVu_NV;
                    DiaChiNV = _selectedNhanVien.DiaChi_NV;
                    NgayVaoLamNV = _selectedNhanVien.NgayVaoLam_NV ?? DateTime.Now;
                }
            }
        }

        private int _maNV;
        public int MaNV
        {
            get => _maNV;
            set { _maNV = value; OnPropertyChanged(); }
        }

        private string _hoTenNV;
        public string HoTenNV
        {
            get => _hoTenNV;
            set { _hoTenNV = value; OnPropertyChanged(); }
        }

        private string _gioiTinhNV;
        public string GioiTinhNV
        {
            get => _gioiTinhNV;
            set { _gioiTinhNV = value; OnPropertyChanged(); }
        }

        private DateTime _ngaySinhNV = DateTime.Now.AddYears(-20);
        public DateTime NgaySinhNV
        {
            get => _ngaySinhNV;
            set { _ngaySinhNV = value; OnPropertyChanged(); }
        }

        private string _sdtNV;
        public string SdtNV
        {
            get => _sdtNV;
            set { _sdtNV = value; OnPropertyChanged(); }
        }

        private string _chucVuNV;
        public string ChucVuNV
        {
            get => _chucVuNV;
            set { _chucVuNV = value; OnPropertyChanged(); }
        }

        private string _diaChiNV;
        public string DiaChiNV
        {
            get => _diaChiNV;
            set { _diaChiNV = value; OnPropertyChanged(); }
        }

        private DateTime _ngayVaoLamNV = DateTime.Now;
        public DateTime NgayVaoLamNV
        {
            get => _ngayVaoLamNV;
            set { _ngayVaoLamNV = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public QuanLyNhanVienViewModel()
        {
            LoadData();

            AddCommand = new Common_RelayCommand(Add, CanAdd);
            EditCommand = new Common_RelayCommand(Edit, CanEdit);
            DeleteCommand = new Common_RelayCommand(Delete, CanDelete);
            ClearCommand = new Common_RelayCommand(Clear);
        }

        private void LoadData()
        {
            using (var db = new QLKhachSan_Model())
            {
                ListNhanVien = new ObservableCollection<NHANVIEN>(db.NHANVIENs.ToList());
            }
        }

        private bool CanAdd(object obj)
        {
            return MaNV > 0 && !string.IsNullOrEmpty(HoTenNV) && !string.IsNullOrEmpty(SdtNV);
        }

        private void Add(object obj)
        {
            using (var db = new QLKhachSan_Model())
            {
                if (db.NHANVIENs.Any(p => p.MA_NV == MaNV))
                {
                    MessageBox.Show("Mã nhân viên này đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nv = new NHANVIEN
                {
                    MA_NV = MaNV,
                    HoTen_NV = HoTenNV,
                    GioiTinh_NV = GioiTinhNV == "Nam",
                    NgaySinh_NV = NgaySinhNV,
                    SoDienThoai_NV = SdtNV,
                    ChucVu_NV = ChucVuNV,
                    DiaChi_NV = DiaChiNV,
                    NgayVaoLam_NV = NgayVaoLamNV
                };
                db.NHANVIENs.Add(nv);
                db.SaveChanges();
            }
            LoadData();
            Clear(null);
            MessageBox.Show("Thêm nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanEdit(object obj)
        {
            return SelectedNhanVien != null && MaNV > 0 && !string.IsNullOrEmpty(HoTenNV) && !string.IsNullOrEmpty(SdtNV);
        }

        private void Edit(object obj)
        {
            using (var db = new QLKhachSan_Model())
            {
                var nv = db.NHANVIENs.Find(MaNV);
                if (nv == null)
                {
                    MessageBox.Show("Không tìm thấy nhân viên để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                nv.HoTen_NV = HoTenNV;
                nv.GioiTinh_NV = GioiTinhNV == "Nam";
                nv.NgaySinh_NV = NgaySinhNV;
                nv.SoDienThoai_NV = SdtNV;
                nv.ChucVu_NV = ChucVuNV;
                nv.DiaChi_NV = DiaChiNV;
                nv.NgayVaoLam_NV = NgayVaoLamNV;
                db.SaveChanges();
            }
            LoadData();
            MessageBox.Show("Cập nhật nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanDelete(object obj)
        {
            return SelectedNhanVien != null;
        }

        private void Delete(object obj)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa nhân viên này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var db = new QLKhachSan_Model())
                {
                    var nv = db.NHANVIENs.Find(MaNV);
                    if (nv != null)
                    {
                        db.NHANVIENs.Remove(nv);
                        db.SaveChanges();
                    }
                }
                LoadData();
                Clear(null);
                MessageBox.Show("Xóa nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Clear(object obj)
        {
            SelectedNhanVien = null;
            MaNV = 0;
            HoTenNV = string.Empty;
            GioiTinhNV = string.Empty;
            NgaySinhNV = DateTime.Now.AddYears(-20);
            SdtNV = string.Empty;
            ChucVuNV = string.Empty;
            DiaChiNV = string.Empty;
            NgayVaoLamNV = DateTime.Now;
        }
    }
}
