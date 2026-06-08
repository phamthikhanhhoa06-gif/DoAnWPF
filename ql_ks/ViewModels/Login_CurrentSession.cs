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
        // ===== TÀI KHOẢN ĐANG ĐĂNG NHẬP =====
        // Tên gốc (được nhiều file khác sử dụng)
        public static TAIKHOAN TaiKhoanDangNhap { get; set; }

        // Tên mới (alias) - trỏ về cùng dữ liệu với TaiKhoanDangNhap
        public static TAIKHOAN TaiKhoanHienTai
        {
            get => TaiKhoanDangNhap;
            set => TaiKhoanDangNhap = value;
        }

        // ===== TRẠNG THÁI ĐĂNG NHẬP =====
        public static bool IsLogin => TaiKhoanDangNhap != null;

        // ===== VAI TRÒ =====
        public static string VaiTro => TaiKhoanDangNhap?.VaiTro_TK ?? "";

        // Hàm tiện ích kiểm tra quyền
        public static bool LaAdmin => VaiTro == "Admin";
        public static bool LaQuanLy => VaiTro == "QuanLy" || LaAdmin;
        public static bool LaLeTan => VaiTro == "LeTan";
        public static bool LaKeToan => VaiTro == "KeToan";

        // ===== ĐĂNG XUẤT =====
        public static void DangXuat() => TaiKhoanDangNhap = null;
    }
}