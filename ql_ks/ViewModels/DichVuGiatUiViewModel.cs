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
    public class DichVuGiatUiViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private int _maPhong = 0; // Phòng cần giặt ủi
        private decimal _soCanNang = 0; // Số kg
        private DateTime? _ngayBatDau = DateTime.Now; // Ngày nhận
        private DateTime? _ngayKetThuc = DateTime.Now.AddDays(1); // Ngày trả dự kiến

        private decimal _tongTien = 0; // Tổng tiền tính ra
        private string _thongBao = "";

        private LOAIGIATUI _selectedLoaiGiGui; // Loại đang chọn

        // Danh sách hiển thị lên UI
        public ObservableCollection<LOAIGIATUI> DanhSachLoaiGiGui { get; set; }
        public ObservableCollection<LuotGiatDaChonVM> DanhSachDaChon { get; set; }
        public ObservableCollection<PhongChonVM> DanhSachPhong { get; set; }

        public int MaPhong
        {
            get => _maPhong;
            set { _maPhong = value; OnPropertyChanged(); }
        }

        public decimal SoCanNang
        {
            get => _soCanNang;
            set { _soCanNang = value < 0 ? 0 : value; OnPropertyChanged(); CapNhatTongTien(); }
        }

        public DateTime? NgayBatDau
        {
            get => _ngayBatDau;
            set { _ngayBatDau = value; OnPropertyChanged(); }
        }

        public DateTime? NgayKetThuc
        {
            get => _ngayKetThuc;
            set { _ngayKetThuc = value; OnPropertyChanged(); }
        }

        public decimal TongTien
        {
            get => _tongTien;
            set { _tongTien = value; OnPropertyChanged(); }
        }

        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        public LOAIGIATUI SelectedLoaiGiGui
        {
            get => _selectedLoaiGiGui;
            set { _selectedLoaiGiGui = value; OnPropertyChanged(); CapNhatTongTien(); }
        }

        public ICommand ThemVaoGioCommand { get; }
        public ICommand XoaKhoiGioCommand { get; }
        public ICommand LapHoaDonCommand { get; }
        public ICommand LamMoiCommand { get; }

        public DichVuGiatUiViewModel()
        {
            DanhSachLoaiGiGui = new ObservableCollection<LOAIGIATUI>();
            DanhSachDaChon = new ObservableCollection<LuotGiatDaChonVM>();
            DanhSachPhong = new ObservableCollection<PhongChonVM>();

            ThemVaoGioCommand = new GiatUi_RelayCommand(_ => ThemVaoGio());
            XoaKhoiGioCommand = new GiatUi_RelayCommand(_ => XoaKhoiGio());
            LapHoaDonCommand = new GiatUi_RelayCommand(_ => LapHoaDon());
            LamMoiCommand = new GiatUi_RelayCommand(_ => LamMoi());

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                // Load danh sách loại giặt ủi từ DB
                var loaiList = _db.LOAIGIATUIs.OrderBy(x => x.Ma_LoaiGU).ToList();
                DanhSachLoaiGiGui = new ObservableCollection<LOAIGIATUI>(loaiList);

                // Load danh sách phòng có thể chọn (có thể lọc trống hoặc không)
                var phongList = from p in _db.PHONGs
                                join lp in _db.LOAIPHONGs on p.Ma_LP equals lp.Ma_LP
                                orderby p.Ma_Phong
                                select new PhongChonVM
                                {
                                    MaPhong = p.Ma_Phong,
                                    HienThi = p.Ma_Phong + " - " + (lp.Ten_TP ?? "")
                                };

                DanhSachPhong = new ObservableCollection<PhongChonVM>(phongList.ToList());

                if (DanhSachLoaiGiGui.Count > 0)
                    SelectedLoaiGiGui = DanhSachLoaiGiGui.First();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu giặt ủi: " + ex.Message);
            }
        }

        private void ThemVaoGio()
        {
            if (_maPhong == 0)
            {
                ThongBao = "Vui lòng nhập số phòng!";
                return;
            }

            if (_selectedLoaiGiGui == null)
            {
                ThongBao = "Vui lòng chọn loại giặt ủi!";
                return;
            }

            if (_soCanNang <= 0)
            {
                ThongBao = "Số cân nặng phải lớn hơn 0!";
                return;
            }

            // Tính tiền: Đơn giá * Số kg (nếu là Giặt sấy/kg) hoặc Đơn giá cố định (nếu là Hấp/Bộ/...)
            decimal giaTri = SelectedLoaiGiGui.DonGia_LoaiGU ?? 0;

            // Kiểm tra xem đã có loại này chưa -> tăng số lượng
            var exist = DanhSachDaChon.FirstOrDefault(x => x.Ma_LoaiGU == SelectedLoaiGiGui.Ma_LoaiGU);

            if (exist != null)
            {
                exist.SoCanNang += _soCanNang;
            }
            else
            {
                DanhSachDaChon.Add(new LuotGiatDaChonVM
                {
                    Ma_LoaiGU = SelectedLoaiGiGui.Ma_LoaiGU,
                    Ten_LoaiGU = SelectedLoaiGiGui.Ten_LoaiGU,
                    DonGia_LoaiGU = giaTri,
                    SoCanNang = _soCanNang,
                    NgayBatDau = _ngayBatDau,
                    NgayKetThuc = _ngayKetThuc
                });
            }

            CapNhatTongTien();
            ThongBao = $"Đã thêm: {SelectedLoaiGiGui.Ten_LoaiGU} ({_soCanNang} kg)";
        }

        private void XoaKhoiGio()
        {
            // Cần phải lưu reference đến item được chọn trong DataGrid
            ThongBao = "Chọn 1 dòng trong danh sách để xóa!";
        }

        public void XoaKhoiGio(object item)
        {
            if (item is LuotGiatDaChonVM vm)
            {
                DanhSachDaChon.Remove(vm);
                CapNhatTongTien();
                ThongBao = "Đã xóa thành công.";
            }
        }

        private void LamMoi()
        {
            MaPhong = 0;
            SoCanNang = 0;
            NgayBatDau = DateTime.Now;
            NgayKetThuc = DateTime.Now.AddDays(1);
            ThongBao = "Đã làm mới dữ liệu.";
            DanhSachDaChon.Clear();
            TongTien = 0;
        }

        private void LapHoaDon()
        {
            if (MaPhong == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DanhSachDaChon.Count == 0)
            {
                MessageBox.Show("Chưa có món nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. Tạo LUOTGIATUI mới
                LUOTGIATUI luotGu = new LUOTGIATUI
                {
                    SoKilogram_LuotGU = Convert.ToInt32(DanhSachDaChon.Sum(x => x.SoCanNang)),
                    NgayBatDau_LuotGU = NgayBatDau,
                    NgayKetThuc_LuotGU = NgayKetThuc,
                    Ma_LoaiGU = SelectedLoaiGiGui != null ? SelectedLoaiGiGui.Ma_LoaiGU : 0
                };

                _db.LUOTGIATUIs.Add(luotGu);
                _db.SaveChanges();

                // 2. Chi tiết hóa đơn giặt ủi (CHITIET_HDGU) - có thể mở rộng sau
                MessageBox.Show(
                    $"Đã tạo lệnh giặt ủi thành công!\n\n" +
                    $"Phòng: {MaPhong}\n" +
                    $"Số lượng đơn: {DanhSachDaChon.Count}\n" +
                    $"Tổng tiền: {TongTien:N0} đ",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LamMoi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu hóa đơn: " + ex.Message);
            }
        }

        private void CapNhatTongTien()
        {
            if (_selectedLoaiGiGui != null && _soCanNang > 0)
            {
                // Hiển thị giá trị preview cho người dùng thấy trước khi thêm
                decimal gia = SelectedLoaiGiGui.DonGia_LoaiGU ?? 0;
                TongTien = gia * _soCanNang + DanhSachDaChon.Sum(x => x.ThanhTien);
            }
            else
            {
                TongTien = DanhSachDaChon.Sum(x => x.ThanhTien);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Class hiển thị danh sách đã chọn (giống giỏ hàng)
    public class LuotGiatDaChonVM : INotifyPropertyChanged
    {
        private decimal _soCanNang = 1;

        public int Ma_LoaiGU { get; set; }
        public string Ten_LoaiGU { get; set; }
        public decimal DonGia_LoaiGU { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }

        public decimal SoCanNang
        {
            get => _soCanNang;
            set
            {
                _soCanNang = value < 1 ? 1 : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public decimal ThanhTien => DonGia_LoaiGU * SoCanNang;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class cho phòng
    public class PhongChonVM
    {
        public int MaPhong { get; set; }
        public string HienThi { get; set; }
    }
}