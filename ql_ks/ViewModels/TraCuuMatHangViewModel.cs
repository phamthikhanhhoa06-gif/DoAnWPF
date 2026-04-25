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
    public class TraCuuMatHangViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private bool _isAddingNew = false;
        private List<MATHANG> _allMatHangs;

        public ObservableCollection<MATHANG_Display> DanhSachHienThi { get; set; }
        public bool IsAddingNew { get => _isAddingNew; set { _isAddingNew = value; OnPropertyChanged(); } }

        private string _tuKhoaTimKiem = "";
        public string TuKhoaTimKiem { get => _tuKhoaTimKiem; set { _tuKhoaTimKiem = value; OnPropertyChanged(); LocTheoDieuKien(); } }

        private DateTime? _ngayBatDau;
        public DateTime? NgayBatDau { get => _ngayBatDau; set { _ngayBatDau = value; OnPropertyChanged(); LocTheoDieuKien(); } }

        private DateTime? _ngayKetThuc;
        public DateTime? NgayKetThuc { get => _ngayKetThuc; set { _ngayKetThuc = value; OnPropertyChanged(); LocTheoDieuKien(); } }

        private string _thongBao = "";
        public string ThongBao { get => _thongBao; set { _thongBao = value; OnPropertyChanged(); } }

        private MATHANG_Display _selectedMatHang;
        public MATHANG_Display SelectedMatHang { get => _selectedMatHang; set { _selectedMatHang = value; OnPropertyChanged(); } }

        public ICommand ThemCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }

        public TraCuuMatHangViewModel()
        {
            DanhSachHienThi = new ObservableCollection<MATHANG_Display>();
            SelectedMatHang = new MATHANG_Display();
            ThemCommand = new TCMatHang_RelayCommand(_ => Them());
            LuuCommand = new TCMatHang_RelayCommand(_ => Luu());
            SuaCommand = new TCMatHang_RelayCommand(_ => Sua());
            XoaCommand = new TCMatHang_RelayCommand(_ => Xoa());
            LamMoiCommand = new TCMatHang_RelayCommand(_ => LamMoi());
            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                _allMatHangs = _db.MATHANGs.OrderByDescending(mh => mh.NgayNhap_MH).ToList();
                LocTheoDieuKien();
                ThongBao = $"Tổng số mặt hàng: {_allMatHangs.Count}";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void LocTheoDieuKien()
        {
            if (_allMatHangs == null) return;
            var result = _allMatHangs.AsEnumerable();
            var keyword = (_tuKhoaTimKiem ?? "").Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(keyword))
                result = result.Where(mh => mh.Ma_MH.ToString().Contains(keyword) || (mh.Ten_MH ?? "").ToLower().Contains(keyword));

            if (_ngayBatDau.HasValue && !_ngayKetThuc.HasValue)
                result = result.Where(mh => mh.NgayNhap_MH >= _ngayBatDau);
            else if (!_ngayBatDau.HasValue && _ngayKetThuc.HasValue)
                result = result.Where(mh => mh.NgayNhap_MH <= _ngayKetThuc);
            else if (_ngayBatDau.HasValue && _ngayKetThuc.HasValue)
                result = result.Where(mh => mh.NgayNhap_MH >= _ngayBatDau && mh.NgayNhap_MH <= _ngayKetThuc);

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả: {DanhSachHienThi.Count} / {_allMatHangs.Count} mặt hàng";
        }

        private void CapNhatDanhSach(List<MATHANG> list)
        {
            DanhSachHienThi.Clear();
            foreach (var mh in list)
                DanhSachHienThi.Add(new MATHANG_Display
                {
                    Ma_MH = mh.Ma_MH,
                    Ten_MH = mh.Ten_MH,
                    DonGia_MH = mh.DonGia_MH,
                    NgayNhap_MH = mh.NgayNhap_MH
                });
        }

        public void Them()
        {
            IsAddingNew = true;
            int nextId = _allMatHangs.Any() ? _allMatHangs.Max(x => x.Ma_MH) + 1 : 1;
            SelectedMatHang = new MATHANG_Display { Ma_MH = nextId, Ten_MH = "", DonGia_MH = 0, NgayNhap_MH = DateTime.Now };
            ThongBao = "Nhập thông tin hàng hóa vào form bên trái, sau đó bấm LƯU";
        }

        public void Luu()
        {
            if (SelectedMatHang == null || string.IsNullOrWhiteSpace(SelectedMatHang.Ten_MH))
            {
                MessageBox.Show("Tên mặt hàng không được để trống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var exists = _db.MATHANGs.Any(mh => mh.Ma_MH == SelectedMatHang.Ma_MH);
                if (IsAddingNew && exists) { MessageBox.Show("Mã mặt hàng đã tồn tại!", "Lỗi"); return; }

                if (IsAddingNew || !exists)
                {
                    var mh = new MATHANG
                    {
                        Ma_MH = SelectedMatHang.Ma_MH,
                        Ten_MH = SelectedMatHang.Ten_MH,
                        DonGia_MH = SelectedMatHang.DonGia_MH,
                        NgayNhap_MH = SelectedMatHang.NgayNhap_MH ?? DateTime.Now
                    };
                    _db.MATHANGs.Add(mh);
                    _db.SaveChanges();
                    MessageBox.Show("Thêm hàng hóa thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAddingNew = false;
                    TaiDuLieu();
                }
                else
                {
                    var mh = _db.MATHANGs.Find(SelectedMatHang.Ma_MH);
                    if (mh != null)
                    {
                        mh.Ten_MH = SelectedMatHang.Ten_MH;
                        mh.DonGia_MH = SelectedMatHang.DonGia_MH;
                        mh.NgayNhap_MH = SelectedMatHang.NgayNhap_MH;
                        _db.SaveChanges();
                        MessageBox.Show("Cập nhật thành công!", "Thành công");
                        TaiDuLieu();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public void Sua()
        {
            if (SelectedMatHang == null || SelectedMatHang.Ma_MH <= 0) { ThongBao = "Vui lòng chọn 1 mặt hàng để sửa!"; return; }
            IsAddingNew = false;
            ThongBao = "Chỉnh sửa thông tin bên trái, sau đó bấm LƯU";
        }

        public void Xoa()
        {
            if (SelectedMatHang == null || SelectedMatHang.Ma_MH <= 0) { ThongBao = "Vui lòng chọn 1 mặt hàng để xóa!"; return; }

            if (MessageBox.Show($"Xóa mặt hàng:\nMã: {SelectedMatHang.Ma_MH}\nTên: {SelectedMatHang.Ten_MH}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var mh = _db.MATHANGs.Find(SelectedMatHang.Ma_MH);
                    if (mh != null) { _db.MATHANGs.Remove(mh); _db.SaveChanges(); TaiDuLieu(); SelectedMatHang = new MATHANG_Display(); ThongBao = "Đã xóa thành công!"; }
                }
                catch (Exception ex) { MessageBox.Show("Không thể xóa: " + ex.Message, "Lỗi"); }
            }
        }

        public void LamMoi()
        {
            TuKhoaTimKiem = ""; NgayBatDau = null; NgayKetThuc = null;
            SelectedMatHang = new MATHANG_Display(); IsAddingNew = false; TaiDuLieu();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class MATHANG_Display : INotifyPropertyChanged
    {
        private int _maMH;
        public int Ma_MH { get => _maMH; set { _maMH = value; OnPropertyChanged(); } }

        private string _tenMH;
        public string Ten_MH { get => _tenMH; set { _tenMH = value; OnPropertyChanged(); } }

        private long? _donGiaMH;
        public long? DonGia_MH { get => _donGiaMH; set { _donGiaMH = value; OnPropertyChanged(); } }

        private DateTime? _ngayNhapMH;
        public DateTime? NgayNhap_MH { get => _ngayNhapMH; set { _ngayNhapMH = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class TCMatHang_RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        public TCMatHang_RelayCommand(Action<object> execute, Predicate<object> canExecute = null) { _execute = execute ?? throw new ArgumentNullException(nameof(execute)); _canExecute = canExecute; }
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
    }
}