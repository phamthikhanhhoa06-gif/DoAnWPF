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
    public class HoaDonAnUongViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private readonly int _maPhongDaChon;

        private int? _maNV;
        private bool _isNewInvoice;

        // ================= THÔNG TIN HÓA ĐƠN =================

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

        // ================= THÔNG TIN KHÁCH HÀNG =================

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

        private string _cmnd;
        public string CMND
        {
            get => _cmnd;
            set { _cmnd = value; OnPropertyChanged(); }
        }

        // ================= THÔNG TIN MẶT HÀNG =================

        private ObservableCollection<MATHANG> _matHangList;
        public ObservableCollection<MATHANG> MatHangList
        {
            get => _matHangList;
            set { _matHangList = value; OnPropertyChanged(); }
        }

        private MATHANG _selectedMatHang;
        public MATHANG SelectedMatHang
        {
            get => _selectedMatHang;
            set
            {
                _selectedMatHang = value;
                OnPropertyChanged();

                if (_selectedMatHang != null)
                {
                    MaMatHang = _selectedMatHang.Ma_MH;
                    TenMatHang = _selectedMatHang.Ten_MH;
                    DonGiaSo = _selectedMatHang.DonGia_MH ?? 0;
                    DonGiaText = DonGiaSo.ToString("N0");
                }
                else
                {
                    MaMatHang = 0;
                    TenMatHang = "";
                    DonGiaSo = 0;
                    DonGiaText = "0";
                }

                TinhTongTien();
            }
        }

        private int _maMatHang;
        public int MaMatHang
        {
            get => _maMatHang;
            set { _maMatHang = value; OnPropertyChanged(); }
        }

        private string _tenMatHang;
        public string TenMatHang
        {
            get => _tenMatHang;
            set { _tenMatHang = value; OnPropertyChanged(); }
        }

        private long _donGiaSo;
        public long DonGiaSo
        {
            get => _donGiaSo;
            set { _donGiaSo = value; OnPropertyChanged(); TinhTongTien(); }
        }

        private string _donGiaText;
        public string DonGiaText
        {
            get => _donGiaText;
            set { _donGiaText = value; OnPropertyChanged(); }
        }

        private int _soLuong;
        public int SoLuong
        {
            get => _soLuong;
            set { _soLuong = value; OnPropertyChanged(); TinhTongTien(); }
        }

        private string _soLuongText;
        public string SoLuongText
        {
            get => _soLuongText;
            set
            {
                _soLuongText = value;
                OnPropertyChanged();

                int sl;
                if (int.TryParse(value, out sl) && sl >= 0)
                {
                    SoLuong = sl;
                }
                else
                {
                    SoLuong = 0;
                    TinhTongTien();
                }
            }
        }

        private long _tongTienSo;
        public long TongTienSo
        {
            get => _tongTienSo;
            set { _tongTienSo = value; OnPropertyChanged(); }
        }

        private string _tongTien;
        public string TongTien
        {
            get => _tongTien;
            set { _tongTien = value; OnPropertyChanged(); }
        }

        // ================= TRẠNG THÁI =================

        private string _thongBao;
        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        // ================= COMMAND =================

        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }

        public HoaDonAnUongViewModel(int maPhong)
        {
            _maPhongDaChon = maPhong;

            MatHangList = new ObservableCollection<MATHANG>();

            LuuCommand = new Common_RelayCommand(_ => LuuHoaDon());
            HuyCommand = new Common_RelayCommand(_ => HuyHoaDon());

            NgayLapHD = DateTime.Now;
            GioLapHD = DateTime.Now.ToString("hh:mm tt");
            SoLuongText = "1";

            LoadData();
        }

        // ================= LOAD DATA =================

        private void LoadData()
        {
            try
            {
                LayThongTinNhanVien();
                LoadDanhSachMatHang();

                // Tìm hóa đơn lưu trú chưa thanh toán của phòng.
                // Nếu có, hóa đơn ăn uống sẽ cộng vào hóa đơn đó.
                var hoaDonPhong = (from hd in _db.HOADONs
                                   join ctlt in _db.CHITIET_HDLT
                                       on hd.MA_HD equals ctlt.MA_HD
                                   where ctlt.Ma_Phong == _maPhongDaChon
                                         && hd.TinhTrang_HD == "Chưa thanh toán"
                                   select hd).FirstOrDefault();

                if (hoaDonPhong != null)
                {
                    _isNewInvoice = false;

                    MaHD = hoaDonPhong.MA_HD;
                    NgayLapHD = hoaDonPhong.ThoiGianLap_HD ?? DateTime.Now;
                    GioLapHD = NgayLapHD.ToString("hh:mm tt");

                    if (hoaDonPhong.MA_NV != null)
                    {
                        var nv = _db.NHANVIENs.Find(hoaDonPhong.MA_NV);
                        if (nv != null)
                        {
                            _maNV = nv.MA_NV;
                            TenNhanVien = nv.HoTen_NV;
                        }
                    }

                    if (hoaDonPhong.MA_KH != null)
                    {
                        var kh = _db.KHACHHANGs.Find(hoaDonPhong.MA_KH);
                        if (kh != null)
                        {
                            TenKhachHang = kh.HoTen_KH ?? "";
                            SoDienThoaiKH = kh.SoDienThoai_KH ?? "";
                            CMND = kh.CMND_KH ?? "";
                        }
                    }

                    ThongBao = "Sử dụng hóa đơn hiện tại - Mã: " + MaHD;
                }
                else
                {
                    _isNewInvoice = true;
                    TaoMaHoaDonMoi();
                    ThongBao = "Tạo hóa đơn ăn uống mới";
                }

                TinhTongTien();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu hóa đơn ăn uống: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDanhSachMatHang()
        {
            MatHangList.Clear();

            var ds = _db.MATHANGs
                .OrderBy(m => m.Ma_MH)
                .ToList();

            foreach (var mh in ds)
            {
                MatHangList.Add(mh);
            }

            if (MatHangList.Count > 0)
            {
                SelectedMatHang = MatHangList[0];
            }
        }

        private void LayThongTinNhanVien()
        {
            try
            {
                if (Login_CurrentSession.IsLogin
                    && Login_CurrentSession.TaiKhoanDangNhap != null)
                {
                    int maTK = Login_CurrentSession.TaiKhoanDangNhap.Ma_TK;

                    var nv = _db.NHANVIENs.FirstOrDefault(n => n.Ma_TK == maTK);

                    if (nv != null)
                    {
                        _maNV = nv.MA_NV;
                        TenNhanVien = nv.HoTen_NV;
                    }
                    else
                    {
                        _maNV = null;
                        TenNhanVien = "Admin (" +
                                      Login_CurrentSession.TaiKhoanDangNhap.TenDangNhap_TK + ")";
                    }
                }
                else
                {
                    _maNV = null;
                    TenNhanVien = "Chưa đăng nhập";
                }
            }
            catch
            {
                _maNV = null;
                TenNhanVien = "Không xác định";
            }
        }

        private void TaoMaHoaDonMoi()
        {
            MaHD = _db.HOADONs.Any()
                ? _db.HOADONs.Max(h => h.MA_HD) + 1
                : 1;
        }

        // ================= TÍNH TIỀN =================

        private void TinhTongTien()
        {
            if (SelectedMatHang == null || SoLuong <= 0 || DonGiaSo <= 0)
            {
                TongTienSo = 0;
                TongTien = "0";
                return;
            }

            TongTienSo = DonGiaSo * SoLuong;
            TongTien = TongTienSo.ToString("N0");
        }

        private bool KiemTraDuLieu()
        {
            if (SelectedMatHang == null)
            {
                MessageBox.Show("Vui lòng chọn món ăn / mặt hàng.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (SoLuong <= 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng hợp lệ.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TenKhachHang))
            {
                MessageBox.Show("Vui lòng nhập họ tên khách hàng.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(SoDienThoaiKH))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại khách hàng.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(CMND))
            {
                MessageBox.Show("Vui lòng nhập CMND/CCCD khách hàng.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TongTienSo <= 0)
            {
                MessageBox.Show("Tổng tiền không hợp lệ.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private DateTime LayThoiGianLap()
        {
            return NgayLapHD.Date + DateTime.Now.TimeOfDay;
        }

        private KHACHHANG TimHoacTaoKhachHang()
        {
            KHACHHANG kh = null;

            if (!string.IsNullOrWhiteSpace(CMND))
            {
                kh = _db.KHACHHANGs.FirstOrDefault(k => k.CMND_KH == CMND);
            }

            if (kh == null && !string.IsNullOrWhiteSpace(SoDienThoaiKH))
            {
                kh = _db.KHACHHANGs.FirstOrDefault(k => k.SoDienThoai_KH == SoDienThoaiKH);
            }

            if (kh == null)
            {
                int maKHMoi = _db.KHACHHANGs.Any()
                    ? _db.KHACHHANGs.Max(k => k.MA_KH) + 1
                    : 1;

                kh = new KHACHHANG
                {
                    MA_KH = maKHMoi,
                    HoTen_KH = TenKhachHang,
                    SoDienThoai_KH = SoDienThoaiKH,
                    CMND_KH = CMND
                };

                _db.KHACHHANGs.Add(kh);
            }
            else
            {
                kh.HoTen_KH = TenKhachHang;
                kh.SoDienThoai_KH = SoDienThoaiKH;
                kh.CMND_KH = CMND;
            }

            return kh;
        }

        // ================= LƯU HÓA ĐƠN =================

        private void LuuHoaDon()
        {
            try
            {
                if (!KiemTraDuLieu())
                    return;

                TinhTongTien();

                var kh = TimHoacTaoKhachHang();

                HOADON hoaDon;

                if (_isNewInvoice)
                {
                    hoaDon = new HOADON
                    {
                        MA_HD = MaHD,
                        ThoiGianLap_HD = LayThoiGianLap(),
                        TinhTrang_HD = "Chưa thanh toán",
                        TriGia_HD = 0,
                        MA_NV = _maNV,
                        MA_KH = kh.MA_KH
                    };

                    _db.HOADONs.Add(hoaDon);
                    _isNewInvoice = false;
                }
                else
                {
                    hoaDon = _db.HOADONs.Find(MaHD);

                    if (hoaDon == null)
                    {
                        MessageBox.Show("Không tìm thấy hóa đơn hiện tại.",
                            "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    hoaDon.MA_KH = kh.MA_KH;

                    if (hoaDon.MA_NV == null)
                    {
                        hoaDon.MA_NV = _maNV;
                    }
                }

                int maCTMoi = _db.CHITIET_HDAU.Any()
                    ? _db.CHITIET_HDAU.Max(c => c.Ma_CTHDAU) + 1
                    : 1;

                var chiTiet = new CHITIET_HDAU
                {
                    Ma_CTHDAU = maCTMoi,
                    ThoiGianLap_CTHDAU = LayThoiGianLap(),
                    TriGia_CTHDAU = TongTienSo,
                    MA_HD = MaHD,
                    Ma_MH = SelectedMatHang.Ma_MH,

                    // Nếu model CHITIET_HDAU của bạn không có dòng này,
                    // hãy xóa dòng SoLuong_CTHDAU bên dưới.
                    SoLuong_MH = SoLuong
                };

                _db.CHITIET_HDAU.Add(chiTiet);

                hoaDon.TriGia_HD = (hoaDon.TriGia_HD ?? 0) + TongTienSo;

                _db.SaveChanges();

                MessageBox.Show(
                    "Lưu hóa đơn ăn uống thành công!\n\n" +
                    "Mã hóa đơn: " + MaHD + "\n" +
                    "Món: " + TenMatHang + "\n" +
                    "Số lượng: " + SoLuong + "\n" +
                    "Thành tiền: " + TongTienSo.ToString("N0") + " VNĐ",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                ThongBao = "Đã lưu hóa đơn ăn uống - Mã HD: " + MaHD;

                // Reset số lượng cho lần nhập tiếp
                SoLuongText = "1";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu hóa đơn ăn uống: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= HỦY =================

        private void HuyHoaDon()
        {
            var result = MessageBox.Show(
                "Bạn có muốn hủy thao tác tạo hóa đơn ăn uống?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            if (MatHangList.Count > 0)
            {
                SelectedMatHang = MatHangList[0];
            }
            else
            {
                SelectedMatHang = null;
            }

            SoLuongText = "1";
            GioLapHD = DateTime.Now.ToString("hh:mm tt");

            if (_isNewInvoice)
            {
                TenKhachHang = "";
                SoDienThoaiKH = "";
                CMND = "";
            }

            ThongBao = "Đã hủy thao tác";
        }

        // ================= PROPERTY CHANGED =================

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}