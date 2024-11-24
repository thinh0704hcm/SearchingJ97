using QLTV.Models;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace QLTV.UserControls
{
    public partial class UcCTPhieuTra : UserControl
    {
        public PHIEUTRA PhieuTra { get; private set; }

        public UcCTPhieuTra(PHIEUTRA phieuTra)
        {
            InitializeComponent();
            
            // Ensure navigation properties are loaded
            using (var context = new QLTVContext())
            {
                PhieuTra = context.PHIEUTRA
                    .Include(pt => pt.CTPHIEUTRA)
                        .ThenInclude(ct => ct.IDSachNavigation)
                            .ThenInclude(s => s.IDTuaSachNavigation)
                    .FirstOrDefault(pt => pt.MaPhieuTra == phieuTra.MaPhieuTra);
            }
            
            DataContext = PhieuTra;
            dgBooks.ItemsSource = PhieuTra.CTPHIEUTRA;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
} 