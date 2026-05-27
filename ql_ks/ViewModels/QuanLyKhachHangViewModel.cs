using ql_ks.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ql_ks.ViewModels
{
    public class QuanLyKhachHangViewModel : Common_BaseViewModel
    {
        private ObservableCollection<KHACHHANG> _listKhachHang;
        public ObservableCollection<KHACHHANG> ListKhachHang
        {
            get => _listKhachHang;
            set { _listKhachHang = value; OnPropertyChanged(); }
        }

        private KHACHHANG _selectedKhachHang;
        public KHACHHANG SelectedKhachHang
        {
            get => _selectedKhachHang;
            set
            {
                _selectedKhachHang = value;
                OnPropertyChanged();
                if (_selectedKhachHang != null)
                {
                    MaKH = _selectedKhachHang.MA_KH;
                    HoTenKH = _selectedKhachHang.HoTen_KH;
                    SDTKH = _selectedKhachHang.SoDienThoai_KH;
                    CMNDKH = _selectedKhachHang.CMND_KH;
                }
            }
        }

        private int _maKH;
        public int MaKH
        {
            get => _maKH;
            set { _maKH = value; OnPropertyChanged(); }
        }

        private string _hoTenKH;
        public string HoTenKH
        {
            get => _hoTenKH;
            set { _hoTenKH = value; OnPropertyChanged(); }
        }

        private string _sdtKH;
        public string SDTKH
        {
            get => _sdtKH;
            set { _sdtKH = value; OnPropertyChanged(); }
        }

        private string _cmndKH;
        public string CMNDKH
        {
            get => _cmndKH;
            set { _cmndKH = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public QuanLyKhachHangViewModel()
        {
            LoadData();

            AddCommand = new Common_RelayCommand(Add, CanAdd);
            EditCommand = new Common_RelayCommand(Edit, CanEdit);
            DeleteCommand = new Common_RelayCommand(Delete, CanDelete);
            ClearCommand = new Common_RelayCommand(Clear);
        }

        private void LoadData()
        {
            using (var db = new QLKhachSan_Model())
            {
                ListKhachHang = new ObservableCollection<KHACHHANG>(db.KHACHHANGs.ToList());
            }
        }

        private bool CanAdd(object obj)
        {
            return MaKH > 0 && !string.IsNullOrEmpty(HoTenKH) && !string.IsNullOrEmpty(SDTKH);
        }

        private void Add(object obj)
        {
            using (var db = new QLKhachSan_Model())
            {
                if (db.KHACHHANGs.Any(p => p.MA_KH == MaKH))
                {
                    MessageBox.Show("Mã khách hàng này đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var kh = new KHACHHANG
                {
                    MA_KH = MaKH,
                    HoTen_KH = HoTenKH,
                    SoDienThoai_KH = SDTKH,
                    CMND_KH = CMNDKH
                };
                db.KHACHHANGs.Add(kh);
                db.SaveChanges();
            }
            LoadData();
            Clear(null);
            MessageBox.Show("Thêm Khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanEdit(object obj)
        {
            return SelectedKhachHang != null && MaKH > 0 && !string.IsNullOrEmpty(HoTenKH) && !string.IsNullOrEmpty(SDTKH);
        }

        private void Edit(object obj)
        {
            using (var db = new QLKhachSan_Model())
            {
                var kh = db.KHACHHANGs.Find(MaKH);
                if (kh == null)
                {
                    MessageBox.Show("Không tìm thấy khách hàng để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                kh.HoTen_KH = HoTenKH;
                kh.SoDienThoai_KH = SDTKH;
                kh.CMND_KH = CMNDKH;
                db.SaveChanges();
            }
            LoadData();
            MessageBox.Show("Cập nhật khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanDelete(object obj)
        {
            return SelectedKhachHang != null;
        }

        private void Delete(object obj)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa khách hàng này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var db = new QLKhachSan_Model())
                {
                    var kh = db.KHACHHANGs.Find(MaKH);
                    if (kh != null)
                    {
                        db.KHACHHANGs.Remove(kh);
                        db.SaveChanges();
                    }
                }
                LoadData();
                Clear(null);
                MessageBox.Show("Xóa khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Clear(object obj)
        {
            SelectedKhachHang = null;
            MaKH = 0;
            HoTenKH = string.Empty;
            SDTKH = string.Empty;
            CMNDKH = string.Empty;
        }
    }
}
