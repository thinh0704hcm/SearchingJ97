using QLTV.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QLTV.UserControls
{
    public partial class UcThemPhieuTra : UserControl
    {
        private readonly QLTVContext _context;
        private ObservableCollection<ReturnBookDetail> _returnDetails;

        public UcThemPhieuTra()
        {
            InitializeComponent();
            _context = new QLTVContext();
            _returnDetails = new ObservableCollection<ReturnBookDetail>();
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
                    _returnDetails.Add(new ReturnBookDetail 
                    { 
                        ReturnDetail = ctpt, 
                        IsSelected = true,
                        BorrowStatus = ctpm.TinhTrangMuon 
                    });
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
                    MaPhieuTra = GenerateNewReturnCode(),
                    NgayTra = DateTime.Now
                };

                _context.PHIEUTRA.Add(phieuTra);
                await _context.SaveChangesAsync();

                foreach (var returnBook in selectedBooks)
                {
                    var ctpt = returnBook.ReturnDetail;
                    ctpt.IDPhieuTra = phieuTra.ID;
                    
                    var book = ctpt.IDSachNavigation;
                    var borrowDetail = selectedBorrow.CTPHIEUMUON.First(ct => ct.IDSach == ctpt.IDSach);
                    
                    // Update TinhTrangMuon based on TinhTrangTra
                    switch (ctpt.TinhTrangTra)
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

                    if (ctpt.TinhTrangTra != "Tốt")
                    {
                        ctpt.TienPhat += book.TriGia * 0.5m;
                    }

                    _context.CTPHIEUTRA.Add(ctpt);
                    book.IsAvailable = true;
                }

                // Update remaining books' status if any
                var unreturnedBooks = selectedBorrow.CTPHIEUMUON
                    .Where(ct => !selectedBooks.Any(sb => sb.ReturnDetail.IDSach == ct.IDSach));
                foreach (var unreturned in unreturnedBooks)
                {
                    if (unreturned.TinhTrangMuon == "Tốt") // Only update if not already modified
                    {
                        unreturned.TinhTrangMuon = "Đang mượn";
                    }
                }

                // Update borrow ticket status
                if (_returnDetails.All(r => r.IsSelected))
                {
                    selectedBorrow.IsPending = false;
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

        private string GenerateNewReturnCode()
        {
            var lastCode = _context.PHIEUTRA
                .OrderByDescending(p => p.MaPhieuTra)
                .Select(p => p.MaPhieuTra)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(lastCode))
            {
                return "PT0001";
            }

            int number = int.Parse(lastCode.Substring(2)) + 1;
            return $"PT{number:D4}";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }

    public class ReturnBookDetail : INotifyPropertyChanged
    {
        private bool _isSelected;
        private CTPHIEUTRA _returnDetail;
        private string _borrowStatus;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public CTPHIEUTRA ReturnDetail
        {
            get => _returnDetail;
            set
            {
                _returnDetail = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MaSach));
                OnPropertyChanged(nameof(TenSach));
                OnPropertyChanged(nameof(HanTra));
            }
        }

        public string BorrowStatus
        {
            get => _borrowStatus;
            set
            {
                _borrowStatus = value;
                OnPropertyChanged();
            }
        }

        public string MaSach => ReturnDetail?.IDSachNavigation?.MaSach;
        public string TenSach => ReturnDetail?.IDSachNavigation?.IDTuaSachNavigation?.TenTuaSach;
        public DateTime HanTra => ReturnDetail?.IDPhieuMuonNavigation.CTPHIEUMUON.Where(x => x.IDSach == ReturnDetail?.IDSach).FirstOrDefault().HanTra ?? DateTime.MinValue;
        public string TinhTrangTra
        {
            get => ReturnDetail?.TinhTrangTra;
            set
            {
                if (ReturnDetail != null)
                {
                    ReturnDetail.TinhTrangTra = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GhiChu
        {
            get => ReturnDetail?.GhiChu;
            set
            {
                if (ReturnDetail != null)
                {
                    ReturnDetail.GhiChu = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 