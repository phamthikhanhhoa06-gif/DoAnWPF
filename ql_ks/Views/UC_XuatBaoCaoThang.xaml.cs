using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ql_ks.Models;

namespace ql_ks.Views
{
    public partial class UC_XuatBaoCaoThang : UserControl
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private double _zoomLevel = 1.0;
        private int _nam, _thang;

        public UC_XuatBaoCaoThang(int nam, int thang)
        {
            InitializeComponent();
            _nam = nam;
            _thang = thang;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Lấy báo cáo năm
                var bcn = _db.BAOCAONAMs.FirstOrDefault(x => x.NAM_BCN == _nam);

                // Lấy doanh thu tháng
                decimal doanhThuThang = 0;
                if (bcn != null)
                {
                    doanhThuThang = GetDoanhThuThang(bcn, _thang);
                }

                // Cập nhật thông tin
                txtNgayLap.Text = "Ngày lập: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                txtNgayLapInfo.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                txtNamBC.Text = _nam.ToString();
                txtThangBC.Text = "Tháng " + _thang;
                txtTongCong.Text = doanhThuThang.ToString("N0") + " VNĐ";

                // Tạo danh sách ngày
                int soNgay = DateTime.DaysInMonth(_nam, _thang);
                var list = new List<XuatThangItem>();

                for (int day = 1; day <= soNgay; day++)
                {
                    decimal dtNgay = doanhThuThang > 0
                        ? Math.Round(doanhThuThang / soNgay, 0)
                        : 0;

                    list.Add(new XuatThangItem
                    {
                        STT = day,
                        Ngay = "Ngày " + day,
                        DoanhThu = dtNgay
                    });
                }

                dgChiTiet.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private decimal GetDoanhThuThang(BAOCAONAM bcn, int thang)
        {
            switch (thang)
            {
                case 1: return bcn.DOANHTHUTHANG1_BCN ?? 0;
                case 2: return bcn.DOANHTHUTHANG2_BCN ?? 0;
                case 3: return bcn.DOANHTHUTHANG3_BCN ?? 0;
                case 4: return bcn.DOANHTHUTHANG4_BCN ?? 0;
                case 5: return bcn.DOANHTHUTHANG5_BCN ?? 0;
                case 6: return bcn.DOANHTHUTHANG6_BCN ?? 0;
                case 7: return bcn.DOANHTHUTHANG7_BCN ?? 0;
                case 8: return bcn.DOANHTHUTHANG8_BCN ?? 0;
                case 9: return bcn.DOANHTHUTHANG9_BCN ?? 0;
                case 10: return bcn.DOANHTHUTHANG10_BCN ?? 0;
                case 11: return bcn.DOANHTHUTHANG11_BCN ?? 0;
                case 12: return bcn.DOANHTHUTHANG12_BCN ?? 0;
                default: return 0;
            }
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

                    dlg.PrintVisual(reportContent, "Báo cáo doanh thu tháng " + _thang + "/" + _nam);

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
                    FileName = "BaoCaoThang_" + _thang + "_" + _nam + ".png"
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

                    MessageBox.Show("Đã lưu tại:\n" + dlg.FileName, "Thành công");
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

        // ========== HELPER ==========
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

    // ===== MODEL =====
    public class XuatThangItem
    {
        public int STT { get; set; }
        public string Ngay { get; set; }
        public decimal DoanhThu { get; set; }
    }
}