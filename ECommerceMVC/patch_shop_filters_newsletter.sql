SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRAN;

    IF COL_LENGTH('dbo.HangHoa', 'MauSac') IS NULL
    BEGIN
        ALTER TABLE dbo.HangHoa ADD MauSac NVARCHAR(30) NULL;
    END

    IF OBJECT_ID('dbo.NewsletterSubscription', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.NewsletterSubscription
        (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NewsletterSubscription PRIMARY KEY,
            Email NVARCHAR(254) NOT NULL,
            CreatedAt DATETIME NOT NULL CONSTRAINT DF_NewsletterSubscription_CreatedAt DEFAULT (GETDATE()),
            IsActive BIT NOT NULL CONSTRAINT DF_NewsletterSubscription_IsActive DEFAULT (1)
        );

        CREATE UNIQUE INDEX UX_NewsletterSubscription_Email ON dbo.NewsletterSubscription(Email);
    END

    IF OBJECT_ID('dbo.ProductReview', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProductReview
        (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductReview PRIMARY KEY,
            MaHH INT NOT NULL,
            MaKH NVARCHAR(20) NOT NULL,
            SoSao INT NOT NULL,
            NoiDung NVARCHAR(500) NOT NULL,
            NgayTao DATETIME NOT NULL CONSTRAINT DF_ProductReview_NgayTao DEFAULT (GETDATE()),
            CONSTRAINT CK_ProductReview_SoSao CHECK (SoSao BETWEEN 1 AND 5),
            CONSTRAINT FK_ProductReview_HangHoa FOREIGN KEY (MaHH) REFERENCES dbo.HangHoa(MaHH),
            CONSTRAINT FK_ProductReview_KhachHang FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH)
        );

        CREATE UNIQUE INDEX UX_ProductReview_MaHH_MaKH ON dbo.ProductReview(MaHH, MaKH);
    END

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Patch failed: %s', 16, 1, @Err);
END CATCH;