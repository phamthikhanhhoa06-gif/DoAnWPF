using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ql_ks.ViewModels;

namespace ql_ks.Views
{
    public partial class UC_TrangChu : UserControl
    {
        public UC_TrangChu()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        public void RefreshData()
        {
            var vm = DataContext as MainViewModel;
            vm?.LoadInitialData();
        }
    }
}