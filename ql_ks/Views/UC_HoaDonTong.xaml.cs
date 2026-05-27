using ql_ks.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ql_ks.Views
{
    public partial class UC_HoaDonTong : UserControl
    {
        public UC_HoaDonTong(int maPhong)
        {
            InitializeComponent();
            DataContext = new HoaDonTongViewModel(maPhong);
        }

        private void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.Close();
            }
        }
    }
}