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
    public class TraCuuLoaiGiatUiViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private bool _isAddingNew = false;
        private List<LOAIGIATUI> _allLoaiGiatUis;

        public ObservableCollection<LOAIGIATUI_Display> DanhSachHienThi { get; set; }

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

        private LOAIGIATUI_Display _selectedLoaiGiatUi;
        public LOAIGIATUI_Display SelectedLoaiGiatUi
        {
            get => _selectedLoaiGiatUi;
            set { _selectedLoaiGiatUi = value; OnPropertyChanged(); }
        }

        public ICommand ThemCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }

        public TraCuuLoaiGiatUiViewModel()
        {
            DanhSachHienThi = new ObservableCollection<LOAIGIATUI_Display>();
            SelectedLoaiGiatUi = new LOAIGIATUI_Display();

            ThemCommand = new TCLoaiGiatUi_RelayCommand(_ => Them());
            LuuCommand = new TCLoaiGiatUi_RelayCommand(_ => Luu());
            SuaCommand = new TCLoaiGiatUi_RelayCommand(_ => Sua());
            XoaCommand = new TCLoaiGiatUi_RelayCommand(_ => Xoa());
            LamMoiCommand = new TCLoaiGiatUi_RelayCommand(_ => LamMoi());

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                _allLoaiGiatUis = _db.LOAIGIATUIs.OrderBy(x => x.Ma_LoaiGU).ToList();
                CapNhatDanhSach(_allLoaiGiatUis);
                ThongBao = $"Tổng số loại giặt ủi: {_allLoaiGiatUis.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void TimKiem()
        {
            if (_allLoaiGiatUis == null) return;

            var keyword = (_tuKhoaTimKiem ?? "").Trim().ToLower();
            var result = string.IsNullOrWhiteSpace(keyword)
                ? _allLoaiGiatUis
                : _allLoaiGiatUis.Where(x =>
                    x.Ma_LoaiGU.ToString().Contains(keyword) ||
                    (x.Ten_LoaiGU ?? "").ToLower().Contains(keyword));

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả: {DanhSachHienThi.Count} loại giặt ủi";
        }

        private void CapNhatDanhSach(List<LOAIGIATUI> list)
        {
            DanhSachHienThi.Clear();
            foreach (var item in list)
            {
                DanhSachHienThi.Add(new LOAIGIATUI_Display
                {
                    Ma_LoaiGU = item.Ma_LoaiGU,
                    Ten_LoaiGU = item.Ten_LoaiGU,
                    DonGia_LoaiGU = item.DonGia_LoaiGU
                });
            }
        }

        public void Them()
        {
            IsAddingNew = true;
            int nextId = _allLoaiGiatUis.Any() ? _allLoaiGiatUis.Max(x => x.Ma_LoaiGU) + 1 : 1;
            SelectedLoaiGiatUi = new LOAIGIATUI_Display
            {
                Ma_LoaiGU = nextId,
                Ten_LoaiGU = "",
                DonGia_LoaiGU = 0
            };
            ThongBao = "Nhập thông tin loại giặt ủi vào form bên trái, sau đó bấm LƯU";
        }

        public void Luu()
        {
            if (SelectedLoaiGiatUi == null || string.IsNullOrWhiteSpace(SelectedLoaiGiatUi.Ten_LoaiGU))
            {
                MessageBox.Show("Tên loại giặt ủi không được để trống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var exists = _db.LOAIGIATUIs.Any(x => x.Ma_LoaiGU == SelectedLoaiGiatUi.Ma_LoaiGU);

                if (IsAddingNew && exists)
                {
                    MessageBox.Show("Mã loại giặt ủi đã tồn tại!", "Lỗi");
                    return;
                }

                if (IsAddingNew || !exists)
                {
                    // THÊM MỚI
                    var entity = new LOAIGIATUI
                    {
                        Ma_LoaiGU = SelectedLoaiGiatUi.Ma_LoaiGU,
                        Ten_LoaiGU = SelectedLoaiGiatUi.Ten_LoaiGU,
                        DonGia_LoaiGU = SelectedLoaiGiatUi.DonGia_LoaiGU
                    };

                    _db.LOAIGIATUIs.Add(entity);
                    _db.SaveChanges();

                    MessageBox.Show("Thêm loại giặt ủi thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAddingNew = false;
                    TaiDuLieu();
                }
                else
                {
                    // CẬP NHẬT
                    var entity = _db.LOAIGIATUIs.Find(SelectedLoaiGiatUi.Ma_LoaiGU);
                    if (entity != null)
                    {
                        entity.Ten_LoaiGU = SelectedLoaiGiatUi.Ten_LoaiGU;
                        entity.DonGia_LoaiGU = SelectedLoaiGiatUi.DonGia_LoaiGU;

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
            if (SelectedLoaiGiatUi == null || SelectedLoaiGiatUi.Ma_LoaiGU <= 0)
            {
                ThongBao = "Vui lòng chọn 1 loại giặt ủi để sửa!";
                return;
            }
            IsAddingNew = false;
            ThongBao = "Chỉnh sửa thông tin bên trái, sau đó bấm LƯU";
        }

        public void Xoa()
        {
            if (SelectedLoaiGiatUi == null || SelectedLoaiGiatUi.Ma_LoaiGU <= 0)
            {
                ThongBao = "Vui lòng chọn 1 loại giặt ủi để xóa!";
                return;
            }

            var rs = MessageBox.Show(
                $"Xóa loại giặt ủi:\nMã: {SelectedLoaiGiatUi.Ma_LoaiGU}\nTên: {SelectedLoaiGiatUi.Ten_LoaiGU}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (rs == MessageBoxResult.Yes)
            {
                try
                {
                    var entity = _db.LOAIGIATUIs.Find(SelectedLoaiGiatUi.Ma_LoaiGU);
                    if (entity != null)
                    {
                        _db.LOAIGIATUIs.Remove(entity);
                        _db.SaveChanges();
                        TaiDuLieu();
                        SelectedLoaiGiatUi = new LOAIGIATUI_Display();
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
            SelectedLoaiGiatUi = new LOAIGIATUI_Display();
            IsAddingNew = false;
            TaiDuLieu();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LOAIGIATUI_Display : INotifyPropertyChanged
    {
        private int _maLoaiGU;
        public int Ma_LoaiGU
        {
            get => _maLoaiGU;
            set { _maLoaiGU = value; OnPropertyChanged(); }
        }

        private string _tenLoaiGU;
        public string Ten_LoaiGU
        {
            get => _tenLoaiGU;
            set { _tenLoaiGU = value; OnPropertyChanged(); }
        }

        private decimal? _donGiaLoaiGU;
        public decimal? DonGia_LoaiGU
        {
            get => _donGiaLoaiGU;
            set { _donGiaLoaiGU = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TCLoaiGiatUi_RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public TCLoaiGiatUi_RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
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