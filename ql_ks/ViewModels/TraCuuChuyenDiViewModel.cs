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
    public class TraCuuChuyenDiViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private bool _isAddingNew = false;
        private List<CHUYENDI> _allChuyenDis;

        public ObservableCollection<CHUYENDI_Display> DanhSachHienThi { get; set; }

        public bool IsAddingNew
        {
            get => _isAddingNew;
            set { _isAddingNew = value; OnPropertyChanged(); }
        }

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

        private CHUYENDI_Display _selectedChuyenDi;
        public CHUYENDI_Display SelectedChuyenDi
        {
            get => _selectedChuyenDi;
            set { _selectedChuyenDi = value; OnPropertyChanged(); }
        }

        public ICommand ThemCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }

        public TraCuuChuyenDiViewModel()
        {
            DanhSachHienThi = new ObservableCollection<CHUYENDI_Display>();
            SelectedChuyenDi = new CHUYENDI_Display();

            ThemCommand = new TCChuyenDi_RelayCommand(_ => Them());
            LuuCommand = new TCChuyenDi_RelayCommand(_ => Luu());
            SuaCommand = new TCChuyenDi_RelayCommand(_ => Sua());
            XoaCommand = new TCChuyenDi_RelayCommand(_ => Xoa());
            LamMoiCommand = new TCChuyenDi_RelayCommand(_ => LamMoi());

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                _allChuyenDis = _db.CHUYENDIs.OrderBy(x => x.Ma_CD).ToList();
                CapNhatDanhSach(_allChuyenDis);
                ThongBao = $"Tổng số chuyến đi: {_allChuyenDis.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void TimKiem()
        {
            if (_allChuyenDis == null) return;

            var keyword = (_tuKhoaTimKiem ?? "").Trim().ToLower();
            var result = string.IsNullOrWhiteSpace(keyword)
                ? _allChuyenDis
                : _allChuyenDis.Where(x =>
                    x.Ma_CD.ToString().Contains(keyword) ||
                    (x.DiemDen_CD ?? "").ToLower().Contains(keyword));

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả: {DanhSachHienThi.Count} chuyến đi";
        }

        private void CapNhatDanhSach(List<CHUYENDI> list)
        {
            DanhSachHienThi.Clear();
            foreach (var item in list)
            {
                DanhSachHienThi.Add(new CHUYENDI_Display
                {
                    Ma_CD = item.Ma_CD,
                    DiemDen_CD = item.DiemDen_CD,
                    DonGia_CD = item.DonGia_CD
                });
            }
        }

        public void Them()
        {
            IsAddingNew = true;
            int nextId = _allChuyenDis.Any() ? _allChuyenDis.Max(x => x.Ma_CD) + 1 : 1;
            SelectedChuyenDi = new CHUYENDI_Display
            {
                Ma_CD = nextId,
                DiemDen_CD = "",
                DonGia_CD = 0
            };
            ThongBao = "Nhập thông tin chuyến đi vào form bên trái, sau đó bấm LƯU";
        }

        public void Luu()
        {
            if (SelectedChuyenDi == null || string.IsNullOrWhiteSpace(SelectedChuyenDi.DiemDen_CD))
            {
                MessageBox.Show("Điểm đến không được để trống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var exists = _db.CHUYENDIs.Any(x => x.Ma_CD == SelectedChuyenDi.Ma_CD);

                if (IsAddingNew && exists)
                {
                    MessageBox.Show("Mã chuyến đi đã tồn tại!", "Lỗi");
                    return;
                }

                if (IsAddingNew || !exists)
                {
                    // THÊM MỚI
                    var entity = new CHUYENDI
                    {
                        Ma_CD = SelectedChuyenDi.Ma_CD,
                        DiemDen_CD = SelectedChuyenDi.DiemDen_CD,
                        DonGia_CD = SelectedChuyenDi.DonGia_CD
                    };

                    _db.CHUYENDIs.Add(entity);
                    _db.SaveChanges();

                    MessageBox.Show("Thêm chuyến đi thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAddingNew = false;
                    TaiDuLieu();
                }
                else
                {
                    // CẬP NHẬT
                    var entity = _db.CHUYENDIs.Find(SelectedChuyenDi.Ma_CD);
                    if (entity != null)
                    {
                        entity.DiemDen_CD = SelectedChuyenDi.DiemDen_CD;
                        entity.DonGia_CD = SelectedChuyenDi.DonGia_CD;

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
            if (SelectedChuyenDi == null || SelectedChuyenDi.Ma_CD <= 0)
            {
                ThongBao = "Vui lòng chọn 1 chuyến đi để sửa!";
                return;
            }
            IsAddingNew = false;
            ThongBao = "Chỉnh sửa thông tin bên trái, sau đó bấm LƯU";
        }

        public void Xoa()
        {
            if (SelectedChuyenDi == null || SelectedChuyenDi.Ma_CD <= 0)
            {
                ThongBao = "Vui lòng chọn 1 chuyến đi để xóa!";
                return;
            }

            var rs = MessageBox.Show(
                $"Xóa chuyến đi:\nMã: {SelectedChuyenDi.Ma_CD}\nĐiểm đến: {SelectedChuyenDi.DiemDen_CD}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (rs == MessageBoxResult.Yes)
            {
                try
                {
                    var entity = _db.CHUYENDIs.Find(SelectedChuyenDi.Ma_CD);
                    if (entity != null)
                    {
                        _db.CHUYENDIs.Remove(entity);
                        _db.SaveChanges();
                        TaiDuLieu();
                        SelectedChuyenDi = new CHUYENDI_Display();
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
            SelectedChuyenDi = new CHUYENDI_Display();
            IsAddingNew = false;
            TaiDuLieu();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CHUYENDI_Display : INotifyPropertyChanged
    {
        private int _maCD;
        public int Ma_CD
        {
            get => _maCD;
            set { _maCD = value; OnPropertyChanged(); }
        }

        private string _diemDenCD;
        public string DiemDen_CD
        {
            get => _diemDenCD;
            set { _diemDenCD = value; OnPropertyChanged(); }
        }

        private long? _donGiaCD;
        public long? DonGia_CD
        {
            get => _donGiaCD;
            set { _donGiaCD = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TCChuyenDi_RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public TCChuyenDi_RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}