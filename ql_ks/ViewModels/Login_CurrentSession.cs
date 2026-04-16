using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ql_ks.Models;

namespace ql_ks.ViewModels
{
    public static class Login_CurrentSession
    {
        // Thuộc tính lưu đối tượng Tài Khoản đăng nhập
        public static TAIKHOAN TaiKhoanDangNhap { get; set; }

        // Hàm kiểm tra xem đã đăng nhập chưa
        public static bool IsLogin => TaiKhoanDangNhap != null;

        // Hàm đăng xuất (xóa thông tin cũ)
        public static void Logout()
        {
            TaiKhoanDangNhap = null;
        }
    }
}