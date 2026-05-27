using ql_ks.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ql_ks.ViewModels
{
    public class QuanLyPhongViewModel : Common_BaseViewModel
    {
        private ObservableCollection<PHONG> _listPhong;
        public ObservableCollection<PHONG> ListPhong
        {
            get => _listPhong;
            set { _listPhong = value; OnPropertyChanged(); }
        }

        private ObservableCollection<LOAIPHONG> _listLoaiPhong;
        public ObservableCollection<LOAIPHONG> ListLoaiPhong
        {
            get => _listLoaiPhong;
            set { _listLoaiPhong = value; OnPropertyChanged(); }
        }

        private PHONG _selectedPhong;
        public PHONG SelectedPhong
        {
            get => _selectedPhong;
            set
            {
                _selectedPhong = value;
                OnPropertyChanged();
                if (_selectedPhong != null)
                {
                    MaPhong = _selectedPhong.Ma_Phong;
                    TinhTrangPhong = _selectedPhong.TinhTrang_Phong;
                    SelectedLoaiPhong = ListLoaiPhong.FirstOrDefault(x => x.Ma_LP == _selectedPhong.Ma_LP);
                }
            }
        }

        private int _maPhong;
        public int MaPhong
        {
            get => _maPhong;
            set { _maPhong = value; OnPropertyChanged(); }
        }

        private string _tinhTrangPhong;
        public string TinhTrangPhong
        {
            get => _tinhTrangPhong;
            set { _tinhTrangPhong = value; OnPropertyChanged(); }
        }

        private LOAIPHONG _selectedLoaiPhong;
        public LOAIPHONG SelectedLoaiPhong
        {
            get => _selectedLoaiPhong;
            set { _selectedLoaiPhong = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public QuanLyPhongViewModel()
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
                ListLoaiPhong = new ObservableCollection<LOAIPHONG>(db.LOAIPHONGs.ToList());
                ListPhong = new ObservableCollection<PHONG>(db.PHONGs.Include("LOAIPHONG").ToList());
            }
        }

        private bool CanAdd(object obj)
        {
            return MaPhong > 0 && SelectedLoaiPhong != null && !string.IsNullOrEmpty(TinhTrangPhong);
        }

        private void Add(object obj)
        {
            using (var db = new QLKhachSan_Model())
            {
                if (db.PHONGs.Any(p => p.Ma_Phong == MaPhong))
                {
                    MessageBox.Show("Mã phòng đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newPhong = new PHONG
                {
                    Ma_Phong = MaPhong,
                    Ma_LP = SelectedLoaiPhong.Ma_LP,
                    TinhTrang_Phong = TinhTrangPhong
                };
                db.PHONGs.Add(newPhong);
                db.SaveChanges();
            }
            LoadData();
            Clear(null);
            MessageBox.Show("Thêm phòng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanEdit(object obj)
        {
            return SelectedPhong != null && MaPhong > 0 && SelectedLoaiPhong != null && !string.IsNullOrEmpty(TinhTrangPhong);
        }

        private void Edit(object obj)
        {
            using (var db = new QLKhachSan_Model())
            {
                var phong = db.PHONGs.Find(MaPhong);
                if (phong == null)
                {
                    MessageBox.Show("Không tìm thấy phòng để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                phong.Ma_LP = SelectedLoaiPhong.Ma_LP;
                phong.TinhTrang_Phong = TinhTrangPhong;
                db.SaveChanges();
            }
            LoadData();
            MessageBox.Show("Cập nhật phòng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanDelete(object obj)
        {
            return SelectedPhong != null;
        }

        private void Delete(object obj)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa phòng này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var db = new QLKhachSan_Model())
                {
                    var phong = db.PHONGs.Find(MaPhong);
                    if (phong != null)
                    {
                        db.PHONGs.Remove(phong);
                        db.SaveChanges();
                    }
                }
                LoadData();
                Clear(null);
                MessageBox.Show("Xóa phòng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Clear(object obj)
        {
            SelectedPhong = null;
            MaPhong = 0;
            TinhTrangPhong = "Trống";
            SelectedLoaiPhong = null;
        }
    }
}
