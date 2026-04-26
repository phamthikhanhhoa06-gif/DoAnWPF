using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class TraCuuLoaiPhongViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private bool _isAddingNew = false;
        public bool IsAddingNew
        {
            get => _isAddingNew;
            set { _isAddingNew = value; OnPropertyChanged(); }
        }

        private List<LOAIPHONG> _allLoaiPhongs;
        public ObservableCollection<LOAIPHONG_Display> DanhSachHienThi { get; set; }

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

        private LOAIPHONG_Display _selectedLoaiPhong;
        public LOAIPHONG_Display SelectedLoaiPhong
        {
            get => _selectedLoaiPhong;
            set { _selectedLoaiPhong = value; OnPropertyChanged(); }
        }

        public ICommand ThemCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }
        public ICommand TimKiemCommand { get; }

        public TraCuuLoaiPhongViewModel()
        {
            DanhSachHienThi = new ObservableCollection<LOAIPHONG_Display>();
            SelectedLoaiPhong = new LOAIPHONG_Display();

            ThemCommand = new TCPhong_RelayCommand(_ => Them());
            LuuCommand = new TCPhong_RelayCommand(_ => Luu());
            SuaCommand = new TCPhong_RelayCommand(_ => Sua());
            XoaCommand = new TCPhong_RelayCommand(_ => Xoa());
            LamMoiCommand = new TCPhong_RelayCommand(_ => LamMoi());
            TimKiemCommand = new TCPhong_RelayCommand(_ => TimKiem());

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                _allLoaiPhongs = _db.LOAIPHONGs
                    .OrderBy(lp => lp.Ma_LP)
                    .ToList();

                CapNhatDanhSach(_allLoaiPhongs);
                ThongBao = $"Tổng số loại phòng: {_allLoaiPhongs.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void TimKiem()
        {
            if (_allLoaiPhongs == null) return;

            var keyword = (_tuKhoaTimKiem ?? "").Trim().ToLower();

            var result = string.IsNullOrWhiteSpace(keyword)
                ? _allLoaiPhongs
                : _allLoaiPhongs.Where(lp =>
                    lp.Ma_LP.ToString().Contains(keyword) ||
                    (lp.Ten_TP ?? "").ToLower().Contains(keyword));

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả: {result.Count()} loại phòng";
        }

        private void CapNhatDanhSach(List<LOAIPHONG> list)
        {
            DanhSachHienThi.Clear();
            foreach (var lp in list)
            {
                DanhSachHienThi.Add(new LOAIPHONG_Display
                {
                    Ma_LP = lp.Ma_LP,
                    Ten_TP = lp.Ten_TP,
                    DonGia_LP = lp.DonGia_LP
                });
            }
        }

        public void Them()
        {
            IsAddingNew = true;
            int nextId = _allLoaiPhongs.Any() ? _allLoaiPhongs.Max(x => x.Ma_LP) + 1 : 1;
            SelectedLoaiPhong = new LOAIPHONG_Display
            {
                Ma_LP = nextId,
                Ten_TP = "",
                DonGia_LP = 0
            };
            ThongBao = "Nhập thông tin loại phòng vào form bên trái, sau đó bấm LƯU";
        }

        public void Luu()
        {
            if (SelectedLoaiPhong == null || string.IsNullOrWhiteSpace(SelectedLoaiPhong.Ten_TP))
            {
                MessageBox.Show("Tên loại phòng không được để trống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedLoaiPhong.Ma_LP <= 0)
            {
                MessageBox.Show("Mã loại phòng phải lớn hơn 0!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var exists = _db.LOAIPHONGs.Any(lp => lp.Ma_LP == SelectedLoaiPhong.Ma_LP);

                if (IsAddingNew && exists)
                {
                    MessageBox.Show("Mã loại phòng đã tồn tại!", "Lỗi");
                    return;
                }

                if (IsAddingNew || !exists)
                {
                    // THÊM MỚI
                    var lp = new LOAIPHONG
                    {
                        Ma_LP = SelectedLoaiPhong.Ma_LP,
                        Ten_TP = SelectedLoaiPhong.Ten_TP,
                        DonGia_LP = SelectedLoaiPhong.DonGia_LP
                    };

                    _db.LOAIPHONGs.Add(lp);
                    _db.SaveChanges();

                    MessageBox.Show("Thêm loại phòng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAddingNew = false;
                    TaiDuLieu();
                }
                else
                {
                    // CẬP NHẬT
                    var lp = _db.LOAIPHONGs.Find(SelectedLoaiPhong.Ma_LP);
                    if (lp != null)
                    {
                        lp.Ten_TP = SelectedLoaiPhong.Ten_TP;
                        lp.DonGia_LP = SelectedLoaiPhong.DonGia_LP;

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
            if (SelectedLoaiPhong == null || SelectedLoaiPhong.Ma_LP <= 0)
            {
                ThongBao = "Vui lòng chọn 1 loại phòng để sửa!";
                return;
            }
            IsAddingNew = false;
            ThongBao = "Chỉnh sửa thông tin bên trái, sau đó bấm LƯU";
        }

        public void Xoa()
        {
            if (SelectedLoaiPhong == null || SelectedLoaiPhong.Ma_LP <= 0)
            {
                ThongBao = "Vui lòng chọn 1 loại phòng để xóa!";
                return;
            }

            var rs = MessageBox.Show(
                $"Xóa loại phòng:\nMã: {SelectedLoaiPhong.Ma_LP}\nTên: {SelectedLoaiPhong.Ten_TP}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (rs == MessageBoxResult.Yes)
            {
                try
                {
                    var lp = _db.LOAIPHONGs.Find(SelectedLoaiPhong.Ma_LP);
                    if (lp != null)
                    {
                        _db.LOAIPHONGs.Remove(lp);
                        _db.SaveChanges();
                        TaiDuLieu();
                        SelectedLoaiPhong = new LOAIPHONG_Display();
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
            SelectedLoaiPhong = new LOAIPHONG_Display();
            IsAddingNew = false;
            TaiDuLieu();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LOAIPHONG_Display : INotifyPropertyChanged
    {
        private int _maLP;
        public int Ma_LP
        {
            get => _maLP;
            set { _maLP = value; OnPropertyChanged(); }
        }

        private string _tenTP;
        public string Ten_TP
        {
            get => _tenTP;
            set { _tenTP = value; OnPropertyChanged(); }
        }

        private long? _donGiaLP;
        public long? DonGia_LP
        {
            get => _donGiaLP;
            set { _donGiaLP = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}