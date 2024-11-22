using QLTV.Models;
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

namespace QLTV.UserControls
{
    /// <summary>
    /// Interaction logic for UcCTBCMuonSach.xaml
    /// </summary>
    public partial class UcCTBCMuonSach : UserControl
    {
        public BCMUONSACH BaoCaoMuonSach { get; private set; }

        public UcCTBCMuonSach(BCMUONSACH baoCaoMuonSach)
        {
            InitializeComponent();
            BaoCaoMuonSach = baoCaoMuonSach;
            DataContext = BaoCaoMuonSach;
            dgBorrowingReportDetails.ItemsSource = BaoCaoMuonSach.CTBCMUONSACH;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
