-- Run after 001–006. Existing demo accounts retain their current credentials.
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN (N'BranchFlow_Dev', N'BranchFlow_Test', N'BranchFlow_UxReview', N'BranchFlow_GroupDemo')
    THROW 51000, 'Unsupported database: password migration aborted.', 1;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.AppUser', 'MustChangePassword') IS NULL
    ALTER TABLE dbo.AppUser ADD MustChangePassword bit NOT NULL CONSTRAINT DF_AppUser_MustChangePassword DEFAULT 0;
IF COL_LENGTH('dbo.AppUser', 'TemporaryPasswordExpiresAt') IS NULL
    ALTER TABLE dbo.AppUser ADD TemporaryPasswordExpiresAt datetime2(7) NULL;
IF COL_LENGTH('dbo.AppUser', 'CredentialVersion') IS NULL
    ALTER TABLE dbo.AppUser ADD CredentialVersion int NOT NULL CONSTRAINT DF_AppUser_CredentialVersion DEFAULT 0;
IF COL_LENGTH('dbo.AuthSession', 'CredentialVersion') IS NULL
    ALTER TABLE dbo.AuthSession ADD CredentialVersion int NOT NULL CONSTRAINT DF_AuthSession_CredentialVersion DEFAULT 0;
IF OBJECT_ID('dbo.CK_AppUser_TemporaryPassword', 'C') IS NULL
    EXEC(N'ALTER TABLE dbo.AppUser ADD CONSTRAINT CK_AppUser_TemporaryPassword CHECK ((MustChangePassword = 0 AND TemporaryPasswordExpiresAt IS NULL) OR (MustChangePassword = 1 AND TemporaryPasswordExpiresAt IS NOT NULL));');
IF OBJECT_ID('dbo.CK_AppUser_CredentialVersion', 'C') IS NULL
    EXEC(N'ALTER TABLE dbo.AppUser ADD CONSTRAINT CK_AppUser_CredentialVersion CHECK (CredentialVersion >= 0);');
IF OBJECT_ID('dbo.CK_AuthSession_CredentialVersion', 'C') IS NULL
    EXEC(N'ALTER TABLE dbo.AuthSession ADD CONSTRAINT CK_AuthSession_CredentialVersion CHECK (CredentialVersion >= 0);');
IF OBJECT_ID('dbo.PasswordResetRequest', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetRequest (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PasswordResetRequest PRIMARY KEY,
        UserId int NOT NULL REFERENCES dbo.AppUser(Id),
        RequestedAt datetime2(7) NOT NULL,
        ResolvedAt datetime2(7) NULL,
        ResolvedByUserId int NULL REFERENCES dbo.AppUser(Id),
        CONSTRAINT CK_PasswordResetRequest_Resolution CHECK (
            (ResolvedAt IS NULL AND ResolvedByUserId IS NULL) OR
            (ResolvedAt IS NOT NULL AND ResolvedByUserId IS NOT NULL))
    );
    CREATE UNIQUE INDEX UX_PasswordResetRequest_PendingUser ON dbo.PasswordResetRequest(UserId) WHERE ResolvedAt IS NULL;
END;
COMMIT;
