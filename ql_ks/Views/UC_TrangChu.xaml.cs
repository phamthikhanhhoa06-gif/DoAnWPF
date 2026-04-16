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
    /// Interaction logic for UC_TrangChu.xaml
    /// </summary>
    public partial class UC_TrangChu : UserControl
    {
        public UC_TrangChu()
        {
            InitializeComponent();
            // Nếu cần set DataContext cho MainViewModel thì thêm ở đây
            // DataContext = new MainViewModel(); 
        }
        // Sự kiện khi bấm menu "Dịch vụ ăn uống"
        private void MenuAnUong_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Tìm cửa sổ cha (MainWindow) và thông báo chuyển màn hình
            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.ChuyenSangDichVuAnUong();
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var rs = MessageBox.Show("Bạn có chắc muốn đăng xuất?",
                                   "Xác nhận",
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Question);

            if (rs == MessageBoxResult.Yes)
            {
                // Quay về màn hình đăng nhập
                var loginWin = new LoginWindow();
                loginWin.Show();

                // Đóng cửa sổ hiện tại (MainWindow chứa UC này)
                Window.GetWindow(this)?.Close();
                // Hoặc: Application.Current.MainWindow.Close(); nếu MainWindow là main window
            }
        }
    }
}