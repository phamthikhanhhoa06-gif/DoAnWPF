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

        // Danh sách gốc (không thay đổi khi lọc)
        private List<PhongViewModel> _allRooms;

        // Danh sách hiển thị (thay đổi khi lọc)
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
        public ICommand ThuePhongCommand { get; }
        public ICommand XemHoaDonCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }

        // === CONSTRUCTOR ===
        public MainViewModel()
        {
            RoomList = new ObservableCollection<PhongViewModel>();
            _allRooms = new List<PhongViewModel>();

            // Commands
            FilterAllCommand = new Main_RelayCommand(_ => FilterRooms("Tất cả"));
            FilterEmptyCommand = new Main_RelayCommand(_ => FilterRooms("Trống"));
            FilterRentedCommand = new Main_RelayCommand(_ => FilterRooms("Có khách"));
            FilterRepairCommand = new Main_RelayCommand(_ => FilterRooms("Đang dọn dẹp"));
            RoomClickCommand = new Main_RelayCommand(param => OnRoomClick(param));
            ThuePhongCommand = new Main_RelayCommand(_ => ThuePhong());
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

                // Cập nhật thống kê
                UpdateStatistics();

                // Hiển thị tất cả
                FilterRooms("Tất cả");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === CẬP NHẬT THỐNG KÊ ===
        private void UpdateStatistics()
        {
            TotalRooms = _allRooms.Count;
            EmptyRooms = _allRooms.Count(r => r.TinhTrang == "Trống");
            RentedRooms = _allRooms.Count(r => r.TinhTrang == "Có khách");
            RepairRooms = _allRooms.Count(r => r.TinhTrang == "Đang dọn dẹp");
            SelectedCount = _allRooms.Count(r => r.IsSelected);
        }

        // === GÁN MÀU PHÒNG ===
        private void SetRoomColor(PhongViewModel item)
        {
            if (item.IsSelected)
            {
                // Phòng đang được chọn → màu tím
                item.ColorBackground = new SolidColorBrush(Color.FromRgb(155, 89, 182));
                item.ColorText = Brushes.White;
                return;
            }

            // Màu theo trạng thái
            if (item.TinhTrang == "Có khách")
            {
                item.ColorBackground = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                item.ColorText = Brushes.White;
            }
            else if (item.TinhTrang == "Đang dọn dẹp")
            {
                item.ColorBackground = new SolidColorBrush(Color.FromRgb(44, 62, 80));
                item.ColorText = Brushes.White;
            }
            else // Trống
            {
                item.ColorBackground = new SolidColorBrush(Color.FromRgb(108, 117, 125));
                item.ColorText = Brushes.White;
            }
        }

        // === LỌC PHÒNG (dùng if-else thay switch expression) ===
        private void FilterRooms(string filter)
        {
            CurrentFilter = filter;
            RoomList.Clear();

            IEnumerable<PhongViewModel> filtered;

            if (filter == "Trống")
            {
                filtered = _allRooms.Where(r => r.TinhTrang == "Trống");
            }
            else if (filter == "Có khách")
            {
                filtered = _allRooms.Where(r => r.TinhTrang == "Có khách");
            }
            else if (filter == "Đang dọn dẹp")
            {
                filtered = _allRooms.Where(r => r.TinhTrang == "Đang dọn dẹp");
            }
            else
            {
                filtered = _allRooms;
            }

            foreach (var room in filtered)
            {
                RoomList.Add(room);
            }
        }

        // === CLICK CHỌN PHÒNG ===
        private void OnRoomClick(object param)
        {
            var clickedRoom = param as PhongViewModel;
            if (clickedRoom == null) return;

            // Toggle selection
            clickedRoom.IsSelected = !clickedRoom.IsSelected;
            SetRoomColor(clickedRoom);

            // Cập nhật đếm
            SelectedCount = _allRooms.Count(r => r.IsSelected);

            // Force UI update
            clickedRoom.NotifyAllChanged();
        }

        // === THUÊ PHÒNG ===
        private void ThuePhong()
        {
            var selectedRooms = _allRooms.Where(r => r.IsSelected).ToList();

            if (selectedRooms.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng cần thuê!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var notEmpty = selectedRooms
                .Where(r => r.TinhTrang != "Trống").ToList();
            if (notEmpty.Count > 0)
            {
                string rooms = string.Join(", ", notEmpty.Select(r => r.Ma_Phong));
                MessageBox.Show("Phòng " + rooms + " không trống!",
                    "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedNames = string.Join(", ",
                selectedRooms.Select(r => r.Ma_Phong));

            var result = MessageBox.Show(
                "Xác nhận thuê phòng: " + selectedNames + "?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var room in selectedRooms)
                    {
                        var phong = _db.PHONGs.Find(room.Ma_Phong);
                        if (phong != null)
                            phong.TinhTrang_Phong = "Có khách";
                    }
                    _db.SaveChanges();

                    // Mở HoaDonWindow cho phòng vừa thuê
                    foreach (var room in selectedRooms)
                    {
                        var hoaDonWindow = new ql_ks.Views.HoaDonWindow(room.Ma_Phong);
                        hoaDonWindow.Closed += (s, e) => LoadInitialData();
                        hoaDonWindow.Show();
                    }

                    LoadInitialData();
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
        // === ĐĂNG XUẤT ===
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

    // === PHONG VIEW MODEL (có INotifyPropertyChanged) ===
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

        // Force cập nhật tất cả property
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