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
    public class TraCuuBaoCaoDichVuViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private bool _isAddingNew = false;
        private List<BAOCAODICHVU> _allBaoCaos;

        public ObservableCollection<BAOCAODICHVU_Display> DanhSachHienThi { get; set; }

        public bool IsAddingNew
        {
            get => _isAddingNew;
            set { _isAddingNew = value; OnPropertyChanged(); }
        }

        private string _tuKhoaTimKiem = "";
        public string TuKhoaTimKiem
        {
            get => _tuKhoaTimKiem;
            set { _tuKhoaTimKiem = value; OnPropertyChanged(); LocTheoDieuKien(); }
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

        private BAOCAODICHVU_Display _selectedBaoCao;
        public BAOCAODICHVU_Display SelectedBaoCao
        {
            get => _selectedBaoCao;
            set { _selectedBaoCao = value; OnPropertyChanged(); }
        }

        public ICommand ThemCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }

        public TraCuuBaoCaoDichVuViewModel()
        {
            DanhSachHienThi = new ObservableCollection<BAOCAODICHVU_Display>();
            SelectedBaoCao = new BAOCAODICHVU_Display();

            ThemCommand = new TCBaoCaoDichVu_RelayCommand(_ => Them());
            LuuCommand = new TCBaoCaoDichVu_RelayCommand(_ => Luu());
            SuaCommand = new TCBaoCaoDichVu_RelayCommand(_ => Sua());
            XoaCommand = new TCBaoCaoDichVu_RelayCommand(_ => Xoa());
            LamMoiCommand = new TCBaoCaoDichVu_RelayCommand(_ => LamMoi());

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                _allBaoCaos = _db.BAOCAODICHVUs
                    .OrderByDescending(x => x.THOIGIANLAP_BCDV)
                    .ToList();

                LocTheoDieuKien();
                ThongBao = $"Tổng số báo cáo: {_allBaoCaos.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void LocTheoDieuKien()
        {
            if (_allBaoCaos == null) return;

            var result = _allBaoCaos.AsEnumerable();

            // Tìm kiếm theo mã
            var keyword = (_tuKhoaTimKiem ?? "").Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                result = result.Where(x => x.MA_BCDV.ToString().Contains(keyword));
            }

            // Lọc theo ngày lập
            if (_ngayBatDau.HasValue && !_ngayKetThuc.HasValue)
                result = result.Where(x => x.THOIGIANLAP_BCDV >= _ngayBatDau);
            else if (!_ngayBatDau.HasValue && _ngayKetThuc.HasValue)
                result = result.Where(x => x.THOIGIANLAP_BCDV <= _ngayKetThuc);
            else if (_ngayBatDau.HasValue && _ngayKetThuc.HasValue)
                result = result.Where(x => x.THOIGIANLAP_BCDV >= _ngayBatDau && x.THOIGIANLAP_BCDV <= _ngayKetThuc);

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả: {DanhSachHienThi.Count} / {_allBaoCaos.Count} báo cáo";
        }

        private void CapNhatDanhSach(List<BAOCAODICHVU> list)
        {
            DanhSachHienThi.Clear();
            foreach (var item in list)
            {
                DanhSachHienThi.Add(new BAOCAODICHVU_Display
                {
                    MA_BCDV = item.MA_BCDV,
                    THOIGIANLAP_BCDV = item.THOIGIANLAP_BCDV,
                    TONGDOANHTHU_BCDV = item.TONGDOANHTHU_BCDV,
                    DOANHTHULUUUTRU_BCDV = item.DOANHTHULUUUTRU_BCDV,
                    DOANHTHUANUONG_BCDV = item.DOANHTHUANUONG_BCDV,
                    DOANHTHUGIATUI_BCDV = item.DOANHTHUGIATUI_BCDV,
                    DOANHTHUDICHUYEN_BCDV = item.DOANHTHUDICHUYEN_BCDV,
                    NGAYBATDAU_BCDV = item.NGAYBATDAU_BCDV,
                    NGAYKETTHUC_BCDV = item.NGAYKETTHUC_BCDV
                });
            }
        }

        public void Them()
        {
            IsAddingNew = true;
            int nextId = _allBaoCaos.Any() ? _allBaoCaos.Max(x => x.MA_BCDV) + 1 : 1;
            SelectedBaoCao = new BAOCAODICHVU_Display
            {
                MA_BCDV = nextId,
                THOIGIANLAP_BCDV = DateTime.Now,
                TONGDOANHTHU_BCDV = 0,
                DOANHTHULUUUTRU_BCDV = 0,
                DOANHTHUANUONG_BCDV = 0,
                DOANHTHUGIATUI_BCDV = 0,
                DOANHTHUDICHUYEN_BCDV = 0,
                NGAYBATDAU_BCDV = DateTime.Now.AddMonths(-1),
                NGAYKETTHUC_BCDV = DateTime.Now
            };
            ThongBao = "Nhập thông tin báo cáo vào form bên trái, sau đó bấm LƯU";
        }

        public void Luu()
        {
            if (SelectedBaoCao == null || SelectedBaoCao.MA_BCDV <= 0)
            {
                MessageBox.Show("Mã báo cáo không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var exists = _db.BAOCAODICHVUs.Any(x => x.MA_BCDV == SelectedBaoCao.MA_BCDV);

                if (IsAddingNew && exists)
                {
                    MessageBox.Show("Mã báo cáo đã tồn tại!", "Lỗi");
                    return;
                }

                if (IsAddingNew || !exists)
                {
                    var entity = new BAOCAODICHVU
                    {
                        MA_BCDV = SelectedBaoCao.MA_BCDV,
                        THOIGIANLAP_BCDV = SelectedBaoCao.THOIGIANLAP_BCDV ?? DateTime.Now,
                        TONGDOANHTHU_BCDV = SelectedBaoCao.TONGDOANHTHU_BCDV,
                        DOANHTHULUUUTRU_BCDV = SelectedBaoCao.DOANHTHULUUUTRU_BCDV,
                        DOANHTHUANUONG_BCDV = SelectedBaoCao.DOANHTHUANUONG_BCDV,
                        DOANHTHUGIATUI_BCDV = SelectedBaoCao.DOANHTHUGIATUI_BCDV,
                        DOANHTHUDICHUYEN_BCDV = SelectedBaoCao.DOANHTHUDICHUYEN_BCDV,
                        NGAYBATDAU_BCDV = SelectedBaoCao.NGAYBATDAU_BCDV,
                        NGAYKETTHUC_BCDV = SelectedBaoCao.NGAYKETTHUC_BCDV
                    };

                    _db.BAOCAODICHVUs.Add(entity);
                    _db.SaveChanges();

                    MessageBox.Show("Thêm báo cáo thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAddingNew = false;
                    TaiDuLieu();
                }
                else
                {
                    var entity = _db.BAOCAODICHVUs.Find(SelectedBaoCao.MA_BCDV);
                    if (entity != null)
                    {
                        entity.THOIGIANLAP_BCDV = SelectedBaoCao.THOIGIANLAP_BCDV;
                        entity.TONGDOANHTHU_BCDV = SelectedBaoCao.TONGDOANHTHU_BCDV;
                        entity.DOANHTHULUUUTRU_BCDV = SelectedBaoCao.DOANHTHULUUUTRU_BCDV;
                        entity.DOANHTHUANUONG_BCDV = SelectedBaoCao.DOANHTHUANUONG_BCDV;
                        entity.DOANHTHUGIATUI_BCDV = SelectedBaoCao.DOANHTHUGIATUI_BCDV;
                        entity.DOANHTHUDICHUYEN_BCDV = SelectedBaoCao.DOANHTHUDICHUYEN_BCDV;
                        entity.NGAYBATDAU_BCDV = SelectedBaoCao.NGAYBATDAU_BCDV;
                        entity.NGAYKETTHUC_BCDV = SelectedBaoCao.NGAYKETTHUC_BCDV;

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
            if (SelectedBaoCao == null || SelectedBaoCao.MA_BCDV <= 0)
            {
                ThongBao = "Vui lòng chọn 1 báo cáo để sửa!";
                return;
            }
            IsAddingNew = false;
            ThongBao = "Chỉnh sửa thông tin bên trái, sau đó bấm LƯU";
        }

        public void Xoa()
        {
            if (SelectedBaoCao == null || SelectedBaoCao.MA_BCDV <= 0)
            {
                ThongBao = "Vui lòng chọn 1 báo cáo để xóa!";
                return;
            }

            var rs = MessageBox.Show(
                $"Xóa báo cáo:\nMã: {SelectedBaoCao.MA_BCDV}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (rs == MessageBoxResult.Yes)
            {
                try
                {
                    var entity = _db.BAOCAODICHVUs.Find(SelectedBaoCao.MA_BCDV);
                    if (entity != null)
                    {
                        _db.BAOCAODICHVUs.Remove(entity);
                        _db.SaveChanges();
                        TaiDuLieu();
                        SelectedBaoCao = new BAOCAODICHVU_Display();
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
            NgayBatDau = null;
            NgayKetThuc = null;
            SelectedBaoCao = new BAOCAODICHVU_Display();
            IsAddingNew = false;
            TaiDuLieu();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BAOCAODICHVU_Display : INotifyPropertyChanged
    {
        private int _maBCDV;
        public int MA_BCDV
        {
            get => _maBCDV;
            set { _maBCDV = value; OnPropertyChanged(); }
        }

        private DateTime? _thoiGianLap;
        public DateTime? THOIGIANLAP_BCDV
        {
            get => _thoiGianLap;
            set { _thoiGianLap = value; OnPropertyChanged(); }
        }

        private decimal? _tongDoanhThu;
        public decimal? TONGDOANHTHU_BCDV
        {
            get => _tongDoanhThu;
            set { _tongDoanhThu = value; OnPropertyChanged(); }
        }

        private decimal? _doanhThuLuuTru;
        public decimal? DOANHTHULUUUTRU_BCDV
        {
            get => _doanhThuLuuTru;
            set { _doanhThuLuuTru = value; OnPropertyChanged(); }
        }

        private decimal? _doanhThuAnUong;
        public decimal? DOANHTHUANUONG_BCDV
        {
            get => _doanhThuAnUong;
            set { _doanhThuAnUong = value; OnPropertyChanged(); }
        }

        private decimal? _doanhThuGiatUi;
        public decimal? DOANHTHUGIATUI_BCDV
        {
            get => _doanhThuGiatUi;
            set { _doanhThuGiatUi = value; OnPropertyChanged(); }
        }

        private decimal? _doanhThuDiChuyen;
        public decimal? DOANHTHUDICHUYEN_BCDV
        {
            get => _doanhThuDiChuyen;
            set { _doanhThuDiChuyen = value; OnPropertyChanged(); }
        }

        private DateTime? _ngayBatDau;
        public DateTime? NGAYBATDAU_BCDV
        {
            get => _ngayBatDau;
            set { _ngayBatDau = value; OnPropertyChanged(); }
        }

        private DateTime? _ngayKetThuc;
        public DateTime? NGAYKETTHUC_BCDV
        {
            get => _ngayKetThuc;
            set { _ngayKetThuc = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TCBaoCaoDichVu_RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public TCBaoCaoDichVu_RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
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