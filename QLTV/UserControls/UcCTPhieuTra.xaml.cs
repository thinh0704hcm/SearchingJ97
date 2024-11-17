using QLTV.Models;
using System.Windows;
using System.Windows.Controls;

namespace QLTV.UserControls
{
    public partial class UcCTPhieuTra : UserControl
    {
        public PHIEUTRA PhieuTra { get; private set; }

        public UcCTPhieuTra(PHIEUTRA phieuTra)
        {
            InitializeComponent();
            PhieuTra = phieuTra;
            DataContext = PhieuTra;
            dgBooks.ItemsSource = PhieuTra.CTPHIEUTRA;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
} 