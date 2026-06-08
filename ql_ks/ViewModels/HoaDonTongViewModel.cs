using ql_ks.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ql_ks.ViewModels
{
    public class HoaDonTongChiTietItem
    {
        public string LoaiDichVu { get; set; }
        public string NoiDung { get; set; }
        public string SoLuong { get; set; }
        public string DonGia { get; set; }
        public string ThanhTien { get; set; }
    }

    public class HoaDonTongViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private readonly int _maPhongDaChon;

        // ================= THÔNG TIN HÓA ĐƠN =================

        private int _maHD;
        public int MaHD
        {
            get => _maHD;
            set { _maHD = value; OnPropertyChanged(); }
        }

        private string _ngayLapHD;
        public string NgayLapHD
        {
            get => _ngayLapHD;
            set { _ngayLapHD = value; OnPropertyChanged(); }
        }

        private string _tinhTrangHD;
        public string TinhTrangHD
        {
            get => _tinhTrangHD;
            set { _tinhTrangHD = value; OnPropertyChanged(); }
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

        // ================= THÔNG TIN PHÒNG =================

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

        private string _donGiaPhong;
        public string DonGiaPhong
        {
            get => _donGiaPhong;
            set { _donGiaPhong = value; OnPropertyChanged(); }
        }

        private string _tinhTrangPhong;
        public string TinhTrangPhong
        {
            get => _tinhTrangPhong;
            set { _tinhTrangPhong = value; OnPropertyChanged(); }
        }

        // ================= TỔNG TIỀN =================

        private long _tongLuuTruSo;
        public long TongLuuTruSo
        {
            get => _tongLuuTruSo;
            set
            {
                _tongLuuTruSo = value;
                TongLuuTru = value.ToString("N0");
                OnPropertyChanged();
            }
        }

        private string _tongLuuTru;
        public string TongLuuTru
        {
            get => _tongLuuTru;
            set { _tongLuuTru = value; OnPropertyChanged(); }
        }

        private long _tongAnUongSo;
        public long TongAnUongSo
        {
            get => _tongAnUongSo;
            set
            {
                _tongAnUongSo = value;
                TongAnUong = value.ToString("N0");
                OnPropertyChanged();
            }
        }

        private string _tongAnUong;
        public string TongAnUong
        {
            get => _tongAnUong;
            set { _tongAnUong = value; OnPropertyChanged(); }
        }

        private long _tongGiatUiSo;
        public long TongGiatUiSo
        {
            get => _tongGiatUiSo;
            set
            {
                _tongGiatUiSo = value;
                TongGiatUi = value.ToString("N0");
                OnPropertyChanged();
            }
        }

        private string _tongGiatUi;
        public string TongGiatUi
        {
            get => _tongGiatUi;
            set { _tongGiatUi = value; OnPropertyChanged(); }
        }

        private long _tongDiChuyenSo;
        public long TongDiChuyenSo
        {
            get => _tongDiChuyenSo;
            set
            {
                _tongDiChuyenSo = value;
                TongDiChuyen = value.ToString("N0");
                OnPropertyChanged();
            }
        }

        private string _tongDiChuyen;
        public string TongDiChuyen
        {
            get => _tongDiChuyen;
            set { _tongDiChuyen = value; OnPropertyChanged(); }
        }

        private long _tongCongSo;
        public long TongCongSo
        {
            get => _tongCongSo;
            set
            {
                _tongCongSo = value;
                TongCong = value.ToString("N0");
                OnPropertyChanged();
            }
        }

        private string _tongCong;
        public string TongCong
        {
            get => _tongCong;
            set { _tongCong = value; OnPropertyChanged(); }
        }

        // ================= DANH SÁCH CHI TIẾT =================

        private ObservableCollection<HoaDonTongChiTietItem> _chiTietList;
        public ObservableCollection<HoaDonTongChiTietItem> ChiTietList
        {
            get => _chiTietList;
            set { _chiTietList = value; OnPropertyChanged(); }
        }

        // ================= TRẠNG THÁI =================

        private string _thongBao;
        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        private bool _coHoaDon;
        public bool CoHoaDon
        {
            get => _coHoaDon;
            set { _coHoaDon = value; OnPropertyChanged(); }
        }

        // ================= COMMAND =================

        public ICommand LamMoiCommand { get; }
        public ICommand ThanhToanCommand { get; }

        public HoaDonTongViewModel(int maPhong)
        {
            _maPhongDaChon = maPhong;

            ChiTietList = new ObservableCollection<HoaDonTongChiTietItem>();

            LamMoiCommand = new Common_RelayCommand(_ => LoadData());
            ThanhToanCommand = new Common_RelayCommand(_ => ThanhToan());

            LoadData();
        }

        // ================= LOAD DATA =================

        private void LoadData()
        {
            try
            {
                ResetData();

                LoadThongTinPhong();

                var hoaDon = TimHoaDonChuaThanhToanTheoPhong();

                if (hoaDon == null)
                {
                    CoHoaDon = false;
                    ThongBao = "Phòng " + _maPhongDaChon + " chưa có hóa đơn chưa thanh toán.";
                    return;
                }

                CoHoaDon = true;

                LoadThongTinHoaDon(hoaDon);
                LoadChiTietLuuTru(hoaDon.MA_HD);
                LoadChiTietAnUong(hoaDon.MA_HD);
                LoadChiTietGiatUi(hoaDon.MA_HD);
                LoadChiTietDiChuyen(hoaDon.MA_HD);

                TongCongSo = TongLuuTruSo + TongAnUongSo + TongGiatUiSo + TongDiChuyenSo;

                // Đồng bộ lại trị giá hóa đơn theo tổng chi tiết
                hoaDon.TriGia_HD = TongCongSo;
                _db.SaveChanges();

                ThongBao = "Đã tải hóa đơn tổng - Mã HD: " + MaHD;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải hóa đơn tổng: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetData()
        {
            MaHD = 0;
            NgayLapHD = "";
            TinhTrangHD = "";
            TenNhanVien = "";

            TenKhachHang = "";
            SoDienThoaiKH = "";
            CMND = "";

            TongLuuTruSo = 0;
            TongAnUongSo = 0;
            TongGiatUiSo = 0;
            TongDiChuyenSo = 0;
            TongCongSo = 0;

            ChiTietList.Clear();

            CoHoaDon = false;
            ThongBao = "";
        }

        private void LoadThongTinPhong()
        {
            var phong = _db.PHONGs
                .Include(p => p.LOAIPHONG)
                .FirstOrDefault(p => p.Ma_Phong == _maPhongDaChon);

            if (phong != null)
            {
                MaPhong = phong.Ma_Phong;
                TinhTrangPhong = phong.TinhTrang_Phong ?? "";

                if (phong.LOAIPHONG != null)
                {
                    LoaiPhong = phong.LOAIPHONG.Ten_TP ?? "";
                    DonGiaPhong = (phong.LOAIPHONG.DonGia_LP ?? 0).ToString("N0");
                }
                else
                {
                    LoaiPhong = "Chưa phân loại";
                    DonGiaPhong = "0";
                }
            }
            else
            {
                MaPhong = _maPhongDaChon;
                LoaiPhong = "Không tìm thấy phòng";
                DonGiaPhong = "0";
                TinhTrangPhong = "";
            }
        }

        private HOADON TimHoaDonChuaThanhToanTheoPhong()
        {
            return (from hd in _db.HOADONs
                    join ctlt in _db.CHITIET_HDLT
                        on hd.MA_HD equals ctlt.MA_HD
                    where ctlt.Ma_Phong == _maPhongDaChon
                          && hd.TinhTrang_HD == "Chưa thanh toán"
                    orderby hd.MA_HD descending
                    select hd).FirstOrDefault();
        }

        private void LoadThongTinHoaDon(HOADON hoaDon)
        {
            MaHD = hoaDon.MA_HD;
            NgayLapHD = hoaDon.ThoiGianLap_HD.HasValue
                ? hoaDon.ThoiGianLap_HD.Value.ToString("dd/MM/yyyy HH:mm")
                : "";

            TinhTrangHD = hoaDon.TinhTrang_HD ?? "";

            if (hoaDon.MA_NV != null)
            {
                var nv = _db.NHANVIENs.Find(hoaDon.MA_NV);
                TenNhanVien = nv != null ? nv.HoTen_NV : "";
            }

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
        }

        // ================= LOAD CHI TIẾT LƯU TRÚ =================

        private void LoadChiTietLuuTru(int maHD)
        {
            var ds = _db.CHITIET_HDLT
                .Include(c => c.PHONG)
                .Include(c => c.PHONG.LOAIPHONG)
                .Where(c => c.MA_HD == maHD)
                .ToList();

            foreach (var ct in ds)
            {
                long thanhTien = ct.TriGia_CTHDLT ?? 0;
                TongLuuTruSo += thanhTien;

                DateTime ngayNhan = ct.ThoiGianNhan_PHONG ?? DateTime.Now;
                DateTime ngayTra = ct.ThoiGianTra_PHONG ?? DateTime.Now;

                int soNgay = (ngayTra.Date - ngayNhan.Date).Days;
                if (soNgay < 1) soNgay = 1;

                string tenLoaiPhong = "";
                long donGia = 0;

                if (ct.PHONG != null && ct.PHONG.LOAIPHONG != null)
                {
                    tenLoaiPhong = ct.PHONG.LOAIPHONG.Ten_TP ?? "";
                    donGia = ct.PHONG.LOAIPHONG.DonGia_LP ?? 0;
                }

                ChiTietList.Add(new HoaDonTongChiTietItem
                {
                    LoaiDichVu = "Lưu trú",
                    NoiDung = "Phòng " + ct.Ma_Phong + " - " + tenLoaiPhong,
                    SoLuong = soNgay + " ngày",
                    DonGia = donGia.ToString("N0") + " VNĐ/ngày",
                    ThanhTien = thanhTien.ToString("N0") + " VNĐ"
                });
            }
        }

        // ================= LOAD CHI TIẾT ĂN UỐNG =================

        private void LoadChiTietAnUong(int maHD)
        {
            var ds = _db.CHITIET_HDAU
                .Include(c => c.MATHANG)
                .Where(c => c.MA_HD == maHD)
                .ToList();

            foreach (var ct in ds)
            {
                long thanhTien = ct.TriGia_CTHDAU ?? 0;
                TongAnUongSo += thanhTien;

                string tenMatHang = "";
                long donGia = 0;

                if (ct.MATHANG != null)
                {
                    tenMatHang = ct.MATHANG.Ten_MH ?? "";
                    donGia = ct.MATHANG.DonGia_MH ?? 0;
                }

                int soLuong = ct.SoLuong_MH ?? 0;

                ChiTietList.Add(new HoaDonTongChiTietItem
                {
                    LoaiDichVu = "Ăn uống",
                    NoiDung = tenMatHang,
                    SoLuong = soLuong.ToString(),
                    DonGia = donGia.ToString("N0") + " VNĐ",
                    ThanhTien = thanhTien.ToString("N0") + " VNĐ"
                });
            }
        }

        // ================= LOAD CHI TIẾT GIẶT ỦI =================

        private void LoadChiTietGiatUi(int maHD)
        {
            var ds = _db.CHITIET_HDGU
                .Include(c => c.LUOTGIATUI)
                .Include(c => c.LUOTGIATUI.LOAIGIATUI)
                .Where(c => c.MA_HD == maHD)
                .ToList();

            foreach (var ct in ds)
            {
                long thanhTien = ct.TriGia_CTHDGU ?? 0;
                TongGiatUiSo += thanhTien;

                string tenLoaiGU = "";
                int soKg = 0;
                decimal donGiaDecimal = 0;

                if (ct.LUOTGIATUI != null)
                {
                    soKg = ct.LUOTGIATUI.SoKilogram_LuotGU ?? 0;

                    if (ct.LUOTGIATUI.LOAIGIATUI != null)
                    {
                        tenLoaiGU = ct.LUOTGIATUI.LOAIGIATUI.Ten_LoaiGU ?? "";
                        donGiaDecimal = ct.LUOTGIATUI.LOAIGIATUI.DonGia_LoaiGU ?? 0;
                    }
                }

                ChiTietList.Add(new HoaDonTongChiTietItem
                {
                    LoaiDichVu = "Giặt ủi",
                    NoiDung = tenLoaiGU,
                    SoLuong = soKg + " kg",
                    DonGia = donGiaDecimal.ToString("N0") + " VNĐ/kg",
                    ThanhTien = thanhTien.ToString("N0") + " VNĐ"
                });
            }
        }

        // ================= LOAD CHI TIẾT DI CHUYỂN =================

        private void LoadChiTietDiChuyen(int maHD)
        {
            var ds = _db.CHITIET_HDDC
                .Include(c => c.CHUYENDI)
                .Where(c => c.MA_HD == maHD)
                .ToList();

            foreach (var ct in ds)
            {
                long thanhTien = ct.TriGia_CTHDDC ?? 0;
                TongDiChuyenSo += thanhTien;

                string diemDen = "";
                long donGia = 0;

                if (ct.CHUYENDI != null)
                {
                    diemDen = ct.CHUYENDI.DiemDen_CD ?? "";
                    donGia = ct.CHUYENDI.DonGia_CD ?? 0;
                }

                ChiTietList.Add(new HoaDonTongChiTietItem
                {
                    LoaiDichVu = "Di chuyển",
                    NoiDung = diemDen,
                    SoLuong = "1 chuyến",
                    DonGia = donGia.ToString("N0") + " VNĐ",
                    ThanhTien = thanhTien.ToString("N0") + " VNĐ"
                });
            }
        }

        // ================= THANH TOÁN =================

        private void ThanhToan()
        {
            try
            {
                if (!CoHoaDon || MaHD <= 0)
                {
                    MessageBox.Show("Không có hóa đơn để thanh toán.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    "Xác nhận thanh toán hóa đơn?\n\n" +
                    "Mã hóa đơn: " + MaHD + "\n" +
                    "Phòng: " + MaPhong + "\n" +
                    "Khách hàng: " + TenKhachHang + "\n\n" +
                    "Tổng tiền: " + TongCongSo.ToString("N0") + " VNĐ",
                    "Xác nhận thanh toán",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                var hd = _db.HOADONs.Find(MaHD);
                if (hd == null)
                {
                    MessageBox.Show("Không tìm thấy hóa đơn.",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                hd.TinhTrang_HD = "Đã thanh toán";
                hd.TriGia_HD = TongCongSo;

                // ✅ Sau thanh toán → phòng chuyển sang "Đang dọn dẹp"
                var phong = _db.PHONGs.Find(_maPhongDaChon);
                if (phong != null)
                {
                    phong.TinhTrang_Phong = "Đang dọn dẹp";
                }

                _db.SaveChanges();

                MessageBox.Show(
                    "Thanh toán thành công!\n\n" +
                    "Mã hóa đơn: " + MaHD + "\n" +
                    "Tổng tiền: " + TongCongSo.ToString("N0") + " VNĐ\n\n" +
                    "Phòng " + _maPhongDaChon + " đang chờ dọn dẹp.",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Đóng cửa sổ hóa đơn
                foreach (Window w in Application.Current.Windows)
                {
                    if (w.Title != null && w.Title.Contains("Hóa đơn"))
                    {
                        w.Close();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thanh toán hóa đơn tổng: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= PROPERTY CHANGED =================

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}