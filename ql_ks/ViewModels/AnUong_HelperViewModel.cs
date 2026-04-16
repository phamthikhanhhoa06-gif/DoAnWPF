using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ql_ks.ViewModels; // Để dùng ICommand
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ql_ks.ViewModels
{
    public class AnUong_HelperViewModel
    {
        public int Ma_MH { get; set; }
        public string Ten_MH { get; set; }

        // Lấy giá, nếu NULL thì trả về 0 để tránh lỗi
        public long GiaTien { get; set; }
    }

    /// <summary>
    /// Đối tượng hiển thị cho danh sách Đã gọi bên phải
    /// </summary>
    public class DanhSachDaGo : INotifyPropertyChanged
    {
        public int Ma_CTHDAU { get; set; }
        public string Ten_MH { get; set; }

        private long _giaTien;
        public long GiaTien
        {
            get => _giaTien;
            set
            {
                _giaTien = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        private int _soLuong = 1;
        public int SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value < 1 ? 1 : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public long ThanhTien => GiaTien * SoLuong;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
