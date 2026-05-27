using System;
using System.Windows;
using System.Windows.Controls;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_HoaDonAnUong : UserControl
    {
        public UC_HoaDonAnUong(int maPhong)
        {
            InitializeComponent();
            DataContext = new HoaDonAnUongViewModel(maPhong);
        }

        private void BtnCapNhatGio_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as HoaDonAnUongViewModel;
            if (vm != null)
            {
                vm.GioLapHD = DateTime.Now.ToString("hh:mm tt");
            }
        }
    }
}