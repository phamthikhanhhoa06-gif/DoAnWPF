using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class DichVuGiatUiViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private int _maPhong = 0;
        private decimal _soCanNang = 0;
        private DateTime? _ngayBatDau = DateTime.Now;
        private DateTime? _ngayKetThuc = DateTime.Now.AddDays(1);
        private decimal _tamTinh = 0;
        private decimal _tongTien = 0;
        private string _thongBao = "";
        private LOAIGIATUI _selectedLoaiGiGui;

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
            set
            {
                _soCanNang = value < 0 ? 0 : value;
                OnPropertyChanged();
                CapNhatTamTinh();
            }
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

        public decimal TamTinh
        {
            get => _tamTinh;
            set { _tamTinh = value; OnPropertyChanged(); }
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
            set
            {
                _selectedLoaiGiGui = value;
                OnPropertyChanged();
                CapNhatTamTinh();
            }
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
            XoaKhoiGioCommand = new GiatUi_RelayCommand(param => XoaKhoiGio(param));
            LapHoaDonCommand = new GiatUi_RelayCommand(_ => LapHoaDon());
            LamMoiCommand = new GiatUi_RelayCommand(_ => LamMoi());

            TaiDuLieu();
        }

        // =============================================
        // TẢI DỮ LIỆU
        // =============================================
        private void TaiDuLieu()
        {
            try
            {
                var loaiList = _db.LOAIGIATUIs.OrderBy(x => x.Ma_LoaiGU).ToList();
                DanhSachLoaiGiGui = new ObservableCollection<LOAIGIATUI>(loaiList);

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

                CapNhatTongTien();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu giặt ủi: " + ex.Message);
            }
        }

        // =============================================
        // TÍNH TIỀN
        // =============================================
        private void CapNhatTamTinh()
        {
            if (_selectedLoaiGiGui != null && _soCanNang > 0)
                TamTinh = (_selectedLoaiGiGui.DonGia_LoaiGU ?? 0) * _soCanNang;
            else
                TamTinh = 0;
        }

        private void CapNhatTongTien()
        {
            TongTien = DanhSachDaChon.Sum(x => x.ThanhTien);
        }

        // =============================================
        // THÊM VÀO GIỎ
        // =============================================
        private void ThemVaoGio()
        {
            if (_maPhong == 0)
            {
                ThongBao = "Vui lòng chọn số phòng!";
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

            decimal giaTri = _selectedLoaiGiGui.DonGia_LoaiGU ?? 0;
            var exist = DanhSachDaChon.FirstOrDefault(x => x.Ma_LoaiGU == _selectedLoaiGiGui.Ma_LoaiGU);

            if (exist != null)
            {
                exist.SoCanNang += _soCanNang;
            }
            else
            {
                DanhSachDaChon.Add(new LuotGiatDaChonVM
                {
                    Ma_LoaiGU = _selectedLoaiGiGui.Ma_LoaiGU,
                    Ten_LoaiGU = _selectedLoaiGiGui.Ten_LoaiGU,
                    DonGia_LoaiGU = giaTri,
                    SoCanNang = _soCanNang,
                    NgayBatDau = _ngayBatDau,
                    NgayKetThuc = _ngayKetThuc
                });
            }

            CapNhatTongTien();
            SoCanNang = 0;
            CapNhatTamTinh();
            ThongBao = $"Đã thêm: {_selectedLoaiGiGui.Ten_LoaiGU}";
        }

        // =============================================
        // XÓA KHỎI GIỎ
        // =============================================
        public void XoaKhoiGio(object item)
        {
            if (item is LuotGiatDaChonVM vm)
            {
                DanhSachDaChon.Remove(vm);
                CapNhatTongTien();
                ThongBao = "Đã xóa thành công.";
            }
            else
            {
                ThongBao = "Chọn 1 dòng trong danh sách để xóa!";
            }
        }

        // =============================================
        // LẬP HÓA ĐƠN - CHỈ CÓ 1 HÀM DUY NHẤT
        // =============================================
        private void LapHoaDon()
        {
            if (MaPhong == 0)
            {
                MessageBox.Show("Vui lòng chọn phòng!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DanhSachDaChon.Count == 0)
            {
                MessageBox.Show("Chưa có dịch vụ nào trong giỏ!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // BƯỚC 1: Tìm hóa đơn lưu trú đang mở của phòng
                var hoaDon = (from hd in _db.HOADONs
                              join ct in _db.CHITIET_HDLT
                                  on hd.MA_HD equals ct.MA_HD
                              where ct.Ma_Phong == MaPhong
                                    && hd.TinhTrang_HD == "Chưa thanh toán"
                              select hd).FirstOrDefault();

                if (hoaDon == null)
                {
                    MessageBox.Show(
                        "Phòng " + MaPhong + " chưa có hóa đơn lưu trú đang mở!\n\n" +
                        "Vui lòng thuê phòng trước khi thêm dịch vụ.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int maHD = hoaDon.MA_HD;
                long tongTienDichVu = 0;

                // BƯỚC 2: Lưu từng dòng trong giỏ hàng
                foreach (var item in DanhSachDaChon)
                {
                    // Tính Ma_LuotGU mới - tính lại mỗi vòng để tránh trùng
                    int maLuotMoi = _db.LUOTGIATUIs.Any()
                        ? _db.LUOTGIATUIs.Max(l => l.Ma_LuotGU) + 1
                        : 1;

                    var luotGU = new LUOTGIATUI
                    {
                        Ma_LuotGU = maLuotMoi,
                        SoKilogram_LuotGU = Convert.ToInt32(item.SoCanNang),
                        NgayBatDau_LuotGU = item.NgayBatDau ?? DateTime.Now,
                        NgayKetThuc_LuotGU = item.NgayKetThuc ?? DateTime.Now.AddDays(1),
                        Ma_LoaiGU = item.Ma_LoaiGU
                    };
                    _db.LUOTGIATUIs.Add(luotGU);
                    _db.SaveChanges(); // Lưu trước để đảm bảo Ma_LuotGU tồn tại

                    // Tính Ma_CTHDGU mới
                    int maCTMoi = _db.CHITIET_HDGU.Any()
                        ? _db.CHITIET_HDGU.Max(c => c.Ma_CTHDGU) + 1
                        : 1;

                    long thanhTien = (long)item.ThanhTien;

                    var chiTiet = new CHITIET_HDGU
                    {
                        Ma_CTHDGU = maCTMoi,
                        ThoiGianLap_CTHDGU = DateTime.Now,
                        TriGia_CTHDGU = thanhTien,
                        MA_HD = maHD,
                        Ma_LuotGU = maLuotMoi
                    };
                    _db.CHITIET_HDGU.Add(chiTiet);
                    _db.SaveChanges();

                    tongTienDichVu += thanhTien;
                }

                // BƯỚC 3: Cập nhật tổng tiền hóa đơn
                var hdCapNhat = _db.HOADONs.Find(maHD);
                if (hdCapNhat != null)
                {
                    hdCapNhat.TriGia_HD = (hdCapNhat.TriGia_HD ?? 0) + tongTienDichVu;
                    _db.SaveChanges();
                }

                // BƯỚC 4: Thông báo thành công
                MessageBox.Show(
                    "✅ Lưu dịch vụ giặt ủi thành công!\n\n" +
                    "Mã hóa đơn : " + maHD + "\n" +
                    "Phòng      : " + MaPhong + "\n" +
                    "Số dịch vụ : " + DanhSachDaChon.Count + " loại\n" +
                    "Tổng tiền  : " + tongTienDichVu.ToString("N0") + " đ",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LamMoi();
            }
            catch (Exception ex)
            {
                string chiTietLoi = ex.Message;
                if (ex.InnerException != null)
                {
                    chiTietLoi += "\n\nChi tiết: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        chiTietLoi += "\n" + ex.InnerException.InnerException.Message;
                }
                MessageBox.Show("❌ Lỗi lưu hóa đơn:\n\n" + chiTietLoi,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =============================================
        // LÀM MỚI
        // =============================================
        private void LamMoi()
        {
            MaPhong = 0;
            SoCanNang = 0;
            NgayBatDau = DateTime.Now;
            NgayKetThuc = DateTime.Now.AddDays(1);
            DanhSachDaChon.Clear();
            TamTinh = 0;
            TongTien = 0;
            ThongBao = "Đã làm mới dữ liệu.";
        }

        // =============================================
        // INOTIFYPROPERTYCHANGED
        // =============================================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // =============================================
    // HELPER CLASSES
    // =============================================
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

    public class PhongChonVM
    {
        public int MaPhong { get; set; }
        public string HienThi { get; set; }
    }
}