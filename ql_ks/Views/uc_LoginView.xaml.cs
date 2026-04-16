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
using ql_ks.ViewModels; // Namespace của ViewModel
using ql_ks.Models; // Namespace của Model
using ql_ks;
namespace ql_ks.Views
{
    /// <summary>
    /// Interaction logic for uc_LoginView.xaml
    /// </summary>
    public partial class uc_LoginView : UserControl
    {
        private readonly LoginViewModel _viewModel;

        public uc_LoginView()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            // Gắn sự kiện
            if (_viewModel != null)
            {
                _viewModel.DangNhapThanhCong += OnLoginSuccess;
            }
        }

        private void PssPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.MatKhau = pssPassword.Password;
        }

        private void OnLoginSuccess(TAIKHOAN taiKhoan)
        {
            try
            {
                // Nếu đăng nhập thành công thì mở MainWindow
                var mainWindow = new MainWindow(taiKhoan);
                mainWindow.Show();

                // Đóng cửa sổ LoginWindow đang chứa UserControl này
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở cửa sổ chính: " + ex.Message);
            }
        }

        // Xử lý bấm nút ĐĂNG KÝ
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Màn hình Đăng ký đang được xây dựng!\n\nBạn có thể dùng thử tài khoản:\n• admin / 123456\n• le_tan_01 / password",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

            // TODO: Sau này bạn mở màn hình Đăng ký như sau:
            // var regWin = new RegisterWindow(); // Cần tạo file này
            // regWin.Show();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}