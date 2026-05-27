using System.Windows;
using System.Windows.Controls;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_BaoCaoThongKeNam : UserControl
    {
        public UC_BaoCaoThongKeNam()
        {
            InitializeComponent();
            DataContext = new BaoCaoThongKeNamViewModel();
        }

        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as BaoCaoThongKeNamViewModel;
            if (vm == null) return;

            var ucXuat = new UC_XuatBaoCaoNam(vm.NamChon);
            var window = new Window
            {
                Title = "Xuất Báo Cáo Năm " + vm.NamChon,
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