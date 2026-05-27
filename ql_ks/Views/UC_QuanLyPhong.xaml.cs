using System.Windows.Controls;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_QuanLyPhong : UserControl
    {
        public UC_QuanLyPhong()
        {
            InitializeComponent();
            this.DataContext = new QuanLyPhongViewModel();
        }
    }
}
