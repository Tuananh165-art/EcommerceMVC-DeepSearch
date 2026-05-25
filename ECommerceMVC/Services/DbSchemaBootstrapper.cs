using ECommerceMVC.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Services;

public static class DbSchemaBootstrapper
{
    public static async Task EnsurePersistentCartAsync(Hshop2023Context db, CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF OBJECT_ID(N'dbo.GioHangItem', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.GioHangItem
                (
                    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_GioHangItem PRIMARY KEY,
                    MaKH NVARCHAR(20) NOT NULL,
                    MaHH INT NOT NULL,
                    SoLuong INT NOT NULL CONSTRAINT DF_GioHangItem_SoLuong DEFAULT (1),
                    CreatedAt DATETIME NOT NULL CONSTRAINT DF_GioHangItem_CreatedAt DEFAULT (GETDATE()),
                    UpdatedAt DATETIME NOT NULL CONSTRAINT DF_GioHangItem_UpdatedAt DEFAULT (GETDATE())
                );
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'UX_GioHangItem_MaKH_MaHH'
                  AND object_id = OBJECT_ID(N'dbo.GioHangItem')
            )
            BEGIN
                CREATE UNIQUE INDEX UX_GioHangItem_MaKH_MaHH ON dbo.GioHangItem(MaKH, MaHH);
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.foreign_keys
                WHERE name = N'FK_GioHangItem_KhachHang'
            )
            BEGIN
                ALTER TABLE dbo.GioHangItem
                    ADD CONSTRAINT FK_GioHangItem_KhachHang
                    FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH)
                    ON DELETE CASCADE;
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.foreign_keys
                WHERE name = N'FK_GioHangItem_HangHoa'
            )
            BEGIN
                ALTER TABLE dbo.GioHangItem
                    ADD CONSTRAINT FK_GioHangItem_HangHoa
                    FOREIGN KEY (MaHH) REFERENCES dbo.HangHoa(MaHH)
                    ON DELETE CASCADE;
            END;
            """;

        await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public static async Task EnsurePasswordResetOtpAsync(Hshop2023Context db, CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF OBJECT_ID(N'dbo.PasswordResetOtp', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.PasswordResetOtp
                (
                    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PasswordResetOtp PRIMARY KEY,
                    MaKH NVARCHAR(20) NOT NULL,
                    Email NVARCHAR(50) NOT NULL,
                    OtpHash NVARCHAR(128) NOT NULL,
                    ExpiresAt DATETIME NOT NULL,
                    UsedAt DATETIME NULL,
                    AttemptCount INT NOT NULL CONSTRAINT DF_PasswordResetOtp_AttemptCount DEFAULT (0),
                    CreatedAt DATETIME NOT NULL CONSTRAINT DF_PasswordResetOtp_CreatedAt DEFAULT (GETDATE()),
                    CONSTRAINT FK_PasswordResetOtp_KhachHang FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH)
                );
            END;

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PasswordResetOtp_MaKH_CreatedAt' AND object_id = OBJECT_ID(N'dbo.PasswordResetOtp'))
            BEGIN
                CREATE INDEX IX_PasswordResetOtp_MaKH_CreatedAt ON dbo.PasswordResetOtp(MaKH, CreatedAt DESC);
            END;

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PasswordResetOtp_Email_CreatedAt' AND object_id = OBJECT_ID(N'dbo.PasswordResetOtp'))
            BEGIN
                CREATE INDEX IX_PasswordResetOtp_Email_CreatedAt ON dbo.PasswordResetOtp(Email, CreatedAt DESC);
            END;
            """;

        await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
