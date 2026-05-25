SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

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
