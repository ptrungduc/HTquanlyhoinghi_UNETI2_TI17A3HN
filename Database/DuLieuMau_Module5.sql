-- =====================================================================
-- DỮ LIỆU MẪU ĐẦY ĐỦ CHO TẤT CẢ MODULE (1-5)
-- Database: QuanLyHoiNghiDb trên (localdb)\mssqllocaldb
--
-- Cách dùng: Mở trong SSMS → Execute (F5)
-- Có thể chạy lại nhiều lần (script tự xóa dữ liệu mẫu cũ)
--
-- Mật khẩu tất cả tài khoản: 123456
-- Hash SHA-256 của "123456":
--   8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92
--
-- TrangThai DangKyThamDus lưu dạng SỐ (enum int):
--   0 = ChoDuyet, 1 = DaDuyet, 2 = DaCheckIn
--   3 = HoanThanh, 4 = TuChoi, 5 = DaHuy
-- =====================================================================
USE QuanLyHoiNghiDb;
GO
SET NOCOUNT ON;
BEGIN TRAN;

-- =============================================================
-- XÓA DỮ LIỆU CŨ (theo thứ tự khóa ngoại)
-- =============================================================
DELETE FROM CheckIns;
DELETE FROM DangKyThamDus;
DELETE FROM NguoiThamDus;
DELETE FROM PhienSuKiens;
DELETE FROM SuKiens;
DELETE FROM DiaDiems;
DELETE FROM LoaiSuKiens;
DELETE FROM TaiKhoans WHERE MaTaiKhoan != 'admin_00';

DECLARE @mk NVARCHAR(MAX) = '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92';

-- =============================================================
-- MODULE 1: TÀI KHOẢN, LOẠI SỰ KIỆN, ĐỊA ĐIỂM
-- =============================================================

-- -------- Tài khoản (admin đã có sẵn từ seed) --------
-- Vai trò: Admin, Ban To Chuc, Le Tan, Nguoi Tham Du
INSERT INTO TaiKhoans (MaTaiKhoan, TenDangNhap, MatKhau, HoTen, Email, VaiTro, TrangThai) VALUES
-- Ban tổ chức
('BTC_01', 'btc_hung',  @mk, N'Phạm Văn Hùng',    'hung@uneti.edu.vn',  'Ban To Chuc', 1),
('BTC_02', 'btc_lan',   @mk, N'Nguyễn Thị Lan',    'lan@uneti.edu.vn',   'Ban To Chuc', 1),
-- Lễ tân
('LT_01',  'letan_mai', @mk, N'Trần Thị Mai',      'mai@uneti.edu.vn',   'Le Tan', 1),
-- Người tham dự
('NTD_TK01', 'sv_an',     @mk, N'Nguyễn Văn An',   'an@sv.uneti.edu.vn',     'Nguoi Tham Du', 1),
('NTD_TK02', 'sv_binh',   @mk, N'Trần Thị Bình',   'binh@sv.uneti.edu.vn',   'Nguoi Tham Du', 1),
('NTD_TK03', 'sv_cuong',  @mk, N'Lê Văn Cường',    'cuong@sv.uneti.edu.vn',  'Nguoi Tham Du', 1),
('NTD_TK04', 'sv_dung',   @mk, N'Phạm Thị Dung',   'dung@sv.uneti.edu.vn',   'Nguoi Tham Du', 1),
('NTD_TK05', 'sv_em',     @mk, N'Hoàng Văn Em',    'em@sv.uneti.edu.vn',     'Nguoi Tham Du', 1),
('NTD_TK06', 'sv_phuong', @mk, N'Vũ Thị Phương',   'phuong@sv.uneti.edu.vn', 'Nguoi Tham Du', 1),
-- Tài khoản bị khóa (test Module 1)
('NTD_TK07', 'sv_khoa',   @mk, N'Đỗ Văn Khóa',    'khoa@sv.uneti.edu.vn',   'Nguoi Tham Du', 0);

-- -------- Loại sự kiện --------
INSERT INTO LoaiSuKiens (MaLoaiSuKien, TenLoaiSuKien, MoTa, TrangThai) VALUES
('LSK_01', N'Hội thảo khoa học',     N'Hội thảo nghiên cứu, báo cáo khoa học',   1),
('LSK_02', N'Workshop kỹ năng',      N'Đào tạo kỹ năng thực hành',                1),
('LSK_03', N'Seminar chuyên đề',     N'Báo cáo chuyên đề, chia sẻ kinh nghiệm',  1),
('LSK_04', N'Sự kiện trường (ngừng)', N'Không dùng nữa',                          0);

-- -------- Địa điểm --------
INSERT INTO DiaDiems (MaDiaDiem, TenDiaDiem, DiaChi, SucChuaToiDa, MoTa, TrangThai) VALUES
('DD_01', N'Hội trường A',    N'Tầng 1, Tòa A, UNETI Lĩnh Nam',    200, N'Có máy chiếu, mic', 1),
('DD_02', N'Phòng họp 301',   N'Tầng 3, Tòa B, UNETI Lĩnh Nam',     50, N'Có bảng trắng',     1),
('DD_03', N'Phòng Lab CNTT',  N'Tầng 4, Tòa C, UNETI Minh Khai',    40, N'30 máy tính',        1),
('DD_04', N'Hội trường B (ngừng)', N'Đang sửa chữa',                150, N'',                   0);

-- =============================================================
-- MODULE 2: SỰ KIỆN + PHIÊN SỰ KIỆN
-- =============================================================

-- -------- Sự kiện (đa dạng trạng thái, thời gian xoay quanh hôm nay) --------
INSERT INTO SuKiens (MaSuKien, TenSuKien, MaLoaiSuKien, MaDiaDiem, ThoiGianBatDau, ThoiGianKetThuc, MoTa, TrangThai) VALUES
-- SK1: Hoàn thành, 2 tháng trước
('SK_01', N'Hội thảo AI và Giáo dục',
 'LSK_01', 'DD_01',
 DATEADD(DAY,-60,GETDATE()), DATEADD(DAY,-60,DATEADD(HOUR,5,GETDATE())),
 N'Ứng dụng trí tuệ nhân tạo trong giảng dạy đại học', 'HoanThanh'),

-- SK2: Hoàn thành, 1 tháng trước
('SK_02', N'Workshop Git & GitHub',
 'LSK_02', 'DD_03',
 DATEADD(DAY,-30,GETDATE()), DATEADD(DAY,-30,DATEADD(HOUR,3,GETDATE())),
 N'Thực hành quản lý phiên bản với Git', 'HoanThanh'),

-- SK3: Hoàn thành, 2 tuần trước
('SK_03', N'Seminar An toàn thông tin',
 'LSK_03', 'DD_02',
 DATEADD(DAY,-14,GETDATE()), DATEADD(DAY,-14,DATEADD(HOUR,3,GETDATE())),
 N'Bảo mật ứng dụng web', 'HoanThanh'),

-- SK4: Đang diễn ra (bắt đầu hôm qua, kết thúc ngày mai)
('SK_04', N'Hội thảo Chuyển đổi số Doanh nghiệp',
 'LSK_01', 'DD_01',
 DATEADD(DAY,-1,GETDATE()), DATEADD(DAY,1,GETDATE()),
 N'Chuyển đổi số cho doanh nghiệp vừa và nhỏ', 'DangDienRa'),

-- SK5: Mở đăng ký, 3 ngày tới (gần hạn)
('SK_05', N'Workshop Thuyết trình hiệu quả',
 'LSK_02', 'DD_02',
 DATEADD(DAY,3,GETDATE()), DATEADD(DAY,3,DATEADD(HOUR,4,GETDATE())),
 N'Kỹ năng trình bày và thuyết trình', 'MoDangKy'),

-- SK6: Mở đăng ký, 2 tuần tới
('SK_06', N'Hội thảo Cloud Computing',
 'LSK_01', 'DD_01',
 DATEADD(DAY,14,GETDATE()), DATEADD(DAY,14,DATEADD(HOUR,5,GETDATE())),
 N'Điện toán đám mây AWS/Azure', 'MoDangKy'),

-- SK7: Mở đăng ký, 5 ngày tới (gần hạn)
('SK_07', N'Seminar Khởi nghiệp Sinh viên',
 'LSK_03', 'DD_02',
 DATEADD(DAY,5,GETDATE()), DATEADD(DAY,5,DATEADD(HOUR,3,GETDATE())),
 N'Chia sẻ từ startup thành công', 'MoDangKy'),

-- SK8: Đóng đăng ký
('SK_08', N'Workshop Data Analysis',
 'LSK_02', 'DD_03',
 DATEADD(DAY,10,GETDATE()), DATEADD(DAY,10,DATEADD(HOUR,4,GETDATE())),
 N'Phân tích dữ liệu với Python', 'DongDangKy'),

-- SK9: Nháp
('SK_09', N'Hội thảo IoT (dự kiến)',
 'LSK_01', 'DD_01',
 DATEADD(DAY,30,GETDATE()), DATEADD(DAY,30,DATEADD(HOUR,5,GETDATE())),
 N'Internet of Things trong công nghiệp', 'SapDienRa'),

-- SK10: Đã hủy
('SK_10', N'Workshop React Native (đã hủy)',
 'LSK_02', 'DD_03',
 DATEADD(DAY,20,GETDATE()), DATEADD(DAY,20,DATEADD(HOUR,3,GETDATE())),
 N'Bị hủy do diễn giả bận', 'DaHuy');

-- -------- Phiên sự kiện --------
INSERT INTO PhienSuKiens (MaPhienSuKien, MaSuKien, TenPhien, ThoiGianBatDau, ThoiGianKetThuc, DienGia, NoiDung, TrangThai) VALUES
-- Phiên SK1 (hoàn thành)
('PS_01', 'SK_01', N'Khai mạc & Báo cáo tổng quan',
 DATEADD(DAY,-60,DATEADD(HOUR,8,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 DATEADD(DAY,-60,DATEADD(HOUR,10,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 N'PGS.TS Nguyễn Văn X', N'Tổng quan AI trong giáo dục', 'HoanThanh'),

('PS_02', 'SK_01', N'Báo cáo chuyên đề: ChatGPT',
 DATEADD(DAY,-60,DATEADD(HOUR,10,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 DATEADD(DAY,-60,DATEADD(HOUR,12,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 N'ThS. Trần Thị Y', N'ChatGPT hỗ trợ giảng dạy', 'HoanThanh'),

-- Phiên SK2 (hoàn thành)
('PS_03', 'SK_02', N'Git cơ bản: init, add, commit',
 DATEADD(DAY,-30,DATEADD(HOUR,8,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 DATEADD(DAY,-30,DATEADD(HOUR,10,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 N'KS. Lê Văn Z', N'Thực hành Git local', 'HoanThanh'),

('PS_04', 'SK_02', N'GitHub: push, pull, PR',
 DATEADD(DAY,-30,DATEADD(HOUR,10,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 DATEADD(DAY,-30,DATEADD(HOUR,12,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 N'KS. Lê Văn Z', N'Làm việc nhóm trên GitHub', 'HoanThanh'),

-- Phiên SK4 (đang diễn ra)
('PS_05', 'SK_04', N'Phiên toàn thể: Xu hướng CĐS',
 DATEADD(DAY,-1,DATEADD(HOUR,8,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 DATEADD(DAY,-1,DATEADD(HOUR,11,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 N'TS. Phạm Văn T', N'Xu hướng chuyển đổi số 2026', 'HoanThanh'),

('PS_06', 'SK_04', N'Thảo luận bàn tròn',
 DATEADD(HOUR,14,CAST(CAST(GETDATE() AS DATE) AS DATETIME)),
 DATEADD(HOUR,16,CAST(CAST(GETDATE() AS DATE) AS DATETIME)),
 N'Panel', N'Thảo luận với doanh nghiệp', 'SapDienRa'),

-- Phiên SK5 (sắp tới)
('PS_07', 'SK_05', N'Kỹ năng trình bày slide',
 DATEADD(DAY,3,DATEADD(HOUR,8,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 DATEADD(DAY,3,DATEADD(HOUR,10,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 N'ThS. Hoàng Văn K', N'Thiết kế slide chuyên nghiệp', 'SapDienRa'),

('PS_08', 'SK_05', N'Thực hành thuyết trình',
 DATEADD(DAY,3,DATEADD(HOUR,10,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 DATEADD(DAY,3,DATEADD(HOUR,12,CAST(CAST(GETDATE() AS DATE) AS DATETIME))),
 N'ThS. Hoàng Văn K', N'Mỗi SV trình bày 5 phút', 'SapDienRa');

-- =============================================================
-- MODULE 3: NGƯỜI THAM DỰ + ĐĂNG KÝ
-- =============================================================

-- -------- Người tham dự --------
INSERT INTO NguoiThamDus (MaNguoiThamDu, MaTaiKhoan, HoTen, SoDienThoai, Email, DonViCongTac, TrangThai) VALUES
('NTD_01', 'NTD_TK01', N'Nguyễn Văn An',   '0901000001', 'an@sv.uneti.edu.vn',     N'K17A - CNTT UNETI', 1),
('NTD_02', 'NTD_TK02', N'Trần Thị Bình',   '0901000002', 'binh@sv.uneti.edu.vn',   N'K17A - CNTT UNETI', 1),
('NTD_03', 'NTD_TK03', N'Lê Văn Cường',    '0901000003', 'cuong@sv.uneti.edu.vn',  N'K17B - CNTT UNETI', 1),
('NTD_04', 'NTD_TK04', N'Phạm Thị Dung',   '0901000004', 'dung@sv.uneti.edu.vn',   N'K17B - CNTT UNETI', 1),
('NTD_05', 'NTD_TK05', N'Hoàng Văn Em',    '0901000005', 'em@sv.uneti.edu.vn',     N'K18A - CNTT UNETI', 1),
('NTD_06', 'NTD_TK06', N'Vũ Thị Phương',   '0901000006', 'phuong@sv.uneti.edu.vn', N'K18A - CNTT UNETI', 1);

-- -------- Đăng ký tham dự --------
-- TrangThai: 0=ChoDuyet, 1=DaDuyet, 2=DaCheckIn, 3=HoanThanh, 4=TuChoi, 5=DaHuy

INSERT INTO DangKyThamDus (MaDangKy, MaNguoiThamDu, MaSuKien, NgayDangKy, TrangThai, GhiChu, NgayDuyet, LyDoTuChoiHuy, MaThamDu) VALUES
-- === SK1 (hoàn thành): 5 đăng ký, 4 duyệt, 3 check-in, 1 hủy ===
('DK_01', 'NTD_01', 'SK_01', DATEADD(DAY,-65,GETDATE()), '3', N'', DATEADD(DAY,-63,GETDATE()), N'', 'TD-0001'),
('DK_02', 'NTD_02', 'SK_01', DATEADD(DAY,-64,GETDATE()), '3', N'', DATEADD(DAY,-63,GETDATE()), N'', 'TD-0002'),
('DK_03', 'NTD_03', 'SK_01', DATEADD(DAY,-64,GETDATE()), '3', N'', DATEADD(DAY,-62,GETDATE()), N'', 'TD-0003'),
('DK_04', 'NTD_04', 'SK_01', DATEADD(DAY,-63,GETDATE()), '1', N'', DATEADD(DAY,-62,GETDATE()), N'', 'TD-0004'),
('DK_05', 'NTD_05', 'SK_01', DATEADD(DAY,-63,GETDATE()), '5', N'', NULL, N'Bận thi cuối kỳ', NULL),

-- === SK2 (hoàn thành): 4 đăng ký, 3 duyệt, 2 check-in, 1 từ chối ===
('DK_06', 'NTD_01', 'SK_02', DATEADD(DAY,-35,GETDATE()), '3', N'', DATEADD(DAY,-33,GETDATE()), N'', 'TD-0006'),
('DK_07', 'NTD_02', 'SK_02', DATEADD(DAY,-35,GETDATE()), '3', N'', DATEADD(DAY,-33,GETDATE()), N'', 'TD-0007'),
('DK_08', 'NTD_03', 'SK_02', DATEADD(DAY,-34,GETDATE()), '1', N'', DATEADD(DAY,-32,GETDATE()), N'', 'TD-0008'),
('DK_09', 'NTD_06', 'SK_02', DATEADD(DAY,-34,GETDATE()), '4', N'', NULL, N'Chưa đủ điều kiện tiên quyết', NULL),

-- === SK3 (hoàn thành): 3 đăng ký, 3 duyệt, 2 check-in ===
('DK_10', 'NTD_02', 'SK_03', DATEADD(DAY,-20,GETDATE()), '3', N'', DATEADD(DAY,-18,GETDATE()), N'', 'TD-0010'),
('DK_11', 'NTD_04', 'SK_03', DATEADD(DAY,-19,GETDATE()), '3', N'', DATEADD(DAY,-18,GETDATE()), N'', 'TD-0011'),
('DK_12', 'NTD_05', 'SK_03', DATEADD(DAY,-18,GETDATE()), '1', N'', DATEADD(DAY,-17,GETDATE()), N'', 'TD-0012'),

-- === SK4 (đang diễn ra): 5 đăng ký, 4 duyệt, 2 đã check-in hôm nay ===
('DK_13', 'NTD_01', 'SK_04', DATEADD(DAY,-7,GETDATE()), '2', N'', DATEADD(DAY,-5,GETDATE()), N'', 'TD-0013'),
('DK_14', 'NTD_02', 'SK_04', DATEADD(DAY,-6,GETDATE()), '2', N'', DATEADD(DAY,-5,GETDATE()), N'', 'TD-0014'),
('DK_15', 'NTD_03', 'SK_04', DATEADD(DAY,-6,GETDATE()), '1', N'', DATEADD(DAY,-4,GETDATE()), N'', 'TD-0015'),
('DK_16', 'NTD_05', 'SK_04', DATEADD(DAY,-5,GETDATE()), '1', N'', DATEADD(DAY,-3,GETDATE()), N'', 'TD-0016'),
('DK_17', 'NTD_06', 'SK_04', DATEADD(DAY,-5,GETDATE()), '0', N'', NULL, N'', NULL),

-- === SK5 (mở đăng ký): 3 đăng ký, 1 duyệt, 2 chờ ===
('DK_18', 'NTD_01', 'SK_05', DATEADD(DAY,-2,GETDATE()), '1', N'', DATEADD(DAY,-1,GETDATE()), N'', 'TD-0018'),
('DK_19', 'NTD_04', 'SK_05', DATEADD(DAY,-1,GETDATE()), '0', N'', NULL, N'', NULL),
('DK_20', 'NTD_06', 'SK_05', GETDATE(),                  '0', N'', NULL, N'', NULL),

-- === SK6 (mở đăng ký): 2 đăng ký, chờ duyệt ===
('DK_21', 'NTD_02', 'SK_06', DATEADD(DAY,-1,GETDATE()), '0', N'', NULL, N'', NULL),
('DK_22', 'NTD_03', 'SK_06', GETDATE(),                  '0', N'', NULL, N'', NULL),

-- === SK7 (mở đăng ký): 1 đăng ký ===
('DK_23', 'NTD_05', 'SK_07', GETDATE(),                  '0', N'', NULL, N'', NULL);

-- =============================================================
-- MODULE 4: CHECK-IN
-- =============================================================

INSERT INTO CheckIns (MaCheckIn, MaDangKy, ThoiGianCheckIn, NguoiThucHien, GhiChu, TrangThai) VALUES
-- Check-in SK1 (2 tháng trước): 3 người
('CI_01', 'DK_01', DATEADD(DAY,-60,DATEADD(HOUR,7,CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'admin_00', N'', N'Thành công'),
('CI_02', 'DK_02', DATEADD(DAY,-60,DATEADD(HOUR,7,CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'admin_00', N'', N'Thành công'),
('CI_03', 'DK_03', DATEADD(DAY,-60,DATEADD(HOUR,8,CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'LT_01',    N'Đến muộn 15p', N'Thành công'),

-- Check-in SK2 (1 tháng trước): 2 người
('CI_04', 'DK_06', DATEADD(DAY,-30,DATEADD(HOUR,7,CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'LT_01',    N'', N'Thành công'),
('CI_05', 'DK_07', DATEADD(DAY,-30,DATEADD(HOUR,8,CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'LT_01',    N'', N'Thành công'),

-- Check-in SK3 (2 tuần trước): 2 người
('CI_06', 'DK_10', DATEADD(DAY,-14,DATEADD(HOUR,7,CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'admin_00', N'', N'Thành công'),
('CI_07', 'DK_11', DATEADD(DAY,-14,DATEADD(HOUR,8,CAST(CAST(GETDATE() AS DATE) AS DATETIME))), 'admin_00', N'', N'Thành công'),

-- Check-in SK4 (HÔM NAY): 2 người → Dashboard hiện "Check-in hôm nay = 2"
('CI_08', 'DK_13', DATEADD(HOUR,8,CAST(CAST(GETDATE() AS DATE) AS DATETIME)), 'LT_01', N'', N'Thành công'),
('CI_09', 'DK_14', DATEADD(HOUR,9,CAST(CAST(GETDATE() AS DATE) AS DATETIME)), 'LT_01', N'', N'Thành công');

COMMIT;
PRINT N'=== THÊM DỮ LIỆU MẪU THÀNH CÔNG! ===';
GO

-- =============================================================
-- KIỂM TRA NHANH
-- =============================================================
SELECT 'TaiKhoans' AS Bang, COUNT(*) AS SoLuong FROM TaiKhoans
UNION ALL SELECT 'LoaiSuKiens',   COUNT(*) FROM LoaiSuKiens
UNION ALL SELECT 'DiaDiems',      COUNT(*) FROM DiaDiems
UNION ALL SELECT 'SuKiens',       COUNT(*) FROM SuKiens
UNION ALL SELECT 'PhienSuKiens',  COUNT(*) FROM PhienSuKiens
UNION ALL SELECT 'NguoiThamDus',  COUNT(*) FROM NguoiThamDus
UNION ALL SELECT 'DangKyThamDus', COUNT(*) FROM DangKyThamDus
UNION ALL SELECT 'CheckIns',      COUNT(*) FROM CheckIns;
