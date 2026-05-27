using System.Windows.Controls;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_BaoCaoThongKe : UserControl
    {
        public UC_BaoCaoThongKe()
        {
            InitializeComponent();
            this.DataContext = new BaoCaoThongKeViewModel();
        }
    }
}
