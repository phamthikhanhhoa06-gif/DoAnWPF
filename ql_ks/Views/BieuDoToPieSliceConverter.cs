using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public class BieuDoToPieSliceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<BieuDoKhachHangItem> items
                && items.Any())
            {
                return items.Select(x => new PieSliceData
                {
                    Label = x.TenKhachHang,
                    Angle = x.GocKet,
                    Percentage = (double)x.TyLe,
                    Fill = x.Mau,
                    Value = x.DoanhThu
                }).ToList();
            }
            return new List<PieSliceData>();
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}