using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
        private string _soCanNangText = "";
        private decimal _soCanNang = 0;
        private DateTime? _ngayBatDau = DateTime.Now;
        private DateTime? _ngayKetThuc = DateTime.Now.AddDays(1);
        private decimal _tongTien = 0;
        private string _thongBao = "";
        private LOAIGIATUI _selectedLoaiGiGui;

        public ObservableCollection<LOAIGIATUI> DanhSachLoaiGiGui { get; set; }
        public ObservableCollection<LuotGiatDaChonVM> DanhSachDaChon { get; set; }
        public ObservableCollection<PhongChonVM> DanhSachPhong { get; set; }

        public int MaPhong
        {
            get => _maPhong;
            set
            {
                _maPhong = value;
                OnPropertyChanged();
            }
        }

        public string SoCanNangText
        {
            get => _soCanNangText;
            set
            {
                _soCanNangText = value;
                OnPropertyChanged();

                string normalized = value?.Replace(',', '.') ?? "0";

                if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed)
                    && parsed >= 0)
                {
                    _soCanNang = parsed;
                }
                else
                {
                    _soCanNang = 0;
                }

                OnPropertyChanged(nameof(SoCanNang));
                CapNhatTongTien();
            }
        }

        public decimal SoCanNang => _soCanNang;

        public DateTime? NgayBatDau
        {
            get => _ngayBatDau;
            set
            {
                _ngayBatDau = value;
                OnPropertyChanged();

                if (_ngayBatDau.HasValue &&
                    _ngayKetThuc.HasValue &&
                    _ngayBatDau.Value.Date > _ngayKetThuc.Value.Date)
                {
                    ThongBao = "Ngày nhận không được trễ hơn ngày trả!";
                }
            }
        }

        public DateTime? NgayKetThuc
        {
            get => _ngayKetThuc;
            set
            {
                _ngayKetThuc = value;
                OnPropertyChanged();

                if (_ngayBatDau.HasValue &&
                    _ngayKetThuc.HasValue &&
                    _ngayBatDau.Value.Date > _ngayKetThuc.Value.Date)
                {
                    ThongBao = "Ngày nhận không được trễ hơn ngày trả!";
                }
            }
        }

        public decimal TongTien
        {
            get => _tongTien;
            set
            {
                _tongTien = value;
                OnPropertyChanged();
            }
        }

        public string ThongBao
        {
            get => _thongBao;
            set
            {
                _thongBao = value;
                OnPropertyChanged();
            }
        }

        public LOAIGIATUI SelectedLoaiGiGui
        {
            get => _selectedLoaiGiGui;
            set
            {
                _selectedLoaiGiGui = value;
                OnPropertyChanged();
                CapNhatTongTien();
            }
        }

        public ICommand ThemVaoGioCommand { get; }
        public ICommand XoaKhoiGioCommand { get; }
        public ICommand LamMoiCommand { get; }

        public DichVuGiatUiViewModel()
        {
            DanhSachLoaiGiGui = new ObservableCollection<LOAIGIATUI>();
            DanhSachDaChon = new ObservableCollection<LuotGiatDaChonVM>();
            DanhSachPhong = new ObservableCollection<PhongChonVM>();

            ThemVaoGioCommand = new GiatUi_RelayCommand(_ => ThemVaoGio());
            XoaKhoiGioCommand = new GiatUi_RelayCommand(p => XoaKhoiGio(p));
            LamMoiCommand = new GiatUi_RelayCommand(_ => LamMoi());

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                var loaiList = _db.LOAIGIATUIs
                    .OrderBy(x => x.Ma_LoaiGU)
                    .ToList();

                DanhSachLoaiGiGui = new ObservableCollection<LOAIGIATUI>(loaiList);

                var phongList = from p in _db.PHONGs
                                join lp in _db.LOAIPHONGs on p.Ma_LP equals lp.Ma_LP
                                where p.TinhTrang_Phong == "Có khách"
                                orderby p.Ma_Phong
                                select new PhongChonVM
                                {
                                    MaPhong = p.Ma_Phong,
                                    HienThi = p.Ma_Phong + " - " + (lp.Ten_TP ?? "")
                                };

                DanhSachPhong = new ObservableCollection<PhongChonVM>(phongList.ToList());

                if (DanhSachLoaiGiGui.Count > 0)
                    SelectedLoaiGiGui = DanhSachLoaiGiGui.First();

                OnPropertyChanged(nameof(DanhSachLoaiGiGui));
                OnPropertyChanged(nameof(DanhSachPhong));

                TongTien = 0;
                ThongBao = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu giặt ủi: " + ex.Message);
            }
        }

        private bool KiemTraNgayHopLe()
        {
            if (!NgayBatDau.HasValue || !NgayKetThuc.HasValue)
            {
                ThongBao = "Vui lòng chọn ngày nhận và ngày trả!";
                return false;
            }

            if (NgayBatDau.Value.Date > NgayKetThuc.Value.Date)
            {
                ThongBao = "Ngày nhận không được trễ hơn ngày trả!";

                MessageBox.Show(
                    "Ngày nhận không được trễ hơn ngày trả.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        private bool CungNgay(DateTime? a, DateTime? b)
        {
            if (!a.HasValue && !b.HasValue)
                return true;

            if (a.HasValue && b.HasValue)
                return a.Value.Date == b.Value.Date;

            return false;
        }

        private void ThemVaoGio()
        {
            if (_maPhong == 0)
            {
                ThongBao = "Vui lòng chọn phòng!";
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

            if (!KiemTraNgayHopLe())
                return;

            int phongDangChon = _maPhong;
            int maLoai = SelectedLoaiGiGui.Ma_LoaiGU;
            string tenLoai = SelectedLoaiGiGui.Ten_LoaiGU;
            decimal donGia = SelectedLoaiGiGui.DonGia_LoaiGU ?? 0;
            decimal soKg = _soCanNang;
            DateTime? ngayNhan = _ngayBatDau;
            DateTime? ngayTra = _ngayKetThuc;

            // Gộp chỉ khi cùng phòng + cùng loại + cùng ngày nhận/trả
            var exist = DanhSachDaChon.FirstOrDefault(x =>
                x.MaPhong == phongDangChon &&
                x.Ma_LoaiGU == maLoai &&
                CungNgay(x.NgayBatDau, ngayNhan) &&
                CungNgay(x.NgayKetThuc, ngayTra));

            if (exist != null)
            {
                exist.SoCanNang += soKg;
            }
            else
            {
                DanhSachDaChon.Add(new LuotGiatDaChonVM
                {
                    MaPhong = phongDangChon,
                    Ma_LoaiGU = maLoai,
                    Ten_LoaiGU = tenLoai,
                    DonGia_LoaiGU = donGia,
                    SoCanNang = soKg,
                    NgayBatDau = ngayNhan,
                    NgayKetThuc = ngayTra
                });
            }

            ThongBao = $"Đã thêm: {tenLoai} ({soKg:N1} kg) cho phòng {phongDangChon}.";

            // Reset form nhập để tránh lỗi tạm tính bị cộng dồn sai
            ResetFormNhap();
        }

        private void ResetFormNhap()
        {
            MaPhong = 0;

            if (DanhSachLoaiGiGui.Count > 0)
                SelectedLoaiGiGui = DanhSachLoaiGiGui.First();
            else
                SelectedLoaiGiGui = null;

            SoCanNangText = "";
            NgayBatDau = DateTime.Now;
            NgayKetThuc = DateTime.Now.AddDays(1);

            // Tạm tính chỉ là dòng đang nhập, sau khi reset phải về 0
            TongTien = 0;
        }

        public void XoaKhoiGio(object item)
        {
            if (item is LuotGiatDaChonVM vm)
            {
                DanhSachDaChon.Remove(vm);
                CapNhatTongTien();
                ThongBao = "Đã xóa đơn giặt ủi khỏi danh sách.";
            }
        }

        public void XacNhanDon(LuotGiatDaChonVM item)
        {
            if (item == null)
                return;

            try
            {
                using (var db = new QLKhachSan_Model())
                {
                    // Lấy hóa đơn chưa thanh toán theo đúng phòng của dòng được chọn
                    var hoaDon = (from hd in db.HOADONs
                                  join ctlt in db.CHITIET_HDLT on hd.MA_HD equals ctlt.MA_HD
                                  where ctlt.Ma_Phong == item.MaPhong
                                        && hd.TinhTrang_HD == "Chưa thanh toán"
                                  orderby hd.MA_HD descending
                                  select hd).FirstOrDefault();

                    if (hoaDon == null)
                    {
                        MessageBox.Show(
                            $"Phòng {item.MaPhong} chưa có hóa đơn lưu trú chưa thanh toán.\nKhông thể xác nhận dịch vụ giặt ủi.",
                            "Thông báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    int maLuotMoi = db.LUOTGIATUIs.Any()
                        ? db.LUOTGIATUIs.Max(l => l.Ma_LuotGU) + 1
                        : 1;

                    var luotGU = new LUOTGIATUI
                    {
                        Ma_LuotGU = maLuotMoi,
                        SoKilogram_LuotGU = (int)Math.Ceiling(item.SoCanNang),
                        NgayBatDau_LuotGU = item.NgayBatDau,
                        NgayKetThuc_LuotGU = item.NgayKetThuc,
                        Ma_LoaiGU = item.Ma_LoaiGU
                    };

                    db.LUOTGIATUIs.Add(luotGU);

                    int maCTMoi = db.CHITIET_HDGU.Any()
                        ? db.CHITIET_HDGU.Max(c => c.Ma_CTHDGU) + 1
                        : 1;

                    long thanhTien = (long)(item.SoCanNang * item.DonGia_LoaiGU);

                    var chiTiet = new CHITIET_HDGU
                    {
                        Ma_CTHDGU = maCTMoi,
                        ThoiGianLap_CTHDGU = DateTime.Now,
                        TriGia_CTHDGU = thanhTien,
                        MA_HD = hoaDon.MA_HD,
                        Ma_LuotGU = maLuotMoi,
                        LUOTGIATUI = luotGU
                    };

                    db.CHITIET_HDGU.Add(chiTiet);

                    hoaDon.TriGia_HD = (hoaDon.TriGia_HD ?? 0) + thanhTien;

                    db.SaveChanges();
                }

                DanhSachDaChon.Remove(item);
                CapNhatTongTien();

                ThongBao = $"Đã xác nhận giặt ủi cho phòng {item.MaPhong}.";

                MessageBox.Show(
                    $"Đã xác nhận dịch vụ giặt ủi cho phòng {item.MaPhong}.\nDữ liệu đã được đẩy về hóa đơn tổng.",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += "\n" + ex.InnerException.Message;

                MessageBox.Show(
                    "Lỗi xác nhận đơn giặt ủi: " + msg,
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LamMoi()
        {
            ResetFormNhap();
            ThongBao = "Đã làm mới form nhập. Danh sách đơn giặt ủi vẫn được giữ nguyên.";
        }

        private void CapNhatTongTien()
        {
            // Chỉ tính tạm tính của dòng đang nhập.
            // Không cộng tổng giỏ để tránh lỗi tạm tính bị lệch sau khi thêm vào giỏ.
            if (_selectedLoaiGiGui != null && _soCanNang > 0)
            {
                decimal gia = SelectedLoaiGiGui.DonGia_LoaiGU ?? 0;
                TongTien = gia * _soCanNang;
            }
            else
            {
                TongTien = 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LuotGiatDaChonVM : INotifyPropertyChanged
    {
        private decimal _soCanNang = 0;

        public int MaPhong { get; set; }

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
                _soCanNang = value < 0 ? 0 : value;
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