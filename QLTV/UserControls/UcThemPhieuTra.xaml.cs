using QLTV.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;

namespace QLTV.UserControls
{
    public partial class UcThemPhieuTra : UserControl
    {
        private readonly QLTVContext _context;
        private ObservableCollection<dynamic> _returnDetails;

        public UcThemPhieuTra()
        {
            InitializeComponent();
            _context = new QLTVContext();
            _returnDetails = new ObservableCollection<dynamic>();
            dgBooks.ItemsSource = _returnDetails;

            var tinhTrangList = new[] { "Tốt", "Hư hỏng nhẹ", "Hư hỏng nặng", "Mất sách" };
            colTinhTrangTra.ItemsSource = tinhTrangList;

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var pendingBorrows = await _context.PHIEUMUON
                    .Include(p => p.IDDocGiaNavigation)
                        .ThenInclude(d => d.IDTaiKhoanNavigation)
                    .Include(p => p.CTPHIEUMUON)
                        .ThenInclude(ct => ct.IDSachNavigation)
                            .ThenInclude(s => s.IDTuaSachNavigation)
                    .Include(p => p.CTPHIEUTRA)
                    .Where(p => !p.IsDeleted && p.IsPending)
                    .ToListAsync();

                var availableBorrows = pendingBorrows.Where(p => p.CTPHIEUMUON.Any(ct =>
                    !p.CTPHIEUTRA.Any(ptr => ptr.IDSach == ct.IDSach))).ToList();

                cboPhieuMuon.ItemsSource = availableBorrows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cboPhieuMuon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedBorrow = cboPhieuMuon.SelectedItem as PHIEUMUON;
            if (selectedBorrow == null)
            {
                txtDocGia.Text = "";
                txtNgayMuon.Text = "";
                _returnDetails.Clear();
                return;
            }

            txtDocGia.Text = selectedBorrow.IDDocGiaNavigation.IDTaiKhoanNavigation.TenTaiKhoan;
            txtNgayMuon.Text = selectedBorrow.NgayMuon.ToString("dd/MM/yyyy");

            _returnDetails.Clear();
            foreach (var ctpm in selectedBorrow.CTPHIEUMUON)
            {
                var isReturned = selectedBorrow.CTPHIEUTRA.Any(ptr => ptr.IDSach == ctpm.IDSach);
                if (!isReturned)
                {
                    var ctpt = new CTPHIEUTRA
                    {
                        IDPhieuMuon = ctpm.IDPhieuMuon,
                        IDSach = ctpm.IDSach,
                        IDSachNavigation = ctpm.IDSachNavigation,
                        SoNgayMuon = (int)(DateTime.Now - selectedBorrow.NgayMuon).TotalDays,
                        TinhTrangTra = "Tốt",
                        GhiChu = "",
                        TienPhat = 0
                    };

                    dynamic returnDetail = new ExpandoObject();
                    returnDetail.IsSelected = true;
                    returnDetail.ReturnDetail = ctpt;
                    returnDetail.BorrowStatus = ctpm.TinhTrangMuon;
                    returnDetail.MaSach = ctpt.IDSachNavigation?.MaSach;
                    returnDetail.TenSach = ctpt.IDSachNavigation?.IDTuaSachNavigation?.TenTuaSach;
                    returnDetail.HanTra = ctpm.HanTra;
                    returnDetail.TinhTrangTra = ctpt.TinhTrangTra;
                    returnDetail.GhiChu = ctpt.GhiChu;

                    _returnDetails.Add(returnDetail);
                }
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var selectedBorrow = cboPhieuMuon.SelectedItem as PHIEUMUON;
            if (selectedBorrow == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu mượn", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedBooks = _returnDetails.Where(r => r.IsSelected).ToList();
            if (!selectedBooks.Any())
            {
                MessageBox.Show("Vui lòng chọn ít nhất một cuốn sách để trả", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var phieuTra = new PHIEUTRA
                {
                    NgayTra = DateTime.Now
                };

                _context.PHIEUTRA.Add(phieuTra);
                await _context.SaveChangesAsync();

                // Check for late returns and update BCTRATRE if necessary
                var lateReturns = selectedBooks
                    .Where(r => DateTime.Now > ((dynamic)r).HanTra)
                    .ToList();

                if (lateReturns.Any())
                {
                    var today = DateTime.Now.Date;
                    var bcTraTre = await _context.BCTRATRE
                        .FirstOrDefaultAsync(bc => bc.Ngay == today);

                    if (bcTraTre == null)
                    {
                        bcTraTre = new BCTRATRE
                        {
                            Ngay = today
                        };
                        _context.BCTRATRE.Add(bcTraTre);
                        await _context.SaveChangesAsync();
                    }

                    foreach (dynamic lateReturn in lateReturns)
                    {
                        var ctBcTraTre = new CTBCTRATRE
                        {
                            IDBCTraTre = bcTraTre.ID,
                            IDPhieuTra = phieuTra.ID,
                            SoNgayTraTre = (int)(DateTime.Now - lateReturn.HanTra).TotalDays
                        };

                        _context.CTBCTRATRE.Add(ctBcTraTre);
                    }
                }

                await _context.SaveChangesAsync();

                foreach (dynamic returnBook in selectedBooks)
                {
                    CTPHIEUTRA ctpt = returnBook.ReturnDetail;
                    ctpt.IDPhieuTra = phieuTra.ID;

                    var book = await _context.SACH.FindAsync(ctpt.IDSach);
                    var borrowDetail = await _context.CTPHIEUMUON
                        .FirstOrDefaultAsync(ct => ct.IDPhieuMuon == ctpt.IDPhieuMuon && ct.IDSach == ctpt.IDSach);

                    if (borrowDetail != null)
                    {
                        // Update TinhTrangMuon based on TinhTrangTra
                        switch (returnBook.TinhTrangTra)
                        {
                            case "Tốt":
                                borrowDetail.TinhTrangMuon = "Hoàn trả tốt";
                                break;
                            case "Hư hỏng nhẹ":
                                borrowDetail.TinhTrangMuon = "Hoàn trả hư hỏng nhẹ";
                                break;
                            case "Hư hỏng nặng":
                                borrowDetail.TinhTrangMuon = "Hoàn trả hư hỏng nặng";
                                break;
                            case "Mất sách":
                                borrowDetail.TinhTrangMuon = "Mất sách";
                                break;
                            default:
                                borrowDetail.TinhTrangMuon = "Đã trả";
                                break;
                        }

                        // Calculate fines
                        if (DateTime.Now > borrowDetail.HanTra)
                        {
                            int daysLate = (int)(DateTime.Now - borrowDetail.HanTra).TotalDays;
                            ctpt.TienPhat = daysLate * 5000;
                            borrowDetail.TinhTrangMuon += " - Trễ hạn";
                        }

                        if (returnBook.TinhTrangTra != "Tốt" && book != null)
                        {
                            ctpt.TienPhat += book.TriGia * 0.5m;
                        }

                        ctpt.TinhTrangTra = returnBook.TinhTrangTra;
                        ctpt.GhiChu = returnBook.GhiChu;

                        _context.CTPHIEUTRA.Add(ctpt);
                        if (book != null)
                        {
                            book.IsAvailable = true;
                            _context.SACH.Update(book);
                        }
                    }
                }

                var selectedBookIds = selectedBooks.Select(sb => ((dynamic)sb).ReturnDetail.IDSach).Cast<int>().ToList();

                // Then use these IDs in the query
                var unreturnedBooks = await _context.CTPHIEUMUON
                    .Where(ct => ct.IDPhieuMuon == selectedBorrow.ID &&
                           !selectedBookIds.Contains(ct.IDSach))
                    .ToListAsync();

                foreach (var unreturned in unreturnedBooks)
                {
                    if (unreturned.TinhTrangMuon == "Tốt") // Only update if not already modified
                    {
                        unreturned.TinhTrangMuon = "Đang mượn";
                        _context.CTPHIEUMUON.Update(unreturned);
                    }
                }

                // Update borrow ticket status
                if (_returnDetails.All(r => r.IsSelected))
                {
                    selectedBorrow.IsPending = false;
                    _context.PHIEUMUON.Update(selectedBorrow);
                }

                await _context.SaveChangesAsync();
                MessageBox.Show("Thêm phiếu trả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                Window.GetWindow(this).DialogResult = true;
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu phiếu trả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}