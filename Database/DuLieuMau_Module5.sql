-- =============================================================
-- CẬP NHẬT CẤU TRÚC CƠ SỞ DỮ LIỆU CHO MODULE 2-5
-- Database: QuanLyHoiNghiDb trên (localdb)\mssqllocaldb
--
-- Script chỉ cập nhật cấu trúc bảng/cột để khớp model hiện tại.
-- Không xóa, thay thế hoặc nạp lại dữ liệu đang có.
-- Các cột mới được điền giá trị mặc định hợp lệ cho bản ghi cũ.
-- =============================================================
USE QuanLyHoiNghiDb;
GO
SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.SuKiens', N'U') IS NULL
        THROW 50001, N'Không tìm thấy bảng dbo.SuKiens.', 1;

    IF OBJECT_ID(N'dbo.PhienSuKiens', N'U') IS NULL
        THROW 50002, N'Không tìm thấy bảng dbo.PhienSuKiens.', 1;

    IF OBJECT_ID(N'dbo.DangKyThamDus', N'U') IS NULL
        THROW 50007, N'Không tìm thấy bảng dbo.DangKyThamDus.', 1;

    IF COL_LENGTH(N'dbo.SuKiens', N'HanDangKy') IS NULL
        ALTER TABLE dbo.SuKiens ADD HanDangKy datetime2 NULL;

    IF COL_LENGTH(N'dbo.SuKiens', N'SoLuongToiDa') IS NULL
        ALTER TABLE dbo.SuKiens ADD SoLuongToiDa int NULL;

    IF COL_LENGTH(N'dbo.PhienSuKiens', N'PhongKhuVuc') IS NULL
        ALTER TABLE dbo.PhienSuKiens ADD PhongKhuVuc nvarchar(100) NULL;

    IF COL_LENGTH(N'dbo.PhienSuKiens', N'SoLuongToiDa') IS NULL
        ALTER TABLE dbo.PhienSuKiens ADD SoLuongToiDa int NULL;

    EXEC sys.sp_executesql N'
        UPDATE s
        SET HanDangKy = COALESCE(s.HanDangKy, s.ThoiGianBatDau),
            SoLuongToiDa = COALESCE(NULLIF(s.SoLuongToiDa, 0), NULLIF(d.SucChuaToiDa, 0), 1)
        FROM dbo.SuKiens AS s
        LEFT JOIN dbo.DiaDiems AS d ON d.MaDiaDiem = s.MaDiaDiem
        WHERE s.HanDangKy IS NULL OR s.SoLuongToiDa IS NULL;';

    EXEC sys.sp_executesql N'
        UPDATE p
        SET PhongKhuVuc = COALESCE(NULLIF(p.PhongKhuVuc, N''''), N''Chưa xác định''),
            SoLuongToiDa = COALESCE(NULLIF(p.SoLuongToiDa, 0), s.SoLuongToiDa, 1)
        FROM dbo.PhienSuKiens AS p
        INNER JOIN dbo.SuKiens AS s ON s.MaSuKien = p.MaSuKien
        WHERE p.PhongKhuVuc IS NULL OR p.SoLuongToiDa IS NULL;';

    DECLARE @invalidSuKien bit = 0;
    EXEC sys.sp_executesql
        N'IF EXISTS (SELECT 1 FROM dbo.SuKiens WHERE HanDangKy IS NULL OR SoLuongToiDa IS NULL OR SoLuongToiDa < 1) SET @invalid = 1;',
        N'@invalid bit OUTPUT',
        @invalidSuKien OUTPUT;

    IF @invalidSuKien = 1
        THROW 50003, N'Không thể tạo giá trị hợp lệ cho cột mới trong SuKiens.', 1;

    DECLARE @invalidPhien bit = 0;
    EXEC sys.sp_executesql
        N'IF EXISTS (SELECT 1 FROM dbo.PhienSuKiens WHERE PhongKhuVuc IS NULL OR SoLuongToiDa IS NULL OR SoLuongToiDa < 1) SET @invalid = 1;',
        N'@invalid bit OUTPUT',
        @invalidPhien OUTPUT;

    IF @invalidPhien = 1
        THROW 50004, N'Không thể tạo giá trị hợp lệ cho cột mới trong PhienSuKiens.', 1;

    EXEC sys.sp_executesql N'ALTER TABLE dbo.SuKiens ALTER COLUMN HanDangKy datetime2 NOT NULL;';
    EXEC sys.sp_executesql N'ALTER TABLE dbo.SuKiens ALTER COLUMN SoLuongToiDa int NOT NULL;';
    EXEC sys.sp_executesql N'ALTER TABLE dbo.PhienSuKiens ALTER COLUMN PhongKhuVuc nvarchar(100) NOT NULL;';
    EXEC sys.sp_executesql N'ALTER TABLE dbo.PhienSuKiens ALTER COLUMN SoLuongToiDa int NOT NULL;';

    DECLARE @trangThaiDangKyType sysname =
    (
        SELECT TYPE_NAME(user_type_id)
        FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.DangKyThamDus')
          AND name = N'TrangThai'
    );

    IF @trangThaiDangKyType IS NULL
        THROW 50008, N'Không tìm thấy cột TrangThai trong dbo.DangKyThamDus.', 1;

    IF @trangThaiDangKyType <> N'int'
    BEGIN
        DECLARE @invalidTrangThaiDangKy bit = 0;
        EXEC sys.sp_executesql
            N'IF EXISTS
              (
                  SELECT 1
                  FROM dbo.DangKyThamDus
                  WHERE TRY_CONVERT(int, TrangThai) IS NULL
                     OR TRY_CONVERT(int, TrangThai) NOT BETWEEN 0 AND 5
              ) SET @invalid = 1;',
            N'@invalid bit OUTPUT',
            @invalidTrangThaiDangKy OUTPUT;

        IF @invalidTrangThaiDangKy = 1
            THROW 50009, N'Trạng thái đăng ký có giá trị không hợp lệ (cần từ 0 đến 5).', 1;

        EXEC sys.sp_executesql N'
            UPDATE dbo.DangKyThamDus
            SET TrangThai = CONVERT(nvarchar(100), TRY_CONVERT(int, TrangThai));';

        EXEC sys.sp_executesql N'ALTER TABLE dbo.DangKyThamDus ALTER COLUMN TrangThai int NOT NULL;';
    END;

    IF OBJECT_ID(N'dbo.DangKyPhiens', N'U') IS NULL
       AND OBJECT_ID(N'dbo.DangKyPhien', N'U') IS NOT NULL
        EXEC sys.sp_rename N'dbo.DangKyPhien', N'DangKyPhiens';

    IF OBJECT_ID(N'dbo.DangKyPhiens', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DangKyPhiens
        (
            MaDangKyPhien nvarchar(450) NOT NULL,
            MaNguoiThamDu nvarchar(450) NOT NULL,
            MaPhien nvarchar(450) NOT NULL,
            NgayDangKy datetime2 NOT NULL,
            TrangThai int NOT NULL,
            CONSTRAINT PK_DangKyPhiens PRIMARY KEY (MaDangKyPhien),
            CONSTRAINT FK_DangKyPhiens_NguoiThamDus_MaNguoiThamDu
                FOREIGN KEY (MaNguoiThamDu) REFERENCES dbo.NguoiThamDus (MaNguoiThamDu),
            CONSTRAINT FK_DangKyPhiens_PhienSuKiens_MaPhien
                FOREIGN KEY (MaPhien) REFERENCES dbo.PhienSuKiens (MaPhienSuKien)
        );
    END;

    DECLARE @trangThaiType sysname =
    (
        SELECT TYPE_NAME(user_type_id)
        FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.DangKyPhiens')
          AND name = N'TrangThai'
    );

    IF @trangThaiType IS NULL
        THROW 50005, N'Không tìm thấy cột TrangThai trong dbo.DangKyPhiens.', 1;

    IF @trangThaiType <> N'int'
    BEGIN
        DECLARE @invalidTrangThai bit = 0;
        EXEC sys.sp_executesql
            N'IF EXISTS
              (
                  SELECT 1
                  FROM dbo.DangKyPhiens
                  WHERE CASE LTRIM(RTRIM(TrangThai))
                      WHEN N''ChoDuyet'' THEN 0
                      WHEN N''DaDuyet'' THEN 1
                      WHEN N''DaCheckIn'' THEN 2
                      WHEN N''HoanThanh'' THEN 3
                      WHEN N''TuChoi'' THEN 4
                      WHEN N''DaHuy'' THEN 5
                      ELSE TRY_CONVERT(int, TrangThai)
                  END IS NULL
              ) SET @invalid = 1;',
            N'@invalid bit OUTPUT',
            @invalidTrangThai OUTPUT;

        IF @invalidTrangThai = 1
            THROW 50006, N'Trạng thái đăng ký phiên có giá trị không thể chuyển sang enum int.', 1;

        EXEC sys.sp_executesql N'
            UPDATE dbo.DangKyPhiens
            SET TrangThai = CONVERT(nvarchar(100),
                CASE LTRIM(RTRIM(TrangThai))
                    WHEN N''ChoDuyet'' THEN 0
                    WHEN N''DaDuyet'' THEN 1
                    WHEN N''DaCheckIn'' THEN 2
                    WHEN N''HoanThanh'' THEN 3
                    WHEN N''TuChoi'' THEN 4
                    WHEN N''DaHuy'' THEN 5
                    ELSE TRY_CONVERT(int, TrangThai)
                END);';

        EXEC sys.sp_executesql N'ALTER TABLE dbo.DangKyPhiens ALTER COLUMN TrangThai int NOT NULL;';
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes i
        JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE i.object_id = OBJECT_ID(N'dbo.DangKyPhiens')
          AND ic.key_ordinal = 1
          AND c.name = N'MaNguoiThamDu'
    )
        EXEC sys.sp_executesql N'CREATE INDEX IX_DangKyPhiens_MaNguoiThamDu ON dbo.DangKyPhiens (MaNguoiThamDu);';

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes i
        JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE i.object_id = OBJECT_ID(N'dbo.DangKyPhiens')
          AND ic.key_ordinal = 1
          AND c.name = N'MaPhien'
    )
        EXEC sys.sp_executesql N'CREATE INDEX IX_DangKyPhiens_MaPhien ON dbo.DangKyPhiens (MaPhien);';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- Kiểm tra schema sau khi cập nhật.
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE (TABLE_NAME = N'SuKiens' AND COLUMN_NAME IN (N'HanDangKy', N'SoLuongToiDa'))
   OR (TABLE_NAME = N'PhienSuKiens' AND COLUMN_NAME IN (N'PhongKhuVuc', N'SoLuongToiDa'))
   OR (TABLE_NAME = N'DangKyThamDus' AND COLUMN_NAME = N'TrangThai')
   OR (TABLE_NAME = N'DangKyPhiens')
ORDER BY TABLE_NAME, ORDINAL_POSITION;
