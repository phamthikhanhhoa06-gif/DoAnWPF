using System.Windows.Controls;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_QuanLyKhachHang : UserControl
    {
        public UC_QuanLyKhachHang()
        {
            InitializeComponent();
            this.DataContext = new QuanLyKhachHangViewModel();
        }
    }
}
