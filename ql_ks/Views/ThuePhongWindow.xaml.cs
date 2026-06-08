using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ql_ks.Models;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class ThuePhongWindow : Window
    {
        private readonly HoaDonLuuTruViewModel _vm;

        public ThuePhongWindow(int maPhong)
        {
            InitializeComponent();
            _vm = new HoaDonLuuTruViewModel(maPhong);
            DataContext = _vm;
            Title = "Thuê phòng " + maPhong;
        }

        private void TxtCMND_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_vm.CMND)) return;

            try
            {
                using (var db = new QLKhachSan_Model())
                {
                    var kh = db.KHACHHANGs.FirstOrDefault(k => k.CMND_KH == _vm.CMND);
                    if (kh != null)
                    {
                        _vm.TenKhachHang = kh.HoTen_KH;
                        _vm.SoDienThoaiKH = kh.SoDienThoai_KH;
                    }
                }
            }
            catch { }
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(_vm.TenKhachHang))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_vm.CMND))
            {
                MessageBox.Show("Vui lòng nhập CMND khách hàng!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_vm.NgayTra <= _vm.NgayNhan)
            {
                MessageBox.Show("Ngày trả phải sau ngày nhận!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "═══ XÁC NHẬN THUÊ PHÒNG ═══\n\n" +
                "Phòng: " + _vm.MaPhong + " (" + _vm.LoaiPhong + ")\n" +
                "Khách hàng: " + _vm.TenKhachHang + "\n" +
                "CMND: " + _vm.CMND + "\n" +
                "Ngày nhận: " + _vm.NgayNhan.ToString("dd/MM/yyyy") + "\n" +
                "Ngày trả: " + _vm.NgayTra.ToString("dd/MM/yyyy") + "\n" +
                "Số ngày: " + _vm.SoNgay + "\n" +
                "Tổng tiền: " + _vm.TongTienSo.ToString("N0") + " VNĐ\n\n" +
                "Đồng ý thuê phòng?",
                "Xác nhận thuê phòng",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new QLKhachSan_Model())
                    {
                        // 1. Tìm hoặc tạo khách hàng
                        var kh = db.KHACHHANGs.FirstOrDefault(k => k.CMND_KH == _vm.CMND);
                        if (kh == null)
                        {
                            int maKHMoi = db.KHACHHANGs.Any()
                                ? db.KHACHHANGs.Max(k => k.MA_KH) + 1 : 1;
                            kh = new KHACHHANG
                            {
                                MA_KH = maKHMoi,
                                HoTen_KH = _vm.TenKhachHang,
                                CMND_KH = _vm.CMND,
                                SoDienThoai_KH = _vm.SoDienThoaiKH
                            };
                            db.KHACHHANGs.Add(kh);
                        }
                        else
                        {
                            kh.HoTen_KH = _vm.TenKhachHang;
                            kh.SoDienThoai_KH = _vm.SoDienThoaiKH;
                        }

                        // 2. Tạo hóa đơn
                        int maHD = db.HOADONs.Any()
                            ? db.HOADONs.Max(h => h.MA_HD) + 1 : 1;

                        var hoaDon = new HOADON
                        {
                            MA_HD = maHD,
                            ThoiGianLap_HD = _vm.NgayLapHD,
                            TinhTrang_HD = "Chưa thanh toán",
                            TriGia_HD = _vm.TongTienSo,
                            MA_NV = _vm.MaNV,
                            MA_KH = kh.MA_KH
                        };
                        db.HOADONs.Add(hoaDon);

                        // 3. Chi tiết lưu trú
                        int maCT = db.CHITIET_HDLT.Any()
                            ? db.CHITIET_HDLT.Max(c => c.Ma_CTHDLT) + 1 : 1;

                        var ct = new CHITIET_HDLT
                        {
                            Ma_CTHDLT = maCT,
                            ThoiGianNhan_PHONG = _vm.NgayNhan,
                            ThoiGianTra_PHONG = _vm.NgayTra,
                            TriGia_CTHDLT = _vm.TongTienSo,
                            MA_HD = maHD,
                            Ma_Phong = _vm.MaPhong
                        };
                        db.CHITIET_HDLT.Add(ct);

                        // 4. Cập nhật trạng thái phòng
                        var phong = db.PHONGs.Find(_vm.MaPhong);
                        if (phong != null)
                            phong.TinhTrang_Phong = "Có khách";

                        db.SaveChanges();

                        MessageBox.Show(
                            "Thuê phòng thành công!\n\n" +
                            "Mã hóa đơn: " + maHD + "\n" +
                            "Phòng: " + _vm.MaPhong + "\n" +
                            "Tổng tiền: " + _vm.TongTienSo.ToString("N0") + " VNĐ",
                            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi thuê phòng: " + ex.Message, "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}