using System;
using System.Windows;
using System.Windows.Controls;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_HoaDonDiChuyen : UserControl
    {
        public UC_HoaDonDiChuyen(int maPhong)
        {
            InitializeComponent();
            DataContext = new HoaDonDiChuyenViewModel(maPhong);
        }

        private void BtnCapNhatGio_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as HoaDonDiChuyenViewModel;
            if (vm != null)
            {
                vm.GioLapHD = DateTime.Now.ToString("hh:mm tt");
            }
        }
    }
}