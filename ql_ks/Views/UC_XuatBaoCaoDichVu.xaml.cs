using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using ql_ks.Models;

namespace ql_ks.Views
{
    public partial class UC_XuatBaoCaoDichVu : UserControl
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private double _zoomLevel = 1.0;
        private decimal _luuTru, _anUong, _giatUi, _diChuyen, _tongCong;
        private DateTime? _ngayBatDau;
        private DateTime? _ngayKetThuc;

        // Constructor mới: nhận ngày lọc
        public UC_XuatBaoCaoDichVu(DateTime? ngayBatDau = null, DateTime? ngayKetThuc = null)
        {
            InitializeComponent();
            _ngayBatDau = ngayBatDau;
            _ngayKetThuc = ngayKetThuc;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // ✅ Lọc theo ngày giống UC_BaoCaoThongKeDichVu
                var query = _db.BAOCAODICHVUs.AsQueryable();

                if (_ngayBatDau.HasValue)
                    query = query.Where(x => x.NGAYBATDAU_BCDV >= _ngayBatDau.Value);

                if (_ngayKetThuc.HasValue)
                    query = query.Where(x => x.NGAYKETTHUC_BCDV <= _ngayKetThuc.Value);

                var list = query.ToList();

                _luuTru = list.Sum(x => x.DOANHTHULUUUTRU_BCDV ?? 0);
                _anUong = list.Sum(x => x.DOANHTHUANUONG_BCDV ?? 0);
                _giatUi = list.Sum(x => x.DOANHTHUGIATUI_BCDV ?? 0);
                _diChuyen = list.Sum(x => x.DOANHTHUDICHUYEN_BCDV ?? 0);
                _tongCong = _luuTru + _anUong + _giatUi + _diChuyen;

                // Cập nhật thông tin
                txtNgayLap.Text = $"Ngày lập: {DateTime.Now:dd/MM/yyyy HH:mm}";
                txtThoiGianLap.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                string tuNgay = _ngayBatDau.HasValue ? _ngayBatDau.Value.ToString("dd/MM/yyyy") : "đầu năm";
                string denNgay = _ngayKetThuc.HasValue ? _ngayKetThuc.Value.ToString("dd/MM/yyyy") : "nay";
                txtKhoangThoiGian.Text = $"Từ {tuNgay} đến {denNgay}";

                txtTongCong.Text = $"{_tongCong:N0} VNĐ";

                // Bảng chi tiết
                var chiTiet = new List<BaoCaoItem>
                {
                    new BaoCaoItem { STT = 1, TenDichVu = "Lưu trú", DoanhThu = _luuTru, TyLe = _tongCong > 0 ? _luuTru / _tongCong : 0 },
                    new BaoCaoItem { STT = 2, TenDichVu = "Ăn uống", DoanhThu = _anUong, TyLe = _tongCong > 0 ? _anUong / _tongCong : 0 },
                    new BaoCaoItem { STT = 3, TenDichVu = "Giặt ủi", DoanhThu = _giatUi, TyLe = _tongCong > 0 ? _giatUi / _tongCong : 0 },
                    new BaoCaoItem { STT = 4, TenDichVu = "Di chuyển", DoanhThu = _diChuyen, TyLe = _tongCong > 0 ? _diChuyen / _tongCong : 0 },
                };
                dgChiTiet.ItemsSource = chiTiet;

                // Legend + Biểu đồ
                var data = new List<PieData>
                {
                    new PieData { Label = $"Lưu trú: {_luuTru:N0}", Color = new SolidColorBrush(Color.FromRgb(52, 152, 219)), Value = _luuTru },
                    new PieData { Label = $"Ăn uống: {_anUong:N0}", Color = new SolidColorBrush(Color.FromRgb(46, 204, 113)), Value = _anUong },
                    new PieData { Label = $"Giặt ủi: {_giatUi:N0}", Color = new SolidColorBrush(Color.FromRgb(241, 196, 15)), Value = _giatUi },
                    new PieData { Label = $"Di chuyển: {_diChuyen:N0}", Color = new SolidColorBrush(Color.FromRgb(231, 76, 60)), Value = _diChuyen },
                };
                lstLegend.ItemsSource = data;
                VeBieuDoTron(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void VeBieuDoTron(List<PieData> data)
        {
            canvasBieuDo.Children.Clear();
            if (_tongCong == 0) return;

            double cx = 150, cy = 140, r = 110;
            double startAngle = -90;

            foreach (var item in data)
            {
                if (item.Value <= 0) continue;
                double sweep = (double)(item.Value / _tongCong) * 360;

                var poly = new Polygon
                {
                    Fill = item.Color,
                    Stroke = Brushes.White,
                    StrokeThickness = 2
                };

                var pts = new PointCollection();
                pts.Add(new Point(cx, cy));

                int steps = Math.Max((int)(sweep / 3), 20);
                for (int i = 0; i <= steps; i++)
                {
                    double angle = (startAngle + sweep * i / steps) * Math.PI / 180;
                    pts.Add(new Point(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle)));
                }

                poly.Points = pts;
                canvasBieuDo.Children.Add(poly);
                startAngle += sweep;
            }

            // Vòng tròn trắng ở giữa
            var centerCircle = new Ellipse
            {
                Width = 80,
                Height = 80,
                Fill = Brushes.White,
                Stroke = Brushes.White
            };
            Canvas.SetLeft(centerCircle, cx - 40);
            Canvas.SetTop(centerCircle, cy - 40);
            canvasBieuDo.Children.Add(centerCircle);

            // Text ở giữa
            var centerText = new TextBlock
            {
                Text = "Tổng",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };
            Canvas.SetLeft(centerText, cx - 18);
            Canvas.SetTop(centerText, cy - 18);
            canvasBieuDo.Children.Add(centerText);
        }

        // ========== 1. IN ==========
        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new PrintDialog();
                if (dlg.ShowDialog() == true)
                {
                    var originalTransform = reportContent.LayoutTransform;
                    reportContent.LayoutTransform = new ScaleTransform(0.7, 0.7);

                    var size = new Size(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight);
                    reportContent.Measure(size);
                    reportContent.Arrange(new Rect(new Point(0, 0), size));

                    dlg.PrintVisual(reportContent, "Báo cáo doanh thu dịch vụ");

                    reportContent.LayoutTransform = originalTransform;
                    MessageBox.Show("Đã gửi lệnh in!", "Thành công");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi in: " + ex.Message);
            }
        }

        // ========== 2. LÀM MỚI ==========
        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            _zoomLevel = 1.0;
            reportContent.LayoutTransform = new ScaleTransform(1, 1);
            LoadData();
        }

        // ========== 3. LƯU FILE ==========
        private void BtnLuuFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg",
                    FileName = $"BaoCaoDichVu_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                };

                if (dlg.ShowDialog() == true)
                {
                    var bmp = RenderToBitmap(reportContent, 2.0);

                    if (dlg.FilterIndex == 1)
                    {
                        using (var fs = new FileStream(dlg.FileName, FileMode.Create))
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bmp));
                            encoder.Save(fs);
                        }
                    }
                    else
                    {
                        using (var fs = new FileStream(dlg.FileName, FileMode.Create))
                        {
                            var encoder = new JpegBitmapEncoder();
                            encoder.QualityLevel = 95;
                            encoder.Frames.Add(BitmapFrame.Create(bmp));
                            encoder.Save(fs);
                        }
                    }

                    MessageBox.Show($"Đã lưu tại:\n{dlg.FileName}", "Thành công");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu: " + ex.Message);
            }
        }

        // ========== 4. COPY CLIPBOARD ==========
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bmp = RenderToBitmap(reportContent, 2.0);
                Clipboard.SetImage(bmp);
                MessageBox.Show("Đã copy vào clipboard!", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi copy: " + ex.Message);
            }
        }

        // ========== 5. ZOOM ==========
        private void BtnZoom_Click(object sender, RoutedEventArgs e)
        {
            _zoomLevel += 0.2;
            if (_zoomLevel > 2.0) _zoomLevel = 0.5;
            reportContent.LayoutTransform = new ScaleTransform(_zoomLevel, _zoomLevel);
        }

        // ========== 6. TRANG TRƯỚC / SAU ==========
        private void BtnTrangTruoc_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đây là trang đầu tiên!", "Thông báo");
        }

        private void BtnTrangSau_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đây là trang cuối cùng!", "Thông báo");
        }

        // ========== HELPER: Render to Bitmap ==========
        private BitmapSource RenderToBitmap(FrameworkElement visual, double scale)
        {
            var size = new Size(visual.ActualWidth, visual.ActualHeight);
            if (size.Width == 0 || size.Height == 0)
            {
                visual.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                size = visual.DesiredSize;
            }

            var bmp = new RenderTargetBitmap(
                (int)(size.Width * scale), (int)(size.Height * scale),
                96 * scale, 96 * scale, PixelFormats.Pbgra32);

            var originalTransform = visual.LayoutTransform;
            visual.LayoutTransform = new ScaleTransform(scale, scale);
            bmp.Render(visual);
            visual.LayoutTransform = originalTransform;

            return bmp;
        }
    }

    // ===== MODEL PHỤ =====
    public class BaoCaoItem
    {
        public int STT { get; set; }
        public string TenDichVu { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyLe { get; set; }
    }

    public class PieData
    {
        public string Label { get; set; }
        public Brush Color { get; set; }
        public decimal Value { get; set; }
    }
}