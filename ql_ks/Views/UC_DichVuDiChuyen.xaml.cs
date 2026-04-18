using System;
using System.Collections.Generic;
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
    /// Interaction logic for UC_DichVuDiChuyen.xaml
    /// </summary>
    public partial class UC_DichVuDiChuyen : UserControl
    {
        private DichVuDiChuyenViewModel Vm => DataContext as DichVuDiChuyenViewModel;

        public UC_DichVuDiChuyen()
        {
            InitializeComponent();
            DataContext = new DichVuDiChuyenViewModel();
        }

        private void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DonDiChuyenVM item)
            {
                Vm?.DanhSachDon.Remove(item);
                Vm?.CapNhatTongTien();
            }
        }
    }
}