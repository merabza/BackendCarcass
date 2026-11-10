-- SSO mode (merabza/SsoLtgGe#6, sso-design.md D11/D14): the Users.SsoSubject column and its unique filtered index.
-- Idempotent: safe to run again. Run on a production database only after a backup (SsoLtgGe#20); dev databases get
-- the column from Recreate Dev Database.
IF COL_LENGTH(N'dbo.Users', N'SsoSubject') IS NULL
    ALTER TABLE dbo.Users ADD SsoSubject nvarchar(64) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_SsoSubject' AND object_id = OBJECT_ID(N'dbo.Users'))
    CREATE UNIQUE INDEX IX_Users_SsoSubject ON dbo.Users (SsoSubject) WHERE SsoSubject IS NOT NULL;
GO
