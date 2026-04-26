using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static System.Net.Mime.MediaTypeNames;

namespace ql_ks.Views
{
    /// <summary>
    /// Interaction logic for UC_TraCuuNhanVien.xaml
    /// </summary>
    public partial class UC_TraCuuNhanVien : UserControl
    {
        public UC_TraCuuNhanVien()
        {
            InitializeComponent();
            DataContext = new TraCuuNhanVienViewModel();
        }
    }
}