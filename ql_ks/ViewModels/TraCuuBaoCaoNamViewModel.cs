using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class TraCuuBaoCaoNamViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private bool _isAddingNew = false;
        private List<BAOCAONAM> _allBaoCaos;

        public ObservableCollection<BAOCAONAM_Display> DanhSachHienThi { get; set; }

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

        private string _thongBao = "";
        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        private BAOCAONAM_Display _selectedBaoCao;
        public BAOCAONAM_Display SelectedBaoCao
        {
            get => _selectedBaoCao;
            set { _selectedBaoCao = value; OnPropertyChanged(); }
        }

        public ICommand ThemCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand SuaCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand LamMoiCommand { get; }

        public TraCuuBaoCaoNamViewModel()
        {
            DanhSachHienThi = new ObservableCollection<BAOCAONAM_Display>();
            SelectedBaoCao = new BAOCAONAM_Display();

            ThemCommand = new TCBaoCaoNam_RelayCommand(_ => Them());
            LuuCommand = new TCBaoCaoNam_RelayCommand(_ => Luu());
            SuaCommand = new TCBaoCaoNam_RelayCommand(_ => Sua());
            XoaCommand = new TCBaoCaoNam_RelayCommand(_ => Xoa());
            LamMoiCommand = new TCBaoCaoNam_RelayCommand(_ => LamMoi());

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                _allBaoCaos = _db.BAOCAONAMs
                    .OrderByDescending(x => x.NAM_BCN)
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
            var keyword = (_tuKhoaTimKiem ?? "").Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                result = result.Where(x =>
                    x.MA_BCN.ToString().Contains(keyword) ||
                    (x.NAM_BCN?.ToString() ?? "").Contains(keyword));
            }

            CapNhatDanhSach(result.ToList());
            ThongBao = $"Kết quả: {DanhSachHienThi.Count} / {_allBaoCaos.Count} báo cáo";
        }

        private void CapNhatDanhSach(List<BAOCAONAM> list)
        {
            DanhSachHienThi.Clear();
            foreach (var item in list)
            {
                DanhSachHienThi.Add(new BAOCAONAM_Display
                {
                    MA_BCN = item.MA_BCN,
                    THOIGIANLAP_BCN = item.THOIGIANLAP_BCN,
                    NAM_BCN = item.NAM_BCN,
                    TONGDOANHTHU_BCN = item.TONGDOANHTHU_BCN,
                    DOANHTHUTHANG1_BCN = item.DOANHTHUTHANG1_BCN,
                    DOANHTHUTHANG2_BCN = item.DOANHTHUTHANG2_BCN,
                    DOANHTHUTHANG3_BCN = item.DOANHTHUTHANG3_BCN,
                    DOANHTHUTHANG4_BCN = item.DOANHTHUTHANG4_BCN,
                    DOANHTHUTHANG5_BCN = item.DOANHTHUTHANG5_BCN,
                    DOANHTHUTHANG6_BCN = item.DOANHTHUTHANG6_BCN,
                    DOANHTHUTHANG7_BCN = item.DOANHTHUTHANG7_BCN,
                    DOANHTHUTHANG8_BCN = item.DOANHTHUTHANG8_BCN,
                    DOANHTHUTHANG9_BCN = item.DOANHTHUTHANG9_BCN,
                    DOANHTHUTHANG10_BCN = item.DOANHTHUTHANG10_BCN,
                    DOANHTHUTHANG11_BCN = item.DOANHTHUTHANG11_BCN,
                    DOANHTHUTHANG12_BCN = item.DOANHTHUTHANG12_BCN
                });
            }
        }

        public void Them()
        {
            IsAddingNew = true;
            int nextId = _allBaoCaos.Any() ? _allBaoCaos.Max(x => x.MA_BCN) + 1 : 1;
            int nextYear = DateTime.Now.Year;
            SelectedBaoCao = new BAOCAONAM_Display
            {
                MA_BCN = nextId,
                THOIGIANLAP_BCN = DateTime.Now,
                NAM_BCN = nextYear,
                TONGDOANHTHU_BCN = 0,
                DOANHTHUTHANG1_BCN = 0,
                DOANHTHUTHANG2_BCN = 0,
                DOANHTHUTHANG3_BCN = 0,
                DOANHTHUTHANG4_BCN = 0,
                DOANHTHUTHANG5_BCN = 0,
                DOANHTHUTHANG6_BCN = 0,
                DOANHTHUTHANG7_BCN = 0,
                DOANHTHUTHANG8_BCN = 0,
                DOANHTHUTHANG9_BCN = 0,
                DOANHTHUTHANG10_BCN = 0,
                DOANHTHUTHANG11_BCN = 0,
                DOANHTHUTHANG12_BCN = 0
            };
            ThongBao = "Nhập thông tin báo cáo năm vào form bên trái, sau đó bấm LƯU";
        }

        public void Luu()
        {
            if (SelectedBaoCao == null || SelectedBaoCao.MA_BCN <= 0)
            {
                MessageBox.Show("Mã báo cáo không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var exists = _db.BAOCAONAMs.Any(x => x.MA_BCN == SelectedBaoCao.MA_BCN);

                if (IsAddingNew && exists)
                {
                    MessageBox.Show("Mã báo cáo đã tồn tại!", "Lỗi");
                    return;
                }

                if (IsAddingNew || !exists)
                {
                    var entity = new BAOCAONAM
                    {
                        MA_BCN = SelectedBaoCao.MA_BCN,
                        THOIGIANLAP_BCN = SelectedBaoCao.THOIGIANLAP_BCN ?? DateTime.Now,
                        NAM_BCN = SelectedBaoCao.NAM_BCN,
                        TONGDOANHTHU_BCN = SelectedBaoCao.TONGDOANHTHU_BCN,
                        DOANHTHUTHANG1_BCN = SelectedBaoCao.DOANHTHUTHANG1_BCN,
                        DOANHTHUTHANG2_BCN = SelectedBaoCao.DOANHTHUTHANG2_BCN,
                        DOANHTHUTHANG3_BCN = SelectedBaoCao.DOANHTHUTHANG3_BCN,
                        DOANHTHUTHANG4_BCN = SelectedBaoCao.DOANHTHUTHANG4_BCN,
                        DOANHTHUTHANG5_BCN = SelectedBaoCao.DOANHTHUTHANG5_BCN,
                        DOANHTHUTHANG6_BCN = SelectedBaoCao.DOANHTHUTHANG6_BCN,
                        DOANHTHUTHANG7_BCN = SelectedBaoCao.DOANHTHUTHANG7_BCN,
                        DOANHTHUTHANG8_BCN = SelectedBaoCao.DOANHTHUTHANG8_BCN,
                        DOANHTHUTHANG9_BCN = SelectedBaoCao.DOANHTHUTHANG9_BCN,
                        DOANHTHUTHANG10_BCN = SelectedBaoCao.DOANHTHUTHANG10_BCN,
                        DOANHTHUTHANG11_BCN = SelectedBaoCao.DOANHTHUTHANG11_BCN,
                        DOANHTHUTHANG12_BCN = SelectedBaoCao.DOANHTHUTHANG12_BCN
                    };

                    _db.BAOCAONAMs.Add(entity);
                    _db.SaveChanges();

                    MessageBox.Show("Thêm báo cáo năm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAddingNew = false;
                    TaiDuLieu();
                }
                else
                {
                    var entity = _db.BAOCAONAMs.Find(SelectedBaoCao.MA_BCN);
                    if (entity != null)
                    {
                        entity.THOIGIANLAP_BCN = SelectedBaoCao.THOIGIANLAP_BCN;
                        entity.NAM_BCN = SelectedBaoCao.NAM_BCN;
                        entity.TONGDOANHTHU_BCN = SelectedBaoCao.TONGDOANHTHU_BCN;
                        entity.DOANHTHUTHANG1_BCN = SelectedBaoCao.DOANHTHUTHANG1_BCN;
                        entity.DOANHTHUTHANG2_BCN = SelectedBaoCao.DOANHTHUTHANG2_BCN;
                        entity.DOANHTHUTHANG3_BCN = SelectedBaoCao.DOANHTHUTHANG3_BCN;
                        entity.DOANHTHUTHANG4_BCN = SelectedBaoCao.DOANHTHUTHANG4_BCN;
                        entity.DOANHTHUTHANG5_BCN = SelectedBaoCao.DOANHTHUTHANG5_BCN;
                        entity.DOANHTHUTHANG6_BCN = SelectedBaoCao.DOANHTHUTHANG6_BCN;
                        entity.DOANHTHUTHANG7_BCN = SelectedBaoCao.DOANHTHUTHANG7_BCN;
                        entity.DOANHTHUTHANG8_BCN = SelectedBaoCao.DOANHTHUTHANG8_BCN;
                        entity.DOANHTHUTHANG9_BCN = SelectedBaoCao.DOANHTHUTHANG9_BCN;
                        entity.DOANHTHUTHANG10_BCN = SelectedBaoCao.DOANHTHUTHANG10_BCN;
                        entity.DOANHTHUTHANG11_BCN = SelectedBaoCao.DOANHTHUTHANG11_BCN;
                        entity.DOANHTHUTHANG12_BCN = SelectedBaoCao.DOANHTHUTHANG12_BCN;

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
            if (SelectedBaoCao == null || SelectedBaoCao.MA_BCN <= 0)
            {
                ThongBao = "Vui lòng chọn 1 báo cáo để sửa!";
                return;
            }
            IsAddingNew = false;
            ThongBao = "Chỉnh sửa thông tin bên trái, sau đó bấm LƯU";
        }

        public void Xoa()
        {
            if (SelectedBaoCao == null || SelectedBaoCao.MA_BCN <= 0)
            {
                ThongBao = "Vui lòng chọn 1 báo cáo để xóa!";
                return;
            }

            var rs = MessageBox.Show(
                $"Xóa báo cáo năm:\nMã: {SelectedBaoCao.MA_BCN}\nNăm: {SelectedBaoCao.NAM_BCN}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (rs == MessageBoxResult.Yes)
            {
                try
                {
                    var entity = _db.BAOCAONAMs.Find(SelectedBaoCao.MA_BCN);
                    if (entity != null)
                    {
                        _db.BAOCAONAMs.Remove(entity);
                        _db.SaveChanges();
                        TaiDuLieu();
                        SelectedBaoCao = new BAOCAONAM_Display();
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
            SelectedBaoCao = new BAOCAONAM_Display();
            IsAddingNew = false;
            TaiDuLieu();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BAOCAONAM_Display : INotifyPropertyChanged
    {
        private int _maBCN;
        public int MA_BCN
        {
            get => _maBCN;
            set { _maBCN = value; OnPropertyChanged(); }
        }

        private DateTime? _thoiGianLap;
        public DateTime? THOIGIANLAP_BCN
        {
            get => _thoiGianLap;
            set { _thoiGianLap = value; OnPropertyChanged(); }
        }

        private int? _namBCN;
        public int? NAM_BCN
        {
            get => _namBCN;
            set { _namBCN = value; OnPropertyChanged(); }
        }

        private decimal? _tongDoanhThu;
        public decimal? TONGDOANHTHU_BCN
        {
            get => _tongDoanhThu;
            set { _tongDoanhThu = value; OnPropertyChanged(); }
        }

        public decimal? DOANHTHUTHANG1_BCN { get; set; }
        public decimal? DOANHTHUTHANG2_BCN { get; set; }
        public decimal? DOANHTHUTHANG3_BCN { get; set; }
        public decimal? DOANHTHUTHANG4_BCN { get; set; }
        public decimal? DOANHTHUTHANG5_BCN { get; set; }
        public decimal? DOANHTHUTHANG6_BCN { get; set; }
        public decimal? DOANHTHUTHANG7_BCN { get; set; }
        public decimal? DOANHTHUTHANG8_BCN { get; set; }
        public decimal? DOANHTHUTHANG9_BCN { get; set; }
        public decimal? DOANHTHUTHANG10_BCN { get; set; }
        public decimal? DOANHTHUTHANG11_BCN { get; set; }
        public decimal? DOANHTHUTHANG12_BCN { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TCBaoCaoNam_RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public TCBaoCaoNam_RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
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