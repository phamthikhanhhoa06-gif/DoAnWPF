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
    public class HoaDonDiChuyenViewModel : INotifyPropertyChanged
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
            set
            {
                _maHD = value;
                OnPropertyChanged();
            }
        }

        private DateTime _ngayLapHD;
        public DateTime NgayLapHD
        {
            get => _ngayLapHD;
            set
            {
                _ngayLapHD = value;
                OnPropertyChanged();
            }
        }

        private string _gioLapHD;
        public string GioLapHD
        {
            get => _gioLapHD;
            set
            {
                _gioLapHD = value;
                OnPropertyChanged();
            }
        }

        private string _tenNhanVien;
        public string TenNhanVien
        {
            get => _tenNhanVien;
            set
            {
                _tenNhanVien = value;
                OnPropertyChanged();
            }
        }

        // ================= THÔNG TIN KHÁCH HÀNG =================

        private string _tenKhachHang;
        public string TenKhachHang
        {
            get => _tenKhachHang;
            set
            {
                _tenKhachHang = value;
                OnPropertyChanged();
            }
        }

        private string _soDienThoaiKH;
        public string SoDienThoaiKH
        {
            get => _soDienThoaiKH;
            set
            {
                _soDienThoaiKH = value;
                OnPropertyChanged();
            }
        }

        private string _cmnd;
        public string CMND
        {
            get => _cmnd;
            set
            {
                _cmnd = value;
                OnPropertyChanged();
            }
        }

        // ================= THÔNG TIN CHUYẾN ĐI =================

        private ObservableCollection<CHUYENDI> _chuyenDiList;
        public ObservableCollection<CHUYENDI> ChuyenDiList
        {
            get => _chuyenDiList;
            set
            {
                _chuyenDiList = value;
                OnPropertyChanged();
            }
        }

        private CHUYENDI _selectedChuyenDi;
        public CHUYENDI SelectedChuyenDi
        {
            get => _selectedChuyenDi;
            set
            {
                _selectedChuyenDi = value;
                OnPropertyChanged();

                if (_selectedChuyenDi != null)
                {
                    MaChuyenDiText = _selectedChuyenDi.Ma_CD.ToString();
                    DiemDen = _selectedChuyenDi.DiemDen_CD ?? "";
                    DonGiaSo = _selectedChuyenDi.DonGia_CD ?? 0;
                    DonGiaText = DonGiaSo.ToString("N0");
                    TinhTongTien();
                }
                else
                {
                    MaChuyenDiText = "";
                    DiemDen = "";
                    DonGiaSo = 0;
                    DonGiaText = "0";
                    TinhTongTien();
                }
            }
        }

        private string _maChuyenDiText;
        public string MaChuyenDiText
        {
            get => _maChuyenDiText;
            set
            {
                _maChuyenDiText = value;
                OnPropertyChanged();
            }
        }

        private string _diemDen;
        public string DiemDen
        {
            get => _diemDen;
            set
            {
                _diemDen = value;
                OnPropertyChanged();
            }
        }

        private long _donGiaSo;
        public long DonGiaSo
        {
            get => _donGiaSo;
            set
            {
                _donGiaSo = value;
                OnPropertyChanged();
            }
        }

        private string _donGiaText;
        public string DonGiaText
        {
            get => _donGiaText;
            set
            {
                _donGiaText = value;
                OnPropertyChanged();
            }
        }

        private long _tongTienSo;
        public long TongTienSo
        {
            get => _tongTienSo;
            set
            {
                _tongTienSo = value;
                OnPropertyChanged();
            }
        }

        private string _tongTien;
        public string TongTien
        {
            get => _tongTien;
            set
            {
                _tongTien = value;
                OnPropertyChanged();
            }
        }

        // ================= TRẠNG THÁI =================

        private string _thongBao;
        public string ThongBao
        {
            get => _thongBao;
            set
            {
                _thongBao = value;
                OnPropertyChanged();
            }
        }

        // ================= COMMAND =================

        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }

        // ================= CONSTRUCTOR =================

        public HoaDonDiChuyenViewModel(int maPhong)
        {
            _maPhongDaChon = maPhong;

            LuuCommand = new Common_RelayCommand(_ => LuuHoaDon());
            HuyCommand = new Common_RelayCommand(_ => HuyHoaDon());

            NgayLapHD = DateTime.Now;
            GioLapHD = DateTime.Now.ToString("hh:mm tt");

            ChuyenDiList = new ObservableCollection<CHUYENDI>();

            LoadData();
        }

        // ================= LOAD DATA =================

        private void LoadData()
        {
            try
            {
                LayThongTinNhanVien();
                LoadDanhSachChuyenDi();

                // Tìm hóa đơn lưu trú chưa thanh toán của phòng hiện tại.
                // Nếu có, hóa đơn di chuyển sẽ được cộng vào hóa đơn đó.
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
                    ThongBao = "Tạo hóa đơn di chuyển mới";
                }

                TinhTongTien();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu hóa đơn di chuyển: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDanhSachChuyenDi()
        {
            ChuyenDiList.Clear();

            var ds = _db.CHUYENDIs
                .OrderBy(cd => cd.Ma_CD)
                .ToList();

            foreach (var cd in ds)
            {
                ChuyenDiList.Add(cd);
            }

            if (ChuyenDiList.Count > 0)
            {
                SelectedChuyenDi = ChuyenDiList[0];
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

        // ================= XỬ LÝ =================

        private void TinhTongTien()
        {
            if (SelectedChuyenDi == null)
            {
                TongTienSo = 0;
                TongTien = "0";
                return;
            }

            TongTienSo = SelectedChuyenDi.DonGia_CD ?? 0;
            TongTien = TongTienSo.ToString("N0");
        }

        private bool KiemTraDuLieu()
        {
            if (SelectedChuyenDi == null)
            {
                MessageBox.Show("Vui lòng chọn chuyến đi.",
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
                MessageBox.Show("Đơn giá chuyến đi không hợp lệ.",
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

                int maCTMoi = _db.CHITIET_HDDC.Any()
                    ? _db.CHITIET_HDDC.Max(c => c.Ma_CTHDDC) + 1
                    : 1;

                var chiTiet = new CHITIET_HDDC
                {
                    Ma_CTHDDC = maCTMoi,
                    ThoiGianLap_CTHDDC = LayThoiGianLap(),
                    TriGia_CTHDDC = TongTienSo,
                    MA_HD = MaHD,
                    Ma_CD = SelectedChuyenDi.Ma_CD
                };

                _db.CHITIET_HDDC.Add(chiTiet);

                hoaDon.TriGia_HD = (hoaDon.TriGia_HD ?? 0) + TongTienSo;

                _db.SaveChanges();

                MessageBox.Show(
                    "Lưu hóa đơn di chuyển thành công!\n\n" +
                    "Mã hóa đơn: " + MaHD + "\n" +
                    "Mã chuyến đi: " + SelectedChuyenDi.Ma_CD + "\n" +
                    "Điểm đến: " + SelectedChuyenDi.DiemDen_CD + "\n" +
                    "Thành tiền: " + TongTienSo.ToString("N0") + " VNĐ",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                ThongBao = "Đã lưu hóa đơn di chuyển - Mã HD: " + MaHD;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu hóa đơn di chuyển: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= HỦY =================

        private void HuyHoaDon()
        {
            var result = MessageBox.Show(
                "Bạn có muốn hủy thao tác tạo hóa đơn di chuyển?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            if (ChuyenDiList.Count > 0)
            {
                SelectedChuyenDi = ChuyenDiList[0];
            }
            else
            {
                SelectedChuyenDi = null;
            }

            if (_isNewInvoice)
            {
                TenKhachHang = "";
                SoDienThoaiKH = "";
                CMND = "";
            }

            GioLapHD = DateTime.Now.ToString("hh:mm tt");
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