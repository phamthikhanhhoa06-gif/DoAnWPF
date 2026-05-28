using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ql_ks.Models;

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

        // ==== MÃ PHÒNG (đổi sang string để validate) ====
        private string _maPhong = "";
        public string MaPhong
        {
            get => _maPhong;
            set
            {
                if (SetProperty(ref _maPhong, value))
                {
                    ValidateMaPhong();
                }
            }
        }

        // ==== LỖI MÃ PHÒNG ====
        private string _loiMaPhong;
        public string LoiMaPhong
        {
            get => _loiMaPhong;
            set => SetProperty(ref _loiMaPhong, value);
        }

        private bool _coLoiMaPhong;
        public bool CoLoiMaPhong
        {
            get => _coLoiMaPhong;
            set => SetProperty(ref _coLoiMaPhong, value);
        }

        // ==== TÌM KIẾM ====
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

        // ==== LỖI TÌM KIẾM ====
        private string _loiTimKiem;
        public string LoiTimKiem
        {
            get => _loiTimKiem;
            set => SetProperty(ref _loiTimKiem, value);
        }

        private bool _coLoiTimKiem;
        public bool CoLoiTimKiem
        {
            get => _coLoiTimKiem;
            set => SetProperty(ref _coLoiTimKiem, value);
        }

        private decimal _tongTien = 0;
        public decimal TongTien
        {
            get => _tongTien;
            set => SetProperty(ref _tongTien, value);
        }

        private string _thongBao;
        public string ThongBao
        {
            get => _thongBao;
            set => SetProperty(ref _thongBao, value);
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
            ValidateMaPhong(); // Validate ban đầu
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

        // ==== VALIDATE MÃ PHÒNG ====
        private void ValidateMaPhong()
        {
            // Trống
            if (string.IsNullOrWhiteSpace(MaPhong))
            {
                LoiMaPhong = "⚠ Bắt buộc nhập mã phòng!";
                CoLoiMaPhong = true;
                DanhSachDaChon?.Clear();
                CapNhatTongTien();
                return;
            }

            // Quá 3 ký tự
            if (MaPhong.Length > 3)
            {
                LoiMaPhong = "⚠ Mã phòng không quá 3 ký tự!";
                CoLoiMaPhong = true;
                DanhSachDaChon?.Clear();
                CapNhatTongTien();
                return;
            }

            // Không phải số
            int maPhongInt;
            if (!int.TryParse(MaPhong, out maPhongInt))
            {
                LoiMaPhong = "⚠ Mã phòng phải là số!";
                CoLoiMaPhong = true;
                DanhSachDaChon?.Clear();
                CapNhatTongTien();
                return;
            }

            // Không tồn tại trong DB
            try
            {
                bool tonTai = _db.PHONGs.Any(p => p.Ma_Phong == maPhongInt);
                if (!tonTai)
                {
                    LoiMaPhong = "⚠ Mã phòng không tồn tại trong hệ thống!";
                    CoLoiMaPhong = true;
                    DanhSachDaChon?.Clear();
                    CapNhatTongTien();
                    return;
                }
            }
            catch (Exception ex)
            {
                LoiMaPhong = "⚠ Lỗi kiểm tra phòng: " + ex.Message;
                CoLoiMaPhong = true;
                return;
            }

            // OK
            LoiMaPhong = "";
            CoLoiMaPhong = false;
        }

        // ==== LỌC + VALIDATE TÌM KIẾM ====
        private void LocDanhSach(string key)
        {
            if (_allProducts == null) return;

            if (string.IsNullOrWhiteSpace(key))
            {
                DanhSachMonAn = new ObservableCollection<AnUong_HelperViewModel>(_allProducts);
                LoiTimKiem = "";
                CoLoiTimKiem = false;
            }
            else
            {
                var lowerKey = key.ToLower();
                var res = _allProducts
                    .Where(x => (x.Ten_MH ?? "").ToLower().Contains(lowerKey))
                    .ToList();

                DanhSachMonAn = new ObservableCollection<AnUong_HelperViewModel>(res);

                if (res.Count == 0)
                {
                    LoiTimKiem = "⚠ Không tìm thấy mặt hàng \"" + key + "\"!";
                    CoLoiTimKiem = true;
                }
                else
                {
                    LoiTimKiem = "";
                    CoLoiTimKiem = false;
                }
            }
        }

        private void ThemVaoGioHang(object parameter)
        {
            // Validate lại trước khi thêm
            ValidateMaPhong();
            if (CoLoiMaPhong)
            {
                MessageBox.Show(LoiMaPhong, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            TongTien = DanhSachDaChon?.Sum(x => x.ThanhTien) ?? 0;
        }

        private void LuuHoaDon()
        {
            ValidateMaPhong();
            if (CoLoiMaPhong)
            {
                MessageBox.Show(LoiMaPhong, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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