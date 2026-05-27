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
    public partial class UC_XuatBaoCaoNam : UserControl
    {
        private readonly QLKhachSan_Model _db = new QLKhachSan_Model();
        private int _nam;

        public UC_XuatBaoCaoNam(int nam)
        {
            InitializeComponent();
            _nam = nam;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var bcn = _db.BAOCAONAMs.FirstOrDefault(x => x.NAM_BCN == _nam);
                decimal tongDoanhThu = bcn?.TONGDOANHTHU_BCN ?? 0;

                txtTieuDePhu.Text = "Năm " + _nam;
                txtNgayLap.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                txtNamBC.Text = _nam.ToString();
                txtTongCong.Text = tongDoanhThu.ToString("N0") + " VNĐ";

                var list = new List<XuatNamItem>();
                for (int thang = 1; thang <= 12; thang++)
                {
                    decimal dt = GetDoanhThuThang(bcn, thang);
                    list.Add(new XuatNamItem
                    {
                        STT = thang,
                        TenThang = "Tháng " + thang,
                        DoanhThu = dt,
                        TyLe = tongDoanhThu > 0 ? dt / tongDoanhThu : 0
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
            if (bcn == null) return 0;
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

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new PrintDialog();
                if (dlg.ShowDialog() == true)
                {
                    var originalTransform = reportContent.LayoutTransform;
                    reportContent.LayoutTransform = new ScaleTransform(0.75, 0.75);

                    var size = new Size(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight);
                    reportContent.Measure(size);
                    reportContent.Arrange(new Rect(new Point(0, 0), size));

                    dlg.PrintVisual(reportContent, "Báo cáo doanh thu năm " + _nam);

                    reportContent.LayoutTransform = originalTransform;
                    MessageBox.Show("Đã gửi lệnh in!", "Thành công");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi in: " + ex.Message);
            }
        }

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            reportContent.LayoutTransform = new ScaleTransform(1, 1);
            LoadData();
        }

        private void BtnLuuFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg",
                    FileName = "BaoCaoNam_" + _nam + ".png"
                };

                if (dlg.ShowDialog() == true)
                {
                    var bmp = RenderToBitmap(reportContent, 2.0);
                    if (dlg.FilterIndex == 1)
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bmp));
                        using (var fs = new FileStream(dlg.FileName, FileMode.Create))
                            encoder.Save(fs);
                    }
                    else
                    {
                        var encoder = new JpegBitmapEncoder { QualityLevel = 95 };
                        encoder.Frames.Add(BitmapFrame.Create(bmp));
                        using (var fs = new FileStream(dlg.FileName, FileMode.Create))
                            encoder.Save(fs);
                    }
                    MessageBox.Show("Đã lưu tại:\n" + dlg.FileName, "Thành công");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu: " + ex.Message);
            }
        }

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

    public class XuatNamItem
    {
        public int STT { get; set; }
        public string TenThang { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal TyLe { get; set; }
    }
}