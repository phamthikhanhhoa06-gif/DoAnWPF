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
            // ✅ Hiển thị thông tin người đăng nhập
            HienThiThongTinNguoiDung(tk);

            // ✅ Áp dụng phân quyền ẩn/hiện menu
            ApDungPhanQuyen();
        }

        // ✅ Hiển thị tên nhân viên + vai trò lên sidebar và title
        private void HienThiThongTinNguoiDung(TAIKHOAN tk)
        {
            string hoTen = tk.TenDangNhap_TK;
            try
            {
                using (var db = new QLKhachSan_Model())
                {
                    var nv = db.NHANVIENs.FirstOrDefault(x => x.Ma_TK == tk.Ma_TK);
                    if (nv != null && !string.IsNullOrWhiteSpace(nv.HoTen_NV))
                        hoTen = nv.HoTen_NV;
                }
            }
            catch { /* nếu lỗi vẫn dùng tên đăng nhập */ }

            this.Title = $"Quản Lý Khách Sạn - {hoTen} ({tk.VaiTro_TK})";
            if (txtUserInfo != null)
                txtUserInfo.Text = $"👤 {hoTen}\n({tk.VaiTro_TK})";
        }

        // ✅ Ẩn/hiện menu theo vai trò
        private void ApDungPhanQuyen()
        {
            switch (Login_CurrentSession.VaiTro)
            {
                case "LeTan":
                    // Lễ tân: chỉ làm dịch vụ, ẩn quản lý + báo cáo
                    menuTraCuuQuanLy.Visibility = Visibility.Collapsed;
                    menuBaoCao.Visibility = Visibility.Collapsed;
                    break;

                case "KeToan":
                    // Kế toán: chỉ xem báo cáo, ẩn các dịch vụ
                    menuDichVuAnUong.Visibility = Visibility.Collapsed;
                    menuDichVuGiatUi.Visibility = Visibility.Collapsed;
                    menuDichVuDiChuyen.Visibility = Visibility.Collapsed;
                    break;

                case "QuanLy":
                case "Admin":
                    // Thấy tất cả → không ẩn gì
                    break;

                default:
                    // Vai trò lạ → chỉ cho xem trang chủ cho an toàn
                    menuDichVuAnUong.Visibility = Visibility.Collapsed;
                    menuDichVuGiatUi.Visibility = Visibility.Collapsed;
                    menuDichVuDiChuyen.Visibility = Visibility.Collapsed;
                    menuTraCuuQuanLy.Visibility = Visibility.Collapsed;
                    menuBaoCao.Visibility = Visibility.Collapsed;
                    break;
            }
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
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Thông báo",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // ✅ Xóa session khi đăng xuất
                Login_CurrentSession.DangXuat();

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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UC_QuanLyContainer();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UC_QuanLyContainer2();
        }
    }
}