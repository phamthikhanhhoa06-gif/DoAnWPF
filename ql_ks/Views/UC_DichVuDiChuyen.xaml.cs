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
    public partial class UC_DichVuDiChuyen : UserControl
    {
        private DichVuDiChuyenViewModel Vm => DataContext as DichVuDiChuyenViewModel;

        public UC_DichVuDiChuyen()
        {
            InitializeComponent();
            DataContext = new DichVuDiChuyenViewModel();
        }

        // Xóa đơn khỏi danh sách tạm
        private void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DonDiChuyenVM item)
            {
                Vm?.XoaDon(item);
            }
        }

        // ✅ Xác nhận 1 đơn cụ thể → đẩy về hóa đơn phòng
        private void BtnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DonDiChuyenVM item)
            {
                Vm?.XacNhanDon(item);
            }
        }
    }
}