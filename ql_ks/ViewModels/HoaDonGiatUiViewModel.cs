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
    public class HoaDonGiatUiViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private int _maPhongDaChon;

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

        // === THÔNG TIN THANH TOÁN ===
        private int _maPhong;
        public int MaPhong
        {
            get => _maPhong;
            set { _maPhong = value; OnPropertyChanged(); }
        }

        private string _theoKilogram;
        public string TheoKilogram
        {
            get => _theoKilogram;
            set { _theoKilogram = value; OnPropertyChanged(); }
        }

        private string _donGiaText;
        public string DonGiaText
        {
            get => _donGiaText;
            set { _donGiaText = value; OnPropertyChanged(); }
        }

        private decimal _donGia;
        public decimal DonGia
        {
            get => _donGia;
            set { _donGia = value; OnPropertyChanged(); TinhTongTien(); }
        }

        private int _soKg;
        public int SoKg
        {
            get => _soKg;
            set { _soKg = value; OnPropertyChanged(); TinhTongTien(); }
        }

        private string _soKgText;
        public string SoKgText
        {
            get => _soKgText;
            set
            {
                _soKgText = value;
                OnPropertyChanged();
                int kg;
                if (int.TryParse(value, out kg) && kg >= 0)
                {
                    SoKg = kg;
                }
            }
        }

        private DateTime _ngayBatDau;
        public DateTime NgayBatDau
        {
            get => _ngayBatDau;
            set { _ngayBatDau = value; OnPropertyChanged(); }
        }

        private DateTime _ngayKetThuc;
        public DateTime NgayKetThuc
        {
            get => _ngayKetThuc;
            set { _ngayKetThuc = value; OnPropertyChanged(); }
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

        // === THÔNG TIN KHÁCH HÀNG ===
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

        // === LOẠI GIẶT ỦI ===
        private System.Collections.ObjectModel.ObservableCollection<LOAIGIATUI> _loaiGiatUiList;
        public System.Collections.ObjectModel.ObservableCollection<LOAIGIATUI> LoaiGiatUiList
        {
            get => _loaiGiatUiList;
            set { _loaiGiatUiList = value; OnPropertyChanged(); }
        }

        private LOAIGIATUI _selectedLoaiGU;
        public LOAIGIATUI SelectedLoaiGU
        {
            get => _selectedLoaiGU;
            set
            {
                _selectedLoaiGU = value;
                OnPropertyChanged();
                if (value != null)
                {
                    DonGia = value.DonGia_LoaiGU ?? 0;
                    DonGiaText = DonGia.ToString("N0") + " VNĐ/kg";
                    TheoKilogram = "Theo kilogram";
                }
            }
        }

        // === TRẠNG THÁI ===
        private string _thongBao;
        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        private bool _isNewInvoice;

        // === COMMANDS ===
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }

        // === CONSTRUCTOR ===
        public HoaDonGiatUiViewModel(int maPhong)
        {
            _maPhongDaChon = maPhong;

            LoaiGiatUiList = new System.Collections.ObjectModel.ObservableCollection<LOAIGIATUI>();

            LuuCommand = new Common_RelayCommand(_ => LuuHoaDon());
            HuyCommand = new Common_RelayCommand(_ => HuyHoaDon());

            NgayLapHD = DateTime.Now;
            GioLapHD = DateTime.Now.ToString("hh:mm tt");
            NgayBatDau = DateTime.Now;
            NgayKetThuc = DateTime.Now.AddDays(1);
            SoKgText = "0";

            LoadData();
        }

        // === LOAD DATA ===
        private void LoadData()
        {
            try
            {
                // 1. Thông tin phòng
                MaPhong = _maPhongDaChon;

                // 2. Nhân viên
                LayThongTinNhanVien();

                // 3. Danh sách loại giặt ủi
                var loaiList = _db.LOAIGIATUIs.OrderBy(l => l.Ma_LoaiGU).ToList();
                LoaiGiatUiList.Clear();
                foreach (var l in loaiList)
                {
                    LoaiGiatUiList.Add(l);
                }

                // Chọn mặc định loại đầu tiên
                if (LoaiGiatUiList.Count > 0)
                {
                    SelectedLoaiGU = LoaiGiatUiList[0];
                }

                // 4. Tìm hóa đơn chưa thanh toán của phòng
                var hoaDon = (from hd in _db.HOADONs
                              join ct in _db.CHITIET_HDLT on hd.MA_HD equals ct.MA_HD
                              where ct.Ma_Phong == _maPhongDaChon
                                    && hd.TinhTrang_HD == "Chưa thanh toán"
                              select hd).FirstOrDefault();

                if (hoaDon != null)
                {
                    MaHD = hoaDon.MA_HD;
                    _isNewInvoice = false;

                    // Load thông tin khách hàng
                    if (hoaDon.MA_KH != null)
                    {
                        var kh = _db.KHACHHANGs.Find(hoaDon.MA_KH);
                        if (kh != null)
                        {
                            TenKhachHang = kh.HoTen_KH ?? "";
                            SoDienThoaiKH = kh.SoDienThoai_KH ?? "";
                            CMND = kh.CMND_KH ?? "";
                        }
                    }

                    ThongBao = "Hóa đơn: " + MaHD;
                }
                else
                {
                    // Chưa có hóa đơn → tạo mới
                    _isNewInvoice = true;
                    if (_db.HOADONs.Any())
                        MaHD = _db.HOADONs.Max(h => h.MA_HD) + 1;
                    else
                        MaHD = 1;

                    ThongBao = "Tạo hóa đơn giặt ủi mới";
                }

                TinhTongTien();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === LẤY THÔNG TIN NHÂN VIÊN ===
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

        // === TÍNH TỔNG TIỀN ===
        private void TinhTongTien()
        {
            if (DonGia <= 0 || SoKg <= 0)
            {
                TongTienSo = 0;
                TongTien = "0";
                return;
            }

            TongTienSo = (long)(SoKg * DonGia);
            TongTien = TongTienSo.ToString("N0");
        }

        // === LƯU HÓA ĐƠN ===
        private void LuuHoaDon()
        {
            try
            {
                // Validate
                if (SelectedLoaiGU == null)
                {
                    MessageBox.Show("Vui lòng chọn loại giặt ủi!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SoKg <= 0)
                {
                    MessageBox.Show("Vui lòng nhập khối lượng giặt ủi!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TinhTongTien();

                // Nếu chưa có hóa đơn → tạo mới
                if (_isNewInvoice)
                {
                    // Tìm hoặc tạo khách hàng
                    KHACHHANG kh = null;
                    if (!string.IsNullOrWhiteSpace(CMND))
                    {
                        kh = _db.KHACHHANGs.FirstOrDefault(k => k.CMND_KH == CMND);
                    }

                    if (kh == null && !string.IsNullOrWhiteSpace(TenKhachHang))
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

                    var hoaDon = new HOADON
                    {
                        MA_HD = MaHD,
                        ThoiGianLap_HD = NgayLapHD,
                        TinhTrang_HD = "Chưa thanh toán",
                        TriGia_HD = TongTienSo,
                        MA_NV = _maNV,
                        MA_KH = kh != null ? kh.MA_KH : (int?)null
                    };
                    _db.HOADONs.Add(hoaDon);
                    _isNewInvoice = false;
                }

                // Tạo lượt giặt ủi mới
                int maLuotMoi = _db.LUOTGIATUIs.Any()
                    ? _db.LUOTGIATUIs.Max(l => l.Ma_LuotGU) + 1
                    : 1;

                var luotGU = new LUOTGIATUI
                {
                    Ma_LuotGU = maLuotMoi,
                    SoKilogram_LuotGU = SoKg,
                    NgayBatDau_LuotGU = NgayBatDau,
                    NgayKetThuc_LuotGU = NgayKetThuc,
                    Ma_LoaiGU = SelectedLoaiGU.Ma_LoaiGU
                };
                _db.LUOTGIATUIs.Add(luotGU);

                // Tạo chi tiết hóa đơn giặt ủi
                int maCTMoi = _db.CHITIET_HDGU.Any()
                    ? _db.CHITIET_HDGU.Max(c => c.Ma_CTHDGU) + 1
                    : 1;

                var chiTiet = new CHITIET_HDGU
                {
                    Ma_CTHDGU = maCTMoi,
                    ThoiGianLap_CTHDGU = NgayLapHD,
                    TriGia_CTHDGU = TongTienSo,
                    MA_HD = MaHD,
                    Ma_LuotGU = maLuotMoi
                };
                _db.CHITIET_HDGU.Add(chiTiet);

                // Cập nhật tổng tiền hóa đơn
                var hd = _db.HOADONs.Find(MaHD);
                if (hd != null)
                {
                    long tongHienTai = hd.TriGia_HD ?? 0;
                    hd.TriGia_HD = tongHienTai + TongTienSo;
                }

                _db.SaveChanges();

                MessageBox.Show(
                    "Lưu hóa đơn giặt ủi thành công!\n\n" +
                    "Mã hóa đơn: " + MaHD + "\n" +
                    "Loại: " + SelectedLoaiGU.Ten_LoaiGU + "\n" +
                    "Khối lượng: " + SoKg + " kg\n" +
                    "Thành tiền: " + TongTienSo.ToString("N0") + " VNĐ",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                ThongBao = "Đã lưu - Mã HD: " + MaHD;

                // Reset form cho lần nhập tiếp
                SoKgText = "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === HỦY HÓA ĐƠN ===
        private void HuyHoaDon()
        {
            var result = MessageBox.Show(
                "Hủy hóa đơn giặt ủi đang tạo?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SoKgText = "0";
                ThongBao = "Đã hủy";

                // Đóng window
                foreach (Window w in Application.Current.Windows)
                {
                    if (w.Title != null && w.Title.Contains("Hóa đơn"))
                    {
                        // Không đóng, chỉ reset
                        break;
                    }
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