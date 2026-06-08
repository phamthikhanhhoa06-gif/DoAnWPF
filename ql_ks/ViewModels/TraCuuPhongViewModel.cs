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
    public class TraCuuPhongViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private bool _isAddingNew = false;
        public bool IsAddingNew
        {
            get => _isAddingNew;
            set { _isAddingNew = value; OnPropertyChanged(); }
        }

        private List<PHONG> _allPhongs;
        public ObservableCollection<PHONG_Display> DanhSachHienThi { get; set; }
        public ObservableCollection<LOAIPHONG> DanhSachLoaiPhong { get; set; }
        public ObservableCollection<string> TinhTrangList { get; set; }

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

        private PHONG_Display _selectedPhong;
        public PHONG_Display SelectedPhong
        {
            get => _selectedPhong;
            set { _selectedPhong = value; OnPropertyChanged(); }
        }

        public ICommand ThemCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }

        public TraCuuPhongViewModel()
        {
            DanhSachHienThi = new ObservableCollection<PHONG_Display>();
            DanhSachLoaiPhong = new ObservableCollection<LOAIPHONG>();
            TinhTrangList = new ObservableCollection<string> { "Trống", "Đang sử dụng", "Đang dọn", "Bảo trì" };
            SelectedPhong = new PHONG_Display();

            ThemCommand = new TCPhong_RelayCommand(_ => Them());
            LuuCommand = new TCPhong_RelayCommand(_ => Luu());
            SuaCommand = new TCPhong_RelayCommand(_ => Sua());
            XoaCommand = new TCPhong_RelayCommand(_ => Xoa());
            LamMoiCommand = new TCPhong_RelayCommand(_ => LamMoi());

            TaiDanhSachLoaiPhong();
            TaiDuLieu();
        }

        private void TaiDanhSachLoaiPhong()
        {
            try
            {
                var list = _db.LOAIPHONGs.ToList();
                DanhSachLoaiPhong.Clear();
                foreach (var lp in list)
                    DanhSachLoaiPhong.Add(lp);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải loại phòng: " + ex.Message);
            }
        }

        private void TaiDuLieu()
        {
            try
            {
                _allPhongs = _db.PHONGs
                    .Include(p => p.LOAIPHONG)
                    .OrderBy(p => p.Ma_Phong)
                    .ToList();

                CapNhatDanhSach(_allPhongs);
                ThongBao = $"Tổng số phòng: {_allPhongs.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void TimKiem()
        {
            if (_allPhongs == null) return;

            var keyword = (_tuKhoaTimKiem ?? "").Trim().ToLower();

            var result = string.IsNullOrWhiteSpace(keyword)
                ? _allPhongs
                : _allPhongs.Where(p =>
                    p.Ma_Phong.ToString().Contains(keyword) ||
                    (p.LOAIPHONG?.Ten_TP ?? "").ToLower().Contains(keyword) ||
                    (p.TinhTrang_Phong ?? "").ToLower().Contains(keyword));

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả: {result.Count()} phòng";
        }

        private void CapNhatDanhSach(List<PHONG> list)
        {
            DanhSachHienThi.Clear();
            foreach (var p in list)
            {
                DanhSachHienThi.Add(new PHONG_Display
                {
                    Ma_Phong = p.Ma_Phong,
                    Ma_LP = p.Ma_LP,
                    TenLoaiPhong = p.LOAIPHONG?.Ten_TP ?? "",
                    TinhTrang_Phong = p.TinhTrang_Phong ?? "Trống",
                    DonGia = p.LOAIPHONG?.DonGia_LP ?? 0
                });
            }
        }

        public void Them()
        {
            IsAddingNew = true;
            SelectedPhong = new PHONG_Display
            {
                Ma_Phong = _allPhongs.Any() ? _allPhongs.Max(x => x.Ma_Phong) + 1 : 101,
                Ma_LP = DanhSachLoaiPhong.FirstOrDefault()?.Ma_LP,
                TinhTrang_Phong = "Trống",
                TenLoaiPhong = "",
                DonGia = 0
            };
            ThongBao = "Nhập thông tin phòng vào form bên trái, sau đó bấm LƯU";
        }

        public void Luu()
        {
            if (SelectedPhong == null)
            {
                MessageBox.Show("Vui lòng nhập thông tin phòng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedPhong.Ma_Phong <= 0)
            {
                MessageBox.Show("Mã phòng phải lớn hơn 0!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var exists = _db.PHONGs.Any(p => p.Ma_Phong == SelectedPhong.Ma_Phong);

                if (IsAddingNew && exists)
                {
                    MessageBox.Show("Mã phòng đã tồn tại!", "Lỗi");
                    return;
                }

                if (IsAddingNew || !exists)
                {
                    // THÊM MỚI
                    var p = new PHONG
                    {
                        Ma_Phong = SelectedPhong.Ma_Phong,
                        Ma_LP = SelectedPhong.Ma_LP,
                        TinhTrang_Phong = SelectedPhong.TinhTrang_Phong
                    };

                    _db.PHONGs.Add(p);
                    _db.SaveChanges();

                    MessageBox.Show("Thêm phòng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAddingNew = false;
                    TaiDuLieu();
                }
                else
                {
                    // CẬP NHẬT
                    var p = _db.PHONGs.Find(SelectedPhong.Ma_Phong);
                    if (p != null)
                    {
                        p.Ma_LP = SelectedPhong.Ma_LP;
                        p.TinhTrang_Phong = SelectedPhong.TinhTrang_Phong;

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
            if (SelectedPhong == null || SelectedPhong.Ma_Phong <= 0)
            {
                ThongBao = "Vui lòng chọn 1 phòng để sửa!";
                return;
            }
            IsAddingNew = false;
            ThongBao = "Chỉnh sửa thông tin bên trái, sau đó bấm LƯU";
        }

        public void Xoa()
        {
            if (SelectedPhong == null || SelectedPhong.Ma_Phong <= 0)
            {
                ThongBao = "Vui lòng chọn 1 phòng để xóa!";
                return;
            }

            var rs = MessageBox.Show(
                $"Xóa phòng:\nMã: {SelectedPhong.Ma_Phong}\nLoại: {SelectedPhong.TenLoaiPhong}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (rs == MessageBoxResult.Yes)
            {
                try
                {
                    var p = _db.PHONGs.Find(SelectedPhong.Ma_Phong);
                    if (p != null)
                    {
                        _db.PHONGs.Remove(p);
                        _db.SaveChanges();
                        TaiDuLieu();
                        SelectedPhong = new PHONG_Display();
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
            SelectedPhong = new PHONG_Display();
            IsAddingNew = false;
            TaiDuLieu();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PHONG_Display : INotifyPropertyChanged
    {
        private int _maPhong;
        public int Ma_Phong
        {
            get => _maPhong;
            set { _maPhong = value; OnPropertyChanged(); }
        }

        private int? _maLP;
        public int? Ma_LP
        {
            get => _maLP;
            set { _maLP = value; OnPropertyChanged(); }
        }

        public string TenLoaiPhong { get; set; }

        private string _tinhTrang;
        public string TinhTrang_Phong
        {
            get => _tinhTrang;
            set { _tinhTrang = value; OnPropertyChanged(); }
        }

        public decimal? DonGia { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}