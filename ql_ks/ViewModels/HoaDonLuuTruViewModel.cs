using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class HoaDonLuuTruViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        // === THÔNG TIN HÓA ĐƠN ===
        private int _maHD;
        public int MaHD
        {
            get => _maHD;
            set { _maHD = value; OnPropertyChanged(); }
        }

        private DateTime _ngayLapHD;
        public DateTime NgayLapHD
        {
            get => _ngayLapHD;
            set { _ngayLapHD = value; OnPropertyChanged(); }
        }

        private string _gioLapHD;
        public string GioLapHD
        {
            get => _gioLapHD;
            set { _gioLapHD = value; OnPropertyChanged(); }
        }

        private string _tenNhanVien;
        public string TenNhanVien
        {
            get => _tenNhanVien;
            set { _tenNhanVien = value; OnPropertyChanged(); }
        }

        private int? _maNV;
        public int? MaNV
        {
            get => _maNV;
            set { _maNV = value; OnPropertyChanged(); }
        }

        // === THÔNG TIN THANH TOÁN ===
        private int _maPhong;
        public int MaPhong
        {
            get => _maPhong;
            set { _maPhong = value; OnPropertyChanged(); }
        }

        private string _loaiPhong;
        public string LoaiPhong
        {
            get => _loaiPhong;
            set { _loaiPhong = value; OnPropertyChanged(); }
        }

        private string _tongTien;
        public string TongTien
        {
            get => _tongTien;
            set { _tongTien = value; OnPropertyChanged(); }
        }

        private long _tongTienSo;
        public long TongTienSo
        {
            get => _tongTienSo;
            set { _tongTienSo = value; OnPropertyChanged(); }
        }

        // === THÔNG TIN KHÁCH HÀNG ===
        private string _cmnd;
        public string CMND
        {
            get => _cmnd;
            set { _cmnd = value; OnPropertyChanged(); }
        }

        private string _tenKhachHang;
        public string TenKhachHang
        {
            get => _tenKhachHang;
            set { _tenKhachHang = value; OnPropertyChanged(); }
        }

        private string _soDienThoaiKH;
        public string SoDienThoaiKH
        {
            get => _soDienThoaiKH;
            set { _soDienThoaiKH = value; OnPropertyChanged(); }
        }

        // === THỜI GIAN LƯU TRÚ ===
        private DateTime _ngayNhan;
        public DateTime NgayNhan
        {
            get => _ngayNhan;
            set
            {
                _ngayNhan = value;
                OnPropertyChanged();
                TinhTongTien();
            }
        }

        private DateTime _ngayTra;
        public DateTime NgayTra
        {
            get => _ngayTra;
            set
            {
                _ngayTra = value;
                OnPropertyChanged();
                TinhTongTien();
            }
        }

        private int _soNgay;
        public int SoNgay
        {
            get => _soNgay;
            set { _soNgay = value; OnPropertyChanged(); }
        }

        private long _donGiaPhong;
        public long DonGiaPhong
        {
            get => _donGiaPhong;
            set { _donGiaPhong = value; OnPropertyChanged(); }
        }

        // === TRẠNG THÁI ===
        private string _thongBao;
        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        private bool _isNewInvoice;
        public bool IsNewInvoice
        {
            get => _isNewInvoice;
            set { _isNewInvoice = value; OnPropertyChanged(); }
        }

        private int _maPhongDaChon;

        // === COMMANDS ===
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }
        public ICommand ThanhToanCommand { get; }

        // === CONSTRUCTOR ===
        public HoaDonLuuTruViewModel(int maPhong)
        {
            _maPhongDaChon = maPhong;

            LuuCommand = new Common_RelayCommand(_ => LuuHoaDon());
            HuyCommand = new Common_RelayCommand(_ => HuyHoaDon());
            ThanhToanCommand = new Common_RelayCommand(_ => ThanhToan());

            NgayLapHD = DateTime.Now;
            GioLapHD = DateTime.Now.ToString("hh:mm tt");
            NgayNhan = DateTime.Now;
            NgayTra = DateTime.Now.AddDays(1);

            LoadData();
        }

        // === LOAD DATA ===
        private void LoadData()
        {
            try
            {
                // 1. Lấy thông tin phòng
                var phong = _db.PHONGs
                    .Include(p => p.LOAIPHONG)
                    .FirstOrDefault(p => p.Ma_Phong == _maPhongDaChon);

                if (phong == null)
                {
                    ThongBao = "Không tìm thấy phòng!";
                    return;
                }

                MaPhong = phong.Ma_Phong;
                LoaiPhong = phong.LOAIPHONG != null
                    ? phong.LOAIPHONG.Ten_TP
                    : "Chưa phân loại";
                DonGiaPhong = phong.LOAIPHONG != null
                    ? (phong.LOAIPHONG.DonGia_LP ?? 0)
                    : 0;

                // 2. Lấy nhân viên từ Login_CurrentSession
                LayThongTinNhanVien();

                // 3. Kiểm tra hóa đơn cũ chưa thanh toán
                var hoaDonCu = (from hd in _db.HOADONs
                                join ct in _db.CHITIET_HDLT on hd.MA_HD equals ct.MA_HD
                                where ct.Ma_Phong == _maPhongDaChon
                                      && hd.TinhTrang_HD == "Chưa thanh toán"
                                select new { HoaDon = hd, ChiTiet = ct })
                               .FirstOrDefault();

                if (hoaDonCu != null)
                {
                    IsNewInvoice = false;
                    LoadHoaDonCu(hoaDonCu.HoaDon, hoaDonCu.ChiTiet);
                }
                else
                {
                    IsNewInvoice = true;
                    TaoMaHoaDonMoi();
                }

                TinhTongTien();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === LẤY THÔNG TIN NHÂN VIÊN TỪ SESSION ===
        private void LayThongTinNhanVien()
        {
            try
            {
                if (Login_CurrentSession.IsLogin
                    && Login_CurrentSession.TaiKhoanDangNhap != null)
                {
                    int maTK = Login_CurrentSession.TaiKhoanDangNhap.Ma_TK;

                    // Tìm nhân viên theo mã tài khoản
                    var nv = _db.NHANVIENs.FirstOrDefault(n => n.Ma_TK == maTK);

                    if (nv != null)
                    {
                        MaNV = nv.MA_NV;
                        TenNhanVien = nv.HoTen_NV;
                    }
                    else
                    {
                        MaNV = null;
                        TenNhanVien = "Admin (" +
                            Login_CurrentSession.TaiKhoanDangNhap.TenDangNhap_TK + ")";
                    }
                }
                else
                {
                    MaNV = null;
                    TenNhanVien = "Chưa đăng nhập";
                }
            }
            catch
            {
                MaNV = null;
                TenNhanVien = "Không xác định";
            }
        }

        // === LOAD HÓA ĐƠN CŨ ===
        private void LoadHoaDonCu(HOADON hd, CHITIET_HDLT ct)
        {
            MaHD = hd.MA_HD;
            NgayLapHD = hd.ThoiGianLap_HD ?? DateTime.Now;
            GioLapHD = NgayLapHD.ToString("hh:mm tt");

            // Nhân viên lập hóa đơn gốc
            if (hd.MA_NV != null)
            {
                var nvLap = _db.NHANVIENs.Find(hd.MA_NV);
                if (nvLap != null)
                {
                    TenNhanVien = nvLap.HoTen_NV;
                    MaNV = nvLap.MA_NV;
                }
            }

            // Khách hàng
            if (hd.MA_KH != null)
            {
                var kh = _db.KHACHHANGs.Find(hd.MA_KH);
                if (kh != null)
                {
                    CMND = kh.CMND_KH ?? "";
                    TenKhachHang = kh.HoTen_KH ?? "";
                    SoDienThoaiKH = kh.SoDienThoai_KH ?? "";
                }
            }

            // Thời gian lưu trú
            NgayNhan = ct.ThoiGianNhan_PHONG ?? DateTime.Now;
            NgayTra = ct.ThoiGianTra_PHONG ?? DateTime.Now.AddDays(1);

            ThongBao = "Hóa đơn đã tồn tại - Mã: " + MaHD;
        }

        // === TẠO MÃ HÓA ĐƠN MỚI ===
        private void TaoMaHoaDonMoi()
        {
            if (_db.HOADONs.Any())
            {
                MaHD = _db.HOADONs.Max(h => h.MA_HD) + 1;
            }
            else
            {
                MaHD = 1;
            }
            ThongBao = "Tạo hóa đơn mới";
        }

        // === TÍNH TỔNG TIỀN ===
        private void TinhTongTien()
        {
            if (NgayTra <= NgayNhan)
            {
                SoNgay = 0;
                TongTienSo = 0;
                TongTien = "0";
                return;
            }

            SoNgay = (NgayTra.Date - NgayNhan.Date).Days;
            if (SoNgay < 1) SoNgay = 1;

            TongTienSo = SoNgay * DonGiaPhong;
            TongTien = TongTienSo.ToString("N0");
        }

        // === LƯU HÓA ĐƠN ===
        private void LuuHoaDon()
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(TenKhachHang))
                {
                    MessageBox.Show("Vui lòng nhập tên khách hàng!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(CMND))
                {
                    MessageBox.Show("Vui lòng nhập CMND khách hàng!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (NgayTra <= NgayNhan)
                {
                    MessageBox.Show("Ngày trả phải sau ngày nhận!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Tìm hoặc tạo khách hàng
                var kh = _db.KHACHHANGs
                    .FirstOrDefault(k => k.CMND_KH == CMND);

                if (kh == null)
                {
                    int maKHMoi = _db.KHACHHANGs.Any()
                        ? _db.KHACHHANGs.Max(k => k.MA_KH) + 1
                        : 1;

                    kh = new KHACHHANG
                    {
                        MA_KH = maKHMoi,
                        HoTen_KH = TenKhachHang,
                        CMND_KH = CMND,
                        SoDienThoai_KH = SoDienThoaiKH
                    };
                    _db.KHACHHANGs.Add(kh);
                }
                else
                {
                    kh.HoTen_KH = TenKhachHang;
                    kh.SoDienThoai_KH = SoDienThoaiKH;
                }

                if (IsNewInvoice)
                {
                    // Tạo hóa đơn mới
                    var hoaDon = new HOADON
                    {
                        MA_HD = MaHD,
                        ThoiGianLap_HD = NgayLapHD,
                        TinhTrang_HD = "Chưa thanh toán",
                        TriGia_HD = TongTienSo,
                        MA_NV = MaNV,
                        MA_KH = kh.MA_KH
                    };
                    _db.HOADONs.Add(hoaDon);

                    // Chi tiết hóa đơn lưu trú
                    int maCTMoi = _db.CHITIET_HDLT.Any()
                        ? _db.CHITIET_HDLT.Max(c => c.Ma_CTHDLT) + 1
                        : 1;

                    var chiTiet = new CHITIET_HDLT
                    {
                        Ma_CTHDLT = maCTMoi,
                        ThoiGianNhan_PHONG = NgayNhan,
                        ThoiGianTra_PHONG = NgayTra,
                        TriGia_CTHDLT = TongTienSo,
                        MA_HD = MaHD,
                        Ma_Phong = MaPhong
                    };
                    _db.CHITIET_HDLT.Add(chiTiet);

                    // Cập nhật trạng thái phòng
                    var phong = _db.PHONGs.Find(MaPhong);
                    if (phong != null)
                    {
                        phong.TinhTrang_Phong = "Có khách";
                    }
                }
                else
                {
                    // Cập nhật hóa đơn cũ
                    var hoaDon = _db.HOADONs.Find(MaHD);
                    if (hoaDon != null)
                    {
                        hoaDon.ThoiGianLap_HD = NgayLapHD;
                        hoaDon.TriGia_HD = TongTienSo;
                        hoaDon.MA_KH = kh.MA_KH;
                    }

                    var ct = _db.CHITIET_HDLT
                        .FirstOrDefault(c => c.MA_HD == MaHD && c.Ma_Phong == MaPhong);
                    if (ct != null)
                    {
                        ct.ThoiGianNhan_PHONG = NgayNhan;
                        ct.ThoiGianTra_PHONG = NgayTra;
                        ct.TriGia_CTHDLT = TongTienSo;
                    }
                }

                _db.SaveChanges();
                IsNewInvoice = false;

                MessageBox.Show("Lưu hóa đơn thành công!\nMã hóa đơn: " + MaHD,
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                ThongBao = "Đã lưu - Mã HD: " + MaHD;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu hóa đơn: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === HỦY HÓA ĐƠN ===
        private void HuyHoaDon()
        {
            if (IsNewInvoice)
            {
                ThongBao = "Đã hủy tạo hóa đơn";
                DongCuaSo();
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc muốn hủy hóa đơn " + MaHD + "?\nDữ liệu sẽ bị xóa!",
                "Xác nhận hủy", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var chiTiets = _db.CHITIET_HDLT
                        .Where(c => c.MA_HD == MaHD).ToList();
                    _db.CHITIET_HDLT.RemoveRange(chiTiets);

                    var hd = _db.HOADONs.Find(MaHD);
                    if (hd != null)
                    {
                        _db.HOADONs.Remove(hd);
                    }

                    var phong = _db.PHONGs.Find(MaPhong);
                    if (phong != null)
                    {
                        phong.TinhTrang_Phong = "Trống";
                    }

                    _db.SaveChanges();

                    MessageBox.Show("Đã hủy hóa đơn " + MaHD, "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    DongCuaSo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi hủy: " + ex.Message, "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // === THANH TOÁN ===
        private void ThanhToan()
        {
            if (IsNewInvoice)
            {
                MessageBox.Show("Vui lòng lưu hóa đơn trước khi thanh toán!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TinhTongTien();

            var result = MessageBox.Show(
                "══════ THANH TOÁN ══════\n\n" +
                "Mã hóa đơn: " + MaHD + "\n" +
                "Phòng: " + MaPhong + " (" + LoaiPhong + ")\n" +
                "Khách hàng: " + TenKhachHang + "\n" +
                "Số ngày: " + SoNgay + " ngày\n" +
                "Đơn giá: " + DonGiaPhong.ToString("N0") + " VNĐ/ngày\n\n" +
                "═══════════════════\n" +
                "TỔNG TIỀN: " + TongTienSo.ToString("N0") + " VNĐ\n" +
                "═══════════════════\n\n" +
                "Xác nhận thanh toán?",
                "Xác nhận thanh toán",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var hd = _db.HOADONs.Find(MaHD);
                    if (hd != null)
                    {
                        hd.TinhTrang_HD = "Đã thanh toán";
                        hd.TriGia_HD = TongTienSo;
                    }

                    var phong = _db.PHONGs.Find(MaPhong);
                    if (phong != null)
                    {
                        phong.TinhTrang_Phong = "Trống";
                    }

                    _db.SaveChanges();

                    MessageBox.Show(
                        "Thanh toán thành công!\n\n" +
                        "Mã HD: " + MaHD + "\n" +
                        "Số tiền: " + TongTienSo.ToString("N0") + " VNĐ",
                        "Thanh toán thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    DongCuaSo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi thanh toán: " + ex.Message, "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // === ĐÓNG CỬA SỔ ===
        private void DongCuaSo()
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w.Title != null && w.Title.Contains("Hóa đơn"))
                {
                    w.Close();
                    return;
                }
            }
        }

        // === INotifyPropertyChanged ===
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}