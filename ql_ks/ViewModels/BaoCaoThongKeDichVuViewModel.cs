using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using LiveCharts;
using LiveCharts.Wpf;
using ql_ks.Models;
using static System.Net.Mime.MediaTypeNames;

namespace ql_ks.ViewModels
{
    public class BaoCaoThongKeDichVuViewModel : INotifyPropertyChanged
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();

        private DateTime? _ngayBatDau;
        public DateTime? NgayBatDau
        {
            get => _ngayBatDau;
            set { _ngayBatDau = value; OnPropertyChanged(); }
        }

        private DateTime? _ngayKetThuc;
        public DateTime? NgayKetThuc
        {
            get => _ngayKetThuc;
            set { _ngayKetThuc = value; OnPropertyChanged(); }
        }

        private decimal _tongDoanhThu;
        public decimal TongDoanhThu
        {
            get => _tongDoanhThu;
            set { _tongDoanhThu = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ChiTietDichVu> ChiTietList { get; set; }

        private SeriesCollection _pieSeriesCollection;
        public SeriesCollection PieSeriesCollection
        {
            get => _pieSeriesCollection;
            set { _pieSeriesCollection = value; OnPropertyChanged(); }
        }

        public ICommand TimKiemCommand { get; }

        public BaoCaoThongKeDichVuViewModel()
        {
            ChiTietList = new ObservableCollection<ChiTietDichVu>();
            PieSeriesCollection = new SeriesCollection();

            // Mặc định: từ đầu năm đến hiện tại
            NgayBatDau = new DateTime(DateTime.Now.Year, 1, 1);
            NgayKetThuc = DateTime.Now;

            TimKiemCommand = new RelayCommand(_ => LoadData());

            LoadData();
        }

        public void LoadData()
        {
            try
            {
                var query = _db.BAOCAODICHVUs.AsQueryable();

                if (NgayBatDau.HasValue)
                    query = query.Where(x => x.NGAYBATDAU_BCDV >= NgayBatDau.Value);

                if (NgayKetThuc.HasValue)
                    query = query.Where(x => x.NGAYKETTHUC_BCDV <= NgayKetThuc.Value);

                var list = query.ToList();

                // Tính tổng từng loại
                decimal luuTru = list.Sum(x => x.DOANHTHULUUUTRU_BCDV ?? 0);
                decimal anUong = list.Sum(x => x.DOANHTHUANUONG_BCDV ?? 0);
                decimal giatUi = list.Sum(x => x.DOANHTHUGIATUI_BCDV ?? 0);
                decimal diChuyen = list.Sum(x => x.DOANHTHUDICHUYEN_BCDV ?? 0);

                TongDoanhThu = luuTru + anUong + giatUi + diChuyen;

                // Cập nhật bảng chi tiết
                ChiTietList.Clear();
                ChiTietList.Add(new ChiTietDichVu { TenDichVu = "Lưu trú", DoanhThu = luuTru, TyLe = TongDoanhThu > 0 ? luuTru / TongDoanhThu : 0 });
                ChiTietList.Add(new ChiTietDichVu { TenDichVu = "Ăn uống", DoanhThu = anUong, TyLe = TongDoanhThu > 0 ? anUong / TongDoanhThu : 0 });
                ChiTietList.Add(new ChiTietDichVu { TenDichVu = "Giặt ủi", DoanhThu = giatUi, TyLe = TongDoanhThu > 0 ? giatUi / TongDoanhThu : 0 });
                ChiTietList.Add(new ChiTietDichVu { TenDichVu = "Di chuyển", DoanhThu = diChuyen, TyLe = TongDoanhThu > 0 ? diChuyen / TongDoanhThu : 0 });

                // Cập nhật PieChart
                PieSeriesCollection.Clear();

                if (luuTru > 0)
                    PieSeriesCollection.Add(new PieSeries
                    {
                        Title = "Lưu trú",
                        Values = new ChartValues<decimal> { luuTru },
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y:N0}"
                    });

                if (anUong > 0)
                    PieSeriesCollection.Add(new PieSeries
                    {
                        Title = "Ăn uống",
                        Values = new ChartValues<decimal> { anUong },
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y:N0}"
                    });

                if (giatUi > 0)
                    PieSeriesCollection.Add(new PieSeries
                    {
                        Title = "Giặt ủi",
                        Values = new ChartValues<decimal> { giatUi },
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y:N0}"
                    });

                if (diChuyen > 0)
                    PieSeriesCollection.Add(new PieSeries
                    {
                        Title = "Di chuyển",
                        Values = new ChartValues<decimal> { diChuyen },
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y:N0}"
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Model phụ cho bảng chi tiết
    public class ChiTietDichVu
    {
        public string TenDichVu { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyLe { get; set; } // 0.00 - 1.00
    }

    // RelayCommand nội bộ
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}