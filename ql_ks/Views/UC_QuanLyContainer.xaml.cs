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
    /// Interaction logic for UC_QuanLyContainer.xaml
    /// </summary>
    public partial class UC_QuanLyContainer : UserControl
    {
        private Button _currentActiveButton;

        public UC_QuanLyContainer()
        {
            InitializeComponent();

            // Mặc định chọn tab Nhân viên khi mở
            _currentActiveButton = btnNhanVien;
            ContentArea.Content = new UC_TraCuuNhanVien();
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

        private void BtnNhanVien_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            ContentArea.Content = new UC_TraCuuNhanVien();
        }

        private void BtnKhachHang_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            // Chuyển sang trang Khách hàng (cần tạo UC_TraCuuKhachHang)
            ContentArea.Content = new UC_TraCuuKhachHang();
        }

        private void BtnPhong_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            //MessageBox.Show("Trang Quản lý Phòng - Đang phát triển", "Thông báo");
            ContentArea.Content = new UC_TraCuuPhong();
        }

        private void BtnLoaiPhong_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            //MessageBox.Show("Trang Loại Phòng - Đang phát triển", "Thông báo");
            ContentArea.Content = new UC_TraCuuLoaiPhong();
        }

        private void BtnHoaDon_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            //MessageBox.Show("Trang Hóa đơn - Đang phát triển", "Thông báo");
            ContentArea.Content = new UC_TraCuuHoaDon();
        }
        private void BtnHangHoa_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            //MessageBox.Show("Trang Hang hoa - Đang phát triển", "Thông báo");
            ContentArea.Content = new UC_TraCuuMatHang();

        }
        private void BtnLoaiGiat_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            //MessageBox.Show("Trang Loại Giặt ủi - Đang phát triển", "Thông báo");
            ContentArea.Content = new UC_TraCuuLoaiGiatUi();

        }

        private void BtnChuyenDi_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            //MessageBox.Show("Trang Chuyến đi - Đang phát triển", "Thông báo");
            ContentArea.Content = new UC_TraCuuChuyenDi();

        }

        private void BtnBaoCaoDV_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            //MessageBox.Show("Trang Báo cáo dịch vụ - Đang phát triển", "Thông báo");
            ContentArea.Content = new UC_TraCuuBaoCaoDichVu();
        }

        private void BtnBaoCaoNam_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(sender as Button);
            MessageBox.Show("Trang Báo cáo năm - Đang phát triển", "Thông báo");
        }
    }
}
