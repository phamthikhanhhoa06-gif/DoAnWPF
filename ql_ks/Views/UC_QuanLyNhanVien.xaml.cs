using System.Windows.Controls;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_QuanLyNhanVien : UserControl
    {
        public UC_QuanLyNhanVien()
        {
            InitializeComponent();
            this.DataContext = new QuanLyNhanVienViewModel();
        }
    }
}
