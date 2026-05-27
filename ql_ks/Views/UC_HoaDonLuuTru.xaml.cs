using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ql_ks.Models;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_HoaDonLuuTru : UserControl
    {
        public UC_HoaDonLuuTru(int maPhong)
        {
            InitializeComponent();
            DataContext = new HoaDonLuuTruViewModel(maPhong);
        }

        // Nút 2: Cập nhật giờ hiện tại
        private void BtnCapNhatGio_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as HoaDonLuuTruViewModel;
            if (vm != null)
            {
                vm.GioLapHD = DateTime.Now.ToString("hh:mm tt");
            }
        }

        // Tự động tìm khách hàng khi nhập CMND
        private void TxtCMND_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as HoaDonLuuTruViewModel;
            if (vm == null || string.IsNullOrWhiteSpace(vm.CMND)) return;

            try
            {
                using (var db = new QLKhachSan_Model())
                {
                    var kh = db.KHACHHANGs
                        .FirstOrDefault(k => k.CMND_KH == vm.CMND);

                    if (kh != null)
                    {
                        vm.TenKhachHang = kh.HoTen_KH;
                        vm.SoDienThoaiKH = kh.SoDienThoai_KH;
                    }
                }
            }
            catch { }
        }
    }
}