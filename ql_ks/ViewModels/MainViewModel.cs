using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class MainViewModel : Main_BaseViewModel
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private List<PhongViewModel> _allRooms;

        public ObservableCollection<PhongViewModel> RoomList { get; set; }

        // === THỐNG KÊ ===
        private int _totalRooms;
        public int TotalRooms
        {
            get => _totalRooms;
            set { _totalRooms = value; OnPropertyChanged(nameof(TotalRooms)); }
        }

        private int _emptyRooms;
        public int EmptyRooms
        {
            get => _emptyRooms;
            set { _emptyRooms = value; OnPropertyChanged(nameof(EmptyRooms)); }
        }

        private int _rentedRooms;
        public int RentedRooms
        {
            get => _rentedRooms;
            set { _rentedRooms = value; OnPropertyChanged(nameof(RentedRooms)); }
        }

        private int _repairRooms;
        public int RepairRooms
        {
            get => _repairRooms;
            set { _repairRooms = value; OnPropertyChanged(nameof(RepairRooms)); }
        }

        private int _selectedCount;
        public int SelectedCount
        {
            get => _selectedCount;
            set { _selectedCount = value; OnPropertyChanged(nameof(SelectedCount)); }
        }

        private string _currentFilter = "Tất cả";
        public string CurrentFilter
        {
            get => _currentFilter;
            set { _currentFilter = value; OnPropertyChanged(nameof(CurrentFilter)); }
        }

        // === COMMANDS ===
        public ICommand FilterAllCommand { get; }
        public ICommand FilterEmptyCommand { get; }
        public ICommand FilterRentedCommand { get; }
        public ICommand FilterRepairCommand { get; }
        public ICommand RoomClickCommand { get; }
        public ICommand XemHoaDonCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }

        // === CONSTRUCTOR ===
        public MainViewModel()
        {
            RoomList = new ObservableCollection<PhongViewModel>();
            _allRooms = new List<PhongViewModel>();

            FilterAllCommand = new Main_RelayCommand(_ => FilterRooms("Tất cả"));
            FilterEmptyCommand = new Main_RelayCommand(_ => FilterRooms("Trống"));
            FilterRentedCommand = new Main_RelayCommand(_ => FilterRooms("Có khách"));
            FilterRepairCommand = new Main_RelayCommand(_ => FilterRooms("Đang dọn dẹp"));
            RoomClickCommand = new Main_RelayCommand(param => OnRoomClick(param));
            XemHoaDonCommand = new Main_RelayCommand(_ => XemHoaDon());
            RefreshCommand = new Main_RelayCommand(_ => LoadInitialData());
            LogoutCommand = new Main_RelayCommand(_ => Logout());

            LoadInitialData();
        }

        // === LOAD DATA ===
        public void LoadInitialData()
        {
            try
            {
                var rooms = (from p in _db.PHONGs
                             join lp in _db.LOAIPHONGs on p.Ma_LP equals lp.Ma_LP
                             select new
                             {
                                 p.Ma_Phong,
                                 lp.Ten_TP,
                                 p.TinhTrang_Phong,
                                 lp.DonGia_LP
                             }).ToList();

                _allRooms.Clear();

                foreach (var r in rooms)
                {
                    string tinhTrang = (r.TinhTrang_Phong ?? "Trống").Trim();
                    string tenLoai = (r.Ten_TP ?? "Chưa phân loại").Trim();

                    var item = new PhongViewModel
                    {
                        Ma_Phong = r.Ma_Phong,
                        Ten_TP = tenLoai,
                        TinhTrang = tinhTrang,
                        DonGia = r.DonGia_LP,
                        IsSelected = false
                    };

                    SetRoomColor(item);
                    _allRooms.Add(item);
                }

                UpdateStatistics();
                FilterRooms("Tất cả");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            TotalRooms = _allRooms.Count;
            EmptyRooms = _allRooms.Count(r => r.TinhTrang == "Trống");
            RentedRooms = _allRooms.Count(r => r.TinhTrang == "Có khách");
            RepairRooms = _allRooms.Count(r => r.TinhTrang == "Đang dọn dẹp");
            SelectedCount = _allRooms.Count(r => r.IsSelected);
        }

        private void SetRoomColor(PhongViewModel item)
        {
            if (item.IsSelected)
            {
                item.ColorBackground = new SolidColorBrush(Color.FromRgb(155, 89, 182));
                item.ColorText = Brushes.White;
                return;
            }

            if (item.TinhTrang == "Có khách")
            {
                item.ColorBackground = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                item.ColorText = Brushes.White;
            }
            else if (item.TinhTrang == "Đang dọn dẹp")
            {
                item.ColorBackground = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Đỏ
                item.ColorText = Brushes.White;
            }
            else // Trống
            {
                item.ColorBackground = new SolidColorBrush(Color.FromRgb(108, 117, 125));
                item.ColorText = Brushes.White;
            }
        }

        private void FilterRooms(string filter)
        {
            CurrentFilter = filter;
            RoomList.Clear();

            IEnumerable<PhongViewModel> filtered;

            if (filter == "Trống")
                filtered = _allRooms.Where(r => r.TinhTrang == "Trống");
            else if (filter == "Có khách")
                filtered = _allRooms.Where(r => r.TinhTrang == "Có khách");
            else if (filter == "Đang dọn dẹp")
                filtered = _allRooms.Where(r => r.TinhTrang == "Đang dọn dẹp");
            else
                filtered = _allRooms;

            foreach (var room in filtered)
                RoomList.Add(room);
        }

        // === CLICK VÀO PHÒNG ===
        private void OnRoomClick(object param)
        {
            var clickedRoom = param as PhongViewModel;
            if (clickedRoom == null) return;

            // === PHÒNG TRỐNG → Mở giao diện thuê phòng ===
            if (clickedRoom.TinhTrang == "Trống")
            {
                MoGiaoDienThuePhong(clickedRoom);
                return;
            }

            // === PHÒNG ĐANG DỌN DẸP → Xác nhận hoàn thành dọn dẹp ===
            if (clickedRoom.TinhTrang == "Đang dọn dẹp")
            {
                XacNhanDonDep(clickedRoom);
                return;
            }

            // === PHÒNG CÓ KHÁCH → Toggle chọn để xem hóa đơn ===
            if (clickedRoom.TinhTrang == "Có khách")
            {
                clickedRoom.IsSelected = !clickedRoom.IsSelected;
                SetRoomColor(clickedRoom);
                SelectedCount = _allRooms.Count(r => r.IsSelected);
                clickedRoom.NotifyAllChanged();
                return;
            }
        }

        // === MỞ GIAO DIỆN THUÊ PHÒNG (Window mới giống hóa đơn lưu trú) ===
        private void MoGiaoDienThuePhong(PhongViewModel room)
        {
            var thuePhongWindow = new ql_ks.Views.ThuePhongWindow(room.Ma_Phong);
            thuePhongWindow.Closed += (s, e) => LoadInitialData();
            thuePhongWindow.ShowDialog();
        }

        // === XÁC NHẬN DỌN DẸP HOÀN THÀNH ===
        private void XacNhanDonDep(PhongViewModel room)
        {
            var result = MessageBox.Show(
                "Xác nhận dọn dẹp hoàn thành cho phòng " + room.Ma_Phong + "?",
                "Xác nhận dọn dẹp hoàn thành",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var phong = _db.PHONGs.Find(room.Ma_Phong);
                    if (phong != null)
                    {
                        phong.TinhTrang_Phong = "Trống";
                        _db.SaveChanges();
                        LoadInitialData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // === XEM HÓA ĐƠN ===
        private void XemHoaDon()
        {
            var selectedRooms = _allRooms.Where(r => r.IsSelected).ToList();

            if (selectedRooms.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng để xem hóa đơn!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var rentedSelected = selectedRooms
                .Where(r => r.TinhTrang == "Có khách").ToList();

            if (rentedSelected.Count == 0)
            {
                MessageBox.Show("Chỉ có thể xem hóa đơn phòng đang có khách!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var room in rentedSelected)
            {
                var hoaDonWindow = new ql_ks.Views.HoaDonWindow(room.Ma_Phong);
                hoaDonWindow.Closed += (s, e) => LoadInitialData();
                hoaDonWindow.Show();
            }
        }

        private void Logout()
        {
            try
            {
                var rs = MessageBox.Show("Bạn có chắc muốn đăng xuất?",
                    "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (rs == MessageBoxResult.Yes)
                {
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    Application.Current.MainWindow.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thoát: " + ex.Message);
            }
        }
    }

    // === PHONG VIEW MODEL ===
    public class PhongViewModel : INotifyPropertyChanged
    {
        private int _maPhong;
        public int Ma_Phong
        {
            get => _maPhong;
            set { _maPhong = value; OnPropertyChanged(nameof(Ma_Phong)); }
        }

        private string _tenTP;
        public string Ten_TP
        {
            get => _tenTP;
            set { _tenTP = value; OnPropertyChanged(nameof(Ten_TP)); }
        }

        private string _tinhTrang;
        public string TinhTrang
        {
            get => _tinhTrang;
            set { _tinhTrang = value; OnPropertyChanged(nameof(TinhTrang)); }
        }

        private long? _donGia;
        public long? DonGia
        {
            get => _donGia;
            set { _donGia = value; OnPropertyChanged(nameof(DonGia)); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        private Brush _colorBackground;
        public Brush ColorBackground
        {
            get => _colorBackground;
            set { _colorBackground = value; OnPropertyChanged(nameof(ColorBackground)); }
        }

        private Brush _colorText;
        public Brush ColorText
        {
            get => _colorText;
            set { _colorText = value; OnPropertyChanged(nameof(ColorText)); }
        }

        public void NotifyAllChanged()
        {
            OnPropertyChanged(nameof(ColorBackground));
            OnPropertyChanged(nameof(ColorText));
            OnPropertyChanged(nameof(IsSelected));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}