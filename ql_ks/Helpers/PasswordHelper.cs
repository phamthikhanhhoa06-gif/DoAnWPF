using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ql_ks.Helpers
{
    public static class PasswordHelper
    {
        // Hash mật khẩu bằng SHA256
        public static string Hash(string matKhau)
        {
            if (matKhau == null) matKhau = "";
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(matKhau));
                var sb = new StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // So sánh mật khẩu nhập với giá trị trong DB
        // Tương thích ngược: nếu DB còn lưu plaintext (độ dài < 64) thì so sánh trực tiếp
        public static bool Verify(string matKhauNhap, string giaTriTrongDB)
        {
            if (giaTriTrongDB == null) return false;

            // Hash SHA256 luôn dài đúng 64 ký tự hex
            if (giaTriTrongDB.Length < 64)
                return matKhauNhap == giaTriTrongDB;   // dữ liệu cũ plaintext

            return Hash(matKhauNhap) == giaTriTrongDB;  // dữ liệu đã hash
        }
    }
}