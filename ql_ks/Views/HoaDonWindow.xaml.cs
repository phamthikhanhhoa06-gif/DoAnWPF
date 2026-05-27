using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ql_ks.Models;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class HoaDonWindow : Window
    {
        private int _maPhong;
        private Button _activeButton;

        public HoaDonWindow(int maPhong)
        {
            InitializeComponent();

            _maPhong = maPhong;

            LoadThongTin();

            // Mặc định mở hóa đơn tổng
            SetActiveButton(btnHoaDonTong);
            MainContent.Content = new UC_HoaDonTong(_maPhong);
        }

        // ================= LOAD THÔNG TIN =================

        private void LoadThongTin()
        {
            try
            {
                txtTieuDe.Text = "HÓA ĐƠN";
                txtPhongInfo.Text = "Phòng " + _maPhong;
                Title = "Hóa đơn - Phòng " + _maPhong;

                if (Login_CurrentSession.IsLogin &&
                    Login_CurrentSession.TaiKhoanDangNhap != null)
                {
                    int maTK = Login_CurrentSession.TaiKhoanDangNhap.Ma_TK;

                    using (var db = new QLKhachSan_Model())
                    {
                        var nv = db.NHANVIENs.FirstOrDefault(n => n.Ma_TK == maTK);

                        if (nv != null)
                        {
                            txtNhanVien.Text = nv.HoTen_NV;
                            txtChucVu.Text = nv.ChucVu_NV ?? "Nhân viên";
                        }
                        else
                        {
                            txtNhanVien.Text = Login_CurrentSession.TaiKhoanDangNhap.TenDangNhap_TK;
                            txtChucVu.Text = "Admin";
                        }
                    }
                }
                else
                {
                    txtNhanVien.Text = "Chưa đăng nhập";
                    txtChucVu.Text = "";
                }
            }
            catch
            {
                txtNhanVien.Text = "N/A";
                txtChucVu.Text = "";
            }
        }

        // ================= ACTIVE BUTTON =================

        private void SetActiveButton(Button btn)
        {
            ResetButton(btnHoaDonTong);
            ResetButton(btnHoaDonLuuTru);
            ResetButton(btnHoaDonAnUong);
            ResetButton(btnHoaDonGiatUi);
            ResetButton(btnHoaDonDiChuyen);

            if (btn != null)
            {
                btn.Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // #3498DB
                btn.Foreground = Brushes.White;
                btn.FontWeight = FontWeights.Bold;
                _activeButton = btn;
            }
        }

        private void ResetButton(Button btn)
        {
            if (btn == null)
                return;

            btn.Background = Brushes.Transparent;
            btn.Foreground = new SolidColorBrush(Color.FromRgb(189, 195, 199)); // #BDC3C7
            btn.FontWeight = FontWeights.Normal;
        }

        // ================= 5 HÀM CLICK ĐANG BỊ THIẾU =================

        private void BtnHoaDonTong_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnHoaDonTong);
            MainContent.Content = new UC_HoaDonTong(_maPhong);
        }

        private void BtnHoaDonLuuTru_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnHoaDonLuuTru);
            MainContent.Content = new UC_HoaDonLuuTru(_maPhong);
        }

        private void BtnHoaDonAnUong_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnHoaDonAnUong);
            MainContent.Content = new UC_HoaDonAnUong(_maPhong);
        }

        private void BtnHoaDonGiatUi_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnHoaDonGiatUi);
            MainContent.Content = new UC_HoaDonGiatUi(_maPhong);
        }

        private void BtnHoaDonDiChuyen_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnHoaDonDiChuyen);
            MainContent.Content = new UC_HoaDonDiChuyen(_maPhong);
        }

        private void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}