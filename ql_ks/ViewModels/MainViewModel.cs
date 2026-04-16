using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;
using ql_ks.ViewModels; // Namespace chứa CurrentSession nếu có

namespace ql_ks.ViewModels
{
    public class MainViewModel : Main_BaseViewModel
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private string _filterType; // "Tat ca", "Con trong", "Dang thue"...

        // Các biến hiển thị thống kê
        private int _totalRooms = 0;
        private int _emptyRooms = 0;
        private int _rentedRooms = 0;
        private int _repairRooms = 0;
        private int _selectedRooms = 0;

        // Danh sách phòng để hiển thị lên Grid
        public ObservableCollection<PhongViewModel> RoomList { get; set; }

        // --- GETTER/SETTER CHO GIAO DIỆN THỐNG KÊ ---
        public int TotalRooms
        {
            get => _totalRooms;
            set { _totalRooms = value; OnPropertyChanged(nameof(TotalRooms)); }
        }
        public int EmptyRooms
        {
            get => _emptyRooms;
            set { _emptyRooms = value; OnPropertyChanged(nameof(EmptyRooms)); }
        }
        public int RentedRooms
        {
            get => _rentedRooms;
            set { _rentedRooms = value; OnPropertyChanged(nameof(RentedRooms)); }
        }
        public int RepairRooms
        {
            get => _repairRooms;
            set { _repairRooms = value; OnPropertyChanged(nameof(RepairRooms)); }
        }

        public string FilterType
        {
            get => _filterType;
            set
            {
                _filterType = value;
                OnPropertyChanged(nameof(FilterType));
                FilterRooms(value); // Tự động lọc khi đổi giá trị
            }
        }

        // Command cho các nút
        public ICommand FilterAllCommand { get; }
        public ICommand FilterEmptyCommand { get; }
        public ICommand FilterRentedCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            // Khởi tạo các lệnh
            FilterAllCommand = new Main_RelayCommand(_ => FilterType = "Tat ca");
            FilterEmptyCommand = new Main_RelayCommand(_ => FilterType = "Con trong");
            FilterRentedCommand = new Main_RelayCommand(_ => FilterType = "Dang thue");
            LogoutCommand = new Main_RelayCommand(_ => Logout());

            // Gọi load dữ liệu khi vào trang
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            try
            {
                // Lấy toàn bộ thông tin phòng + loại phòng
                var rooms = (from p in _db.PHONGs
                             join lp in _db.LOAIPHONGs on p.Ma_LP equals lp.Ma_LP
                             select new PhongViewModel
                             {
                                 Ma_Phong = p.Ma_Phong,
                                 Ten_TP = lp.Ten_TP ?? "Chưa đặt tên",
                                 TinhTrang = p.TinhTrang_Phong,
                                 DonGia = lp.DonGia_LP
                             }).ToList();

                RoomList = new ObservableCollection<PhongViewModel>(rooms);

                // Tính toán số liệu thống kê dựa trên trạng thái
                UpdateStatistics(rooms);

                // Mặc định hiện tất cả
                FilterType = "Tat ca";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void UpdateStatistics(System.Collections.Generic.List<PhongViewModel> rooms)
        {
            TotalRooms = rooms.Count;
            // Lưu ý: Kiểm tra chuỗi trùng khớp với dữ liệu trong DB bạn tạo ban đầu
            EmptyRooms = rooms.Count(r => r.TinhTrang == "Trống");
            RentedRooms = rooms.Count(r => r.TinhTrang == "Có khách");
            RepairRooms = rooms.Count(r => r.TinhTrang == "Đang dọn dẹp"); // Hoặc 'Sửa chữa'
        }

        private void FilterRooms(string type)
        {
            var allRooms = RoomList.ToList();
            System.Collections.Generic.List<PhongViewModel> filteredList = new System.Collections.Generic.List<PhongViewModel>();

            switch (type)
            {
                case "Tat ca":
                    filteredList = allRooms;
                    break;
                case "Con trong":
                    filteredList = allRooms.Where(r => r.TinhTrang == "Trống").ToList();
                    break;
                case "Dang thue":
                    filteredList = allRooms.Where(r => r.TinhTrang == "Có khách").ToList();
                    break;
                case "Sua chua":
                    filteredList = allRooms.Where(r => r.TinhTrang == "Đang dọn dẹp").ToList();
                    break;
            }

            // Gán lại collection (cách đơn giản nhất)
            RoomList.Clear();
            foreach (var item in filteredList) RoomList.Add(item);
        }

        private void Logout()
        {
            try
            {
                // Tạo cửa sổ đăng nhập (LoginWindow chứa uc_LoginView bên trong)
                var loginWindow = new LoginWindow();

                // Hiển thị nó
                loginWindow.Show();

                // Đóng cửa sổ chính hiện tại (MainWindow) để quay về màn hình login
                Application.Current?.MainWindow?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thoát: " + ex.Message);
            }
        }
    }

    // Class nhỏ chỉ dùng để Bind dữ liệu phòng lên giao diện card
    public class PhongViewModel
    {
        public int Ma_Phong { get; set; }
        public string Ten_TP { get; set; }
        public string TinhTrang { get; set; }
        public long? DonGia { get; set; }

        // Hỗ trợ đổi màu nền Card tùy theo Trạng thái (Logic đơn giản)
        public string ColorBackground
        {
            get
            {
                if (TinhTrang == "Trống") return "#EAECEF"; // Màu xám nhạt
                if (TinhTrang == "Có khách") return "#3498DB"; // Màu xanh
                if (TinhTrang == "Đang dọn dẹp") return "#F1C40F"; // Màu vàng cam
                return "#FFF";
            }
        }

        // Đổi màu chữ cho dễ nhìn
        public string ColorText
        {
            get
            {
                if (TinhTrang == "Có khách") return "White";
                if (TinhTrang == "Đang dọn dẹp") return "Black";
                return "#333";
            }
        }
    }
}