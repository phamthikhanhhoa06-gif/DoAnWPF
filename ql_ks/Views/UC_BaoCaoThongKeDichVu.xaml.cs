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
    /// Interaction logic for UC_BaoCaoThongKeDichVu.xaml
    /// </summary>
    public partial class UC_BaoCaoThongKeDichVu : UserControl
    {
        public UC_BaoCaoThongKeDichVu()
        {
            InitializeComponent();
            DataContext = new BaoCaoThongKeDichVuViewModel();
        }
        // ✅ THÊM METHOD NÀY
        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            // ✅ Lấy ngày từ ViewModel
            var vm = DataContext as BaoCaoThongKeDichVuViewModel;
            if (vm == null) return;

            // ✅ Truyền ngày sang UC_XuatBaoCaoDichVu
            var ucXuat = new UC_XuatBaoCaoDichVu(
                vm.NgayBatDau,
                vm.NgayKetThuc);

            var window = new Window
            {
                Title = "Báo Cáo Doanh Thu Dịch Vụ",
                Width = 950,
                Height = 750,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = ucXuat,
                WindowStyle = WindowStyle.ToolWindow,
                Background = System.Windows.Media.Brushes.WhiteSmoke
            };
            window.Show();
        }
    }
}
