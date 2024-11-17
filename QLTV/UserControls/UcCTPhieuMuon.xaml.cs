using QLTV.Models;
using System.Windows;
using System.Windows.Controls;

namespace QLTV.UserControls
{
    public partial class UcCTPhieuMuon : UserControl
    {
        public PHIEUMUON PhieuMuon { get; private set; }

        public UcCTPhieuMuon(PHIEUMUON phieuMuon)
        {
            InitializeComponent();
            PhieuMuon = phieuMuon;
            DataContext = PhieuMuon;
            dgBooks.ItemsSource = PhieuMuon.CTPHIEUMUON;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
} 