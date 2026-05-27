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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    /// <summary>
    /// Interaction logic for UC_BaoCaoThongKeKhachHang.xaml
    /// </summary>
    public partial class UC_BaoCaoThongKeKhachHang : UserControl
    {
        public UC_BaoCaoThongKeKhachHang()
        {
            InitializeComponent();
            DataContext = new BaoCaoThongKeKhachHangViewModel();
        }

        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as BaoCaoThongKeKhachHangViewModel;
            if (vm == null) return;

            var ucXuat = new UC_XuatBaoCaoKhachHang(vm.NgayBatDau, vm.NgayKetThuc);
            var window = new Window
            {
                Title = $"Xuất Báo Cáo Khách Hàng ({vm.NgayBatDau:dd/MM/yyyy} - {vm.NgayKetThuc:dd/MM/yyyy})",
                Width = 950,
                Height = 750,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = ucXuat,
                WindowStyle = WindowStyle.ToolWindow
            };
            window.Show();
        }
    }
}

