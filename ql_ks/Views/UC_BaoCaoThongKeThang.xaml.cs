using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for UC_BaoCaoThongKeThang.xaml
    /// </summary>
    public partial class UC_BaoCaoThongKeThang : UserControl
    {
        public UC_BaoCaoThongKeThang()
        {
            InitializeComponent();
            DataContext = new BaoCaoThongKeThangViewModel();

            // ✅ Sort mặc định theo SoThuTu tăng dần
            Loaded += (s, e) =>
            {
                var view = CollectionViewSource.GetDefaultView(dgNgay.ItemsSource);
                if (view != null)
                {
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription("SoThuTu", ListSortDirection.Ascending));
                }
            };
        }

        // ✅ Ngăn DataGrid sort theo chuỗi khi click header
        private void DgNgay_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                var view = CollectionViewSource.GetDefaultView(dgNgay.ItemsSource);
                if (view != null && view.SortDescriptions.Count == 0)
                {
                    view.SortDescriptions.Add(new SortDescription("SoThuTu", ListSortDirection.Ascending));
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

      
        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as BaoCaoThongKeThangViewModel;
            if (vm == null) return;

            var ucXuat = new UC_XuatBaoCaoThang(
                vm.NamChon,
                vm.ThangChon);

            var window = new Window
            {
                Title = "Xuất Báo Cáo Thống Kê Tháng " + vm.ThangChon + "/" + vm.NamChon,
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