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
using Microsoft.EntityFrameworkCore;
using QLTV.Models;

namespace QLTV.UserControls
{
    /// <summary>
    /// Interaction logic for UcBCMuonTra.xaml
    /// </summary>
    public partial class UcBCMuonTra : UserControl
    {
        private readonly QLTVContext _context;
        private ObservableCollection<BCMUONSACH> _borrowReports;


        public UcBCMuonTra()
        {
            InitializeComponent();
            _context = new QLTVContext();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // Load borrowings with related data, excluding soft-deleted records
                var borrowReports = await _context.BCMUONSACH
                    .Include(p => p.CTBCMUONSACH)
                        .ThenInclude(ct => ct.IDTheLoaiNavigation)
                    .ToListAsync();
                _borrowReports = new ObservableCollection<BCMUONSACH>(borrowReports);
                dgBorrowingReports.ItemsSource = _borrowReports;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnViewBorrowReportDetail_Click(object sender, RoutedEventArgs e)
        {
            var bcMuonSach = ((FrameworkElement)sender).DataContext as BCMUONSACH;
            if (bcMuonSach == null) return;

            var window = new Window
            {
                Title = $"Chi tiết phiếu mượn: {bcMuonSach.MaBCMuonSach}",
                Content = new UcCTBCMuonSach(bcMuonSach),
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            window.ShowDialog();
        }
    }
}
