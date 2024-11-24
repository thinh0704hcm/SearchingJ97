using QLTV.Models;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace QLTV.UserControls
{
    public partial class UcCTPhieuMuon : UserControl
    {
        public PHIEUMUON PhieuMuon { get; private set; }

        public UcCTPhieuMuon(PHIEUMUON phieuMuon)
        {
            InitializeComponent();
            
            // Ensure navigation properties are loaded
            using (var context = new QLTVContext())
            {
                PhieuMuon = context.PHIEUMUON
                    .Include(pm => pm.CTPHIEUMUON)
                        .ThenInclude(ct => ct.IDSachNavigation)
                            .ThenInclude(s => s.IDTuaSachNavigation)
                    .FirstOrDefault(pm => pm.MaPhieuMuon == phieuMuon.MaPhieuMuon);
            }
            
            DataContext = PhieuMuon;
            dgBooks.ItemsSource = PhieuMuon.CTPHIEUMUON;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
} 