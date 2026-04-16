using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models; // Import namespace chứa MATHANG và QLKhachSan_Model
using ql_ks.ViewModels; // Namespace chứa RelayCommand và BaseViewModel


namespace ql_ks.ViewModels
{
    public class DichVuAnUongViewModel : AnUong_BaseViewModel
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private List<AnUong_HelperViewModel> _allProducts = new List<AnUong_HelperViewModel>();

        private ObservableCollection<AnUong_HelperViewModel> _danhSachMonAn;
        public ObservableCollection<AnUong_HelperViewModel> DanhSachMonAn
        {
            get => _danhSachMonAn;
            set => SetProperty(ref _danhSachMonAn, value);
        }

        private ObservableCollection<DanhSachDaGo> _danhSachDaChon;
        public ObservableCollection<DanhSachDaGo> DanhSachDaChon
        {
            get => _danhSachDaChon;
            set => SetProperty(ref _danhSachDaChon, value);
        }

        private int _maPhong = 0;
        public int MaPhong
        {
            get => _maPhong;
            set
            {
                if (SetProperty(ref _maPhong, value))
                {
                    if (value != 0 && DanhSachDaChon != null)
                        DanhSachDaChon.Clear();

                    CapNhatTongTien();
                }
            }
        }

        private string _tuKhoa = "";
        public string TuKhoaTimKiem
        {
            get => _tuKhoa;
            set
            {
                if (SetProperty(ref _tuKhoa, value))
                {
                    LocDanhSach(value);
                }
            }
        }

        private decimal _tongTien = 0;
        public decimal TongTien
        {
            get => _tongTien;
            set => SetProperty(ref _tongTien, value);
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand SaveCommand { get; }

        public DichVuAnUongViewModel()
        {
            DanhSachMonAn = new ObservableCollection<AnUong_HelperViewModel>();
            DanhSachDaChon = new ObservableCollection<DanhSachDaGo>();

            AddCommand = new AnUong_RelayCommand_T<object>(ThemVaoGioHang);
            RemoveCommand = new AnUong_RelayCommand_T<object>(XoaMonTrenGio);
            SaveCommand = new AnUong_RelayCommand(_ => LuuHoaDon());

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var list = _db.MATHANGs
                    .Select(m => new AnUong_HelperViewModel
                    {
                        Ma_MH = m.Ma_MH,
                        Ten_MH = m.Ten_MH ?? "Không rõ",
                        GiaTien = m.DonGia_MH ?? 0
                    })
                    .ToList();

                _allProducts = list;
                DanhSachMonAn = new ObservableCollection<AnUong_HelperViewModel>(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu món ăn: " + ex.Message);
            }
        }

        private void LocDanhSach(string key)
        {
            if (_allProducts == null) return;

            if (string.IsNullOrWhiteSpace(key))
            {
                DanhSachMonAn = new ObservableCollection<AnUong_HelperViewModel>(_allProducts);
            }
            else
            {
                var lowerKey = key.ToLower();
                var res = _allProducts
                    .Where(x => (x.Ten_MH ?? "").ToLower().Contains(lowerKey))
                    .ToList();

                DanhSachMonAn = new ObservableCollection<AnUong_HelperViewModel>(res);
            }
        }

        private void ThemVaoGioHang(object parameter)
        {
            if (MaPhong == 0)
            {
                MessageBox.Show("Vui lòng chọn số phòng muốn gọi dịch vụ!");
                return;
            }

            var mon = parameter as AnUong_HelperViewModel;
            if (mon == null) return;

            var exists = DanhSachDaChon.FirstOrDefault(x => x.Ma_CTHDAU == mon.Ma_MH);

            if (exists != null)
            {
                exists.SoLuong++;
            }
            else
            {
                DanhSachDaChon.Add(new DanhSachDaGo
                {
                    Ma_CTHDAU = mon.Ma_MH,
                    Ten_MH = mon.Ten_MH,
                    GiaTien = mon.GiaTien,
                    SoLuong = 1
                });
            }

            CapNhatTongTien();
        }

        private void XoaMonTrenGio(object parameter)
        {
            var item = parameter as DanhSachDaGo;
            if (item == null) return;

            DanhSachDaChon.Remove(item);
            CapNhatTongTien();
        }

        private void CapNhatTongTien()
        {
            TongTien = DanhSachDaChon.Sum(x => x.ThanhTien);
        }

        private void LuuHoaDon()
        {
            if (DanhSachDaChon.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống!");
                return;
            }

            MessageBox.Show(
                $"Đã lưu đơn cho phòng {MaPhong}. Tổng tiền: {TongTien:N0}",
                "Thành công");

            DanhSachDaChon.Clear();
            CapNhatTongTien();
        }
    }
}
