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

        public HoaDonWindow(int maPhong)
        {
            InitializeComponent();
            _maPhong = maPhong;
            LoadThongTin();

            // Mặc định và duy nhất: hóa đơn tổng
            MainContent.Content = new UC_HoaDonTong(_maPhong);
        }

        private void LoadThongTin()
        {
            try
            {
                txtTieuDe.Text = "HÓA ĐƠN";
                txtPhongInfo.Text = "Phòng " + _maPhong;
                Title = "Hóa đơn - Phòng " + _maPhong;

                if (Login_CurrentSession.IsLogin && Login_CurrentSession.TaiKhoanDangNhap != null)
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

        private void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}