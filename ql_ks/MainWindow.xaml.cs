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
using ql_ks.Models;
using ql_ks.Views;
namespace ql_ks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new UC_TrangChu(); // mặc định mở trang chủ
        }
        public void ChuyenSangDichVuAnUong()
        {
            MainContent.Content = new uc_DichVuAnUongView();
        }

        public void ChuyenSangTrangChu()
        {
            MainContent.Content = new UC_TrangChu();
        }
        public MainWindow(TAIKHOAN tk) : this()
        {
            this.Title = $"Chào mừng Admin: {tk.TenDangNhap_TK}";
        }

        private void BtnTrangChu_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UC_TrangChu();
        }

        private void BtnDichVuAnUong_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new uc_DichVuAnUongView();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn thoát?", "Thông báo",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var loginWin = new LoginWindow();
                loginWin.Show();
                this.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UC_DichVuGiatUi();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UC_DichVuDiChuyen();
        }
    }
}
