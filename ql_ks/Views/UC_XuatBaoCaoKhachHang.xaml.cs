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
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_XuatBaoCaoKhachHang : UserControl
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private DateTime _tuNgay;
        private DateTime _denNgay;
        private double _currentZoom = 1.0;

        public UC_XuatBaoCaoKhachHang(DateTime tuNgay, DateTime denNgay)
        {
            InitializeComponent();
            _tuNgay = tuNgay;
            _denNgay = denNgay;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                DateTime tuNgay = _tuNgay.Date;
                DateTime denNgay = _denNgay.Date.AddDays(1);

                var query = _db.HOADONs
                    .Where(hd => hd.ThoiGianLap_HD >= tuNgay
                              && hd.ThoiGianLap_HD < denNgay
                              && hd.MA_KH != null
                              && hd.TriGia_HD != null)
                    .GroupBy(hd => new
                    {
                        hd.MA_KH,
                        hd.KHACHHANG.HoTen_KH,
                        hd.KHACHHANG.SoDienThoai_KH
                    })
                    .Select(g => new
                    {
                        MaKH = g.Key.MA_KH,
                        HoTen = g.Key.HoTen_KH,
                        SoDienThoai = g.Key.SoDienThoai_KH,
                        TongTien = g.Sum(x => x.TriGia_HD ?? 0),
                        SoHoaDon = g.Count()
                    })
                    .OrderByDescending(x => x.TongTien)
                    .ToList();

                decimal tongDoanhThu = query.Sum(x => (decimal)x.TongTien);

                // Cập nhật thông tin header
                txtNgayLap.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                txtTuNgay.Text = _tuNgay.ToString("dd/MM/yyyy");
                txtDenNgay.Text = _denNgay.ToString("dd/MM/yyyy");
                txtMaBC.Text = "BC-KH-" + DateTime.Now.ToString("yyyyMMdd");
                txtTongKH.Text = query.Count.ToString() + " khách hàng";
                txtTongCong.Text = tongDoanhThu.ToString("N0") + " VNĐ";

                // Tạo danh sách bảng
                var list = new List<XuatKhachHangItem>();
                int stt = 0;
                foreach (var item in query)
                {
                    stt++;
                    decimal dt = (decimal)item.TongTien;
                    list.Add(new XuatKhachHangItem
                    {
                        STT = stt,
                        MaKH = item.MaKH ?? 0,
                        HoTen = item.HoTen ?? "N/A",
                        SoDienThoai = item.SoDienThoai ?? "N/A",
                        SoHoaDon = item.SoHoaDon,
                        DoanhThu = dt,
                        TyLe = tongDoanhThu > 0 ? dt / tongDoanhThu : 0
                    });
                }
                dgChiTiet.ItemsSource = list;

                // Tạo biểu đồ tròn
                TaoBieuDoTron(query, tongDoanhThu);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void TaoBieuDoTron(dynamic queryResult, decimal tongDoanhThu)
        {
            Brush[] colors = new Brush[]
            {
                new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                new SolidColorBrush(Color.FromRgb(241, 196, 15)),
                new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                new SolidColorBrush(Color.FromRgb(26, 188, 156)),
                new SolidColorBrush(Color.FromRgb(230, 126, 34)),
                new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                new SolidColorBrush(Color.FromRgb(22, 160, 133)),
                new SolidColorBrush(Color.FromRgb(192, 57, 43)),
            };

            // Tạo dữ liệu cho PieChart
            var pieData = new List<PieSliceData>();
            var items = ((IEnumerable<dynamic>)queryResult).Take(10).ToList();
            decimal tongTop = 0;

            for (int i = 0; i < items.Count; i++)
            {
                decimal dt = (decimal)items[i].TongTien;
                tongTop += dt;
                double tyLe = tongDoanhThu > 0 ? (double)(dt / tongDoanhThu) : 0;

                pieData.Add(new PieSliceData
                {
                    Label = items[i].HoTen ?? "N/A",
                    Angle = tyLe * 360,
                    Percentage = tyLe,
                    Fill = colors[i % colors.Length],
                    Value = dt
                });
            }

            // Phần "Khác"
            decimal doanhThuKhac = tongDoanhThu - tongTop;
            if (doanhThuKhac > 0)
            {
                double tyLeKhac = (double)(doanhThuKhac / tongDoanhThu);
                pieData.Add(new PieSliceData
                {
                    Label = "Khác",
                    Angle = tyLeKhac * 360,
                    Percentage = tyLeKhac,
                    Fill = new SolidColorBrush(Color.FromRgb(189, 195, 199)),
                    Value = doanhThuKhac
                });
            }

            pieChart.ItemsSource = pieData;

            // Tạo chú thích (legend)
            TaoLegend(pieData);
        }

        private void TaoLegend(List<PieSliceData> pieData)
        {
            legendPanel.Children.Clear();

            foreach (var item in pieData)
            {
                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 2, 0, 2)
                };

                // Ô màu
                var colorBox = new Border
                {
                    Width = 12,
                    Height = 12,
                    Background = item.Fill,
                    CornerRadius = new CornerRadius(2),
                    Margin = new Thickness(0, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Tên + tỷ lệ
                var label = new TextBlock
                {
                    Text = $"{item.Label} ({item.Percentage:P1})",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 180
                };

                row.Children.Add(colorBox);
                row.Children.Add(label);
                legendPanel.Children.Add(row);
            }
        }

        // ============ NÚT 1: IN BÁO CÁO ============
        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new PrintDialog();
                if (dlg.ShowDialog() == true)
                {
                    // Lưu transform hiện tại
                    var savedZoom = _currentZoom;

                    // Reset zoom về 1 để in đúng kích thước
                    reportScale.ScaleX = 0.75;
                    reportScale.ScaleY = 0.75;

                    var size = new Size(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight);
                    reportContent.Measure(size);
                    reportContent.Arrange(new Rect(new Point(0, 0), size));

                    dlg.PrintVisual(reportContent,
                        $"Báo cáo KH {_tuNgay:dd/MM/yyyy} - {_denNgay:dd/MM/yyyy}");

                    // Khôi phục zoom
                    reportScale.ScaleX = savedZoom;
                    reportScale.ScaleY = savedZoom;

                    MessageBox.Show("Đã gửi lệnh in thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi in: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============ NÚT 2: LÀM MỚI ============
        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            _currentZoom = 1.0;
            reportScale.ScaleX = 1;
            reportScale.ScaleY = 1;
            txtZoomLevel.Text = "100%";
            LoadData();
            MessageBox.Show("Đã làm mới báo cáo!", "Thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ============ NÚT 3: LƯU FILE ============
        private void BtnLuuFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg|BMP Image|*.bmp",
                    FileName = $"BaoCao_KhachHang_{_tuNgay:yyyyMMdd}_{_denNgay:yyyyMMdd}",
                    Title = "Lưu báo cáo"
                };

                if (dlg.ShowDialog() == true)
                {
                    // Lưu zoom hiện tại, reset về 1 để lưu rõ nét
                    var savedZoom = _currentZoom;
                    reportScale.ScaleX = 1;
                    reportScale.ScaleY = 1;

                    reportContent.UpdateLayout();

                    var bmp = RenderToBitmap(reportContent, 2.0);

                    BitmapEncoder encoder;
                    switch (dlg.FilterIndex)
                    {
                        case 2:
                            encoder = new JpegBitmapEncoder { QualityLevel = 95 };
                            break;
                        case 3:
                            encoder = new BmpBitmapEncoder();
                            break;
                        default:
                            encoder = new PngBitmapEncoder();
                            break;
                    }

                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    using (var fs = new FileStream(dlg.FileName, FileMode.Create))
                        encoder.Save(fs);

                    // Khôi phục zoom
                    reportScale.ScaleX = savedZoom;
                    reportScale.ScaleY = savedZoom;

                    MessageBox.Show("Đã lưu báo cáo tại:\n" + dlg.FileName, "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu file: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============ NÚT 4: COPY CLIPBOARD ============
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savedZoom = _currentZoom;
                reportScale.ScaleX = 1;
                reportScale.ScaleY = 1;
                reportContent.UpdateLayout();

                var bmp = RenderToBitmap(reportContent, 2.0);
                Clipboard.SetImage(bmp);

                reportScale.ScaleX = savedZoom;
                reportScale.ScaleY = savedZoom;

                MessageBox.Show("Đã copy báo cáo vào clipboard!\nBạn có thể dán (Ctrl+V) vào Word, Email...",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi copy: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============ NÚT 5: TÙY CHỈNH GÓC NHÌN (ZOOM) ============
        private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentZoom < 2.0)
            {
                _currentZoom += 0.1;
                ApplyZoom();
            }
        }

        private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (_currentZoom > 0.5)
            {
                _currentZoom -= 0.1;
                ApplyZoom();
            }
        }

        private void BtnZoomReset_Click(object sender, RoutedEventArgs e)
        {
            _currentZoom = 1.0;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            reportScale.ScaleX = _currentZoom;
            reportScale.ScaleY = _currentZoom;
            txtZoomLevel.Text = $"{(int)(_currentZoom * 100)}%";
        }

        // ============ RENDER TO BITMAP ============
        private BitmapSource RenderToBitmap(FrameworkElement visual, double scale)
        {
            var size = new Size(visual.ActualWidth, visual.ActualHeight);
            if (size.Width == 0 || size.Height == 0)
            {
                visual.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                size = visual.DesiredSize;
            }

            var bmp = new RenderTargetBitmap(
                (int)(size.Width * scale),
                (int)(size.Height * scale),
                96 * scale, 96 * scale,
                PixelFormats.Pbgra32);

            // Tạo DrawingVisual để render nền trắng
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null,
                    new Rect(0, 0, size.Width * scale, size.Height * scale));
            }
            bmp.Render(dv);
            bmp.Render(visual);

            return bmp;
        }
    }

    public class XuatKhachHangItem
    {
        public int STT { get; set; }
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public int SoHoaDon { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyLe { get; set; }
    }
}