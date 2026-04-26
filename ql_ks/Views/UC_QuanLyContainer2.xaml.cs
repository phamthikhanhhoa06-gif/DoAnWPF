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

namespace ql_ks.Views
{
    /// <summary>
    /// Interaction logic for UC_QuanLyContainer2.xaml
    /// </summary>
    public partial class UC_QuanLyContainer2 : UserControl
    {
        private Button _currentActiveButton;

        public UC_QuanLyContainer2()
        {
            InitializeComponent();

            // Mặc định chọn tab "Báo cáo dịch vụ" khi mở
            _currentActiveButton = btnBaoCaoDV;
            ContentArea.Content = new UC_BaoCaoThongKeDichVu();
        }

        // Helper: Đổi style tab active
        private void SetActiveTab(Button activeButton)
        {
            if (_currentActiveButton != null)
            {
                _currentActiveButton.Style = (Style)FindResource("TabButtonStyle");
            }
            activeButton.Style = (Style)FindResource("ActiveTabStyle");
            _currentActiveButton = activeButton;
        }

        // Tab 1: Báo cáo dịch vụ
        private void BtnBaoCaoDV_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            ContentArea.Content = new UC_BaoCaoThongKeDichVu();
        }

        // Tab 2: Báo cáo tháng (chưa có UC → placeholder)
        private void BtnBaoCaoThang_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            // TODO: Khi có UC_TraCuuBaoCaoThang thì thay vào đây
            ContentArea.Content = new UC_BaoCaoThongKeThang();
          
        }

        // Tab 3: Báo cáo năm
        private void BtnBaoCaoNam_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            ContentArea.Content = new UC_TraCuuBaoCaoNam();
        }

        // Tab 4: Báo cáo khách hàng (chưa có UC → placeholder)
        private void BtnBaoCaoKH_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            // TODO: Khi có UC_TraCuuBaoCaoKhachHang thì thay vào đây
            ContentArea.Content = new TextBlock
            {
                Text = "Báo cáo khách hàng - Đang phát triển",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                Foreground = System.Windows.Media.Brushes.Gray
            };
        }
    }
}