using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    /// <summary>
    /// Interaction logic for UC_TraCuuLoaiPhong.xaml
    /// </summary>
    public partial class UC_TraCuuLoaiPhong : UserControl
    {
        public UC_TraCuuLoaiPhong()
        {
            InitializeComponent();
            DataContext = new TraCuuLoaiPhongViewModel();
        }
    }
}
