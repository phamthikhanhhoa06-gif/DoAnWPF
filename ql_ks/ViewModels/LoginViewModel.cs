using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ql_ks.Models;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ql_ks.Helpers;   // ✅ THÊM: để dùng PasswordHelper

namespace ql_ks.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private string _tenDangNhap;
        private string _matKhau;
        private string _thongBao;

        public string TenDangNhap
        {
            get => _tenDangNhap;
            set
            {
                _tenDangNhap = value;
                OnPropertyChanged();
            }
        }

        public string MatKhau
        {
            get => _matKhau;
            set
            {
                _matKhau = value;
                OnPropertyChanged();
            }
        }

        public string ThongBao
        {
            get => _thongBao;
            set
            {
                _thongBao = value;
                OnPropertyChanged();
            }
        }

        public ICommand DangNhapCommand { get; }
        public ICommand ThoatCommand { get; }

        public event Action<TAIKHOAN> DangNhapThanhCong;

        public LoginViewModel()
        {
            DangNhapCommand = new Login_RelayCommand(_ => DangNhap());
            ThoatCommand = new Login_RelayCommand(_ => Application.Current.Shutdown());
        }

        private void DangNhap()
        {
            ThongBao = "";

            if (string.IsNullOrWhiteSpace(TenDangNhap) || string.IsNullOrWhiteSpace(MatKhau))
            {
                ThongBao = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return;
            }

            // ✅ Bước 1: Tìm tài khoản theo tên đăng nhập (KHÔNG so mật khẩu trong SQL)
            var tk = _db.TAIKHOANs.FirstOrDefault(x => x.TenDangNhap_TK == TenDangNhap);

            if (tk == null)
            {
                ThongBao = "Sai tên đăng nhập hoặc mật khẩu.";
                return;
            }

            // ✅ Bước 2: Kiểm tra tài khoản có bị khóa không
            if (tk.TrangThai_TK == false)
            {
                ThongBao = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.";
                return;
            }

            // ✅ Bước 3: Xác thực mật khẩu (hỗ trợ cả plaintext cũ lẫn hash mới)
            if (!PasswordHelper.Verify(MatKhau, tk.MatKhau_TK))
            {
                ThongBao = "Sai tên đăng nhập hoặc mật khẩu.";
                return;
            }

            // ✅ Đăng nhập thành công → lưu session
            Login_CurrentSession.TaiKhoanDangNhap = tk;
            ThongBao = "Đăng nhập thành công.";
            DangNhapThanhCong?.Invoke(tk);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}