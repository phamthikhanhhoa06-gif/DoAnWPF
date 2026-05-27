using ql_ks.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ql_ks.ViewModels
{
    public class HoaDonItem
    {
        public int MaHD { get; set; }
        public DateTime? ThoiGianLap { get; set; }
        public string TinhTrang { get; set; }
        public long? TriGia { get; set; }
        public string TenKhachHang { get; set; }
        public string TenNhanVien { get; set; }
        public int? MaPhong { get; set; }
        public DateTime? NhanPhong { get; set; }
        public DateTime? TraPhong { get; set; }
    }

    public class QuanLyHoaDonViewModel : Common_BaseViewModel
    {
        private ObservableCollection<HoaDonItem> _listHoaDon;
        public ObservableCollection<HoaDonItem> ListHoaDon
        {
            get => _listHoaDon;
            set { _listHoaDon = value; OnPropertyChanged(); }
        }

        private ObservableCollection<KHACHHANG> _listKhachHang;
        public ObservableCollection<KHACHHANG> ListKhachHang
        {
            get => _listKhachHang;
            set { _listKhachHang = value; OnPropertyChanged(); }
        }

        private ObservableCollection<NHANVIEN> _listNhanVien;
        public ObservableCollection<NHANVIEN> ListNhanVien
        {
            get => _listNhanVien;
            set { _listNhanVien = value; OnPropertyChanged(); }
        }

        private ObservableCollection<PHONG> _listPhong;
        public ObservableCollection<PHONG> ListPhong
        {
            get => _listPhong;
            set { _listPhong = value; OnPropertyChanged(); }
        }

        private int _maHD;
        public int MaHD
        {
            get => _maHD;
            set { _maHD = value; OnPropertyChanged(); }
        }

        private KHACHHANG _selectedKhachHang;
        public KHACHHANG SelectedKhachHang
        {
            get => _selectedKhachHang;
            set { _selectedKhachHang = value; OnPropertyChanged(); }
        }

        private NHANVIEN _selectedNhanVien;
        public NHANVIEN SelectedNhanVien
        {
            get => _selectedNhanVien;
            set { _selectedNhanVien = value; OnPropertyChanged(); }
        }

        private PHONG _selectedPhong;
        public PHONG SelectedPhong
        {
            get => _selectedPhong;
            set { _selectedPhong = value; OnPropertyChanged(); }
        }

        private DateTime _ngayNhanPhong = DateTime.Now;
        public DateTime NgayNhanPhong
        {
            get => _ngayNhanPhong;
            set { _ngayNhanPhong = value; OnPropertyChanged(); }
        }

        private DateTime? _ngayTraPhong = null;
        public DateTime? NgayTraPhong
        {
            get => _ngayTraPhong;
            set 
            { 
                _ngayTraPhong = value; 
                OnPropertyChanged();
                CalculatePrice();
            }
        }

        private string _tinhTrangHD = "Chưa thanh toán";
        public string TinhTrangHD
        {
            get => _tinhTrangHD;
            set { _tinhTrangHD = value; OnPropertyChanged(); }
        }

        private long _triGiaDaTinh = 0;
        public long TriGiaDaTinh
        {
            get => _triGiaDaTinh;
            set { _triGiaDaTinh = value; OnPropertyChanged(); }
        }

        private HoaDonItem _selectedHoaDon;
        public HoaDonItem SelectedHoaDon
        {
            get => _selectedHoaDon;
            set
            {
                _selectedHoaDon = value;
                OnPropertyChanged();
                if (_selectedHoaDon != null)
                {
                    MaHD = _selectedHoaDon.MaHD;
                    SelectedKhachHang = ListKhachHang.FirstOrDefault(k => k.HoTen_KH == _selectedHoaDon.TenKhachHang);
                    SelectedNhanVien = ListNhanVien.FirstOrDefault(n => n.HoTen_NV == _selectedHoaDon.TenNhanVien);
                    SelectedPhong = ListPhong.FirstOrDefault(p => p.Ma_Phong == _selectedHoaDon.MaPhong);
                    NgayNhanPhong = _selectedHoaDon.NhanPhong ?? DateTime.Now;
                    NgayTraPhong = _selectedHoaDon.TraPhong;
                    TinhTrangHD = _selectedHoaDon.TinhTrang;
                    TriGiaDaTinh = _selectedHoaDon.TriGia ?? 0;
                }
            }
        }

        public ICommand CheckInCommand { get; set; }
        public ICommand CheckOutCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public QuanLyHoaDonViewModel()
        {
            LoadData();

            CheckInCommand = new Common_RelayCommand(CheckIn, CanCheckIn);
            CheckOutCommand = new Common_RelayCommand(CheckOut, CanCheckOut);
            DeleteCommand = new Common_RelayCommand(Delete, CanDelete);
            ClearCommand = new Common_RelayCommand(Clear);
        }

        private void LoadData()
        {
            using (var db = new QLKhachSan_Model())
            {
                ListKhachHang = new ObservableCollection<KHACHHANG>(db.KHACHHANGs.ToList());
                ListNhanVien = new ObservableCollection<NHANVIEN>(db.NHANVIENs.ToList());
                ListPhong = new ObservableCollection<PHONG>(db.PHONGs.Include("LOAIPHONG").ToList());
                
                var query = from hd in db.HOADONs
                            join ctlt in db.CHITIET_HDLT on hd.MA_HD equals ctlt.MA_HD into ctltGroup
                            from ct in ctltGroup.DefaultIfEmpty()
                            select new HoaDonItem
                            {
                                MaHD = hd.MA_HD,
                                ThoiGianLap = hd.ThoiGianLap_HD,
                                TinhTrang = hd.TinhTrang_HD,
                                TriGia = hd.TriGia_HD,
                                TenKhachHang = hd.KHACHHANG != null ? hd.KHACHHANG.HoTen_KH : "",
                                TenNhanVien = hd.NHANVIEN != null ? hd.NHANVIEN.HoTen_NV : "",
                                MaPhong = ct != null ? ct.Ma_Phong : (int?)null,
                                NhanPhong = ct != null ? ct.ThoiGianNhan_PHONG : (DateTime?)null,
                                TraPhong = ct != null ? ct.ThoiGianTra_PHONG : (DateTime?)null
                            };
                            
                ListHoaDon = new ObservableCollection<HoaDonItem>(query.ToList());
            }
        }
        
        private void CalculatePrice()
        {
            if (SelectedPhong != null && NgayTraPhong.HasValue && SelectedPhong.LOAIPHONG != null)
            {
                var days = (NgayTraPhong.Value - NgayNhanPhong).TotalDays;
                if (days < 1) days = 1; // Thu tối thiểu 1 ngày
                TriGiaDaTinh = (long)(days * (SelectedPhong.LOAIPHONG.DonGia_LP ?? 0));
            }
            else
            {
                TriGiaDaTinh = 0;
            }
        }

        private bool CanCheckIn(object obj)
        {
            return MaHD > 0 && SelectedKhachHang != null && SelectedNhanVien != null && SelectedPhong != null;
        }

        private void CheckIn(object obj)
        {
            using (var db = new QLKhachSan_Model())
            {
                if (db.HOADONs.Any(h => h.MA_HD == MaHD))
                {
                    MessageBox.Show("Mã Hóa Đơn đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                autoGenCtId:
                int randomIdCt = new Random().Next(1000, 99999);
                if (db.CHITIET_HDLT.Any(x => x.Ma_CTHDLT == randomIdCt)) goto autoGenCtId;

                var hd = new HOADON
                {
                    MA_HD = MaHD,
                    MA_KH = SelectedKhachHang.MA_KH,
                    MA_NV = SelectedNhanVien.MA_NV,
                    ThoiGianLap_HD = DateTime.Now,
                    TinhTrang_HD = TinhTrangHD,
                    TriGia_HD = 0
                };
                
                var ctlt = new CHITIET_HDLT
                {
                    Ma_CTHDLT = randomIdCt,
                    MA_HD = MaHD,
                    Ma_Phong = SelectedPhong.Ma_Phong,
                    ThoiGianNhan_PHONG = NgayNhanPhong,
                    ThoiGianTra_PHONG = null,
                    TriGia_CTHDLT = 0
                };
                
                // Update room status
                var phong = db.PHONGs.Find(SelectedPhong.Ma_Phong);
                if (phong != null) phong.TinhTrang_Phong = "Đang ở";

                db.HOADONs.Add(hd);
                db.CHITIET_HDLT.Add(ctlt);
                db.SaveChanges();
            }
            LoadData();
            Clear(null);
            MessageBox.Show("Nhận phòng (Tạo HĐ mới) thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanCheckOut(object obj)
        {
            return SelectedHoaDon != null && SelectedPhong != null;
        }

        private void CheckOut(object obj)
        {
            using (var db = new QLKhachSan_Model())
            {
                var hd = db.HOADONs.Find(MaHD);
                if (hd != null)
                {
                    CalculatePrice();
                    hd.TinhTrang_HD = "Đã thanh toán";
                    hd.TriGia_HD = TriGiaDaTinh;
                    
                    var ctlt = db.CHITIET_HDLT.FirstOrDefault(c => c.MA_HD == MaHD);
                    if (ctlt != null)
                    {
                        ctlt.ThoiGianTra_PHONG = NgayTraPhong ?? DateTime.Now;
                        ctlt.TriGia_CTHDLT = TriGiaDaTinh;
                    }
                    
                    var phong = db.PHONGs.Find(SelectedPhong.Ma_Phong);
                    if (phong != null) phong.TinhTrang_Phong = "Trống";
                    
                    db.SaveChanges();
                }
            }
            LoadData();
            MessageBox.Show("Trả phòng & Thanh toán hoàn tất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanDelete(object obj)
        {
            return SelectedHoaDon != null;
        }

        private void Delete(object obj)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa Hóa Đơn này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var db = new QLKhachSan_Model())
                {
                    var ctlt = db.CHITIET_HDLT.FirstOrDefault(c => c.MA_HD == MaHD);
                    if (ctlt != null) db.CHITIET_HDLT.Remove(ctlt);
                    
                    var hd = db.HOADONs.Find(MaHD);
                    if (hd != null) db.HOADONs.Remove(hd);
                    
                    db.SaveChanges();
                }
                LoadData();
                Clear(null);
                MessageBox.Show("Xóa Hóa Đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Clear(object obj)
        {
            SelectedHoaDon = null;
            MaHD = 0;
            SelectedKhachHang = null;
            SelectedNhanVien = null;
            SelectedPhong = null;
            NgayNhanPhong = DateTime.Now;
            NgayTraPhong = null;
            TinhTrangHD = "Chưa thanh toán";
            TriGiaDaTinh = 0;
        }
    }
}
