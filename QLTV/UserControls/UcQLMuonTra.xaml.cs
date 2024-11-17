using QLTV.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace QLTV.UserControls
{
    public partial class UcQLMuonTra : UserControl
    {
        private readonly QLTVContext _context;
        private ObservableCollection<PHIEUMUON> _borrowings;
        private ObservableCollection<PHIEUTRA> _returns;

        public UcQLMuonTra()
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
                var borrowings = await _context.PHIEUMUON
                    .Include(p => p.IDDocGiaNavigation)
                        .ThenInclude(d => d.IDTaiKhoanNavigation)
                    .Include(p => p.CTPHIEUMUON)
                        .ThenInclude(ct => ct.IDSachNavigation)
                    .Where(p => !p.IsDeleted)
                    .ToListAsync();
                _borrowings = new ObservableCollection<PHIEUMUON>(borrowings);
                dgBorrowings.ItemsSource = _borrowings;

                // Load returns with related data, excluding soft-deleted records
                var returns = await _context.PHIEUTRA
                    .Include(p => p.CTPHIEUTRA)
                        .ThenInclude(ct => ct.IDSachNavigation)
                    .Where(p => !p.IsDeleted)
                    .ToListAsync();
                _returns = new ObservableCollection<PHIEUTRA>(returns);
                dgReturns.ItemsSource = _returns;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddBorrow_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Title = "Thêm phiếu mượn",
                Content = new UcThemPhieuMuon(),
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            
            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void btnViewBorrowDetail_Click(object sender, RoutedEventArgs e)
        {
            var phieuMuon = ((FrameworkElement)sender).DataContext as PHIEUMUON;
            if (phieuMuon == null) return;

            var window = new Window
            {
                Title = $"Chi tiết phiếu mượn: {phieuMuon.MaPhieuMuon}",
                Content = new UcCTPhieuMuon(phieuMuon),
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            window.ShowDialog();
        }

        private async void btnDeleteBorrow_Click(object sender, RoutedEventArgs e)
        {
            var phieuMuon = ((FrameworkElement)sender).DataContext as PHIEUMUON;
            if (phieuMuon == null) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa phiếu mượn này?", "Xác nhận", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    // Soft delete the borrowing record
                    phieuMuon.IsDeleted = true;

                    // Also mark related CTPHIEUMUON records as deleted if they have IsDeleted property
                    foreach (var ctPhieuMuon in phieuMuon.CTPHIEUMUON)
                    {
                        // Update the book's availability
                        var sach = ctPhieuMuon.IDSachNavigation;
                        if (sach != null)
                        {
                            sach.IsAvailable = true;
                        }
                    }

                    await _context.SaveChangesAsync();
                    _borrowings.Remove(phieuMuon);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phiếu mượn: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAddReturn_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Title = "Thêm phiếu trả",
                Content = new UcThemPhieuTra(),
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            
            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void btnViewReturnDetail_Click(object sender, RoutedEventArgs e)
        {
            var phieuTra = ((FrameworkElement)sender).DataContext as PHIEUTRA;
            if (phieuTra == null) return;

            var window = new Window
            {
                Title = $"Chi tiết phiếu trả: {phieuTra.MaPhieuTra}",
                Content = new UcCTPhieuTra(phieuTra),
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            window.ShowDialog();
        }

        private async void btnDeleteReturn_Click(object sender, RoutedEventArgs e)
        {
            var phieuTra = ((FrameworkElement)sender).DataContext as PHIEUTRA;
            if (phieuTra == null) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa phiếu trả này?", "Xác nhận", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    // Soft delete the return record
                    phieuTra.IsDeleted = true;

                    // Handle related CTPHIEUTRA records
                    foreach (var ctPhieuTra in phieuTra.CTPHIEUTRA)
                    {
                        // Revert the book's status if needed
                        var sach = ctPhieuTra.IDSachNavigation;
                        if (sach != null)
                        {
                            sach.IsAvailable = false;
                        }
                    }

                    await _context.SaveChangesAsync();
                    _returns.Remove(phieuTra);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phiếu trả: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
