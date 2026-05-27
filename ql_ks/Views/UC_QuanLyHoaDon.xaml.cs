using System.Windows.Controls;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_QuanLyHoaDon : UserControl
    {
        public UC_QuanLyHoaDon()
        {
            InitializeComponent();
            this.DataContext = new QuanLyHoaDonViewModel();
        }
    }
}
