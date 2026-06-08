using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public class DichVuDiChuyenViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private int _maPhong = 0;
        private string _thongBao = "";

        public ObservableCollection<CHUYENDI> DanhSachDiemDen { get; set; }
        public ObservableCollection<PhongChonDC_VM> DanhSachPhong { get; set; }
        public ObservableCollection<DonDiChuyenVM> DanhSachDon { get; set; }

        private CHUYENDI _selectedDiemDen;
        private long _tongTien = 0;

        public int MaPhong
        {
            get => _maPhong;
            set { _maPhong = value; OnPropertyChanged(); }
        }

        public string ThongBao
        {
            get => _thongBao;
            set { _thongBao = value; OnPropertyChanged(); }
        }

        public long TongTien
        {
            get => _tongTien;
            set { _tongTien = value; OnPropertyChanged(); }
        }

        public CHUYENDI SelectedDiemDen
        {
            get => _selectedDiemDen;
            set
            {
                _selectedDiemDen = value;
                OnPropertyChanged();
                if (value != null) CapNhatThongBao();
            }
        }

        public ICommand ThemDonCommand { get; }
        public ICommand LamMoiCommand { get; }

        public DichVuDiChuyenViewModel()
        {
            DanhSachDiemDen = new ObservableCollection<CHUYENDI>();
            DanhSachPhong = new ObservableCollection<PhongChonDC_VM>();
            DanhSachDon = new ObservableCollection<DonDiChuyenVM>();

            ThemDonCommand = new DiChuyen_RelayCommand(_ => ThemDon());
            LamMoiCommand = new DiChuyen_RelayCommand(_ => LamMoiForm());

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                // Load điểm đến
                var dsDiem = _db.CHUYENDIs.OrderBy(x => x.Ma_CD).ToList();
                DanhSachDiemDen = new ObservableCollection<CHUYENDI>(dsDiem);
                OnPropertyChanged(nameof(DanhSachDiemDen));

                // ✅ CHỈ LẤY PHÒNG ĐANG CÓ KHÁCH
                var dsPhong = from p in _db.PHONGs
                              join lp in _db.LOAIPHONGs on p.Ma_LP equals lp.Ma_LP
                              where p.TinhTrang_Phong == "Có khách"
                              orderby p.Ma_Phong
                              select new PhongChonDC_VM
                              {
                                  MaPhong = p.Ma_Phong,
                                  HienThi = p.Ma_Phong + " - " + (lp.Ten_TP ?? "")
                              };

                DanhSachPhong = new ObservableCollection<PhongChonDC_VM>(dsPhong.ToList());
                OnPropertyChanged(nameof(DanhSachPhong));

                if (DanhSachDiemDen.Count > 0)
                    SelectedDiemDen = DanhSachDiemDen.First();

                ThongBao = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu di chuyển: " + ex.Message);
            }
        }

        // ========== THÊM ĐƠN VÀO DANH SÁCH TẠM ==========
        private void ThemDon()
        {
            if (_maPhong == 0)
            {
                ThongBao = "⚠ Vui lòng chọn phòng!";
                return;
            }

            if (_selectedDiemDen == null)
            {
                ThongBao = "⚠ Vui lòng chọn điểm đến!";
                return;
            }

            // Nếu đã có đơn cùng phòng + cùng điểm đến → tăng số lượng
            var exist = DanhSachDon.FirstOrDefault(x => x.MaPhong == _maPhong && x.Ma_CD == SelectedDiemDen.Ma_CD);

            if (exist != null)
            {
                exist.SoLuong++;
            }
            else
            {
                // ✅ Tạo object MỚI hoàn toàn, không reference chung
                DanhSachDon.Add(new DonDiChuyenVM
                {
                    MaPhong = _maPhong,
                    Ma_CD = SelectedDiemDen.Ma_CD,
                    DiemDen_CD = SelectedDiemDen.DiemDen_CD,
                    DonGia_CD = SelectedDiemDen.DonGia_CD ?? 0,
                    SoLuong = 1,
                    NgayDat = DateTime.Now,
                    TrangThai = "Chờ"
                });
            }

            CapNhatTongTien();
            ThongBao = $"✓ Đã thêm: {SelectedDiemDen.DiemDen_CD} cho phòng {_maPhong}";
        }

        // ========== XÓA 1 ĐƠN KHỎI DANH SÁCH TẠM ==========
        public void XoaDon(DonDiChuyenVM item)
        {
            if (item == null) return;
            DanhSachDon.Remove(item);
            CapNhatTongTien();
        }

        // ========== ✅ XÁC NHẬN 1 ĐƠN → ĐẨY VỀ HÓA ĐƠN PHÒNG ==========
        public void XacNhanDon(DonDiChuyenVM item)
        {
            if (item == null) return;
            try
            {
                using (var db = new QLKhachSan_Model())
                {
                    // Tìm HOADON chưa thanh toán của phòng
                    var hoaDon = (from hd in db.HOADONs
                                  join ctlt in db.CHITIET_HDLT on hd.MA_HD equals ctlt.MA_HD
                                  where ctlt.Ma_Phong == item.MaPhong
                                        && hd.TinhTrang_HD == "Chưa thanh toán"
                                  orderby hd.MA_HD descending
                                  select hd).FirstOrDefault();

                    if (hoaDon == null)
                    {
                        MessageBox.Show($"Phòng {item.MaPhong} chưa có hóa đơn lưu trú!\nKhông thể xác nhận.",
                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int maCTMoi = db.CHITIET_HDDC.Any()
                        ? db.CHITIET_HDDC.Max(c => c.Ma_CTHDDC) + 1
                        : 1;

                    var chiTiet = new CHITIET_HDDC
                    {
                        Ma_CTHDDC = maCTMoi,
                        ThoiGianLap_CTHDDC = DateTime.Now,
                        TriGia_CTHDDC = item.ThanhTien,
                        MA_HD = hoaDon.MA_HD,
                        Ma_CD = item.Ma_CD
                    };

                    db.CHITIET_HDDC.Add(chiTiet);
                    hoaDon.TriGia_HD = (hoaDon.TriGia_HD ?? 0) + item.ThanhTien;
                    db.SaveChanges();
                }

                // Xóa đơn khỏi danh sách tạm
                DanhSachDon.Remove(item);
                CapNhatTongTien();

                ThongBao = $"✓ Đã xác nhận đẩy về hóa đơn phòng {item.MaPhong}";
                MessageBox.Show($"Đã xác nhận dịch vụ '{item.DiemDen_CD}' cho phòng {item.MaPhong}.\n" +
                                $"Dữ liệu đã được đẩy về hóa đơn tổng.",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xác nhận đơn: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== LÀM MỚI - KHÔNG XÓA DANH SÁCH ĐƠN ==========
        private void LamMoiForm()
        {
            // ✅ CHỈ reset form nhập, KHÔNG xóa DanhSachDon
            MaPhong = 0;
            ThongBao = "Đã làm mới form. Danh sách đơn vẫn được giữ nguyên.";

            if (DanhSachDiemDen.Count > 0)
                SelectedDiemDen = DanhSachDiemDen.First();
        }

        public void CapNhatTongTien()
        {
            TongTien = DanhSachDon.Sum(x => x.ThanhTien);
        }

        private void CapNhatThongBao()
        {
            if (_selectedDiemDen != null)
            {
                ThongBao = $"Giá: {_selectedDiemDen.DonGia_CD:N0} đ";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ========== CLASS ĐƠN TẠM ==========
    public class DonDiChuyenVM : INotifyPropertyChanged
    {
        private int _soLuong = 1;

        public int MaPhong { get; set; }
        public int Ma_CD { get; set; }
        public string DiemDen_CD { get; set; }
        public long DonGia_CD { get; set; }
        public DateTime NgayDat { get; set; }
        public string TrangThai { get; set; }

        public int SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value < 1 ? 1 : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public long ThanhTien => DonGia_CD * SoLuong;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper cho phòng
    public class PhongChonDC_VM
    {
        public int MaPhong { get; set; }
        public string HienThi { get; set; }
    }
}