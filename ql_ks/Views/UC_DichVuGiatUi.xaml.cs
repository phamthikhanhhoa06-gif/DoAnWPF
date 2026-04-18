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
    /// Interaction logic for UC_DichVuGiatUi.xaml
    /// </summary>
    public partial class UC_DichVuGiatUi : UserControl
    {
        public UC_DichVuGiatUi()
        {
            InitializeComponent();
            DataContext = new DichVuGiatUiViewModel();

            // Lưu reference để gọi Xóa với parameter
            if (DataContext is DichVuGiatUiViewModel vm)
            {
                this.Loaded += (s, e) =>
                {
                    // Có thể thêm event handler tại đây nếu cần
                };
            }
        }

        // Event handler nếu cần xử lý nút ngoài DataGrid
        private void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is LuotGiatDaChonVM item)
            {
                var vm = DataContext as DichVuGiatUiViewModel;
                vm?.XoaKhoiGio(item);
            }
        }
    }
}