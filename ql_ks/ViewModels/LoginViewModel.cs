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

            var tk = _db.TAIKHOANs.FirstOrDefault(x =>
                x.TenDangNhap_TK == TenDangNhap &&
                x.MatKhau_TK == MatKhau);

            if (tk != null)
            {
                Login_CurrentSession.TaiKhoanDangNhap = tk;
                ThongBao = "Đăng nhập thành công.";
                DangNhapThanhCong?.Invoke(tk);
            }
            else
            {
                ThongBao = "Sai tên đăng nhập hoặc mật khẩu.";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}