using System;
using System.Windows;
using System.Windows.Controls;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_HoaDonGiatUi : UserControl
    {
        public UC_HoaDonGiatUi(int maPhong)
        {
            InitializeComponent();
            DataContext = new HoaDonGiatUiViewModel(maPhong);
        }

        private void BtnCapNhatGio_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as HoaDonGiatUiViewModel;
            if (vm != null)
            {
                vm.GioLapHD = DateTime.Now.ToString("hh:mm tt");
            }
        }
    }
}