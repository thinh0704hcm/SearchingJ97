﻿create database QLTV
go
use QLTV
go

create table [PHANQUYEN]
(
	[ID] int identity(1, 1) constraint [PK_PHANQUYEN] primary key,
	[MaPhanQuyen] as ('PQ' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
	[MaHanhDong] varchar(100) not null,
	[MoTa] nvarchar(100) not null,
	[IsDeleted] bit not null,
	-- Xóa đi thì trước khi xóa thay bằng 1 phân quyền khác có tồn tại.
	-- Thêm phân quyền Banned hoặc xét IsDeleted = 1 bằng Banned cũng được
	-- Khi xóa 1 PQ thì hiện thông báo: Hãy gán quyền khác cho các tài khoản được gán quyền này
	-- Quyền Ko Xac Dinh: phân quyền của bạn không xác định, vấn đề này do bên admin, 
	-- liên hệ họ để được hỗ trợ (trường hợp cần sửa chữa hệ thống dành cho 1 phân quyền nào đó)
)

create table [LOAIDOCGIA]
(
    [ID] int identity(1, 1) constraint [PK_LOAIDOCGIA] primary key,
    [MaLoaiDocGia] as ('LDG' + right('00000' + cast([ID] as varchar(5)), 5)),
    [TenLoaiDocGia] nvarchar(100),
    [SoSachMuonToiDa] int not null,
    [IsDeleted] bit not null
)

create table [TAIKHOAN]
(
	[ID] int identity(1, 1) constraint [PK_TAIKHOAN] primary key,
	[MaTaiKhoan] as ('TK' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
	[MatKhau] varchar(50) not null,
	[TenTaiKhoan] nvarchar(100) unique not null,
	[Email] varchar(100) unique not null,
	[SinhNhat] datetime not null,
	[DiaChi] nvarchar(200) not null,
	[SDT] varchar(20) not null,
	[Avatar] varchar(500) not null,
	[TrangThai] bit not null, -- co dang dang nhap hay ko
	[IDPhanQuyen] int not null, -- FK
	[IsDeleted] bit not null,
)

create table [ADMIN] 
(
	[ID] int identity(1, 1) constraint [PK_ADMIN] primary key,
	[MaAdmin] as ('AD' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
	[IDTaiKhoan] int not null, -- FK 
	[NgayVaoLam] date not null,
	[NgayKetThuc] date not null, -- Tai khoan admin het han giong nhu The doc gia het han
)

create table [DOCGIA]
(
    [ID] int identity(1, 1) constraint [PK_DOCGIA] primary key,
    [MaDocGia] as ('DG' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [IDTaiKhoan] int not null, -- FK
    [IDLoaiDocGia] int not null, -- FK
    [NgayLapThe] date not null,
    [NgayHetHan] date not null,
    [TongNo] decimal(18, 0) not null,
    [GioiThieu] nvarchar(500) not null -- bo cung duoc
)

create table [TACGIA]
(
    [ID] int identity(1, 1) constraint [PK_TACGIA] primary key,
    [MaTacGia] as ('TG' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [NamSinh] int not null,
    [QuocTich] nvarchar(100) not null,
    [IsDeleted] bit not null -- thu xem them cai global query excluder thi khi query sach tac gia, voi mot sach co nhieu tac gia thi co tu dong ko lay cac tac gia da delete ko
)

create table [THELOAI]
(
    [ID] int identity(1, 1) constraint [PK_THELOAI] primary key,
    [MaTheLoai] as ('TL' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [TenTheLoai] nvarchar(100) not null,
    [IsDeleted] bit not null
)

create table [TUASACH]
(
    [ID] int identity(1, 1) constraint [PK_TUASACH] primary key,
    [MaTuaSach] as ('TS' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [TenTuaSach] nvarchar(100) not null,
    [BiaSach] varchar(500) not null,
    [SoLuong] int not null, -- them rang buoc so luong nay phai bang so luong sach co ma tua sach nay
    [HanMuonToiDa] int not null, -- don vi: thang
    [IsDeleted] bit not null
)

create table [TUASACH_TACGIA]
(
    [IDTuaSach] int not null,
    [IDTacGia] int not null,
    constraint [PK_TUASACH_TACGIA] primary key([IDTuaSach], [IDTacGia])
)

create table [TUASACH_THELOAI]
(
    [IDTuaSach] int not null,
    [IDTheLoai] int not null,
    constraint [PK_TUASACH_THELOAI] primary key([IDTuaSach], [IDTheLoai])
)

create table [TINHTRANG]
(
    -- vd: TT1: Nguyen ven, TT2: Hu muc 1 (25), TT3: muc 2 (50), TT4: 75, TT5: (Hu nang ne/ hoan toan) 100
    -- -> tien phat bang (MHH(moi) - MHH(cu)) x 2 x GiaTri
    [ID] int identity(1, 1) constraint [PK_TINHTRANG] primary key,
    [MaTinhTrang] as ('TT' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [TenTinhTrang] nvarchar(100) not null,
    [MucHuHong] int not null,
    [IsDeleted] bit not null
)

create table [SACH]
(
    -- Một đợt nhập nhiều quyển giống nhau thì vẫn cho là 2 sách khác nhau vì sau này có thể khác tình trạng
    [ID] int identity(1, 1) constraint [PK_SACH] primary key,
    [MaSach] as ('S' + right('00000' + cast([ID] as varchar(5)), 5)) persisted,
    [IDTuaSach] int not null, -- FK
    [NamXuatBan] int not null,
    [NhaXuatBan] nvarchar(100) not null,
    [NgayNhap] date not null,
    [TriGia] decimal(18, 0) not null,
    [IDTinhTrang] int not null, -- FK
    [IsAvailable] bit not null,
    [IsDeleted] bit not null
)

create table [PHIEUMUON]
(
    [ID] int identity(1, 1) constraint [PK_PHIEUMUON] primary key,
    [MaPhieuMuon] as ('PM' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [IDDocGia] int not null, -- FK
    [NgayMuon] datetime not null,
    [IsPending] bit not null, -- kiem tra sau 3 ngay ma IsPending van bang 1 thi IsDeleted = 1
    [IsDeleted] bit not null
    -- IsPending = 0/1 xu ly tren code chu ko set default 
)

create table [CTPHIEUMUON]
(
    [IDPhieuMuon] int not null, -- FK
    [IDSach] int not null, -- FK
    constraint [PK_CTPHIEUMUON] primary key([IDPhieuMuon], [IDSach]),
    [HanTra] datetime not null,
    [TinhTrangMuon] nvarchar(100) not null -- tinh trang moi/cu cua sach luc muon?
)

create table [PHIEUTRA]
(
    [ID] int identity(1, 1) constraint [PK_PHIEUTRA] primary key,
    [MaPhieuTra] as ('PT' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [NgayTra] datetime not null,
    [IsDeleted] bit not null -- cho xoa vi lo nhap sai
)

create table [CTPHIEUTRA]
(
    [IDPhieuTra] int not null,
    [IDPhieuMuon] int not null,
    [IDSach] int not null,
    constraint [PK_CTPHIEUTRA] primary key([IDPhieuTra], [IDPhieuMuon], [IDSach]),
    [SoNgayMuon] int not null,
    [TienPhat] decimal(18, 0) not null,
    [TinhTrangTra] nvarchar(100) not null,
    [GhiChu] nvarchar(200) not null
)

create table [BCTRATRE]
(
    [ID] int identity(1, 1) constraint [PK_BCTRATRE] primary key,
    [MaBCTraTre] as ('BCTT' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [Ngay] date not null
)

create table [CTBCTRATRE]
(
    [IDBCTraTre] int not null,
    [IDPhieuTra] int not null,
    constraint [PK_CTBCTRATRE] primary key([IDBCTraTre], [IDPhieuTra]),
    [SoNgayTraTre] int not null
)

create table [BCMUONSACH]
(
    [ID] int identity(1, 1) constraint [PK_BCMUONSACH] primary key,
    [MaBCMuonSach] as ('BCMS' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [Thang] date not null, -- vd: 11/2024
    [TongSoLuotMuon] int not null
)

create table [CTBCMUONSACH]
(
    [IDBCMuonSach] int not null,
    [IDTheLoai] int not null,
    constraint [PK_CTBCMUONSACH] primary key([IDBCMuonSach], [IDTheLoai]),
    [SoLuotMuon] int not null,
    [TiLe] float not null
)

create table [PHIEUTHUTIENPHAT]
(
    [ID] int identity(1, 1) constraint [PK_PHIEUTHUTIENPHAT] primary key,
    [MaPTTP] as ('PTTP' + right('00000' + cast ([ID] as varchar(5)), 5)) persisted,
    [IDDocGia] int not null,
    [NgayThu] datetime not null,
    [TongNo] decimal(18, 0) not null, -- = TongNo doc gia, lay ben DOCGIA khoi luu cung duoc, ma luu cho tien?
    [SoTienThu] decimal(18, 0) not null,
    [ConLai] decimal(18, 0) not null,
	[IsDeleted] bit not null
)

create table [THAMSO]
(
    [ID] int identity(1, 1) constraint [PK_THAMSO] primary key,
    [TuoiToiThieu] int not null,
    [TienPhatTraTreMotNgay] decimal(18, 0) not null
)


-- default BiaSach
alter table [TUASACH]
ADD CONSTRAINT [DF_BiaSach] 
DEFAULT 'https://upload.wikimedia.org/wikipedia/commons/thumb/a/ac/No_image_available.svg/800px-No_image_available.svg.png' 
FOR [BiaSach];


-- default TongNo
alter table [DOCGIA]
add constraint [DF_DOCGIA_TongNo] default 0 for [TongNo];


-- default SoLuong 
alter table [TUASACH]
add constraint [DF_TUASACH_SoLuong] default 0 for [SoLuong];


-- default IsDeleted
alter table [PHANQUYEN]
add constraint [DF_PHANQUYEN_IsDeleted] default 0 for [IsDeleted];

alter table [LOAIDOCGIA]
add constraint [DF_LOAIDOCGIA_IsDeleted] default 0 for [IsDeleted];

alter table [TAIKHOAN]
add constraint [DF_TAIKHOAN_IsDeleted] default 0 for [IsDeleted];

alter table [TACGIA]
add constraint [DF_TACGIA_IsDeleted] default 0 for [IsDeleted];

alter table [THELOAI]
add constraint [DF_THELOAI_IsDeleted] default 0 for [IsDeleted];

alter table [TUASACH]
add constraint [DF_TUASACH_IsDeleted] default 0 for [IsDeleted];

alter table [TINHTRANG]
add constraint [DF_TINHTRANG_IsDeleted] default 0 for [IsDeleted];

alter table [SACH]
add constraint [DF_SACH_IsDeleted] default 0 for [IsDeleted];

alter table [PHIEUMUON]
add constraint [DF_PHIEUMUON_IsDeleted] default 0 for [IsDeleted];

alter table [PHIEUTRA]
add constraint [DF_PHIEUTRA_IsDeleted] default 0 for [IsDeleted];

alter table [PHIEUTHUTIENPHAT]
add constraint [DF_PHIEUTHUTIENPHAT_IsDeleted] default 0 for [IsDeleted];


-- FK
alter table [TAIKHOAN] add constraint [FK_TAIKHOAN_IDPhanQuyen]
foreign key ([IDPhanQuyen]) references [PHANQUYEN]([ID]);

alter table [ADMIN] add constraint [FK_ADMIN_IDTaiKhoan]
foreign key ([IDTaiKhoan]) references [TAIKHOAN]([ID]);

alter table [DOCGIA] add constraint [FK_DOCGIA_IDTaiKhoan]
foreign key ([IDTaiKhoan]) references [TAIKHOAN]([ID]);

alter table [DOCGIA] add constraint [FK_DOCGIA_IDLoaiDocGia]
foreign key ([IDLoaiDocGia]) references [LOAIDOCGIA]([ID]);

alter table [TUASACH_TACGIA] add constraint [FK_TUASACH_TACGIA_IDTuaSach]
foreign key ([IDTuaSach]) references [TUASACH]([ID]);

alter table [TUASACH_TACGIA] add constraint [FK_TUASACH_TACGIA_IDTacGia]
foreign key ([IDTacGia]) references [TACGIA]([ID]);

alter table [TUASACH_THELOAI] add constraint [FK_TUASACH_THELOAI_IDTuaSach]
foreign key ([IDTuaSach]) references [TUASACH]([ID]);

alter table [TUASACH_THELOAI] add constraint [FK_TUASACH_THELOAI_IDTheLoai]
foreign key ([IDTheLoai]) references [THELOAI]([ID]);

alter table [SACH] add constraint [FK_SACH_IDTuaSach]
foreign key ([IDTuaSach]) references [TUASACH]([ID]);

alter table [SACH] add constraint [FK_SACH_IDTinhTrang]
foreign key ([IDTinhTrang]) references [TINHTRANG]([ID]);

alter table [PHIEUMUON] add constraint [FK_PHIEUMUON_IDDocGia]
foreign key ([IDDocGia]) references [DOCGIA]([ID]);

alter table [CTPHIEUMUON] add constraint [FK_CTPHIEUMUON_IDPhieuMuon]
foreign key ([IDPhieuMuon]) references [PHIEUMUON]([ID]);

alter table [CTPHIEUMUON] add constraint [FK_CTPHIEUMUON_IDSach]
foreign key ([IDSach]) references [SACH]([ID]);

alter table [CTPHIEUTRA] add constraint [FK_CTPHIEUTRA_IDPhieuTra]
foreign key ([IDPhieuTra]) references [PHIEUTRA]([ID]);

alter table [CTPHIEUTRA] add constraint [FK_CTPHIEUTRA_IDPhieuMuon]
foreign key ([IDPhieuMuon]) references [PHIEUMUON]([ID]);

alter table [CTPHIEUTRA] add constraint [FK_CTPHIEUTRA_IDSach]
foreign key ([IDSach]) references [SACH]([ID]);

alter table [CTBCTRATRE] add constraint [FK_CTBCTRATRE_IDBCTraTre]
foreign key ([IDBCTraTre]) references [BCTRATRE]([ID]);

alter table [CTBCTRATRE] add constraint [FK_CTBCTRATRE_IDPhieuTra]
foreign key ([IDPhieuTra]) references [PHIEUTRA]([ID]);

alter table [CTBCMUONSACH] add constraint [FK_CTBCMUONSACH_IDBCMuonSach]
foreign key ([IDBCMuonSach]) references [BCMUONSACH]([ID]);

alter table [CTBCMUONSACH] add constraint [FK_CTBCMUONSACH_IDTheLoai]
foreign key ([IDTheLoai]) references [THELOAI]([ID]);

alter table [PHIEUTHUTIENPHAT] add constraint [FK_PHIEUTHUTIENPHAT_IDDocGia]
foreign key ([IDDocGia]) references [DOCGIA]([ID]);
