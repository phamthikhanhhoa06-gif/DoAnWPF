using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ql_ks.Models;
using ql_ks.ViewModels;

namespace ql_ks.ViewModels
{
    public class DichVuAnUongViewModel : AnUong_BaseViewModel
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private List<AnUong_HelperViewModel> _allProducts = new List<AnUong_HelperViewModel>();

        private ObservableCollection<AnUong_HelperViewModel> _danhSachMonAn;
        public ObservableCollection<AnUong_HelperViewModel> DanhSachMonAn
        {
            get => _danhSachMonAn;
            set => SetProperty(ref _danhSachMonAn, value);
        }

        private ObservableCollection<DanhSachDaGo> _danhSachDaChon;
        public ObservableCollection<DanhSachDaGo> DanhSachDaChon
        {
            get => _danhSachDaChon;
            set => SetProperty(ref _danhSachDaChon, value);
        }

        private ObservableCollection<PhongViewModel> _danhSachPhong;
        public ObservableCollection<PhongViewModel> DanhSachPhong
        {
            get => _danhSachPhong;
            set => SetProperty(ref _danhSachPhong, value);
        }

        private PhongViewModel _phongDangChon;
        public PhongViewModel PhongDangChon
        {
            get => _phongDangChon;
            set => SetProperty(ref _phongDangChon, value);
        }

        private string _thongTinPhong = "(Chưa chọn phòng)";
        public string ThongTinPhong
        {
            get => _thongTinPhong;
            set => SetProperty(ref _thongTinPhong, value);
        }

        private int _maPhong = 0;
        public int MaPhong
        {
            get => _maPhong;
            set
            {
                if (SetProperty(ref _maPhong, value))
                {
                    ValidateMaPhong();
                }
            }
        }

        // ==== LỖI MÃ PHÒNG ====
        private string _loiMaPhong;
        public string LoiMaPhong
        {
            get => _loiMaPhong;
            set => SetProperty(ref _loiMaPhong, value);
        }

        private bool _coLoiMaPhong;
        public bool CoLoiMaPhong
        {
            get => _coLoiMaPhong;
            set => SetProperty(ref _coLoiMaPhong, value);
        }

        // ==== TÌM KIẾM ====
        private string _tuKhoa = "";
        public string TuKhoaTimKiem
        {
            get => _tuKhoa;
            set
            {
                if (SetProperty(ref _tuKhoa, value))
                {
                    LocDanhSach(value);
                }
            }
        }

        // ==== LỖI TÌM KIẾM ====
        private string _loiTimKiem;
        public string LoiTimKiem
        {
            get => _loiTimKiem;
            set => SetProperty(ref _loiTimKiem, value);
        }

        private bool _coLoiTimKiem;
        public bool CoLoiTimKiem
        {
            get => _coLoiTimKiem;
            set => SetProperty(ref _coLoiTimKiem, value);
        }

        private decimal _tongTien = 0;
        public decimal TongTien
        {
            get => _tongTien;
            set => SetProperty(ref _tongTien, value);
        }

        private string _thongBao = "";
        public string ThongBao
        {
            get => _thongBao;
            set => SetProperty(ref _thongBao, value);
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ChonPhongCommand { get; }

        public DichVuAnUongViewModel()
        {
            DanhSachMonAn = new ObservableCollection<AnUong_HelperViewModel>();
            DanhSachDaChon = new ObservableCollection<DanhSachDaGo>();
            DanhSachPhong = new ObservableCollection<PhongViewModel>();

            AddCommand = new AnUong_RelayCommand_T<object>(ThemVaoGioHang);
            RemoveCommand = new AnUong_RelayCommand_T<object>(XoaMonTrenGio);
            SaveCommand = new AnUong_RelayCommand(_ => LuuHoaDon());
            ChonPhongCommand = new AnUong_RelayCommand_T<object>(ChonPhong);

            LoadData();
            LoadDanhSachPhong();
            ValidateMaPhong();
        }

        private void LoadDanhSachPhong()
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

                DanhSachPhong.Clear();

                foreach (var r in rooms)
                {
                    string tinhTrang = (r.TinhTrang_Phong ?? "").Trim();
                    if (tinhTrang != "Có khách") continue;

                    var item = new PhongViewModel
                    {
                        Ma_Phong = r.Ma_Phong,
                        Ten_TP = (r.Ten_TP ?? "Chưa phân loại").Trim(),
                        TinhTrang = tinhTrang,
                        DonGia = r.DonGia_LP,
                        IsSelected = false
                    };

                    SetMauPhong(item);
                    DanhSachPhong.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách phòng: " + ex.Message);
            }
        }

        private void SetMauPhong(PhongViewModel item)
        {
            if (item.IsSelected)
            {
                item.ColorBackground = new SolidColorBrush(Color.FromRgb(155, 89, 182));
                item.ColorText = Brushes.White;
            }
            else
            {
                item.ColorBackground = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                item.ColorText = Brushes.White;
            }
        }

        private void ChonPhong(object parameter)
        {
            var phong = parameter as PhongViewModel;
            if (phong == null) return;

            foreach (var p in DanhSachPhong)
            {
                p.IsSelected = (p == phong);
                SetMauPhong(p);
                p.NotifyAllChanged();
            }

            PhongDangChon = phong;
            MaPhong = phong.Ma_Phong;
            ThongTinPhong = $"→ Phòng {phong.Ma_Phong}";
        }

        private void LoadData()
        {
            try
            {
                var list = _db.MATHANGs
                    .Select(m => new AnUong_HelperViewModel
                    {
                        Ma_MH = m.Ma_MH,
                        Ten_MH = m.Ten_MH ?? "Không rõ",
                        GiaTien = m.DonGia_MH ?? 0
                    })
                    .ToList();

                _allProducts = list;
                DanhSachMonAn = new ObservableCollection<AnUong_HelperViewModel>(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu món ăn: " + ex.Message);
            }
        }

        // ==== VALIDATE MÃ PHÒNG ====
        private void ValidateMaPhong()
        {
            if (MaPhong <= 0)
            {
                LoiMaPhong = "⚠ Vui lòng chọn phòng (bấm chọn Card phòng phía trên)!";
                CoLoiMaPhong = true;
                DanhSachDaChon?.Clear();
                CapNhatTongTien();
                return;
            }

            try
            {
                bool tonTai = _db.PHONGs.Any(p => p.Ma_Phong == MaPhong && p.TinhTrang_Phong == "Có khách");
                if (!tonTai)
                {
                    LoiMaPhong = "⚠ Phòng đang chọn không có khách hoặc không tồn tại!";
                    CoLoiMaPhong = true;
                    DanhSachDaChon?.Clear();
                    CapNhatTongTien();
                    return;
                }
            }
            catch (Exception ex)
            {
                LoiMaPhong = "⚠ Lỗi kiểm tra trạng thái phòng: " + ex.Message;
                CoLoiMaPhong = true;
                return;
            }

            LoiMaPhong = "";
            CoLoiMaPhong = false;
        }

        // ==== LỌC DANH SÁCH MÓN ĂN ====
        private void LocDanhSach(string key)
        {
            if (_allProducts == null) return;

            if (string.IsNullOrWhiteSpace(key))
            {
                DanhSachMonAn = new ObservableCollection<AnUong_HelperViewModel>(_allProducts);
                LoiTimKiem = "";
                CoLoiTimKiem = false;
            }
            else
            {
                var lowerKey = key.ToLower();
                var res = _allProducts
                    .Where(x => (x.Ten_MH ?? "").ToLower().Contains(lowerKey))
                    .ToList();
                DanhSachMonAn = new ObservableCollection<AnUong_HelperViewModel>(res);

                if (res.Count == 0)
                {
                    LoiTimKiem = "⚠ Không tìm thấy mặt hàng \"" + key + "\"!";
                    CoLoiTimKiem = true;
                }
                else
                {
                    LoiTimKiem = "";
                    CoLoiTimKiem = false;
                }
            }
        }

        private void ThemVaoGioHang(object parameter)
        {
            ValidateMaPhong();
            if (CoLoiMaPhong)
            {
                MessageBox.Show(LoiMaPhong, "Lỗi gọi món", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var mon = parameter as AnUong_HelperViewModel;
            if (mon == null) return;

            var exists = DanhSachDaChon.FirstOrDefault(x => x.Ma_CTHDAU == mon.Ma_MH);

            if (exists != null)
            {
                exists.SoLuong++;
            }
            else
            {
                DanhSachDaChon.Add(new DanhSachDaGo
                {
                    Ma_CTHDAU = mon.Ma_MH,
                    Ten_MH = mon.Ten_MH,
                    GiaTien = mon.GiaTien,
                    SoLuong = 1
                });
            }

            CapNhatTongTien();
        }

        private void XoaMonTrenGio(object parameter)
        {
            var item = parameter as DanhSachDaGo;
            if (item == null) return;

            DanhSachDaChon.Remove(item);
            CapNhatTongTien();
        }

        private void CapNhatTongTien()
        {
            TongTien = DanhSachDaChon?.Sum(x => x.ThanhTien) ?? 0;
        }

        // ✅ LƯU HÓA ĐƠN ĂN UỐNG VÀO DATABASE
        private void LuuHoaDon()
        {
            ValidateMaPhong();
            if (CoLoiMaPhong)
            {
                MessageBox.Show(LoiMaPhong, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DanhSachDaChon.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. Tìm hóa đơn chưa thanh toán của phòng
                var hoaDon = (from hd in _db.HOADONs
                              join ct in _db.CHITIET_HDLT on hd.MA_HD equals ct.MA_HD
                              where ct.Ma_Phong == MaPhong && hd.TinhTrang_HD == "Chưa thanh toán"
                              select hd).FirstOrDefault();

                if (hoaDon == null)
                {
                    MessageBox.Show(
                        "Phòng " + MaPhong + " chưa có hóa đơn lưu trú (chưa thanh toán).\n" +
                        "Vui lòng thuê phòng trước từ Trang chủ.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int maHD = hoaDon.MA_HD;

                // 2. Lưu từng món trong giỏ hàng vào CHITIET_HDAU
                int maCTMoi = _db.CHITIET_HDAU.Any() ? _db.CHITIET_HDAU.Max(c => c.Ma_CTHDAU) + 1 : 1;

                foreach (var item in DanhSachDaChon)
                {
                    long thanhTien = (long)(item.GiaTien * item.SoLuong);

                    var chiTiet = new CHITIET_HDAU
                    {
                        Ma_CTHDAU = maCTMoi,
                        SoLuong_MH = item.SoLuong,
                        ThoiGianLap_CTHDAU = DateTime.Now,
                        TriGia_CTHDAU = thanhTien,
                        MA_HD = maHD,
                        Ma_MH = item.Ma_CTHDAU
                    };

                    _db.CHITIET_HDAU.Add(chiTiet);
                    maCTMoi++;
                }

                // 3. Cập nhật tổng tiền hóa đơn tổng
                long tongAnUong = DanhSachDaChon.Sum(x => (long)(x.GiaTien * x.SoLuong));
                long tongHienTai = hoaDon.TriGia_HD ?? 0;
                hoaDon.TriGia_HD = tongHienTai + tongAnUong;

                _db.SaveChanges();

                MessageBox.Show(
                    $"Đã lưu dịch vụ ăn uống vào hóa đơn!\n\n" +
                    $"Mã hóa đơn: {maHD}\n" +
                    $"Phòng: {MaPhong}\n" +
                    $"Số món: {DanhSachDaChon.Count}\n" +
                    $"Tổng tiền ăn uống: {tongAnUong:N0} ₫\n\n" +
                    $"Vui lòng mở Hóa đơn từ Trang chủ để xem chi tiết.",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ThongBao = "Đã lưu thành công - HD: " + maHD;

                // Reset giỏ hàng
                DanhSachDaChon.Clear();
                CapNhatTongTien();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu hóa đơn ăn uống: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}