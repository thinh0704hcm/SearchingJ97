using QLTV.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Windows.Input;

namespace QLTV.UserControls
{
    public partial class UcThemPhieuMuon : UserControl
    {
        private readonly QLTVContext _context;
        private List<SACH> _allBooks;
        private ObservableCollection<BookWithCustomDate> _selectedBooks;

        private class BookWithCustomDate : INotifyPropertyChanged
        {
            private SACH _book;
            private int _customBorrowDays;
            private DateTime _customReturnDate;

            public SACH Book
            {
                get => _book;
                set
                {
                    _book = value;
                    OnPropertyChanged(nameof(Book));
                    OnPropertyChanged(nameof(MaSach));
                    OnPropertyChanged(nameof(IDTuaSachNavigation));
                }
            }

            public int CustomBorrowDays
            {
                get => _customBorrowDays;
                set
                {
                    _customBorrowDays = value;
                    UpdateCustomReturnDate();
                    OnPropertyChanged(nameof(CustomBorrowDays));
                }
            }

            public DateTime CustomReturnDate
            {
                get => _customReturnDate;
                set
                {
                    _customReturnDate = value;
                    OnPropertyChanged(nameof(CustomReturnDate));
                }
            }

            public string MaSach => Book.MaSach;
            public TUASACH IDTuaSachNavigation => Book.IDTuaSachNavigation;
            public int ID => Book.ID;

            private void UpdateCustomReturnDate()
            {
                CustomReturnDate = DateTime.Now.AddDays(CustomBorrowDays > 0 ? CustomBorrowDays : Book.IDTuaSachNavigation.HanMuonToiDa);
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public UcThemPhieuMuon()
        {
            InitializeComponent();
            _context = new QLTVContext();
            _selectedBooks = new ObservableCollection<BookWithCustomDate>();
            dgSelectedBooks.ItemsSource = _selectedBooks;
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var docGia = await _context.DOCGIA
                    .Include(d => d.IDTaiKhoanNavigation)
                    .ToListAsync();
                cboDocGia.ItemsSource = docGia;

                // Load all available books
                _allBooks = await _context.SACH
                    .Include(s => s.IDTuaSachNavigation)
                    .Where(s => !s.IsDeleted && s.IsAvailable)
                    .ToListAsync();
                    
                // Initially show all books
                dgAvailableBooks.ItemsSource = _allBooks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearchBook_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allBooks == null) return;

            var searchText = txtSearchBook.Text.Trim().ToLower();
            var searchType = ((ComboBoxItem)cboSearchType.SelectedItem).Content.ToString();

            IEnumerable<SACH> filteredBooks = _allBooks;

            if (!string.IsNullOrEmpty(searchText))
            {
                switch (searchType)
                {
                    case "Mã sách":
                        filteredBooks = _allBooks.Where(s => 
                            s.MaSach.ToLower().Contains(searchText));
                        break;

                    case "Tên sách":
                        filteredBooks = _allBooks.Where(s => 
                            s.IDTuaSachNavigation.TenTuaSach.ToLower().Contains(searchText));
                        break;

                    //case "Thể loại":
                    //    filteredBooks = _allBooks.Where(s => 
                    //        s.IDTuaSachNavigation.IDTheLoaiNavigation.TenTheLoai.ToLower().Contains(searchText));
                    //    break;

                    default: // "Tất cả"
                        filteredBooks = _allBooks.Where(s =>
                            s.MaSach.ToLower().Contains(searchText) ||
                            s.IDTuaSachNavigation.TenTuaSach.ToLower().Contains(searchText));
                            //s.IDTuaSachNavigation.IDTheLoaiNavigation.TenTheLoai.ToLower().Contains(searchText));
                        break;
                }
            }

            dgAvailableBooks.ItemsSource = filteredBooks.ToList();
        }

        private void btnSelectBook_Click(object sender, RoutedEventArgs e)
        {
            var book = ((Button)sender).DataContext as SACH;
            if (book == null) return;

            if (!_selectedBooks.Any(b => b.Book.ID == book.ID))
            {
                var bookWithDate = new BookWithCustomDate 
                { 
                    Book = book,
                    CustomBorrowDays = book.IDTuaSachNavigation.HanMuonToiDa
                };
                _selectedBooks.Add(bookWithDate);
            }
        }

        private void btnRemoveBook_Click(object sender, RoutedEventArgs e)
        {
            var bookWithDate = ((Button)sender).DataContext as BookWithCustomDate;
            if (bookWithDate != null)
            {
                _selectedBooks.Remove(bookWithDate);
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cboDocGia.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn độc giả", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedBooks.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một cuốn sách", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var phieuMuon = new PHIEUMUON
                {
                    IDDocGia = (int)cboDocGia.SelectedValue,
                    NgayMuon = DateTime.Now,
                    MaPhieuMuon = GenerateNewBorrowCode(),
                    IsPending = true
                };

                _context.PHIEUMUON.Add(phieuMuon);
                await _context.SaveChangesAsync();

                foreach (var bookWithDate in _selectedBooks)
                {
                    var ctPhieuMuon = new CTPHIEUMUON
                    {
                        IDPhieuMuon = phieuMuon.ID,
                        IDSach = bookWithDate.ID,
                        HanTra = bookWithDate.CustomReturnDate,
                        TinhTrangMuon = "Tốt"
                    };
                    _context.CTPHIEUMUON.Add(ctPhieuMuon);

                    bookWithDate.Book.IsAvailable = false;
                }

                await _context.SaveChangesAsync();
                MessageBox.Show("Thêm phiếu mượn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                Window.GetWindow(this).DialogResult = true;
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateNewBorrowCode()
        {
            var lastCode = _context.PHIEUMUON
                .OrderByDescending(p => p.MaPhieuMuon)
                .Select(p => p.MaPhieuMuon)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(lastCode))
            {
                return "PM0001";
            }

            int number = int.Parse(lastCode.Substring(2)) + 1;
            return $"PM{number:D4}";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void txtBorrowDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            //UpdateReturnDates();
        }

        private void txtCustomDays_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
} 