/*
===============================================================================
 FreeGency — FINAL SEED DATA (SQL Server / SSMS)
===============================================================================
 Purpose:
   Populates a CLEAN FreeGency_DB with a fully-linked demo dataset where every
   table holds at least 8 rows and all required columns are populated.

 Target database:
   A brand-new FreeGency_DB with the EF migrations applied. No prior seed
   scripts are required — this file also inserts the catalog taxonomy using the
   same deterministic category IDs the TaxonomySeeder relies on, so a later API
   startup will find taxonomy by name and skip.

 Prerequisites:
   1. Database FreeGency_DB exists and EF migrations are applied.
   2. Database contains NO rows yet (this script is not an upsert).

 Idempotency:
   If [identity].[Users] already contains any user, the script prints a message
   and exits without touching anything.

 Demo login:
   Every seeded user signs in with email <login>@freegency.local and password:

       Password123!

   (ASP.NET Core Identity V3 hash baked into @PasswordHash below.)

 User.code:
   Kept NULL on purpose — the column is protected by EF Core's EncryptColumn
   and SQL has no way to produce a valid encrypted value.

 Nulls:
   All NOT NULL columns are populated. NULL is used ONLY where the schema
   requires an exclusive or optional value (e.g. Wallet.OwnerTeamId on a
   personal wallet, ChatRoom.ProjectId on a team-main room, XOR profile-scoped
   rows such as UserSkill.ClientProfileId / UserSkill.DeveloperProfileId).

 Enum storage note:
   Most enums are stored as nvarchar (their names, e.g. N'Team', N'InProgress',
   N'Public'). Two are stored as int: Notification.Type and
   paymentTransactions.Status (PaymentStatus: 0=Pending 1=Succeeded 2=Failed
   3=Cancelled 4=Refunded).
===============================================================================
*/

USE [FreeGency_DB];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;

IF EXISTS (SELECT 1 FROM [identity].[Users])
BEGIN
    PRINT N'Final seed already applied (users exist). Skipping.';
    RETURN;
END;

BEGIN TRANSACTION;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @By  NVARCHAR(256) = N'seed-sql';
DECLARE @PasswordHash NVARCHAR(MAX) =
    N'AQAAAAIAAYagAAAAEHfsErsnfAc+hYS1KXdNaS/O1H4FvDDzeB/zacTHbbT1paRNRVbLhIrLqV6HHo3YqQ==';

/* -------------------------------------------------------------------------- */
/* 1. CATALOG TAXONOMY (fallback when catalog is empty)                        */
/* -------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM [catalog].[Categories])
BEGIN
    PRINT N'Catalog empty — inserting taxonomy fallback.';

    INSERT INTO [catalog].[Categories] ([Id], [Name], [NameEn], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        ('11111111-1111-1111-1111-000000000001', N'تطوير الويب',               N'Web Development',        @Now, @By, 0),
        ('11111111-1111-1111-1111-000000000002', N'تطوير الموبايل',             N'Mobile Development',     @Now, @By, 0),
        ('11111111-1111-1111-1111-000000000003', N'برمجيات مخصصة / SaaS',       N'Custom Software / SaaS', @Now, @By, 0),
        ('11111111-1111-1111-1111-000000000004', N'تصميم واجهات البرمجيات',     N'UI/UX for Software',     @Now, @By, 0),
        ('11111111-1111-1111-1111-000000000005', N'الباك اند وواجهات البرمجة',  N'Backend & APIs',         @Now, @By, 0),
        ('11111111-1111-1111-1111-000000000006', N'ديف أوبس والسحابة',          N'DevOps & Cloud',         @Now, @By, 0),
        ('11111111-1111-1111-1111-000000000007', N'ضمان الجودة والاختبار',      N'QA & Testing',           @Now, @By, 0),
        ('11111111-1111-1111-1111-000000000008', N'البيانات والذكاء الاصطناعي', N'Data & AI',              @Now, @By, 0),
        ('11111111-1111-1111-1111-000000000009', N'الأمن السيبراني',            N'Cybersecurity',          @Now, @By, 0),
        ('11111111-1111-1111-1111-00000000000a', N'تطوير التجارة الإلكترونية',  N'E-commerce Development', @Now, @By, 0);

    INSERT INTO [catalog].[Specialties] ([Id], [NameEn], [NameAr], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        ('44444444-4444-4444-4444-000000000001', N'Frontend Development',       N'تطوير الواجهات الأمامية',     @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000002', N'Backend Development',         N'تطوير الباك اند',             @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000003', N'Full Stack Development',      N'تطوير Full Stack',           @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000004', N'API Development',             N'تطوير واجهات البرمجة',       @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000005', N'iOS Development',             N'تطوير iOS',                  @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000006', N'Android Development',         N'تطوير Android',              @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000007', N'Cross-Platform Mobile Development', N'تطوير موبايل Cross-Platform', @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000008', N'SaaS Product Development',    N'تطوير منتجات SaaS',          @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000009', N'Product UI Design',           N'تصميم واجهات المنتج',        @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000010', N'Design Systems',              N'أنظمة التصميم',              @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000011', N'CI/CD Pipelines',             N'خطوط CI/CD',                 @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000012', N'Cloud Infrastructure',        N'بنية سحابية',                @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000013', N'Automated Testing',           N'اختبار آلي',                 @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000014', N'Manual Testing',              N'اختبار يدوي',                @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000015', N'Machine Learning',            N'تعلم الآلة',                 @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000016', N'Data Engineering / ETL',      N'هندسة البيانات / ETL',      @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000017', N'Penetration Testing',         N'اختبار الاختراق',           @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000018', N'Application Security',        N'أمان التطبيقات',            @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000019', N'Shopify Development',         N'تطوير Shopify',             @Now, @By, 0),
        ('44444444-4444-4444-4444-000000000020', N'Payment Gateway Integration', N'تكامل بوابات الدفع',        @Now, @By, 0);

    INSERT INTO [catalog].[Skills] ([Id], [Name], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        ('44444444-4444-4444-4444-000000001001', N'React',        @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001002', N'TypeScript',   @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001003', N'Node.js',      @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001004', N'C#',           @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001005', N'ASP.NET Core', @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001006', N'Flutter',      @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001007', N'Dart',         @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001008', N'SQL Server',   @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001009', N'Docker',       @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001010', N'Kubernetes',   @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001011', N'AWS',          @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001012', N'Figma',        @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001013', N'Python',       @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001014', N'Pandas',       @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001015', N'Cypress',      @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001016', N'Postman',      @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001017', N'OWASP',        @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001018', N'Burp Suite',   @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001019', N'Shopify',      @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001020', N'Stripe',       @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001021', N'Swift',        @Now, @By, 0),
        ('44444444-4444-4444-4444-000000001022', N'Kotlin',       @Now, @By, 0);

    INSERT INTO [catalog].[CategorySpecialties]
        ([Id], [CategoryId], [SpecialtyId], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), '11111111-1111-1111-1111-000000000001', '44444444-4444-4444-4444-000000000001', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000001', '44444444-4444-4444-4444-000000000002', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000001', '44444444-4444-4444-4444-000000000003', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000001', '44444444-4444-4444-4444-000000000004', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000002', '44444444-4444-4444-4444-000000000005', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000002', '44444444-4444-4444-4444-000000000006', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000002', '44444444-4444-4444-4444-000000000007', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000003', '44444444-4444-4444-4444-000000000008', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000004', '44444444-4444-4444-4444-000000000009', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000004', '44444444-4444-4444-4444-000000000010', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000005', '44444444-4444-4444-4444-000000000002', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000005', '44444444-4444-4444-4444-000000000004', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000006', '44444444-4444-4444-4444-000000000011', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000006', '44444444-4444-4444-4444-000000000012', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000007', '44444444-4444-4444-4444-000000000013', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000007', '44444444-4444-4444-4444-000000000014', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000008', '44444444-4444-4444-4444-000000000015', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000008', '44444444-4444-4444-4444-000000000016', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000009', '44444444-4444-4444-4444-000000000017', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000009', '44444444-4444-4444-4444-000000000018', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-00000000000a', '44444444-4444-4444-4444-000000000019', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-00000000000a', '44444444-4444-4444-4444-000000000020', @Now, @By, 0);

    INSERT INTO [catalog].[SpecialtySkills]
        ([Id], [SpecialtyId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), '44444444-4444-4444-4444-000000000001', '44444444-4444-4444-4444-000000001001', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000001', '44444444-4444-4444-4444-000000001002', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000002', '44444444-4444-4444-4444-000000001003', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000002', '44444444-4444-4444-4444-000000001004', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000002', '44444444-4444-4444-4444-000000001005', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000003', '44444444-4444-4444-4444-000000001001', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000003', '44444444-4444-4444-4444-000000001002', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000003', '44444444-4444-4444-4444-000000001005', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000004', '44444444-4444-4444-4444-000000001003', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000004', '44444444-4444-4444-4444-000000001005', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000005', '44444444-4444-4444-4444-000000001021', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000006', '44444444-4444-4444-4444-000000001022', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000007', '44444444-4444-4444-4444-000000001006', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000007', '44444444-4444-4444-4444-000000001007', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000008', '44444444-4444-4444-4444-000000001005', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000008', '44444444-4444-4444-4444-000000001009', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000009', '44444444-4444-4444-4444-000000001012', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000010', '44444444-4444-4444-4444-000000001012', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000010', '44444444-4444-4444-4444-000000001001', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000011', '44444444-4444-4444-4444-000000001009', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000011', '44444444-4444-4444-4444-000000001010', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000012', '44444444-4444-4444-4444-000000001011', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000012', '44444444-4444-4444-4444-000000001009', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000012', '44444444-4444-4444-4444-000000001010', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000013', '44444444-4444-4444-4444-000000001015', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000013', '44444444-4444-4444-4444-000000001016', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000014', '44444444-4444-4444-4444-000000001016', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000015', '44444444-4444-4444-4444-000000001013', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000015', '44444444-4444-4444-4444-000000001014', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000016', '44444444-4444-4444-4444-000000001013', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000016', '44444444-4444-4444-4444-000000001014', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000016', '44444444-4444-4444-4444-000000001008', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000017', '44444444-4444-4444-4444-000000001017', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000017', '44444444-4444-4444-4444-000000001018', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000018', '44444444-4444-4444-4444-000000001017', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000018', '44444444-4444-4444-4444-000000001018', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000019', '44444444-4444-4444-4444-000000001019', @Now, @By, 0),
        (NEWID(), '44444444-4444-4444-4444-000000000020', '44444444-4444-4444-4444-000000001020', @Now, @By, 0);
END;

/* -------------------------------------------------------------------------- */
/* 2. TAXONOMY LOOKUPS (work whether taxonomy came from this script or the API) */
/* -------------------------------------------------------------------------- */

DECLARE @CatWeb     UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Web Development');
DECLARE @CatMobile  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Mobile Development');
DECLARE @CatSaas    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Custom Software / SaaS');
DECLARE @CatUiux    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'UI/UX for Software');
DECLARE @CatBackend UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Backend & APIs');
DECLARE @CatDevops  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'DevOps & Cloud');
DECLARE @CatQa      UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'QA & Testing');
DECLARE @CatDataai  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Data & AI');
DECLARE @CatSecurity UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Cybersecurity');
DECLARE @CatEcom    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'E-commerce Development');

IF @CatWeb IS NULL OR @CatMobile IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50001, N'Required categories (Web Development / Mobile Development) not found. Run API taxonomy seed first.', 1;
END;

IF @CatSaas IS NULL SET @CatSaas = @CatWeb;
IF @CatUiux IS NULL SET @CatUiux = @CatWeb;
IF @CatBackend IS NULL SET @CatBackend = @CatWeb;
IF @CatDevops IS NULL SET @CatDevops = @CatWeb;
IF @CatQa IS NULL SET @CatQa = @CatWeb;
IF @CatDataai IS NULL SET @CatDataai = @CatWeb;
IF @CatSecurity IS NULL SET @CatSecurity = @CatWeb;
IF @CatEcom IS NULL SET @CatEcom = @CatWeb;

DECLARE @SpecFrontend   UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Frontend Development');
DECLARE @SpecBackend    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Backend Development');
DECLARE @SpecFullStack  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Full Stack Development');
DECLARE @SpecApi        UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'API Development');
DECLARE @SpecIos        UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'iOS Development');
DECLARE @SpecAndroid    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Android Development');
DECLARE @SpecCrossMobile UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Cross-Platform Mobile Development');
DECLARE @SpecSaas       UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'SaaS Product Development');
DECLARE @SpecProductUi  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Product UI Design');
DECLARE @SpecDesignSystems UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Design Systems');
DECLARE @SpecCiCd       UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'CI/CD Pipelines');
DECLARE @SpecCloudInfra UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Cloud Infrastructure');
DECLARE @SpecAutomated  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Automated Testing');
DECLARE @SpecManual     UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Manual Testing');
DECLARE @SpecMl         UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Machine Learning');
DECLARE @SpecDataEng    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Data Engineering / ETL');
DECLARE @SpecPentest    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Penetration Testing');
DECLARE @SpecAppSec     UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Application Security');
DECLARE @SpecShopify    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Shopify Development');
DECLARE @SpecPayment    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Payment Gateway Integration');

IF @SpecFrontend IS NULL SET @SpecFrontend = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] ORDER BY [NameEn]);
IF @SpecBackend IS NULL SET @SpecBackend = @SpecFrontend;
IF @SpecFullStack IS NULL SET @SpecFullStack = @SpecFrontend;
IF @SpecApi IS NULL SET @SpecApi = @SpecFrontend;
IF @SpecIos IS NULL SET @SpecIos = @SpecFrontend;
IF @SpecAndroid IS NULL SET @SpecAndroid = @SpecFrontend;
IF @SpecCrossMobile IS NULL SET @SpecCrossMobile = @SpecFrontend;
IF @SpecSaas IS NULL SET @SpecSaas = @SpecFrontend;
IF @SpecProductUi IS NULL SET @SpecProductUi = @SpecFrontend;
IF @SpecDesignSystems IS NULL SET @SpecDesignSystems = @SpecFrontend;
IF @SpecCiCd IS NULL SET @SpecCiCd = @SpecFrontend;
IF @SpecCloudInfra IS NULL SET @SpecCloudInfra = @SpecFrontend;
IF @SpecAutomated IS NULL SET @SpecAutomated = @SpecFrontend;
IF @SpecManual IS NULL SET @SpecManual = @SpecFrontend;
IF @SpecMl IS NULL SET @SpecMl = @SpecFrontend;
IF @SpecDataEng IS NULL SET @SpecDataEng = @SpecFrontend;
IF @SpecPentest IS NULL SET @SpecPentest = @SpecFrontend;
IF @SpecAppSec IS NULL SET @SpecAppSec = @SpecFrontend;
IF @SpecShopify IS NULL SET @SpecShopify = @SpecFrontend;
IF @SpecPayment IS NULL SET @SpecPayment = @SpecFrontend;

DECLARE @SkillReact    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'React');
DECLARE @SkillTs       UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'TypeScript');
DECLARE @SkillNode     UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Node.js');
DECLARE @SkillCsharp   UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'C#');
DECLARE @SkillAsp      UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'ASP.NET Core');
DECLARE @SkillFlutter  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Flutter');
DECLARE @SkillDart     UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Dart');
DECLARE @SkillSql      UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'SQL Server');
DECLARE @SkillDocker   UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Docker');
DECLARE @SkillK8s      UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Kubernetes');
DECLARE @SkillAws      UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'AWS');
DECLARE @SkillFigma    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Figma');
DECLARE @SkillPython   UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Python');
DECLARE @SkillPandas   UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Pandas');
DECLARE @SkillCypress  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Cypress');
DECLARE @SkillPostman  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Postman');
DECLARE @SkillOwasp    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'OWASP');
DECLARE @SkillBurp     UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Burp Suite');
DECLARE @SkillShopify  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Shopify');
DECLARE @SkillStripe   UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Stripe');

IF @SkillReact IS NULL SET @SkillReact = (SELECT TOP 1 [Id] FROM [catalog].[Skills] ORDER BY [Name]);
IF @SkillTs IS NULL SET @SkillTs = @SkillReact;
IF @SkillNode IS NULL SET @SkillNode = @SkillReact;
IF @SkillCsharp IS NULL SET @SkillCsharp = @SkillReact;
IF @SkillAsp IS NULL SET @SkillAsp = @SkillReact;
IF @SkillFlutter IS NULL SET @SkillFlutter = @SkillReact;
IF @SkillDart IS NULL SET @SkillDart = @SkillReact;
IF @SkillSql IS NULL SET @SkillSql = @SkillReact;
IF @SkillDocker IS NULL SET @SkillDocker = @SkillReact;
IF @SkillK8s IS NULL SET @SkillK8s = @SkillReact;
IF @SkillAws IS NULL SET @SkillAws = @SkillReact;
IF @SkillFigma IS NULL SET @SkillFigma = @SkillReact;
IF @SkillPython IS NULL SET @SkillPython = @SkillReact;
IF @SkillPandas IS NULL SET @SkillPandas = @SkillReact;
IF @SkillCypress IS NULL SET @SkillCypress = @SkillReact;
IF @SkillPostman IS NULL SET @SkillPostman = @SkillReact;
IF @SkillOwasp IS NULL SET @SkillOwasp = @SkillReact;
IF @SkillBurp IS NULL SET @SkillBurp = @SkillReact;
IF @SkillShopify IS NULL SET @SkillShopify = @SkillReact;
IF @SkillStripe IS NULL SET @SkillStripe = @SkillReact;

/* -------------------------------------------------------------------------- */
/* 3. ROLES + USERS + PROFILES                                                 */
/* -------------------------------------------------------------------------- */

DECLARE @RoleClient   UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000001';
DECLARE @RoleDeveloper UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000002';
DECLARE @RoleAdmin    UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000003';
DECLARE @RoleModerator UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000004';
DECLARE @RoleSupport  UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000005';
DECLARE @RoleBilling  UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000006';
DECLARE @RoleContent  UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000007';
DECLARE @RoleQa       UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000008';

INSERT INTO [identity].[Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES
    (@RoleClient,     N'Client',         N'CLIENT',         CONVERT(NVARCHAR(36), NEWID())),
    (@RoleDeveloper,  N'Developer',      N'DEVELOPER',      CONVERT(NVARCHAR(36), NEWID())),
    (@RoleAdmin,      N'Admin',          N'ADMIN',          CONVERT(NVARCHAR(36), NEWID())),
    (@RoleModerator,  N'Moderator',      N'MODERATOR',      CONVERT(NVARCHAR(36), NEWID())),
    (@RoleSupport,    N'Support',        N'SUPPORT',        CONVERT(NVARCHAR(36), NEWID())),
    (@RoleBilling,    N'BillingManager', N'BILLINGMANAGER', CONVERT(NVARCHAR(36), NEWID())),
    (@RoleContent,    N'ContentManager', N'CONTENTMANAGER', CONVERT(NVARCHAR(36), NEWID())),
    (@RoleQa,         N'QA',             N'QA',             CONVERT(NVARCHAR(36), NEWID()));

DECLARE @Client1 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001';
DECLARE @Client2 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002';
DECLARE @Client3 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003';
DECLARE @Client4 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000004';
DECLARE @Client5 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000005';
DECLARE @Client6 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000006';
DECLARE @Client7 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000007';
DECLARE @Client8 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000008';
DECLARE @Dev1    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000011';
DECLARE @Dev2    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000012';
DECLARE @Dev3    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000013';
DECLARE @Dev4    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000014';
DECLARE @Dev5    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000015';
DECLARE @Dev6    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000016';
DECLARE @Dev7    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000017';
DECLARE @Dev8    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000018';

INSERT INTO [identity].[Users]
(
    [Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
    [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp],
    [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled],
    [LockoutEnabled], [AccessFailedCount],
    [FristName], [LastName], [IsVerified], [HasCompletedOnboarding],
    [ActiveProfileMode], [Country], [code],
    [CreatedAt], [CreatedBy], [IsDeleted]
)
VALUES
(@Client1, N'client1@freegency.local', N'CLIENT1@FREEGENCY.LOCAL', N'client1@freegency.local', N'CLIENT1@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201000000001', 1, 0, 1, 0,
 N'Ahmed', N'Hassan', 1, 1, N'Client', N'Egypt', NULL, @Now, @By, 0),
(@Client2, N'client2@freegency.local', N'CLIENT2@FREEGENCY.LOCAL', N'client2@freegency.local', N'CLIENT2@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201000000002', 1, 0, 1, 0,
 N'Sara', N'Ali', 1, 1, N'Client', N'Egypt', NULL, @Now, @By, 0),
(@Client3, N'client3@freegency.local', N'CLIENT3@FREEGENCY.LOCAL', N'client3@freegency.local', N'CLIENT3@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201000000003', 1, 0, 1, 0,
 N'Omar', N'Nabil', 1, 1, N'Client', N'Saudi Arabia', NULL, @Now, @By, 0),
(@Client4, N'client4@freegency.local', N'CLIENT4@FREEGENCY.LOCAL', N'client4@freegency.local', N'CLIENT4@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201000000004', 1, 0, 1, 0,
 N'Mariam', N'Fathy', 1, 1, N'Client', N'UAE', NULL, @Now, @By, 0),
(@Client5, N'client5@freegency.local', N'CLIENT5@FREEGENCY.LOCAL', N'client5@freegency.local', N'CLIENT5@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201000000005', 1, 0, 1, 0,
 N'Khaled', N'Mostafa', 1, 1, N'Client', N'Kuwait', NULL, @Now, @By, 0),
(@Client6, N'client6@freegency.local', N'CLIENT6@FREEGENCY.LOCAL', N'client6@freegency.local', N'CLIENT6@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201000000006', 1, 0, 1, 0,
 N'Nour', N'El-Din', 1, 1, N'Client', N'Qatar', NULL, @Now, @By, 0),
(@Client7, N'client7@freegency.local', N'CLIENT7@FREEGENCY.LOCAL', N'client7@freegency.local', N'CLIENT7@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201000000007', 1, 0, 1, 0,
 N'Hala', N'Adel', 1, 1, N'Client', N'Jordan', NULL, @Now, @By, 0),
(@Client8, N'client8@freegency.local', N'CLIENT8@FREEGENCY.LOCAL', N'client8@freegency.local', N'CLIENT8@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201000000008', 1, 0, 1, 0,
 N'Tarek', N'Samir', 1, 1, N'Client', N'Morocco', NULL, @Now, @By, 0),
(@Dev1, N'dev1@freegency.local', N'DEV1@FREEGENCY.LOCAL', N'dev1@freegency.local', N'DEV1@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201100000001', 1, 0, 1, 0,
 N'Youssef', N'Kamal', 1, 1, N'Developer', N'Egypt', NULL, @Now, @By, 0),
(@Dev2, N'dev2@freegency.local', N'DEV2@FREEGENCY.LOCAL', N'dev2@freegency.local', N'DEV2@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201100000002', 1, 0, 1, 0,
 N'Nour', N'Salem', 1, 1, N'Developer', N'Egypt', NULL, @Now, @By, 0),
(@Dev3, N'dev3@freegency.local', N'DEV3@FREEGENCY.LOCAL', N'dev3@freegency.local', N'DEV3@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201100000003', 1, 0, 1, 0,
 N'Layla', N'Farid', 1, 1, N'Developer', N'UAE', NULL, @Now, @By, 0),
(@Dev4, N'dev4@freegency.local', N'DEV4@FREEGENCY.LOCAL', N'dev4@freegency.local', N'DEV4@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201100000004', 1, 0, 1, 0,
 N'Karim', N'Adel', 1, 1, N'Developer', N'Egypt', NULL, @Now, @By, 0),
(@Dev5, N'dev5@freegency.local', N'DEV5@FREEGENCY.LOCAL', N'dev5@freegency.local', N'DEV5@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201100000005', 1, 0, 1, 0,
 N'Mona', N'Zaki', 1, 1, N'Developer', N'Egypt', NULL, @Now, @By, 0),
(@Dev6, N'dev6@freegency.local', N'DEV6@FREEGENCY.LOCAL', N'dev6@freegency.local', N'DEV6@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201100000006', 1, 0, 1, 0,
 N'Adam', N'Yassin', 1, 1, N'Developer', N'Jordan', NULL, @Now, @By, 0),
(@Dev7, N'dev7@freegency.local', N'DEV7@FREEGENCY.LOCAL', N'dev7@freegency.local', N'DEV7@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201100000007', 1, 0, 1, 0,
 N'Salma', N'Refaat', 1, 1, N'Developer', N'Egypt', NULL, @Now, @By, 0),
(@Dev8, N'dev8@freegency.local', N'DEV8@FREEGENCY.LOCAL', N'dev8@freegency.local', N'DEV8@FREEGENCY.LOCAL',
 1, @PasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()),
 N'+201100000008', 1, 0, 1, 0,
 N'Fares', N'Gamal', 1, 1, N'Developer', N'Qatar', NULL, @Now, @By, 0);

DECLARE @Cp1 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000001';
DECLARE @Cp2 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000002';
DECLARE @Cp3 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000003';
DECLARE @Cp4 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000004';
DECLARE @Cp5 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000005';
DECLARE @Cp6 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000006';
DECLARE @Cp7 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000007';
DECLARE @Cp8 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000008';
DECLARE @Dp1 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000011';
DECLARE @Dp2 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000012';
DECLARE @Dp3 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000013';
DECLARE @Dp4 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000014';
DECLARE @Dp5 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000015';
DECLARE @Dp6 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000016';
DECLARE @Dp7 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000017';
DECLARE @Dp8 UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000018';

INSERT INTO [identity].[ClientProfiles]
    ([Id], [UserId], [ProfileImage], [Bio], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(@Cp1, @Client1, N'https://cdn.example.com/seed/cp1.jpg', N'Product founder looking for solid delivery teams.', 4.50, 2, @Now, @By, 0),
(@Cp2, @Client2, N'https://cdn.example.com/seed/cp2.jpg', N'Startup CTO outsourcing product builds.', 4.80, 1, @Now, @By, 0),
(@Cp3, @Client3, N'https://cdn.example.com/seed/cp3.jpg', N'Agency client for web and mobile apps.', 5.00, 1, @Now, @By, 0),
(@Cp4, @Client4, N'https://cdn.example.com/seed/cp4.jpg', N'Mobile-first startup founder.', 4.30, 2, @Now, @By, 0),
(@Cp5, @Client5, N'https://cdn.example.com/seed/cp5.jpg', N'Data-driven product owner.', 4.60, 1, @Now, @By, 0),
(@Cp6, @Client6, N'https://cdn.example.com/seed/cp6.jpg', N'E-commerce store operator.', 4.90, 3, @Now, @By, 0),
(@Cp7, @Client7, N'https://cdn.example.com/seed/cp7.jpg', N'Digital agency client for DevOps work.', 4.20, 1, @Now, @By, 0),
(@Cp8, @Client8, N'https://cdn.example.com/seed/cp8.jpg', N'Security-conscious platform owner.', 4.70, 2, @Now, @By, 0);

INSERT INTO [identity].[DeveloperProfiles]
    ([Id], [UserId], [ProfileImage], [Bio], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(@Dp1, @Dev1, N'https://cdn.example.com/seed/dp1.jpg', N'Full-stack lead. React + .NET.', 4.90, 3, @Now, @By, 0),
(@Dp2, @Dev2, N'https://cdn.example.com/seed/dp2.jpg', N'Frontend engineer specializing in React/TypeScript.', 4.70, 2, @Now, @By, 0),
(@Dp3, @Dev3, N'https://cdn.example.com/seed/dp3.jpg', N'Mobile engineer — Flutter & React Native.', 4.60, 2, @Now, @By, 0),
(@Dp4, @Dev4, N'https://cdn.example.com/seed/dp4.jpg', N'Backend / API engineer.', 4.40, 1, @Now, @By, 0),
(@Dp5, @Dev5, N'https://cdn.example.com/seed/dp5.jpg', N'QA + DevOps hybrid.', 4.20, 1, @Now, @By, 0),
(@Dp6, @Dev6, N'https://cdn.example.com/seed/dp6.jpg', N'Cloud infrastructure specialist.', 4.80, 2, @Now, @By, 0),
(@Dp7, @Dev7, N'https://cdn.example.com/seed/dp7.jpg', N'UI/UX designer and design systems builder.', 4.75, 2, @Now, @By, 0),
(@Dp8, @Dev8, N'https://cdn.example.com/seed/dp8.jpg', N'Data engineer and ML practitioner.', 4.55, 1, @Now, @By, 0);

INSERT INTO [dbo].[clientNotificationSettings]
    ([Id], [ProfileId], [NewMessageInApp], [NewMessageEmail], [ProposalReceivedInApp], [ProposalReceivedEmail],
     [MilestoneAddedInApp], [MilestoneAddedEmail], [WalletUpdatedInApp], [WalletUpdatedEmail],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @Cp1, 1, 0, 1, 1, 1, 0, 1, 1, @Now, @By, 0),
(NEWID(), @Cp2, 1, 0, 1, 0, 1, 1, 1, 1, @Now, @By, 0),
(NEWID(), @Cp3, 1, 1, 1, 0, 1, 0, 1, 1, @Now, @By, 0),
(NEWID(), @Cp4, 1, 0, 1, 1, 1, 0, 1, 0, @Now, @By, 0),
(NEWID(), @Cp5, 1, 1, 1, 0, 1, 1, 1, 1, @Now, @By, 0),
(NEWID(), @Cp6, 1, 0, 1, 0, 1, 0, 1, 1, @Now, @By, 0),
(NEWID(), @Cp7, 1, 0, 1, 1, 1, 0, 1, 0, @Now, @By, 0),
(NEWID(), @Cp8, 1, 1, 1, 0, 1, 1, 1, 1, @Now, @By, 0);

INSERT INTO [dbo].[developerNotificationSettings]
    ([Id], [ProfileId], [MessagesInApp], [MessagesEmail], [ProjectsInApp], [ProjectsEmail],
     [MilestonesInApp], [MilestonesEmail], [WalletInApp], [WalletEmail], [TeamsInApp], [TeamsEmail],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @Dp1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, @Now, @By, 0),
(NEWID(), @Dp2, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, @Now, @By, 0),
(NEWID(), @Dp3, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, @Now, @By, 0),
(NEWID(), @Dp4, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, @Now, @By, 0),
(NEWID(), @Dp5, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, @Now, @By, 0),
(NEWID(), @Dp6, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, @Now, @By, 0),
(NEWID(), @Dp7, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, @Now, @By, 0),
(NEWID(), @Dp8, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 4. IDENTITY EXTRAS (user roles, claims, logins, tokens, refresh tokens)     */
/* -------------------------------------------------------------------------- */

INSERT INTO [identity].[UserRoles] ([UserId], [RoleId])
VALUES
(@Client1, @RoleClient), (@Client2, @RoleClient), (@Client3, @RoleClient), (@Client4, @RoleClient),
(@Client5, @RoleClient), (@Client6, @RoleClient), (@Client7, @RoleClient), (@Client8, @RoleClient),
(@Dev1, @RoleDeveloper), (@Dev2, @RoleDeveloper), (@Dev3, @RoleDeveloper), (@Dev4, @RoleDeveloper),
(@Dev5, @RoleDeveloper), (@Dev6, @RoleDeveloper), (@Dev7, @RoleDeveloper), (@Dev8, @RoleDeveloper);

INSERT INTO [identity].[UserClaims] ([UserId], [ClaimType], [ClaimValue])
VALUES
(@Client1, N'profile', N'complete'), (@Client2, N'profile', N'complete'),
(@Client3, N'profile', N'complete'), (@Client4, N'profile', N'complete'),
(@Client5, N'profile', N'complete'), (@Client6, N'profile', N'complete'),
(@Client7, N'profile', N'complete'), (@Client8, N'profile', N'complete'),
(@Dev1, N'profile', N'complete'), (@Dev2, N'profile', N'complete'),
(@Dev3, N'profile', N'complete'), (@Dev4, N'profile', N'complete'),
(@Dev5, N'profile', N'complete'), (@Dev6, N'profile', N'complete'),
(@Dev7, N'profile', N'complete'), (@Dev8, N'profile', N'complete');

INSERT INTO [identity].[RoleClaims] ([RoleId], [ClaimType], [ClaimValue])
VALUES
(@RoleAdmin, N'permission', N'manage_platform'),
(@RoleModerator, N'permission', N'moderate_content'),
(@RoleSupport, N'permission', N'resolve_tickets'),
(@RoleBilling, N'permission', N'manage_billing'),
(@RoleContent, N'permission', N'publish_content'),
(@RoleQa, N'permission', N'review_reports'),
(@RoleClient, N'permission', N'post_projects'),
(@RoleDeveloper, N'permission', N'apply_projects');

INSERT INTO [identity].[UserLogins] ([LoginProvider], [ProviderKey], [ProviderDisplayName], [UserId])
VALUES
(N'Google', N'google-seed-000001', N'Google', @Client1),
(N'Google', N'google-seed-000002', N'Google', @Client2),
(N'Google', N'google-seed-000003', N'Google', @Client3),
(N'Google', N'google-seed-000004', N'Google', @Client4),
(N'Google', N'google-seed-000005', N'Google', @Client5),
(N'Google', N'google-seed-000006', N'Google', @Client6),
(N'Google', N'google-seed-000007', N'Google', @Client7),
(N'Google', N'google-seed-000008', N'Google', @Client8);

INSERT INTO [identity].[UserTokens] ([UserId], [LoginProvider], [Name], [Value])
VALUES
(@Client1, N'Google', N'AccessToken', N'seed-access-token-000001'),
(@Client2, N'Google', N'AccessToken', N'seed-access-token-000002'),
(@Client3, N'Google', N'AccessToken', N'seed-access-token-000003'),
(@Client4, N'Google', N'AccessToken', N'seed-access-token-000004'),
(@Client5, N'Google', N'AccessToken', N'seed-access-token-000005'),
(@Client6, N'Google', N'AccessToken', N'seed-access-token-000006'),
(@Client7, N'Google', N'AccessToken', N'seed-access-token-000007'),
(@Client8, N'Google', N'AccessToken', N'seed-access-token-000008');

INSERT INTO [dbo].[RefreshTokens] ([UserId], [Token], [CreatedOn], [ExpiresOn], [RevokedOn])
VALUES
(@Dev1, N'refresh-token-seed-000001', DATEADD(DAY, -1, @Now), DATEADD(DAY, 13, @Now), NULL),
(@Dev2, N'refresh-token-seed-000002', DATEADD(DAY, -1, @Now), DATEADD(DAY, 13, @Now), NULL),
(@Dev3, N'refresh-token-seed-000003', DATEADD(DAY, -1, @Now), DATEADD(DAY, 13, @Now), NULL),
(@Dev4, N'refresh-token-seed-000004', DATEADD(DAY, -1, @Now), DATEADD(DAY, 13, @Now), NULL),
(@Dev5, N'refresh-token-seed-000005', DATEADD(DAY, -1, @Now), DATEADD(DAY, 13, @Now), NULL),
(@Dev6, N'refresh-token-seed-000006', DATEADD(DAY, -1, @Now), DATEADD(DAY, 13, @Now), NULL),
(@Dev7, N'refresh-token-seed-000007', DATEADD(DAY, -1, @Now), DATEADD(DAY, 13, @Now), NULL),
(@Dev8, N'refresh-token-seed-000008', DATEADD(DAY, -1, @Now), DATEADD(DAY, 13, @Now), NULL);

/* -------------------------------------------------------------------------- */
/* 5. USER PROFILE LINKS (interests, specialties, skills)                      */
/* -------------------------------------------------------------------------- */

INSERT INTO [identity].[UserInterests]
    ([Id], [ClientProfileId], [DeveloperProfileId], [CategoryId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Cp1, NULL, @CatWeb,     @Now, @By, 0),
    (NEWID(), @Cp1, NULL, @CatSaas,    @Now, @By, 0),
    (NEWID(), @Cp2, NULL, @CatWeb,     @Now, @By, 0),
    (NEWID(), @Cp2, NULL, @CatBackend, @Now, @By, 0),
    (NEWID(), @Cp3, NULL, @CatMobile,  @Now, @By, 0),
    (NEWID(), @Cp3, NULL, @CatWeb,     @Now, @By, 0),
    (NEWID(), @Cp4, NULL, @CatMobile,  @Now, @By, 0),
    (NEWID(), @Cp4, NULL, @CatSaas,    @Now, @By, 0),
    (NEWID(), @Cp5, NULL, @CatDataai,  @Now, @By, 0),
    (NEWID(), @Cp5, NULL, @CatBackend, @Now, @By, 0),
    (NEWID(), @Cp6, NULL, @CatEcom,    @Now, @By, 0),
    (NEWID(), @Cp6, NULL, @CatWeb,     @Now, @By, 0),
    (NEWID(), @Cp7, NULL, @CatDevops,  @Now, @By, 0),
    (NEWID(), @Cp7, NULL, @CatSecurity,@Now, @By, 0),
    (NEWID(), @Cp8, NULL, @CatSecurity,@Now, @By, 0),
    (NEWID(), @Cp8, NULL, @CatBackend, @Now, @By, 0),
    (NEWID(), NULL, @Dp1, @CatWeb,     @Now, @By, 0),
    (NEWID(), NULL, @Dp1, @CatBackend, @Now, @By, 0),
    (NEWID(), NULL, @Dp2, @CatWeb,     @Now, @By, 0),
    (NEWID(), NULL, @Dp2, @CatUiux,    @Now, @By, 0),
    (NEWID(), NULL, @Dp3, @CatMobile,  @Now, @By, 0),
    (NEWID(), NULL, @Dp4, @CatBackend, @Now, @By, 0),
    (NEWID(), NULL, @Dp5, @CatQa,      @Now, @By, 0),
    (NEWID(), NULL, @Dp5, @CatDevops,  @Now, @By, 0),
    (NEWID(), NULL, @Dp6, @CatDevops,  @Now, @By, 0),
    (NEWID(), NULL, @Dp7, @CatUiux,    @Now, @By, 0),
    (NEWID(), NULL, @Dp7, @CatWeb,     @Now, @By, 0),
    (NEWID(), NULL, @Dp8, @CatDataai,  @Now, @By, 0);

INSERT INTO [identity].[UserSpecialties]
    ([Id], [ClientProfileId], [DeveloperProfileId], [SpecialtyId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Cp1, NULL, @SpecFullStack,  @Now, @By, 0),
    (NEWID(), @Cp2, NULL, @SpecBackend,    @Now, @By, 0),
    (NEWID(), @Cp3, NULL, @SpecCrossMobile,@Now, @By, 0),
    (NEWID(), @Cp4, NULL, @SpecSaas,       @Now, @By, 0),
    (NEWID(), @Cp5, NULL, @SpecDataEng,    @Now, @By, 0),
    (NEWID(), @Cp6, NULL, @SpecPayment,    @Now, @By, 0),
    (NEWID(), @Cp7, NULL, @SpecCloudInfra, @Now, @By, 0),
    (NEWID(), @Cp8, NULL, @SpecAppSec,     @Now, @By, 0),
    (NEWID(), NULL, @Dp1, @SpecFullStack,  @Now, @By, 0),
    (NEWID(), NULL, @Dp1, @SpecBackend,    @Now, @By, 0),
    (NEWID(), NULL, @Dp2, @SpecFrontend,   @Now, @By, 0),
    (NEWID(), NULL, @Dp3, @SpecCrossMobile,@Now, @By, 0),
    (NEWID(), NULL, @Dp3, @SpecAndroid,    @Now, @By, 0),
    (NEWID(), NULL, @Dp4, @SpecBackend,    @Now, @By, 0),
    (NEWID(), NULL, @Dp4, @SpecApi,        @Now, @By, 0),
    (NEWID(), NULL, @Dp5, @SpecAutomated,  @Now, @By, 0),
    (NEWID(), NULL, @Dp5, @SpecManual,     @Now, @By, 0),
    (NEWID(), NULL, @Dp6, @SpecCiCd,       @Now, @By, 0),
    (NEWID(), NULL, @Dp6, @SpecCloudInfra, @Now, @By, 0),
    (NEWID(), NULL, @Dp7, @SpecProductUi,  @Now, @By, 0),
    (NEWID(), NULL, @Dp7, @SpecDesignSystems, @Now, @By, 0),
    (NEWID(), NULL, @Dp8, @SpecMl,         @Now, @By, 0),
    (NEWID(), NULL, @Dp8, @SpecDataEng,    @Now, @By, 0);

INSERT INTO [identity].[UserSkills]
    ([Id], [ClientProfileId], [DeveloperProfileId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Cp1, NULL, @SkillReact,   @Now, @By, 0),
    (NEWID(), @Cp2, NULL, @SkillCsharp,  @Now, @By, 0),
    (NEWID(), @Cp3, NULL, @SkillFlutter, @Now, @By, 0),
    (NEWID(), @Cp4, NULL, @SkillNode,    @Now, @By, 0),
    (NEWID(), @Cp5, NULL, @SkillPython,  @Now, @By, 0),
    (NEWID(), @Cp6, NULL, @SkillShopify, @Now, @By, 0),
    (NEWID(), @Cp7, NULL, @SkillDocker,  @Now, @By, 0),
    (NEWID(), @Cp8, NULL, @SkillOwasp,   @Now, @By, 0),
    (NEWID(), NULL, @Dp1, @SkillReact,   @Now, @By, 0),
    (NEWID(), NULL, @Dp1, @SkillCsharp,  @Now, @By, 0),
    (NEWID(), NULL, @Dp1, @SkillAsp,     @Now, @By, 0),
    (NEWID(), NULL, @Dp2, @SkillReact,   @Now, @By, 0),
    (NEWID(), NULL, @Dp2, @SkillTs,      @Now, @By, 0),
    (NEWID(), NULL, @Dp2, @SkillFigma,   @Now, @By, 0),
    (NEWID(), NULL, @Dp3, @SkillFlutter, @Now, @By, 0),
    (NEWID(), NULL, @Dp3, @SkillDart,    @Now, @By, 0),
    (NEWID(), NULL, @Dp4, @SkillCsharp,  @Now, @By, 0),
    (NEWID(), NULL, @Dp4, @SkillNode,    @Now, @By, 0),
    (NEWID(), NULL, @Dp4, @SkillSql,     @Now, @By, 0),
    (NEWID(), NULL, @Dp5, @SkillCypress, @Now, @By, 0),
    (NEWID(), NULL, @Dp5, @SkillPostman, @Now, @By, 0),
    (NEWID(), NULL, @Dp5, @SkillDocker,  @Now, @By, 0),
    (NEWID(), NULL, @Dp6, @SkillDocker,  @Now, @By, 0),
    (NEWID(), NULL, @Dp6, @SkillK8s,     @Now, @By, 0),
    (NEWID(), NULL, @Dp6, @SkillAws,     @Now, @By, 0),
    (NEWID(), NULL, @Dp7, @SkillFigma,   @Now, @By, 0),
    (NEWID(), NULL, @Dp7, @SkillTs,      @Now, @By, 0),
    (NEWID(), NULL, @Dp8, @SkillPython,  @Now, @By, 0),
    (NEWID(), NULL, @Dp8, @SkillPandas,  @Now, @By, 0),
    (NEWID(), NULL, @Dp8, @SkillSql,     @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 6. TEAMS                                                                    */
/* -------------------------------------------------------------------------- */

DECLARE @Team1 UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000001';
DECLARE @Team2 UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000002';
DECLARE @Team3 UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000003';
DECLARE @Team4 UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000004';
DECLARE @Team5 UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000005';
DECLARE @Team6 UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000006';
DECLARE @Team7 UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000007';
DECLARE @Team8 UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000008';

INSERT INTO [teams].[Teams]
    ([Id], [OwnerUserId], [Name], [Logo], [Cover], [TeamCode], [AboutUs],
     [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@Team1, @Dev1, N'Nile Code Studio', N'https://cdn.example.com/seed/team1-logo.png',
     N'https://cdn.example.com/seed/team1-cover.png', N'NLECODE01',
     N'Full-stack web and SaaS product team.', 4.80, 3, @Now, @By, 0),
    (@Team2, @Dev2, N'Pixel Crafters', N'https://cdn.example.com/seed/team2-logo.png',
     N'https://cdn.example.com/seed/team2-cover.png', N'PXLCRFT01',
     N'Frontend and UI/UX craft team.', 4.60, 2, @Now, @By, 0),
    (@Team3, @Dev3, N'Mobiflex', N'https://cdn.example.com/seed/team3-logo.png',
     N'https://cdn.example.com/seed/team3-cover.png', N'MOBIFLEX1',
     N'Cross-platform mobile development.', 4.70, 2, @Now, @By, 0),
    (@Team4, @Dev4, N'API Forge', N'https://cdn.example.com/seed/team4-logo.png',
     N'https://cdn.example.com/seed/team4-cover.png', N'APIFRG001',
     N'Backend services and API design.', 4.40, 1, @Now, @By, 0),
    (@Team5, @Dev5, N'Quality First Labs', N'https://cdn.example.com/seed/team5-logo.png',
     N'https://cdn.example.com/seed/team5-cover.png', N'QLTYFRST1',
     N'Automation and manual QA specialists.', 4.20, 1, @Now, @By, 0),
    (@Team6, @Dev6, N'CloudNest', N'https://cdn.example.com/seed/team6-logo.png',
     N'https://cdn.example.com/seed/team6-cover.png', N'CLDNEST01',
     N'DevOps, CI/CD and cloud infrastructure.', 4.80, 2, @Now, @By, 0),
    (@Team7, @Dev7, N'Design Orbit', N'https://cdn.example.com/seed/team7-logo.png',
     N'https://cdn.example.com/seed/team7-cover.png', N'DSGORBIT1',
     N'Product UI and design systems.', 4.75, 2, @Now, @By, 0),
    (@Team8, @Dev8, N'Data Minds', N'https://cdn.example.com/seed/team8-logo.png',
     N'https://cdn.example.com/seed/team8-cover.png', N'DATAMIND1',
     N'Data engineering and machine learning.', 4.55, 1, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 7. TEAM MEMBERS                                                             */
/* -------------------------------------------------------------------------- */

INSERT INTO [teams].[TeamMembers]
    ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Team1, @Dev1, N'TeamLeader', N'Full-Stack Lead',    N'2024-01-10', @Now, @By, 0),
    (NEWID(), @Team1, @Dev2, N'TeamMember', N'Frontend Engineer',  N'2024-02-14', @Now, @By, 0),
    (NEWID(), @Team1, @Dev7, N'TeamMember', N'UI Designer',        N'2024-03-01', @Now, @By, 0),
    (NEWID(), @Team2, @Dev2, N'TeamLeader', N'Frontend Lead',      N'2024-01-20', @Now, @By, 0),
    (NEWID(), @Team2, @Dev3, N'TeamMember', N'Mobile Engineer',    N'2024-04-02', @Now, @By, 0),
    (NEWID(), @Team2, @Dev8, N'TeamMember', N'Data Engineer',      N'2024-05-18', @Now, @By, 0),
    (NEWID(), @Team3, @Dev3, N'TeamLeader', N'Mobile Lead',        N'2024-01-05', @Now, @By, 0),
    (NEWID(), @Team3, @Dev1, N'TeamMember', N'Full-Stack Engineer',N'2024-02-22', @Now, @By, 0),
    (NEWID(), @Team3, @Dev6, N'TeamMember', N'Cloud Engineer',     N'2024-03-15', @Now, @By, 0),
    (NEWID(), @Team4, @Dev4, N'TeamLeader', N'Backend Lead',       N'2024-01-12', @Now, @By, 0),
    (NEWID(), @Team4, @Dev5, N'TeamMember', N'QA Engineer',        N'2024-06-01', @Now, @By, 0),
    (NEWID(), @Team4, @Dev8, N'TeamMember', N'Data Engineer',      N'2024-06-20', @Now, @By, 0),
    (NEWID(), @Team5, @Dev5, N'TeamLeader', N'QA Lead',            N'2024-02-08', @Now, @By, 0),
    (NEWID(), @Team5, @Dev6, N'TeamMember', N'DevOps Engineer',    N'2024-07-11', @Now, @By, 0),
    (NEWID(), @Team6, @Dev6, N'TeamLeader', N'DevOps Lead',        N'2024-01-25', @Now, @By, 0),
    (NEWID(), @Team6, @Dev7, N'TeamMember', N'UI Designer',        N'2024-05-09', @Now, @By, 0),
    (NEWID(), @Team7, @Dev7, N'TeamLeader', N'Design Lead',        N'2024-01-30', @Now, @By, 0),
    (NEWID(), @Team7, @Dev2, N'TeamMember', N'Frontend Engineer',  N'2024-08-04', @Now, @By, 0),
    (NEWID(), @Team7, @Dev4, N'TeamMember', N'Backend Engineer',   N'2024-08-15', @Now, @By, 0),
    (NEWID(), @Team8, @Dev8, N'TeamLeader', N'Data Lead',          N'2024-02-01', @Now, @By, 0),
    (NEWID(), @Team8, @Dev5, N'TeamMember', N'QA Engineer',        N'2024-09-10', @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 8. TEAM CATEGORIES / SKILLS / SPECIALTIES                                   */
/* -------------------------------------------------------------------------- */

INSERT INTO [teams].[TeamCategories]
    ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Team1, @CatWeb,     1, @Now, @By, 0),
    (NEWID(), @Team1, @CatSaas,    0, @Now, @By, 0),
    (NEWID(), @Team2, @CatWeb,     1, @Now, @By, 0),
    (NEWID(), @Team2, @CatUiux,    0, @Now, @By, 0),
    (NEWID(), @Team3, @CatMobile,  1, @Now, @By, 0),
    (NEWID(), @Team3, @CatBackend, 0, @Now, @By, 0),
    (NEWID(), @Team4, @CatBackend, 1, @Now, @By, 0),
    (NEWID(), @Team4, @CatWeb,     0, @Now, @By, 0),
    (NEWID(), @Team5, @CatQa,      1, @Now, @By, 0),
    (NEWID(), @Team5, @CatBackend, 0, @Now, @By, 0),
    (NEWID(), @Team6, @CatDevops,  1, @Now, @By, 0),
    (NEWID(), @Team6, @CatBackend, 0, @Now, @By, 0),
    (NEWID(), @Team7, @CatUiux,    1, @Now, @By, 0),
    (NEWID(), @Team7, @CatWeb,     0, @Now, @By, 0),
    (NEWID(), @Team8, @CatDataai,  1, @Now, @By, 0),
    (NEWID(), @Team8, @CatBackend, 0, @Now, @By, 0);

INSERT INTO [teams].[TeamSkills]
    ([Id], [TeamId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Team1, @SkillReact,  @Now, @By, 0),
    (NEWID(), @Team1, @SkillAsp,    @Now, @By, 0),
    (NEWID(), @Team2, @SkillReact,  @Now, @By, 0),
    (NEWID(), @Team2, @SkillTs,     @Now, @By, 0),
    (NEWID(), @Team3, @SkillFlutter,@Now, @By, 0),
    (NEWID(), @Team3, @SkillDart,   @Now, @By, 0),
    (NEWID(), @Team4, @SkillCsharp, @Now, @By, 0),
    (NEWID(), @Team4, @SkillNode,   @Now, @By, 0),
    (NEWID(), @Team5, @SkillCypress,@Now, @By, 0),
    (NEWID(), @Team5, @SkillPostman,@Now, @By, 0),
    (NEWID(), @Team6, @SkillDocker, @Now, @By, 0),
    (NEWID(), @Team6, @SkillAws,    @Now, @By, 0),
    (NEWID(), @Team7, @SkillFigma,  @Now, @By, 0),
    (NEWID(), @Team7, @SkillReact,  @Now, @By, 0),
    (NEWID(), @Team8, @SkillPython, @Now, @By, 0),
    (NEWID(), @Team8, @SkillPandas, @Now, @By, 0);

INSERT INTO [teams].[TeamSpecialties]
    ([Id], [TeamId], [SpecialtyId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Team1, @SpecFullStack,  @Now, @By, 0),
    (NEWID(), @Team1, @SpecBackend,    @Now, @By, 0),
    (NEWID(), @Team2, @SpecFrontend,   @Now, @By, 0),
    (NEWID(), @Team2, @SpecProductUi,  @Now, @By, 0),
    (NEWID(), @Team3, @SpecCrossMobile,@Now, @By, 0),
    (NEWID(), @Team3, @SpecAndroid,    @Now, @By, 0),
    (NEWID(), @Team4, @SpecBackend,    @Now, @By, 0),
    (NEWID(), @Team4, @SpecApi,        @Now, @By, 0),
    (NEWID(), @Team5, @SpecAutomated,  @Now, @By, 0),
    (NEWID(), @Team5, @SpecManual,     @Now, @By, 0),
    (NEWID(), @Team6, @SpecCiCd,       @Now, @By, 0),
    (NEWID(), @Team6, @SpecCloudInfra, @Now, @By, 0),
    (NEWID(), @Team7, @SpecProductUi,  @Now, @By, 0),
    (NEWID(), @Team7, @SpecDesignSystems, @Now, @By, 0),
    (NEWID(), @Team8, @SpecMl,         @Now, @By, 0),
    (NEWID(), @Team8, @SpecDataEng,    @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 9. TEAM JOBS + TEAM JOB SKILLS                                              */
/* -------------------------------------------------------------------------- */

DECLARE @TeamJob1 UNIQUEIDENTIFIER = 'e0e0e0e0-e0e0-e0e0-e0e0-000000000001';
DECLARE @TeamJob2 UNIQUEIDENTIFIER = 'e0e0e0e0-e0e0-e0e0-e0e0-000000000002';
DECLARE @TeamJob3 UNIQUEIDENTIFIER = 'e0e0e0e0-e0e0-e0e0-e0e0-000000000003';
DECLARE @TeamJob4 UNIQUEIDENTIFIER = 'e0e0e0e0-e0e0-e0e0-e0e0-000000000004';
DECLARE @TeamJob5 UNIQUEIDENTIFIER = 'e0e0e0e0-e0e0-e0e0-e0e0-000000000005';
DECLARE @TeamJob6 UNIQUEIDENTIFIER = 'e0e0e0e0-e0e0-e0e0-e0e0-000000000006';
DECLARE @TeamJob7 UNIQUEIDENTIFIER = 'e0e0e0e0-e0e0-e0e0-e0e0-000000000007';
DECLARE @TeamJob8 UNIQUEIDENTIFIER = 'e0e0e0e0-e0e0-e0e0-e0e0-000000000008';
DECLARE @TeamJob9 UNIQUEIDENTIFIER = 'e0e0e0e0-e0e0-e0e0-e0e0-000000000009';

INSERT INTO [teams].[TeamJobs]
    ([Id], [TeamId], [Title], [Description], [Status], [CreatedByUserId], [ClosedAt],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@TeamJob1, @Team1, N'Senior Full-Stack Developer',
     N'Own end-to-end delivery of web products across React and .NET stacks.', N'open', @Dev1, NULL,
     @Now, @By, 0),
    (@TeamJob2, @Team1, N'React Frontend Engineer',
     N'Build performant component libraries and product UI with React and TypeScript.', N'open', @Dev1, NULL,
     @Now, @By, 0),
    (@TeamJob3, @Team2, N'UI Designer',
     N'Produce high-fidelity product screens in Figma for web and mobile.', N'open', @Dev2, NULL,
     @Now, @By, 0),
    (@TeamJob4, @Team3, N'Flutter Developer',
     N'Develop cross-platform mobile apps for iOS and Android with Flutter.', N'open', @Dev3, NULL,
     @Now, @By, 0),
    (@TeamJob5, @Team4, N'.NET Backend Engineer',
     N'Design REST APIs and data models with ASP.NET Core and SQL Server.', N'open', @Dev4, NULL,
     @Now, @By, 0),
    (@TeamJob6, @Team5, N'Automation QA',
     N'Write and maintain end-to-end test suites with Cypress and Postman.', N'closed', @Dev5, DATEADD(DAY, -30, @Now),
     @Now, @By, 0),
    (@TeamJob7, @Team6, N'DevOps Engineer',
     N'Manage CI/CD pipelines and AWS infrastructure for client projects.', N'open', @Dev6, NULL,
     @Now, @By, 0),
    (@TeamJob8, @Team7, N'Design Systems Specialist',
     N'Maintain a reusable token-based design system across products.', N'completed', @Dev7, DATEADD(DAY, -15, @Now),
     @Now, @By, 0),
    (@TeamJob9, @Team8, N'Data Engineer',
     N'Build ETL pipelines and ML feature stores with Python and Pandas.', N'open', @Dev8, NULL,
     @Now, @By, 0);

INSERT INTO [teams].[TeamJobSkills]
    ([Id], [TeamJobId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @TeamJob1, @SkillReact,  @Now, @By, 0),
    (NEWID(), @TeamJob1, @SkillAsp,    @Now, @By, 0),
    (NEWID(), @TeamJob2, @SkillReact,  @Now, @By, 0),
    (NEWID(), @TeamJob2, @SkillTs,     @Now, @By, 0),
    (NEWID(), @TeamJob3, @SkillFigma,  @Now, @By, 0),
    (NEWID(), @TeamJob4, @SkillFlutter,@Now, @By, 0),
    (NEWID(), @TeamJob4, @SkillDart,   @Now, @By, 0),
    (NEWID(), @TeamJob5, @SkillCsharp, @Now, @By, 0),
    (NEWID(), @TeamJob5, @SkillSql,    @Now, @By, 0),
    (NEWID(), @TeamJob6, @SkillCypress,@Now, @By, 0),
    (NEWID(), @TeamJob6, @SkillPostman,@Now, @By, 0),
    (NEWID(), @TeamJob7, @SkillDocker, @Now, @By, 0),
    (NEWID(), @TeamJob7, @SkillAws,    @Now, @By, 0),
    (NEWID(), @TeamJob8, @SkillFigma,  @Now, @By, 0),
    (NEWID(), @TeamJob9, @SkillPython, @Now, @By, 0),
    (NEWID(), @TeamJob9, @SkillPandas, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 10. TEAM JOIN REQUESTS                                                      */
/* -------------------------------------------------------------------------- */

INSERT INTO [teams].[TeamJoinRequests]
    ([Id], [TeamId], [TeamJobId], [UserId], [CoverLetter], [Job], [Status],
     [RequestedAt], [ResponseAt], [RespondedByUserId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Team1, @TeamJob2, @Dev4, N'Strong React background and API integration experience.', N'React Developer',
     N'pending', @Now, NULL, NULL, @Now, @By, 0),
    (NEWID(), @Team1, @TeamJob1, @Dev6, N'Full-stack shipping history; comfortable with cloud deploys.', N'Full-Stack Developer',
     N'Accepted', DATEADD(DAY, -20, @Now), DATEADD(DAY, -18, @Now), CONVERT(NVARCHAR(36), @Dev1), @Now, @By, 0),
    (NEWID(), @Team2, NULL,       @Dev1, N'Looking to contribute senior full-stack expertise.', NULL,
     N'pending', DATEADD(DAY, -10, @Now), NULL, NULL, @Now, @By, 0),
    (NEWID(), @Team2, NULL,       @Dev5, N'QA-first mindset for your frontend products.', NULL,
     N'pending', DATEADD(DAY, -8, @Now), NULL, NULL, @Now, @By, 0),
    (NEWID(), @Team3, @TeamJob4, @Dev7, N'Hands-on Flutter UI work and design collaboration.', N'Flutter Developer',
     N'pending', DATEADD(DAY, -12, @Now), NULL, NULL, @Now, @By, 0),
    (NEWID(), @Team4, @TeamJob5, @Dev2, N'Backend patterns and clean API design are my focus.', N'.NET Backend Engineer',
     N'pending', DATEADD(DAY, -6, @Now), NULL, NULL, @Now, @By, 0),
    (NEWID(), @Team5, NULL,       @Dev1, N'Want to help with automated testing coverage.', NULL,
     N'pending', DATEADD(DAY, -4, @Now), NULL, NULL, @Now, @By, 0),
    (NEWID(), @Team6, @TeamJob7, @Dev8, N'Infra and IaC experience on AWS.', N'DevOps Engineer',
     N'Rejected', DATEADD(DAY, -25, @Now), DATEADD(DAY, -23, @Now), CONVERT(NVARCHAR(36), @Dev6), @Now, @By, 0),
    (NEWID(), @Team6, @TeamJob7, @Dev5, N'CI/CD and release automation track record.', N'DevOps Engineer',
     N'pending', DATEADD(DAY, -3, @Now), NULL, NULL, @Now, @By, 0),
    (NEWID(), @Team7, @TeamJob8, @Dev3, N'Interested in contributing design-system components.', N'Design Systems Specialist',
     N'pending', DATEADD(DAY, -9, @Now), NULL, NULL, @Now, @By, 0),
    (NEWID(), @Team7, NULL,       @Dev6, N'Can support design ops with cloud tooling.', NULL,
     N'Accepted', DATEADD(DAY, -30, @Now), DATEADD(DAY, -28, @Now), CONVERT(NVARCHAR(36), @Dev7), @Now, @By, 0),
    (NEWID(), @Team8, @TeamJob9, @Dev2, N'Data pipelines and dashboard work experience.', N'Data Engineer',
     N'Rejected', DATEADD(DAY, -40, @Now), DATEADD(DAY, -38, @Now), CONVERT(NVARCHAR(36), @Dev8), @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 11. TEAM FEEDBACKS (clients reviewing teams)                                */
/* -------------------------------------------------------------------------- */

INSERT INTO [teams].[TeamFeedbacks]
    ([Id], [TeamId], [ReviewerUserId], [Rating], [Comment], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Team1, @Client1, 5, N'Delivered ahead of schedule with clean code.', @Now, @By, 0),
    (NEWID(), @Team2, @Client2, 4, N'Great design taste, minor timeline slips.', @Now, @By, 0),
    (NEWID(), @Team3, @Client3, 5, N'Two apps shipped to both stores.', @Now, @By, 0),
    (NEWID(), @Team4, @Client4, 4, N'Solid APIs, documentation could improve.', @Now, @By, 0),
    (NEWID(), @Team5, @Client5, 5, N'Caught regressions our team missed.', @Now, @By, 0),
    (NEWID(), @Team6, @Client6, 5, N'Infrastructure was stable through launch.', @Now, @By, 0),
    (NEWID(), @Team7, @Client7, 4, N'Beautiful design system, a bit slow initially.', @Now, @By, 0),
    (NEWID(), @Team8, @Client8, 5, N'Data pipelines ran flawlessly in production.', @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 12. SOCIAL LINKS (users and teams)                                          */
/* -------------------------------------------------------------------------- */

INSERT INTO [core].[SocialLinks]
    ([Id], [OwnerType], [OwnerUserId], [OwnerTeamId], [Platform], [Url], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), N'User', @Dev1, NULL, N'LinkedIn', N'https://linkedin.com/in/youssef-kamal', @Now, @By, 0),
    (NEWID(), N'User', @Dev1, NULL, N'GitHub',   N'https://github.com/youssef-kamal',     @Now, @By, 0),
    (NEWID(), N'User', @Dev2, NULL, N'GitHub',   N'https://github.com/nour-salem',        @Now, @By, 0),
    (NEWID(), N'User', @Dev3, NULL, N'LinkedIn', N'https://linkedin.com/in/layla-farid',  @Now, @By, 0),
    (NEWID(), N'User', @Dev4, NULL, N'GitHub',   N'https://github.com/karim-adel',        @Now, @By, 0),
    (NEWID(), N'User', @Dev5, NULL, N'LinkedIn', N'https://linkedin.com/in/mona-zaki',    @Now, @By, 0),
    (NEWID(), N'User', @Dev6, NULL, N'LinkedIn', N'https://linkedin.com/in/adam-yassin',  @Now, @By, 0),
    (NEWID(), N'User', @Dev7, NULL, N'Behance',  N'https://behance.net/salma-refaat',     @Now, @By, 0),
    (NEWID(), N'User', @Dev8, NULL, N'GitHub',   N'https://github.com/fares-gamal',       @Now, @By, 0),
    (NEWID(), N'Team', NULL, @Team1, N'LinkedIn', N'https://linkedin.com/company/nile-code', @Now, @By, 0),
    (NEWID(), N'Team', NULL, @Team2, N'Behance',  N'https://behance.net/pixelcrafters',      @Now, @By, 0),
    (NEWID(), N'Team', NULL, @Team3, N'LinkedIn', N'https://linkedin.com/company/mobiflex',  @Now, @By, 0),
    (NEWID(), N'Team', NULL, @Team4, N'GitHub',   N'https://github.com/apiforge',            @Now, @By, 0),
    (NEWID(), N'Team', NULL, @Team5, N'LinkedIn', N'https://linkedin.com/company/qflabs',    @Now, @By, 0),
    (NEWID(), N'Team', NULL, @Team6, N'LinkedIn', N'https://linkedin.com/company/cloudnest', @Now, @By, 0),
    (NEWID(), N'Team', NULL, @Team7, N'Behance',  N'https://behance.net/designorbit',        @Now, @By, 0),
    (NEWID(), N'Team', NULL, @Team8, N'LinkedIn', N'https://linkedin.com/company/dataminds', @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 13. WALLETS (finance)                                                       */
/* -------------------------------------------------------------------------- */

DECLARE @WalletC1 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000001';
DECLARE @WalletC2 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000002';
DECLARE @WalletC3 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000003';
DECLARE @WalletC4 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000004';
DECLARE @WalletC5 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000005';
DECLARE @WalletC6 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000006';
DECLARE @WalletC7 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000007';
DECLARE @WalletC8 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000008';
DECLARE @WalletD1 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000011';
DECLARE @WalletD2 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000012';
DECLARE @WalletD3 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000013';
DECLARE @WalletD4 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000014';
DECLARE @WalletD5 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000015';
DECLARE @WalletD6 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000016';
DECLARE @WalletD7 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000017';
DECLARE @WalletD8 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000018';
DECLARE @WalletT1 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000101';
DECLARE @WalletT2 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000102';
DECLARE @WalletT3 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000103';
DECLARE @WalletT4 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000104';
DECLARE @WalletT5 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000105';
DECLARE @WalletT6 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000106';
DECLARE @WalletT7 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000107';
DECLARE @WalletT8 UNIQUEIDENTIFIER = 'f1f1f1f1-f1f1-f1f1-f1f1-000000000108';

INSERT INTO [finance].[Wallets]
    ([Id], [OwnerType], [OwnerUserId], [OwnerTeamId], [Currency], [Available], [Reserved], [Pending],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@WalletC1, N'User', @Client1, NULL, N'USD', 2500.00,    0.00,    0.00,    @Now, @By, 0),
    (@WalletC2, N'User', @Client2, NULL, N'USD', 6000.00,    0.00,    0.00,    @Now, @By, 0),
    (@WalletC3, N'User', @Client3, NULL, N'USD', 7000.00,    0.00,    0.00,    @Now, @By, 0),
    (@WalletC4, N'User', @Client4, NULL, N'USD', 3000.00,    0.00,    0.00,    @Now, @By, 0),
    (@WalletC5, N'User', @Client5, NULL, N'USD', 1500.00,    0.00,    0.00,    @Now, @By, 0),
    (@WalletC6, N'User', @Client6, NULL, N'USD', 9000.00,    0.00,    0.00,    @Now, @By, 0),
    (@WalletC7, N'User', @Client7, NULL, N'USD', 5000.00,    0.00,    0.00,    @Now, @By, 0),
    (@WalletC8, N'User', @Client8, NULL, N'USD', 2000.00,    0.00,    0.00,    @Now, @By, 0),
    (@WalletD1, N'User', @Dev1,    NULL, N'USD', 1800.00,    600.00,  400.00,  @Now, @By, 0),
    (@WalletD2, N'User', @Dev2,    NULL, N'USD', 1600.00,    800.00,  400.00,  @Now, @By, 0),
    (@WalletD3, N'User', @Dev3,    NULL, N'USD', 2100.00,    500.00,  300.00,  @Now, @By, 0),
    (@WalletD4, N'User', @Dev4,    NULL, N'USD', 900.00,     0.00,    200.00,  @Now, @By, 0),
    (@WalletD5, N'User', @Dev5,    NULL, N'USD', 750.00,     250.00,  150.00,  @Now, @By, 0),
    (@WalletD6, N'User', @Dev6,    NULL, N'USD', 1400.00,    300.00,  200.00,  @Now, @By, 0),
    (@WalletD7, N'User', @Dev7,    NULL, N'USD', 1100.00,    200.00,  100.00,  @Now, @By, 0),
    (@WalletD8, N'User', @Dev8,    NULL, N'USD', 2200.00,    1000.00, 500.00,  @Now, @By, 0),
    (@WalletT1, N'Team', NULL, @Team1, N'USD', 3200.00,    800.00,  400.00,  @Now, @By, 0),
    (@WalletT2, N'Team', NULL, @Team2, N'USD', 4000.00,    1000.00, 2000.00, @Now, @By, 0),
    (@WalletT3, N'Team', NULL, @Team3, N'USD', 4500.00,    1500.00, 2500.00, @Now, @By, 0),
    (@WalletT4, N'Team', NULL, @Team4, N'USD', 1000.00,    0.00,    0.00,    @Now, @By, 0),
    (@WalletT5, N'Team', NULL, @Team5, N'USD', 600.00,     0.00,    100.00,  @Now, @By, 0),
    (@WalletT6, N'Team', NULL, @Team6, N'USD', 2000.00,    500.00,  300.00,  @Now, @By, 0),
    (@WalletT7, N'Team', NULL, @Team7, N'USD', 1200.00,    0.00,    0.00,    @Now, @By, 0),
    (@WalletT8, N'Team', NULL, @Team8, N'USD', 8550.00,    0.00,    0.00,    @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 14. PROJECTS (marketplace)                                                  */
/* -------------------------------------------------------------------------- */

DECLARE @Project1 UNIQUEIDENTIFIER = '90909090-9090-9090-9090-000000000001';
DECLARE @Project2 UNIQUEIDENTIFIER = '90909090-9090-9090-9090-000000000002';
DECLARE @Project3 UNIQUEIDENTIFIER = '90909090-9090-9090-9090-000000000003';
DECLARE @Project4 UNIQUEIDENTIFIER = '90909090-9090-9090-9090-000000000004';
DECLARE @Project5 UNIQUEIDENTIFIER = '90909090-9090-9090-9090-000000000005';
DECLARE @Project6 UNIQUEIDENTIFIER = '90909090-9090-9090-9090-000000000006';
DECLARE @Project7 UNIQUEIDENTIFIER = '90909090-9090-9090-9090-000000000007';
DECLARE @Project8 UNIQUEIDENTIFIER = '90909090-9090-9090-9090-000000000008';

INSERT INTO [marketplace].[Projects]
    ([Id], [Title], [Description], [ClientId], [CategoryId], [IsFixedPrice],
     [BudgetMin], [BudgetMax], [Currency], [Deadline], [EstimatedDurationDays],
     [Status], [AssignedTeamId], [AssignedUserId], [CompletedAt],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@Project1, N'E-commerce platform revamp',
     N'Modernize an existing storefront with faster checkout, new product pages and admin dashboard.', @Client1, @CatEcom, 1,
     4000.00, 6500.00, N'USD', DATEADD(DAY, 45, @Now), 40, N'Open', NULL, NULL, NULL,
     @Now, @By, 0),
    (@Project2, N'SaaS dashboard MVP',
     N'Build the first version of a B2B analytics dashboard with auth, role management and reports.', @Client2, @CatSaas, 0,
     4000.00, 6500.00, N'USD', DATEADD(DAY, 30, @Now), 35, N'InProgress', @Team2, NULL, NULL,
     @Now, @By, 0),
    (@Project3, N'Mobile fitness app',
     N'Cross-platform fitness tracking app with workout plans, progress charts and offline mode.', @Client3, @CatMobile, 0,
     6000.00, 8500.00, N'USD', DATEADD(DAY, 50, @Now), 50, N'InProgress', @Team3, NULL, NULL,
     @Now, @By, 0),
    (@Project4, N'Booking platform backend',
     N'Design and build the backend for a booking platform: inventory, availability and payments API.', @Client4, @CatBackend, 1,
     3500.00, 5000.00, N'USD', DATEADD(DAY, 40, @Now), 30, N'Open', NULL, NULL, NULL,
     @Now, @By, 0),
    (@Project5, N'ML churn prediction service',
     N'Internal service to predict customer churn from usage telemetry and expose an API.', @Client5, @CatDataai, 0,
     5000.00, 8000.00, N'USD', NULL, 45, N'Draft', NULL, NULL, NULL,
     @Now, @By, 0),
    (@Project6, N'Shopify store optimization',
     N'Optimize a Shopify store: speed, conversion funnel, analytics tracking and payment flow.', @Client6, @CatEcom, 1,
     8000.00, 9500.00, N'USD', DATEADD(DAY, -5, @Now), 60, N'Completed', @Team8, NULL, DATEADD(DAY, -3, @Now),
     @Now, @By, 0),
    (@Project7, N'CI/CD migration for client platform',
     N'Move a production platform to automated CI/CD with containerized deployments and monitoring.', @Client7, @CatDevops, 1,
     4000.00, 5500.00, N'USD', DATEADD(DAY, 25, @Now), 25, N'InProgress', @Team6, NULL, NULL,
     @Now, @By, 0),
    (@Project8, N'Security audit web application',
     N'Full penetration test and hardening plan for a public-facing web application.', @Client8, @CatSecurity, 1,
     3000.00, 4500.00, N'USD', DATEADD(DAY, 20, @Now), 20, N'Open', NULL, NULL, NULL,
     @Now, @By, 0);

INSERT INTO [marketplace].[ProjectSkills]
    ([Id], [ProjectId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Project1, @SkillReact,  @Now, @By, 0),
    (NEWID(), @Project1, @SkillTs,     @Now, @By, 0),
    (NEWID(), @Project2, @SkillReact,  @Now, @By, 0),
    (NEWID(), @Project2, @SkillCsharp, @Now, @By, 0),
    (NEWID(), @Project3, @SkillFlutter,@Now, @By, 0),
    (NEWID(), @Project3, @SkillDart,   @Now, @By, 0),
    (NEWID(), @Project4, @SkillCsharp, @Now, @By, 0),
    (NEWID(), @Project4, @SkillNode,   @Now, @By, 0),
    (NEWID(), @Project5, @SkillPython, @Now, @By, 0),
    (NEWID(), @Project5, @SkillPandas, @Now, @By, 0),
    (NEWID(), @Project6, @SkillShopify,@Now, @By, 0),
    (NEWID(), @Project6, @SkillStripe, @Now, @By, 0),
    (NEWID(), @Project7, @SkillDocker, @Now, @By, 0),
    (NEWID(), @Project7, @SkillAws,    @Now, @By, 0),
    (NEWID(), @Project8, @SkillOwasp,  @Now, @By, 0),
    (NEWID(), @Project8, @SkillBurp,   @Now, @By, 0);

INSERT INTO [marketplace].[ProjectSpecialties]
    ([Id], [ProjectId], [SpecialtyId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Project1, @SpecFullStack,  @Now, @By, 0),
    (NEWID(), @Project1, @SpecFrontend,   @Now, @By, 0),
    (NEWID(), @Project2, @SpecSaas,       @Now, @By, 0),
    (NEWID(), @Project2, @SpecFrontend,   @Now, @By, 0),
    (NEWID(), @Project3, @SpecCrossMobile,@Now, @By, 0),
    (NEWID(), @Project3, @SpecAndroid,    @Now, @By, 0),
    (NEWID(), @Project4, @SpecBackend,    @Now, @By, 0),
    (NEWID(), @Project4, @SpecApi,        @Now, @By, 0),
    (NEWID(), @Project5, @SpecMl,         @Now, @By, 0),
    (NEWID(), @Project5, @SpecDataEng,    @Now, @By, 0),
    (NEWID(), @Project6, @SpecShopify,    @Now, @By, 0),
    (NEWID(), @Project6, @SpecPayment,    @Now, @By, 0),
    (NEWID(), @Project7, @SpecCiCd,       @Now, @By, 0),
    (NEWID(), @Project7, @SpecCloudInfra, @Now, @By, 0),
    (NEWID(), @Project8, @SpecPentest,    @Now, @By, 0),
    (NEWID(), @Project8, @SpecAppSec,     @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 15. PROJECT PROPOSALS + ATTACHMENTS                                         */
/* -------------------------------------------------------------------------- */

DECLARE @Proposal1  UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000001';
DECLARE @Proposal2  UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000002';
DECLARE @Proposal3  UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000003';
DECLARE @Proposal4  UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000004';
DECLARE @Proposal5  UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000005';
DECLARE @Proposal6  UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000006';
DECLARE @Proposal7  UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000007';
DECLARE @Proposal8  UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000008';
DECLARE @Proposal9  UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000009';
DECLARE @Proposal10 UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000010';
DECLARE @Proposal11 UNIQUEIDENTIFIER = 'a1a1a1a1-a1a1-a1a1-a1a1-000000000011';

INSERT INTO [marketplace].[ProjectProposals]
    ([Id], [ProjectId], [ApplicantType], [TeamId], [UserId], [CoverLetter], [Approach],
     [ProposedTimeline], [SimilarLinksUrl], [ProposedBudget], [Status], [RejectReason],
     [AppliedAt], [ResponseAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@Proposal1, @Project1, N'Team', @Team1, @Dev1,
     N'Our full-stack team has shipped three e-commerce rebuilds this year.',
     N'Two-week discovery, then sprint-based rebuild with weekly demo builds.',
     N'6-8 weeks', N'https://nilecode.example.com/portfolio', 4500.00, N'InDiscussion', NULL,
     DATEADD(DAY, -12, @Now), DATEADD(DAY, -9, @Now), @Now, @By, 0),
    (@Proposal2, @Project1, N'User', NULL, @Dev2,
     N'Experienced frontend engineer with a focus on fast storefronts.',
     N'Component-driven refactor of the storefront first, then checkout flows.',
     N'7 weeks', N'https://github.com/nour-salem', 5200.00, N'Rejected', N'Budget above range.',
     DATEADD(DAY, -11, @Now), DATEADD(DAY, -8, @Now), @Now, @By, 0),
    (@Proposal3, @Project1, N'User', NULL, @Dev4,
     N'Backend specialist who can also drive the admin dashboard build.',
     N'API-first approach with clean data model design before UI work.',
     N'8 weeks', NULL, 4800.00, N'Pending', NULL,
     DATEADD(DAY, -6, @Now), NULL, @Now, @By, 0),
    (@Proposal4, @Project2, N'Team', @Team2, @Dev2,
     N'Pixel Crafters can deliver the full MVP within your timeline.',
     N'Design system first, then auth, roles and reporting modules.',
     N'5 weeks', N'https://pixelcrafters.example.com/work', 6000.00, N'InDiscussion', NULL,
     DATEADD(DAY, -15, @Now), DATEADD(DAY, -13, @Now), @Now, @By, 0),
    (@Proposal5, @Project3, N'Team', @Team3, @Dev3,
     N'We build cross-platform fitness apps with offline-first architecture.',
     N'Shared Flutter codebase with workout engine and synced progress store.',
     N'7 weeks', N'https://mobiflex.example.com/cases', 7000.00, N'InDiscussion', NULL,
     DATEADD(DAY, -18, @Now), DATEADD(DAY, -16, @Now), @Now, @By, 0),
    (@Proposal6, @Project4, N'Team', @Team4, @Dev4,
     N'API Forge specializes in exactly this kind of inventory/booking backend.',
     N'REST APIs with idempotent booking mutations and availability cache.',
     N'5 weeks', N'https://apiforge.example.com/apis', 4200.00, N'InDiscussion', NULL,
     DATEADD(DAY, -10, @Now), DATEADD(DAY, -8, @Now), @Now, @By, 0),
    (@Proposal7, @Project4, N'User', NULL, @Dev2,
     N'Comfortable designing booking APIs end to end.',
     N'Domain-driven design with explicit availability slot model.',
     N'6 weeks', NULL, 4600.00, N'Pending', NULL,
     DATEADD(DAY, -4, @Now), NULL, @Now, @By, 0),
    (@Proposal8, @Project6, N'Team', @Team8, @Dev8,
     N'Data Minds handled the full optimization sprint and delivery.',
     N'Performance audit, conversion instrumentation, payment flow review.',
     N'9 weeks', N'https://dataminds.example.com/shopify', 9000.00, N'InDiscussion', NULL,
     DATEADD(DAY, -90, @Now), DATEADD(DAY, -88, @Now), @Now, @By, 0),
    (@Proposal9, @Project8, N'Team', @Team5, @Dev5,
     N'Quality First Labs runs structured security test campaigns.',
     N'Threat modeling, pentest, then a written hardening plan.',
     N'3-4 weeks', N'https://qflabs.example.com/security', 3800.00, N'Pending', NULL,
     DATEADD(DAY, -5, @Now), NULL, @Now, @By, 0),
    (@Proposal10, @Project8, N'User', NULL, @Dev8,
     N'Background in application security tooling and reporting.',
     N'OWASP-aligned testing with automated scanning where applicable.',
     N'4 weeks', NULL, 4100.00, N'Rejected', N'Limited solo capacity for audit scope.',
     DATEADD(DAY, -7, @Now), DATEADD(DAY, -5, @Now), @Now, @By, 0),
    (@Proposal11, @Project7, N'Team', @Team6, @Dev6,
     N'CloudNest runs these migration playbooks monthly.',
     N'Assess, containerize, wire CI/CD, then shift traffic gradually.',
     N'4 weeks', N'https://cloudnest.example.com/cicd', 5000.00, N'InDiscussion', NULL,
     DATEADD(DAY, -20, @Now), DATEADD(DAY, -18, @Now), @Now, @By, 0);

INSERT INTO [marketplace].[ProposalAttachments]
    ([Id], [ProposalId], [FileName], [FileUrl], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Proposal1, N'portfolio.pdf',      N'https://cdn.example.com/seed/att-proposal1-portfolio.pdf', @Now, @By, 0),
    (NEWID(), @Proposal1, N'timeline.pdf',       N'https://cdn.example.com/seed/att-proposal1-timeline.pdf',  @Now, @By, 0),
    (NEWID(), @Proposal4, N'team-profile.pdf',   N'https://cdn.example.com/seed/att-proposal4-team.pdf',      @Now, @By, 0),
    (NEWID(), @Proposal4, N'mockups.fig',        N'https://cdn.example.com/seed/att-proposal4-mockups.fig',   @Now, @By, 0),
    (NEWID(), @Proposal5, N'case-study.pdf',     N'https://cdn.example.com/seed/att-proposal5-case.pdf',      @Now, @By, 0),
    (NEWID(), @Proposal6, N'api-spec.pdf',       N'https://cdn.example.com/seed/att-proposal6-api.pdf',       @Now, @By, 0),
    (NEWID(), @Proposal8, N'data-approach.pdf',  N'https://cdn.example.com/seed/att-proposal8-data.pdf',      @Now, @By, 0),
    (NEWID(), @Proposal9, N'qa-plan.pdf',        N'https://cdn.example.com/seed/att-proposal9-qa.pdf',        @Now, @By, 0),
    (NEWID(), @Proposal11, N'playbook.pdf',      N'https://cdn.example.com/seed/att-proposal11-playbook.pdf', @Now, @By, 0),
    (NEWID(), @Proposal11, N'infra.md',          N'https://cdn.example.com/seed/att-proposal11-infra.md',     @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 16. ESCROW HOLDS (finance)                                                  */
/* -------------------------------------------------------------------------- */

INSERT INTO [finance].[EscrowHolds]
    ([Id], [ProjectId], [TotalAmount], [TotalReleased], [FundingStatus], [planStatus],
     [LockedAt], [PlanAgreedAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Project1, 0.00,    0.00,    N'Unlocked', N'AwaitingPlan',    NULL, NULL,             @Now, @By, 0),
    (NEWID(), @Project2, 6000.00, 4000.00, N'Locked',   N'PlanAgreed',      DATEADD(DAY, -30, @Now), DATEADD(DAY, -25, @Now), @Now, @By, 0),
    (NEWID(), @Project3, 7000.00, 4500.00, N'Locked',   N'PlanAgreed',      DATEADD(DAY, -28, @Now), DATEADD(DAY, -24, @Now), @Now, @By, 0),
    (NEWID(), @Project4, 0.00,    0.00,    N'Unlocked', N'AwaitingPlan',    NULL, NULL,             @Now, @By, 0),
    (NEWID(), @Project5, 0.00,    0.00,    N'Unlocked', N'AwaitingPlan',    NULL, NULL,             @Now, @By, 0),
    (NEWID(), @Project6, 9000.00, 9000.00, N'Completed',N'PlanAgreed',      DATEADD(DAY, -80, @Now), DATEADD(DAY, -75, @Now), @Now, @By, 0),
    (NEWID(), @Project7, 5000.00, 2000.00, N'Locked',   N'PlanSubmitted',   DATEADD(DAY, -14, @Now), NULL,             @Now, @By, 0),
    (NEWID(), @Project8, 0.00,    0.00,    N'Unlocked', N'AwaitingPlan',    NULL, NULL,             @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 17. MILESTONE PLAN VERSIONS + ITEMS                                         */
/* -------------------------------------------------------------------------- */

DECLARE @PlanV1 UNIQUEIDENTIFIER = 'c3c3c3c3-c3c3-c3c3-c3c3-000000000001';
DECLARE @PlanV2 UNIQUEIDENTIFIER = 'c3c3c3c3-c3c3-c3c3-c3c3-000000000002';
DECLARE @PlanV3 UNIQUEIDENTIFIER = 'c3c3c3c3-c3c3-c3c3-c3c3-000000000003';
DECLARE @PlanV4 UNIQUEIDENTIFIER = 'c3c3c3c3-c3c3-c3c3-c3c3-000000000004';
DECLARE @PlanV5 UNIQUEIDENTIFIER = 'c3c3c3c3-c3c3-c3c3-c3c3-000000000005';
DECLARE @PlanV6 UNIQUEIDENTIFIER = 'c3c3c3c3-c3c3-c3c3-c3c3-000000000006';
DECLARE @PlanV7 UNIQUEIDENTIFIER = 'c3c3c3c3-c3c3-c3c3-c3c3-000000000007';
DECLARE @PlanV8 UNIQUEIDENTIFIER = 'c3c3c3c3-c3c3-c3c3-c3c3-000000000008';
DECLARE @PlanV9 UNIQUEIDENTIFIER = 'c3c3c3c3-c3c3-c3c3-c3c3-000000000009';

INSERT INTO [marketplace].[MilestonePlanVersions]
    ([Id], [ProjectId], [ProposalId], [Version], [Status], [ChangeComment], [ProposedByUserId],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@PlanV1, @Project1, @Proposal1, 1, N'Proposed',         NULL, @Dev1, @Now, @By, 0),
    (@PlanV2, @Project2, @Proposal4, 1, N'Proposed',         NULL, @Dev2, @Now, @By, 0),
    (@PlanV3, @Project2, @Proposal4, 2, N'Accepted',         N'Budget split into three equal milestones.', @Dev2, @Now, @By, 0),
    (@PlanV4, @Project3, @Proposal5, 1, N'Accepted',         NULL, @Dev3, @Now, @By, 0),
    (@PlanV5, @Project4, @Proposal6, 1, N'Proposed',         NULL, @Dev4, @Now, @By, 0),
    (@PlanV6, @Project6, @Proposal8, 1, N'Accepted',         NULL, @Dev8, @Now, @By, 0),
    (@PlanV7, @Project7, @Proposal11, 1, N'Proposed',        NULL, @Dev6, @Now, @By, 0),
    (@PlanV8, @Project7, @Proposal11, 2, N'ChangesRequested', N'Client asked to add a monitoring milestone.', @Dev6, @Now, @By, 0),
    (@PlanV9, @Project8, @Proposal9, 1, N'Proposed',         NULL, @Dev5, @Now, @By, 0);

INSERT INTO [marketplace].[MilestonePlanItems]
    ([Id], [PlanVersionId], [Title], [DefinitionOfDone], [Amount], [DueDate], [SortOrder], [ChangeTag],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @PlanV1, N'Storefront refresh', N'New product pages and checkout live on staging.', 2000.00, DATEADD(DAY, 21, @Now), 1, NULL, @Now, @By, 0),
    (NEWID(), @PlanV1, N'Admin dashboard',    N'Order and product management screens usable.', 1500.00, DATEADD(DAY, 35, @Now), 2, NULL, @Now, @By, 0),
    (NEWID(), @PlanV1, N'Launch & polish',    N'Performance targets met and deploy to production.', 1000.00, DATEADD(DAY, 45, @Now), 3, NULL, @Now, @By, 0),
    (NEWID(), @PlanV2, N'Auth & roles',       N'Login, registration and role management work.', 2000.00, DATEADD(DAY, 10, @Now), 1, NULL, @Now, @By, 0),
    (NEWID(), @PlanV2, N'Dashboard widgets',  N'Core report widgets render from live data.', 2000.00, DATEADD(DAY, 20, @Now), 2, NULL, @Now, @By, 0),
    (NEWID(), @PlanV2, N'Reporting module',   N'Full reporting module and export available.', 2000.00, DATEADD(DAY, 30, @Now), 3, NULL, @Now, @By, 0),
    (NEWID(), @PlanV3, N'Auth & roles',       N'Login, registration and role management work.', 2000.00, DATEADD(DAY, 10, @Now), 1, N'Updated', @Now, @By, 0),
    (NEWID(), @PlanV3, N'Dashboard widgets',  N'Core report widgets render from live data.', 2000.00, DATEADD(DAY, 20, @Now), 2, N'Updated', @Now, @By, 0),
    (NEWID(), @PlanV3, N'Reporting module',   N'Full reporting module and export available.', 2000.00, DATEADD(DAY, 30, @Now), 3, N'Updated', @Now, @By, 0),
    (NEWID(), @PlanV4, N'App foundation',     N'Navigation and onboarding flows in place.', 2000.00, DATEADD(DAY, 15, @Now), 1, NULL, @Now, @By, 0),
    (NEWID(), @PlanV4, N'Workout module',     N'Workout creation and tracking functional.', 2500.00, DATEADD(DAY, 30, @Now), 2, NULL, @Now, @By, 0),
    (NEWID(), @PlanV4, N'Offline & store',    N'Offline sync works and stores submissions pass.', 2500.00, DATEADD(DAY, 50, @Now), 3, NULL, @Now, @By, 0),
    (NEWID(), @PlanV5, N'Domain model',       N'Inventory and booking domain model implemented.', 1500.00, DATEADD(DAY, 15, @Now), 1, NULL, @Now, @By, 0),
    (NEWID(), @PlanV5, N'Booking API',        N'Booking and availability endpoints live.', 1700.00, DATEADD(DAY, 25, @Now), 2, NULL, @Now, @By, 0),
    (NEWID(), @PlanV5, N'Payments & docs',    N'Payment flow integrated and docs published.', 1000.00, DATEADD(DAY, 30, @Now), 3, NULL, @Now, @By, 0),
    (NEWID(), @PlanV6, N'Data pipeline',      N'Shopify data flows into the warehouse daily.', 3000.00, DATEADD(DAY, 30, @Now), 1, NULL, @Now, @By, 0),
    (NEWID(), @PlanV6, N'Model training',     N'Churn model trained and validated.', 3000.00, DATEADD(DAY, 45, @Now), 2, NULL, @Now, @By, 0),
    (NEWID(), @PlanV6, N'Dashboard & handoff', N'Reporting dashboard live and docs handed over.', 3000.00, DATEADD(DAY, 60, @Now), 3, NULL, @Now, @By, 0),
    (NEWID(), @PlanV7, N'Pipeline setup',     N'Build pipeline runs on every commit.', 2000.00, DATEADD(DAY, 12, @Now), 1, NULL, @Now, @By, 0),
    (NEWID(), @PlanV7, N'Containerization',   N'All services run in containers on staging.', 1500.00, DATEADD(DAY, 20, @Now), 2, NULL, @Now, @By, 0),
    (NEWID(), @PlanV7, N'Traffic migration',  N'Production traffic migrated with rollback plan.', 1500.00, DATEADD(DAY, 25, @Now), 3, NULL, @Now, @By, 0),
    (NEWID(), @PlanV8, N'Pipeline setup',     N'Build pipeline runs on every commit.', 2000.00, DATEADD(DAY, 12, @Now), 1, N'Updated', @Now, @By, 0),
    (NEWID(), @PlanV8, N'Containerization',   N'All services run in containers on staging.', 1500.00, DATEADD(DAY, 20, @Now), 2, N'Updated', @Now, @By, 0),
    (NEWID(), @PlanV8, N'Traffic migration',  N'Production traffic migrated with rollback plan.', 1000.00, DATEADD(DAY, 25, @Now), 3, N'Updated', @Now, @By, 0),
    (NEWID(), @PlanV8, N'Monitoring setup',   N'Dashboards and alerts configured for all services.', 500.00, DATEADD(DAY, 25, @Now), 4, N'New', @Now, @By, 0),
    (NEWID(), @PlanV9, N'Threat modeling',    N'Threat model reviewed with the client.', 1000.00, DATEADD(DAY, 7, @Now), 1, NULL, @Now, @By, 0),
    (NEWID(), @PlanV9, N'Penetration test',   N'Full pentest executed and documented.', 1800.00, DATEADD(DAY, 15, @Now), 2, NULL, @Now, @By, 0),
    (NEWID(), @PlanV9, N'Hardening plan',     N'Written remediation plan delivered.', 1000.00, DATEADD(DAY, 20, @Now), 3, NULL, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 18. MILESTONES                                                              */
/* -------------------------------------------------------------------------- */

DECLARE @M1  UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000001';
DECLARE @M2  UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000002';
DECLARE @M3  UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000003';
DECLARE @M4  UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000004';
DECLARE @M5  UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000005';
DECLARE @M6  UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000006';
DECLARE @M7  UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000007';
DECLARE @M8  UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000008';
DECLARE @M9  UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000009';
DECLARE @M10 UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000010';
DECLARE @M11 UNIQUEIDENTIFIER = 'b2b2b2b2-b2b2-b2b2-b2b2-000000000011';

INSERT INTO [marketplace].[Milestones]
    ([Id], [ProjectId], [Title], [Description], [Amount], [ReleasedAmount], [SortOrder], [DueDate],
     [IsFunded], [ReleaseStatus], [WorkStatus], [ProposedByUserId], [SubmittedAt], [AvailableAt], [ReleasedAt],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@M1, @Project2, N'Design & frontend foundation',
     N'Design system, auth screens and dashboard shell.', 2000.00, 2000.00, 1, DATEADD(DAY, 10, @Now),
     1, N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev2), DATEADD(DAY, -22, @Now), DATEADD(DAY, -18, @Now), DATEADD(DAY, -15, @Now),
     @Now, @By, 0),
    (@M2, @Project2, N'Core dashboard features',
     N'Main widgets and report views.', 2000.00, 2000.00, 2, DATEADD(DAY, 20, @Now),
     1, N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev2), DATEADD(DAY, -10, @Now), DATEADD(DAY, -8, @Now), DATEADD(DAY, -6, @Now),
     @Now, @By, 0),
    (@M3, @Project2, N'Integration & launch',
     N'Full integration, QA pass and production launch.', 2000.00, 0.00, 3, DATEADD(DAY, 30, @Now),
     1, N'Pending', N'Submitted', CONVERT(NVARCHAR(36), @Dev2), DATEADD(DAY, -2, @Now), NULL, NULL,
     @Now, @By, 0),
    (@M4, @Project3, N'App architecture & navigation',
     N'Flutter foundation, navigation and onboarding.', 2000.00, 2000.00, 1, DATEADD(DAY, 15, @Now),
     1, N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev3), DATEADD(DAY, -24, @Now), DATEADD(DAY, -20, @Now), DATEADD(DAY, -17, @Now),
     @Now, @By, 0),
    (@M5, @Project3, N'Workout tracking module',
     N'Workout plans, tracking and progress charts.', 2500.00, 2500.00, 2, DATEADD(DAY, 30, @Now),
     1, N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev3), DATEADD(DAY, -9, @Now), DATEADD(DAY, -6, @Now), DATEADD(DAY, -4, @Now),
     @Now, @By, 0),
    (@M6, @Project3, N'Offline sync & store launch',
     N'Offline mode and store submission.', 2500.00, 0.00, 3, DATEADD(DAY, 50, @Now),
     1, N'InReview', N'Submitted', CONVERT(NVARCHAR(36), @Dev3), DATEADD(DAY, -1, @Now), NULL, NULL,
     @Now, @By, 0),
    (@M7, @Project6, N'Data ingestion pipeline',
     N'Daily ingestion of store data into the warehouse.', 3000.00, 3000.00, 1, DATEADD(DAY, 30, @Now),
     1, N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev8), DATEADD(DAY, -60, @Now), DATEADD(DAY, -55, @Now), DATEADD(DAY, -52, @Now),
     @Now, @By, 0),
    (@M8, @Project6, N'ML model training',
     N'Churn model trained and validated on live data.', 3000.00, 3000.00, 2, DATEADD(DAY, 45, @Now),
     1, N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev8), DATEADD(DAY, -35, @Now), DATEADD(DAY, -30, @Now), DATEADD(DAY, -27, @Now),
     @Now, @By, 0),
    (@M9, @Project6, N'Reporting dashboard & handoff',
     N'Dashboard shipped and documentation handed over.', 3000.00, 3000.00, 3, DATEADD(DAY, 60, @Now),
     1, N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev8), DATEADD(DAY, -10, @Now), DATEADD(DAY, -7, @Now), DATEADD(DAY, -3, @Now),
     @Now, @By, 0),
    (@M10, @Project7, N'CI/CD pipeline setup',
     N'Automated build and test pipeline in place.', 2000.00, 2000.00, 1, DATEADD(DAY, 12, @Now),
     1, N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev6), DATEADD(DAY, -8, @Now), DATEADD(DAY, -5, @Now), DATEADD(DAY, -3, @Now),
     @Now, @By, 0),
    (@M11, @Project7, N'Cloud migration',
     N'Containers deployed and traffic migrated.', 3000.00, 0.00, 2, DATEADD(DAY, 25, @Now),
     1, N'Locked', N'InProgress', CONVERT(NVARCHAR(36), @Dev6), NULL, NULL, NULL,
     @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 19. PROJECT FILES                                                           */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[ProjectFiles]
    ([Id], [ProjectId], [MilestoneId], [UploadedByUserId], [FileName], [FileUrl], [FileKind],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Project1, NULL, @Client1, N'brief.docx',        N'https://cdn.example.com/seed/p1-brief.docx',       N'Brief', @Now, @By, 0),
    (NEWID(), @Project2, @M1,   @Dev2,   N'design-system.fig', N'https://cdn.example.com/seed/p2-design.fig',        N'Deliverable', @Now, @By, 0),
    (NEWID(), @Project2, @M1,   @Dev2,   N'wireframes.pdf',    N'https://cdn.example.com/seed/p2-wireframes.pdf',    N'Deliverable', @Now, @By, 0),
    (NEWID(), @Project2, @M2,   @Dev2,   N'dashboard-src.zip', N'https://cdn.example.com/seed/p2-dashboard.zip',     N'Deliverable', @Now, @By, 0),
    (NEWID(), @Project3, @M4,   @Dev3,   N'flow-diagrams.pdf', N'https://cdn.example.com/seed/p3-flows.pdf',         N'Shared', @Now, @By, 0),
    (NEWID(), @Project3, @M5,   @Dev3,   N'app-snapshot.apk',  N'https://cdn.example.com/seed/p3-app.apk',          N'Deliverable', @Now, @By, 0),
    (NEWID(), @Project6, @M7,   @Dev8,   N'pipeline-docs.pdf', N'https://cdn.example.com/seed/p6-pipeline.pdf',      N'Shared', @Now, @By, 0),
    (NEWID(), @Project6, @M9,   @Dev8,   N'report-demo.mp4',   N'https://cdn.example.com/seed/p6-report.mp4',        N'Deliverable', @Now, @By, 0),
    (NEWID(), @Project7, @M10,  @Dev6,   N'infra-diagram.png', N'https://cdn.example.com/seed/p7-infra.png',         N'Deliverable', @Now, @By, 0),
    (NEWID(), @Project8, NULL, @Client8, N'security-scope.pdf',N'https://cdn.example.com/seed/p8-scope.pdf',        N'Brief', @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 20. PROJECT EVENTS                                                          */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[ProjectEvents]
    ([Id], [ProjectId], [MilestoneId], [ActorUserId], [EventType], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Project1, NULL, @Dev1,   N'ProposalAccepted',          DATEADD(DAY, -9, @Now), @By, 0),
    (NEWID(), @Project2, NULL, @Dev2,   N'ProposalAccepted',          DATEADD(DAY, -13, @Now), @By, 0),
    (NEWID(), @Project2, NULL, @Client2, N'MilestonePlanAgreed',      DATEADD(DAY, -12, @Now), @By, 0),
    (NEWID(), @Project2, NULL, @Client2, N'EscrowLocked',             DATEADD(DAY, -11, @Now), @By, 0),
    (NEWID(), @Project2, @M1,   @Client2, N'MilestoneReleased',       DATEADD(DAY, -15, @Now), @By, 0),
    (NEWID(), @Project2, @M2,   @Client2, N'MilestoneReleased',       DATEADD(DAY, -6, @Now),  @By, 0),
    (NEWID(), @Project2, @M3,   @Dev2,   N'MilestoneSubmitted',       DATEADD(DAY, -2, @Now),  @By, 0),
    (NEWID(), @Project3, NULL, @Dev3,   N'ProposalAccepted',          DATEADD(DAY, -16, @Now), @By, 0),
    (NEWID(), @Project3, @M5,   @Dev3,   N'MilestoneSubmitted',       DATEADD(DAY, -9, @Now),  @By, 0),
    (NEWID(), @Project3, @M5,   @Client3, N'MilestoneApproved',       DATEADD(DAY, -4, @Now),  @By, 0),
    (NEWID(), @Project6, NULL, @Dev8,   N'ProposalAccepted',          DATEADD(DAY, -88, @Now), @By, 0),
    (NEWID(), @Project6, @M9,   @Client6, N'MilestoneReleased',       DATEADD(DAY, -3, @Now),  @By, 0),
    (NEWID(), @Project6, NULL, @Client6, N'ProjectCompleted',         DATEADD(DAY, -3, @Now),  @By, 0),
    (NEWID(), @Project7, NULL, @Dev6,   N'ProposalAccepted',          DATEADD(DAY, -18, @Now), @By, 0),
    (NEWID(), @Project7, NULL, @Dev6,   N'MemberAdded',               DATEADD(DAY, -15, @Now), @By, 0),
    (NEWID(), @Project7, @M10,  @Dev6,   N'MilestoneSubmitted',       DATEADD(DAY, -8, @Now),  @By, 0),
    (NEWID(), @Project8, NULL, @Dev5,   N'ProposalAccepted',          DATEADD(DAY, -5, @Now),  @By, 0);

/* -------------------------------------------------------------------------- */
/* 21. LEDGER ENTRIES (finance)                                                */
/* -------------------------------------------------------------------------- */

INSERT INTO [finance].[LedgerEntries]
    ([Id], [WalletId], [EntryType], [Amount], [Currency], [ProjectId], [MilestoneId],
     [IdempotencyKey], [PaymentProviderRef], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @WalletC2, N'TopUp',         6000.00, N'USD', @Project2, @M1, N'ledger-000001', N'stripe-pay-000001', DATEADD(DAY, -30, @Now), @By, 0),
    (NEWID(), @WalletT2, N'EscrowRelease', 2000.00, N'USD', @Project2, @M1, N'ledger-000002', N'stripe-pay-000002', DATEADD(DAY, -15, @Now), @By, 0),
    (NEWID(), @WalletT2, N'EscrowRelease', 2000.00, N'USD', @Project2, @M2, N'ledger-000003', N'stripe-pay-000003', DATEADD(DAY, -6, @Now), @By, 0),
    (NEWID(), @WalletT2, N'PendingCredit', 2000.00, N'USD', @Project2, @M3, N'ledger-000004', NULL,                DATEADD(DAY, -2, @Now), @By, 0),
    (NEWID(), @WalletC3, N'TopUp',         7000.00, N'USD', @Project3, @M4, N'ledger-000005', N'stripe-pay-000005', DATEADD(DAY, -28, @Now), @By, 0),
    (NEWID(), @WalletT3, N'EscrowRelease', 4500.00, N'USD', @Project3, @M5, N'ledger-000006', N'stripe-pay-000006', DATEADD(DAY, -4, @Now), @By, 0),
    (NEWID(), @WalletC6, N'TopUp',         9000.00, N'USD', @Project6, @M7, N'ledger-000007', N'stripe-pay-000007', DATEADD(DAY, -80, @Now), @By, 0),
    (NEWID(), @WalletT8, N'EscrowRelease', 9000.00, N'USD', @Project6, @M9, N'ledger-000008', N'stripe-pay-000008', DATEADD(DAY, -3, @Now), @By, 0),
    (NEWID(), @WalletT8, N'PlatformFee',   450.00,  N'USD', @Project6, @M9, N'ledger-000009', NULL,                DATEADD(DAY, -3, @Now), @By, 0),
    (NEWID(), @WalletT8, N'AvailableCredit',8550.00,N'USD', @Project6, @M9, N'ledger-000010', NULL,                DATEADD(DAY, -3, @Now), @By, 0),
    (NEWID(), @WalletC7, N'TopUp',         5000.00, N'USD', @Project7, @M10, N'ledger-000011', N'stripe-pay-000011', DATEADD(DAY, -14, @Now), @By, 0),
    (NEWID(), @WalletT6, N'EscrowRelease', 2000.00, N'USD', @Project7, @M10, N'ledger-000012', N'stripe-pay-000012', DATEADD(DAY, -3, @Now), @By, 0),
    (NEWID(), @WalletT6, N'PendingCredit', 3000.00, N'USD', @Project7, @M11, N'ledger-000013', NULL,                @Now, @By, 0),
    (NEWID(), @WalletD8, N'TeamSplit',     4500.00, N'USD', @Project6, @M9,  N'ledger-000014', NULL,                DATEADD(DAY, -3, @Now), @By, 0),
    (NEWID(), @WalletD8, N'Withdrawal',    3000.00, N'USD', NULL,       NULL, N'ledger-000015', N'payout-000001',    DATEADD(DAY, -1, @Now), @By, 0);

/* -------------------------------------------------------------------------- */
/* 22. PAYMENT TRANSACTIONS (dbo)                                              */
/* -------------------------------------------------------------------------- */

INSERT INTO [dbo].[paymentTransactions]
    ([Id], [WalletId], [PaymentProviderRef], [Amount], [Currency], [Status],
     [ProjectId], [MilestoneId], [FailureReason], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @WalletC2, N'stripe-pay-000001', 6000.00, N'USD', 1, @Project2, @M1, NULL, DATEADD(DAY, -30, @Now), @By, 0),
    (NEWID(), @WalletT2, N'stripe-pay-000002', 2000.00, N'USD', 1, @Project2, @M1, NULL, DATEADD(DAY, -15, @Now), @By, 0),
    (NEWID(), @WalletT2, N'stripe-pay-000003', 2000.00, N'USD', 1, @Project2, @M2, NULL, DATEADD(DAY, -6, @Now), @By, 0),
    (NEWID(), @WalletC3, N'stripe-pay-000005', 7000.00, N'USD', 1, @Project3, @M4, NULL, DATEADD(DAY, -28, @Now), @By, 0),
    (NEWID(), @WalletT3, N'stripe-pay-000006', 4500.00, N'USD', 1, @Project3, @M5, NULL, DATEADD(DAY, -4, @Now), @By, 0),
    (NEWID(), @WalletC6, N'stripe-pay-000007', 9000.00, N'USD', 1, @Project6, @M7, NULL, DATEADD(DAY, -80, @Now), @By, 0),
    (NEWID(), @WalletT8, N'stripe-pay-000008', 9000.00, N'USD', 1, @Project6, @M9, NULL, DATEADD(DAY, -3, @Now), @By, 0),
    (NEWID(), @WalletC1, N'stripe-pay-000009', 4000.00, N'USD', 2, @Project1, NULL, N'Card declined by issuer.', DATEADD(DAY, -2, @Now), @By, 0),
    (NEWID(), @WalletC1, N'stripe-pay-000009', 4000.00, N'USD', 4, @Project1, NULL, NULL, DATEADD(DAY, -1, @Now), @By, 0),
    (NEWID(), @WalletT6, N'stripe-pay-000012', 2000.00, N'USD', 1, @Project7, @M10, NULL, DATEADD(DAY, -3, @Now), @By, 0),
    (NEWID(), @WalletD8, N'payout-000001',     3000.00, N'USD', 1, NULL,       NULL, NULL, DATEADD(DAY, -1, @Now), @By, 0);

/* -------------------------------------------------------------------------- */
/* 23. TEAM PAYOUT SPLITS (finance)                                            */
/* -------------------------------------------------------------------------- */

INSERT INTO [finance].[TeamPayoutSplits]
    ([Id], [TeamId], [ProjectId], [UserId], [SplitType], [Value], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Team2, @Project2, @Dev2, N'Percent', 40.00, @Now, @By, 0),
    (NEWID(), @Team2, @Project2, @Dev3, N'Percent', 30.00, @Now, @By, 0),
    (NEWID(), @Team2, @Project2, @Dev8, N'Percent', 30.00, @Now, @By, 0),
    (NEWID(), @Team3, @Project3, @Dev3, N'Percent', 40.00, @Now, @By, 0),
    (NEWID(), @Team3, @Project3, @Dev1, N'Percent', 35.00, @Now, @By, 0),
    (NEWID(), @Team3, @Project3, @Dev6, N'Percent', 25.00, @Now, @By, 0),
    (NEWID(), @Team8, @Project6, @Dev8, N'Percent', 60.00, @Now, @By, 0),
    (NEWID(), @Team8, @Project6, @Dev5, N'Percent', 40.00, @Now, @By, 0),
    (NEWID(), @Team6, @Project7, @Dev6, N'Percent', 70.00, @Now, @By, 0),
    (NEWID(), @Team6, @Project7, @Dev7, N'Percent', 30.00, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 24. REVIEWS (marketplace)                                                   */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[Reviews]
    ([Id], [ProjectId], [ReviewerUserId], [RevieweeType], [RevieweeTeamId], [RevieweeUserId],
     [Rating], [Comment], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Project6, @Client6, N'Team', @Team8, NULL, 5, N'Delivered on time with excellent documentation.', @Now, @By, 0),
    (NEWID(), @Project6, @Client6, N'User', NULL, @Dev8, 5, N'Great communication throughout the project.',       @Now, @By, 0),
    (NEWID(), @Project2, @Client2, N'Team', @Team2, NULL, 4, N'Strong design, minor reporting delays.',           @Now, @By, 0),
    (NEWID(), @Project2, @Client2, N'User', NULL, @Dev2, 4, N'Reliable lead, clear status updates.',              @Now, @By, 0),
    (NEWID(), @Project3, @Client3, N'Team', @Team3, NULL, 5, N'Two milestones approved with no rework.',          @Now, @By, 0),
    (NEWID(), @Project3, @Client3, N'User', NULL, @Dev3, 5, N'Very responsive and detail oriented.',              @Now, @By, 0),
    (NEWID(), @Project7, @Client7, N'Team', @Team6, NULL, 4, N'Pipeline work was smooth and well tested.',        @Now, @By, 0),
    (NEWID(), @Project1, @Client1, N'Team', @Team1, NULL, 4, N'Good proposal and planning phase.',                @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 25. SAVED PROJECTS                                                          */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[SavedProjects]
    ([Id], [UserId], [ProjectId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Dev1, @Project4, @Now, @By, 0),
    (NEWID(), @Dev1, @Project5, @Now, @By, 0),
    (NEWID(), @Dev2, @Project8, @Now, @By, 0),
    (NEWID(), @Dev3, @Project1, @Now, @By, 0),
    (NEWID(), @Dev4, @Project2, @Now, @By, 0),
    (NEWID(), @Dev5, @Project8, @Now, @By, 0),
    (NEWID(), @Dev6, @Project1, @Now, @By, 0),
    (NEWID(), @Dev7, @Project4, @Now, @By, 0),
    (NEWID(), @Dev8, @Project5, @Now, @By, 0),
    (NEWID(), @Dev3, @Project8, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 26. PROJECT MEMBERS                                                         */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[ProjectMembers]
    ([Id], [ProjectId], [UserId], [RoleInProject], [AssignedByUserId], [AssignedAt],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Project2, @Dev2, N'Frontend Lead', @Client2, DATEADD(DAY, -25, @Now), @Now, @By, 0),
    (NEWID(), @Project2, @Dev3, N'Mobile Support', @Client2, DATEADD(DAY, -25, @Now), @Now, @By, 0),
    (NEWID(), @Project2, @Dev8, N'Data & Reports', @Client2, DATEADD(DAY, -25, @Now), @Now, @By, 0),
    (NEWID(), @Project3, @Dev3, N'App Lead',       @Client3, DATEADD(DAY, -24, @Now), @Now, @By, 0),
    (NEWID(), @Project3, @Dev1, N'Full-Stack',     @Client3, DATEADD(DAY, -24, @Now), @Now, @By, 0),
    (NEWID(), @Project3, @Dev6, N'Infrastructure', @Client3, DATEADD(DAY, -24, @Now), @Now, @By, 0),
    (NEWID(), @Project6, @Dev8, N'Data Lead',      @Client6, DATEADD(DAY, -75, @Now), @Now, @By, 0),
    (NEWID(), @Project6, @Dev5, N'QA Engineer',    @Client6, DATEADD(DAY, -75, @Now), @Now, @By, 0),
    (NEWID(), @Project7, @Dev6, N'DevOps Lead',    @Client7, DATEADD(DAY, -15, @Now), @Now, @By, 0),
    (NEWID(), @Project7, @Dev7, N'Design Support', @Client7, DATEADD(DAY, -15, @Now), @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 27. CHAT ROOMS (chat)                                                       */
/* -------------------------------------------------------------------------- */

DECLARE @RoomMain1 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000001';
DECLARE @RoomMain2 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000002';
DECLARE @RoomMain3 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000003';
DECLARE @RoomMain4 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000004';
DECLARE @RoomMain5 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000005';
DECLARE @RoomMain6 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000006';
DECLARE @RoomMain7 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000007';
DECLARE @RoomMain8 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000008';
DECLARE @RoomGroup1 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000011';
DECLARE @RoomGroup2 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000012';
DECLARE @RoomProp1 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000021';
DECLARE @RoomProp2 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000022';
DECLARE @RoomProp3 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000023';
DECLARE @RoomProp4 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000024';
DECLARE @RoomProp5 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000025';
DECLARE @RoomProp6 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000026';
DECLARE @RoomProp7 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000027';
DECLARE @RoomProp8 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000028';
DECLARE @RoomProp9 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000029';
DECLARE @RoomProp10 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000030';
DECLARE @RoomProp11 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000031';
DECLARE @RoomProj2 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000041';
DECLARE @RoomProj3 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000042';
DECLARE @RoomProj6 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000043';
DECLARE @RoomProj7 UNIQUEIDENTIFIER = 'a7a7a7a7-a7a7-a7a7-a7a7-000000000044';

INSERT INTO [chat].[ChatRooms]
    ([Id], [RoomType], [Status], [TeamId], [ProjectId], [ProposalId], [CreatedByUserId],
     [Title], [Logo], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@RoomMain1, N'TeamMain', N'Active', @Team1, NULL, NULL, @Dev1,
     N'Nile Code Studio — Main Room', N'https://cdn.example.com/seed/team1-logo.png', @Now, @By, 0),
    (@RoomMain2, N'TeamMain', N'Active', @Team2, NULL, NULL, @Dev2,
     N'Pixel Crafters — Main Room', N'https://cdn.example.com/seed/team2-logo.png', @Now, @By, 0),
    (@RoomMain3, N'TeamMain', N'Active', @Team3, NULL, NULL, @Dev3,
     N'Mobiflex — Main Room', N'https://cdn.example.com/seed/team3-logo.png', @Now, @By, 0),
    (@RoomMain4, N'TeamMain', N'Active', @Team4, NULL, NULL, @Dev4,
     N'API Forge — Main Room', N'https://cdn.example.com/seed/team4-logo.png', @Now, @By, 0),
    (@RoomMain5, N'TeamMain', N'Active', @Team5, NULL, NULL, @Dev5,
     N'Quality First Labs — Main Room', N'https://cdn.example.com/seed/team5-logo.png', @Now, @By, 0),
    (@RoomMain6, N'TeamMain', N'Active', @Team6, NULL, NULL, @Dev6,
     N'CloudNest — Main Room', N'https://cdn.example.com/seed/team6-logo.png', @Now, @By, 0),
    (@RoomMain7, N'TeamMain', N'Active', @Team7, NULL, NULL, @Dev7,
     N'Design Orbit — Main Room', N'https://cdn.example.com/seed/team7-logo.png', @Now, @By, 0),
    (@RoomMain8, N'TeamMain', N'Active', @Team8, NULL, NULL, @Dev8,
     N'Data Minds — Main Room', N'https://cdn.example.com/seed/team8-logo.png', @Now, @By, 0),
    (@RoomGroup1, N'TeamGroup', N'Active', @Team1, NULL, NULL, @Dev1,
     N'Design & Frontend', NULL, @Now, @By, 0),
    (@RoomGroup2, N'TeamGroup', N'Active', @Team2, NULL, NULL, @Dev2,
     N'Mobile Squad', NULL, @Now, @By, 0),
    (@RoomProp1, N'Proposal', N'Active', @Team1, NULL, @Proposal1, @Dev1,
     N'Proposal — E-commerce platform revamp', NULL, @Now, @By, 0),
    (@RoomProp2, N'Proposal', N'Active', NULL, NULL, @Proposal2, @Dev2,
     N'Proposal — E-commerce platform revamp', NULL, @Now, @By, 0),
    (@RoomProp3, N'Proposal', N'Active', NULL, NULL, @Proposal3, @Dev4,
     N'Proposal — E-commerce platform revamp', NULL, @Now, @By, 0),
    (@RoomProp4, N'Proposal', N'Active', @Team2, NULL, @Proposal4, @Dev2,
     N'Proposal — SaaS dashboard MVP', NULL, @Now, @By, 0),
    (@RoomProp5, N'Proposal', N'Active', @Team3, NULL, @Proposal5, @Dev3,
     N'Proposal — Mobile fitness app', NULL, @Now, @By, 0),
    (@RoomProp6, N'Proposal', N'Active', @Team4, NULL, @Proposal6, @Dev4,
     N'Proposal — Booking platform backend', NULL, @Now, @By, 0),
    (@RoomProp7, N'Proposal', N'Active', NULL, NULL, @Proposal7, @Dev2,
     N'Proposal — Booking platform backend', NULL, @Now, @By, 0),
    (@RoomProp8, N'Proposal', N'Active', @Team8, NULL, @Proposal8, @Dev8,
     N'Proposal — Shopify store optimization', NULL, @Now, @By, 0),
    (@RoomProp9, N'Proposal', N'Active', @Team5, NULL, @Proposal9, @Dev5,
     N'Proposal — Security audit web application', NULL, @Now, @By, 0),
    (@RoomProp10, N'Proposal', N'Active', NULL, NULL, @Proposal10, @Dev8,
     N'Proposal — Security audit web application', NULL, @Now, @By, 0),
    (@RoomProp11, N'Proposal', N'Active', @Team6, NULL, @Proposal11, @Dev6,
     N'Proposal — CI/CD migration for client platform', NULL, @Now, @By, 0);

INSERT INTO [chat].[ChatRooms]
    ([Id], [RoomType], [Status], [TeamId], [ProjectId], [ProposalId], [SourceProposalRoomId],
     [CreatedByUserId], [Title], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@RoomProj2, N'Project', N'Active', @Team2, @Project2, NULL, @RoomProp4, @Client2,
     N'SaaS dashboard MVP', DATEADD(DAY, -13, @Now), @By, 0),
    (@RoomProj3, N'Project', N'Active', @Team3, @Project3, NULL, @RoomProp5, @Client3,
     N'Mobile fitness app', DATEADD(DAY, -16, @Now), @By, 0),
    (@RoomProj6, N'Project', N'Active', @Team8, @Project6, NULL, @RoomProp8, @Client6,
     N'Shopify store optimization', DATEADD(DAY, -88, @Now), @By, 0),
    (@RoomProj7, N'Project', N'Active', @Team6, @Project7, NULL, @RoomProp11, @Client7,
     N'CI/CD migration for client platform', DATEADD(DAY, -18, @Now), @By, 0);

/* -------------------------------------------------------------------------- */
/* 28. CHAT ROOM MEMBERS                                                       */
/* -------------------------------------------------------------------------- */

INSERT INTO [chat].[ChatRoomMembers]
    ([Id], [ChatRoomId], [ClientProfileId], [DeveloperProfileId], [JoinedAt], [LastReadAt],
     [RoleLabel], [CanSend], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @RoomMain1, NULL, @Dp1, N'2024-01-10', DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomMain1, NULL, @Dp2, N'2024-02-14', DATEADD(DAY, -1, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain1, NULL, @Dp7, N'2024-03-01', DATEADD(DAY, -2, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain2, NULL, @Dp2, N'2024-01-20', DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomMain2, NULL, @Dp3, N'2024-04-02', DATEADD(DAY, -1, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain2, NULL, @Dp8, N'2024-05-18', DATEADD(DAY, -2, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain3, NULL, @Dp3, N'2024-01-05', DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomMain3, NULL, @Dp1, N'2024-02-22', DATEADD(DAY, -2, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain3, NULL, @Dp6, N'2024-03-15', DATEADD(DAY, -2, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain4, NULL, @Dp4, N'2024-01-12', DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomMain4, NULL, @Dp5, N'2024-06-01', DATEADD(DAY, -1, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain4, NULL, @Dp8, N'2024-06-20', DATEADD(DAY, -2, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain5, NULL, @Dp5, N'2024-02-08', DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomMain5, NULL, @Dp6, N'2024-07-11', DATEADD(DAY, -2, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain6, NULL, @Dp6, N'2024-01-25', DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomMain6, NULL, @Dp7, N'2024-05-09', DATEADD(DAY, -2, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain7, NULL, @Dp7, N'2024-01-30', DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomMain7, NULL, @Dp2, N'2024-08-04', DATEADD(DAY, -2, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain7, NULL, @Dp4, N'2024-08-15', DATEADD(DAY, -2, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomMain8, NULL, @Dp8, N'2024-02-01', DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomMain8, NULL, @Dp5, N'2024-09-10', DATEADD(DAY, -2, @Now), N'Member',       1, @Now, @By, 0),
    (NEWID(), @RoomGroup1, NULL, @Dp1, N'2024-03-02', DATEADD(DAY, -1, @Now), N'Member', 1, @Now, @By, 0),
    (NEWID(), @RoomGroup1, NULL, @Dp2, N'2024-03-02', DATEADD(DAY, -1, @Now), N'Member', 1, @Now, @By, 0),
    (NEWID(), @RoomGroup1, NULL, @Dp7, N'2024-03-02', DATEADD(DAY, -1, @Now), N'Member', 1, @Now, @By, 0),
    (NEWID(), @RoomGroup2, NULL, @Dp2, N'2024-04-10', DATEADD(DAY, -1, @Now), N'Member', 1, @Now, @By, 0),
    (NEWID(), @RoomGroup2, NULL, @Dp3, N'2024-04-10', DATEADD(DAY, -1, @Now), N'Member', 1, @Now, @By, 0),
    (NEWID(), @RoomProp1, @Cp1, NULL, DATEADD(DAY, -12, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp1, NULL, @Dp1, DATEADD(DAY, -12, @Now), DATEADD(DAY, -1, @Now), N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProp2, @Cp1, NULL, DATEADD(DAY, -11, @Now), NULL, N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp2, NULL, @Dp2, DATEADD(DAY, -11, @Now), NULL, N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProp3, @Cp1, NULL, DATEADD(DAY, -6, @Now), NULL, N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp3, NULL, @Dp4, DATEADD(DAY, -6, @Now), NULL, N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProp4, @Cp2, NULL, DATEADD(DAY, -15, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp4, NULL, @Dp2, DATEADD(DAY, -15, @Now), DATEADD(DAY, -1, @Now), N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProp5, @Cp3, NULL, DATEADD(DAY, -18, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp5, NULL, @Dp3, DATEADD(DAY, -18, @Now), DATEADD(DAY, -1, @Now), N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProp6, @Cp4, NULL, DATEADD(DAY, -10, @Now), NULL, N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp6, NULL, @Dp4, DATEADD(DAY, -10, @Now), NULL, N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProp7, @Cp4, NULL, DATEADD(DAY, -4, @Now), NULL, N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp7, NULL, @Dp2, DATEADD(DAY, -4, @Now), NULL, N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProp8, @Cp6, NULL, DATEADD(DAY, -90, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp8, NULL, @Dp8, DATEADD(DAY, -90, @Now), DATEADD(DAY, -1, @Now), N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProp9, @Cp8, NULL, DATEADD(DAY, -5, @Now), NULL, N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp9, NULL, @Dp5, DATEADD(DAY, -5, @Now), NULL, N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProp10, @Cp8, NULL, DATEADD(DAY, -7, @Now), NULL, N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp10, NULL, @Dp8, DATEADD(DAY, -7, @Now), NULL, N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProp11, @Cp7, NULL, DATEADD(DAY, -20, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp11, NULL, @Dp6, DATEADD(DAY, -20, @Now), DATEADD(DAY, -1, @Now), N'Applicant', 1, @Now, @By, 0),
    (NEWID(), @RoomProj2, @Cp2, NULL, DATEADD(DAY, -13, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProj2, NULL, @Dp2, DATEADD(DAY, -13, @Now), DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomProj2, NULL, @Dp3, DATEADD(DAY, -13, @Now), DATEADD(DAY, -2, @Now), N'Member', 1, @Now, @By, 0),
    (NEWID(), @RoomProj2, NULL, @Dp8, DATEADD(DAY, -13, @Now), DATEADD(DAY, -2, @Now), N'Member', 1, @Now, @By, 0),
    (NEWID(), @RoomProj3, @Cp3, NULL, DATEADD(DAY, -16, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProj3, NULL, @Dp3, DATEADD(DAY, -16, @Now), DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomProj3, NULL, @Dp1, DATEADD(DAY, -16, @Now), DATEADD(DAY, -2, @Now), N'Member', 1, @Now, @By, 0),
    (NEWID(), @RoomProj3, NULL, @Dp6, DATEADD(DAY, -16, @Now), DATEADD(DAY, -2, @Now), N'Member', 1, @Now, @By, 0),
    (NEWID(), @RoomProj6, @Cp6, NULL, DATEADD(DAY, -88, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProj6, NULL, @Dp8, DATEADD(DAY, -88, @Now), DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomProj6, NULL, @Dp5, DATEADD(DAY, -88, @Now), DATEADD(DAY, -2, @Now), N'Member', 1, @Now, @By, 0),
    (NEWID(), @RoomProj7, @Cp7, NULL, DATEADD(DAY, -18, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProj7, NULL, @Dp6, DATEADD(DAY, -18, @Now), DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomProj7, NULL, @Dp7, DATEADD(DAY, -18, @Now), DATEADD(DAY, -2, @Now), N'Member', 1, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 29. MESSAGES (chat)                                                         */
/* -------------------------------------------------------------------------- */

DECLARE @Msg1  UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000001';
DECLARE @Msg2  UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000002';
DECLARE @Msg3  UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000003';
DECLARE @Msg4  UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000004';
DECLARE @Msg5  UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000005';
DECLARE @Msg6  UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000006';
DECLARE @Msg7  UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000007';
DECLARE @Msg8  UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000008';
DECLARE @Msg9  UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000009';
DECLARE @Msg10 UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000010';
DECLARE @Msg11 UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000011';
DECLARE @Msg12 UNIQUEIDENTIFIER = 'a8a8a8a8-a8a8-a8a8-a8a8-000000000012';

INSERT INTO [chat].[Messages]
    ([Id], [ChatRoomId], [SenderClientProfileId], [SenderDeveloperProfileId], [MessageType],
     [Text], [FileUrl], [FileName], [PlanVersionId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@Msg1, @RoomProp1, @Cp1, NULL, N'Text',
     N'Thanks for the proposal. Can you share a few portfolio references before we continue?', NULL, NULL, NULL,
     DATEADD(DAY, -11, @Now), @By, 0),
    (@Msg2, @RoomProp1, NULL, @Dp1, N'Attachment',
     N'Attached our portfolio and timeline for the rebuild.', N'https://cdn.example.com/seed/att-proposal1-portfolio.pdf', N'portfolio.pdf', NULL,
     DATEADD(DAY, -11, @Now), @By, 0),
    (@Msg3, @RoomProp1, @Cp1, NULL, N'Text',
     N'Great, the timeline looks workable for us.', NULL, NULL, NULL,
     DATEADD(DAY, -10, @Now), @By, 0),
    (@Msg4, @RoomProj2, NULL, @Dp2, N'MilestonePlan',
     N'Updated milestone plan — three equal milestones for the MVP.', NULL, NULL, @PlanV3,
     DATEADD(DAY, -12, @Now), @By, 0),
    (@Msg5, @RoomProj2, @Cp2, NULL, N'Text',
     N'The updated milestones look good. Go ahead with development.', NULL, NULL, NULL,
     DATEADD(DAY, -11, @Now), @By, 0),
    (@Msg6, @RoomProj2, NULL, @Dp3, N'Text',
     N'Frontend package is building cleanly on staging now.', NULL, NULL, NULL,
     DATEADD(DAY, -3, @Now), @By, 0),
    (@Msg7, @RoomProj2, @Cp2, NULL, N'Text',
     N'M2 dashboard exports approved. Nice work.', NULL, NULL, NULL,
     DATEADD(DAY, -5, @Now), @By, 0),
    (@Msg8, @RoomMain1, NULL, @Dp1, N'Text',
     N'Quick stand-up in 10 minutes.', NULL, NULL, NULL,
     DATEADD(DAY, -2, @Now), @By, 0),
    (@Msg9, @RoomMain1, NULL, @Dp2, N'Text',
     N'Design review ready for the new checkout flow.', NULL, NULL, NULL,
     DATEADD(DAY, -2, @Now), @By, 0),
    (@Msg10, @RoomMain1, NULL, @Dp7, N'Attachment',
     N'Figma links posted below.', N'https://cdn.example.com/seed/team1-checkout.fig', N'checkout-flow.fig', NULL,
     DATEADD(DAY, -2, @Now), @By, 0),
    (@Msg11, @RoomProj6, NULL, NULL, N'System',
     N'Project marked as completed.', NULL, NULL, NULL,
     DATEADD(DAY, -3, @Now), @By, 0),
    (@Msg12, @RoomProp8, NULL, @Dp8, N'Attachment',
     N'Full data approach attached.', N'https://cdn.example.com/seed/att-proposal8-data.pdf', N'data-approach.pdf', NULL,
     DATEADD(DAY, -88, @Now), @By, 0);

/* -------------------------------------------------------------------------- */
/* 30. NOTIFICATIONS (core)                                                    */
/* -------------------------------------------------------------------------- */

INSERT INTO [core].[Notifications]
    ([Id], [Title], [Body], [Type], [ActionUrl], [Data], [IsRead], [ReadAt],
     [UserId], [ClientProfileId], [DeveloperProfileId], [ProjectId], [ProjectProposalId], [TeamId],
     [MilestoneId], [ChatRoomId], [MessageId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), N'New proposal received',
     N'A new proposal was submitted for your E-commerce platform revamp.', 1,
     N'/projects/90909090-9090-9090-9090-000000000001', N'{"proposalId":"a1a1a1a1-a1a1-a1a1-a1a1-000000000001"}',
     1, DATEADD(DAY, -12, @Now), @Client1, @Cp1, NULL, @Project1, @Proposal1, NULL, NULL, NULL, NULL,
     DATEADD(DAY, -12, @Now), @By, 0),
    (NEWID(), N'Proposal accepted',
     N'Your proposal for the SaaS dashboard MVP was accepted.', 2,
     N'/projects/90909090-9090-9090-9090-000000000002', N'{"proposalId":"a1a1a1a1-a1a1-a1a1-a1a1-000000000004"}',
     1, DATEADD(DAY, -13, @Now), @Dev2, NULL, @Dp2, @Project2, @Proposal4, @Team2, NULL, NULL, NULL,
     DATEADD(DAY, -13, @Now), @By, 0),
    (NEWID(), N'Milestone plan agreed',
     N'Your client approved the milestone plan for the SaaS dashboard MVP.', 9,
     N'/projects/90909090-9090-9090-9090-000000000002/plan', NULL,
     1, DATEADD(DAY, -12, @Now), @Dev2, NULL, @Dp2, @Project2, NULL, @Team2, NULL, NULL, NULL,
     DATEADD(DAY, -12, @Now), @By, 0),
    (NEWID(), N'Milestone released',
     N'Design & frontend foundation milestone payment has been released.', 15,
     N'/projects/90909090-9090-9090-9090-000000000002', NULL,
     1, DATEADD(DAY, -15, @Now), @Dev2, NULL, @Dp2, @Project2, NULL, @Team2, @M1, NULL, NULL,
     DATEADD(DAY, -15, @Now), @By, 0),
    (NEWID(), N'Milestone approved',
     N'Workout tracking module was approved by the client.', 14,
     N'/projects/90909090-9090-9090-9090-000000000003', NULL,
     1, DATEADD(DAY, -4, @Now), @Dev3, NULL, @Dp3, @Project3, NULL, @Team3, @M5, NULL, NULL,
     DATEADD(DAY, -4, @Now), @By, 0),
    (NEWID(), N'Plan changes requested',
     N'The client requested changes to the CI/CD migration milestone plan.', 8,
     N'/projects/90909090-9090-9090-9090-000000000007/plan', NULL,
     0, NULL, @Dev6, NULL, @Dp6, @Project7, @Proposal11, @Team6, NULL, NULL, NULL,
     DATEADD(DAY, -6, @Now), @By, 0),
    (NEWID(), N'Escrow locked',
     N'Your escrow for the CI/CD migration project is now locked.', 11,
     N'/projects/90909090-9090-9090-9090-000000000007', NULL,
     1, DATEADD(DAY, -14, @Now), @Client7, @Cp7, NULL, @Project7, NULL, NULL, NULL, NULL, NULL,
     DATEADD(DAY, -14, @Now), @By, 0),
    (NEWID(), N'Milestone submitted',
     N'Offline sync & store launch milestone was submitted for review.', 12,
     N'/projects/90909090-9090-9090-9090-000000000003', NULL,
     0, NULL, @Client3, @Cp3, NULL, @Project3, NULL, NULL, @M6, NULL, NULL,
     DATEADD(DAY, -1, @Now), @By, 0),
    (NEWID(), N'Join request received',
     N'A developer requested to join Nile Code Studio.', 4,
     N'/teams/dddddddd-dddd-dddd-dddd-000000000001/members', NULL,
     1, DATEADD(DAY, -2, @Now), @Dev1, NULL, @Dp1, NULL, NULL, @Team1, NULL, NULL, NULL,
     DATEADD(DAY, -2, @Now), @By, 0),
    (NEWID(), N'Join request accepted',
     N'Your request to join Nile Code Studio was accepted.', 5,
     N'/teams/dddddddd-dddd-dddd-dddd-000000000001', NULL,
     1, DATEADD(DAY, -18, @Now), @Dev6, NULL, @Dp6, NULL, NULL, @Team1, NULL, NULL, NULL,
     DATEADD(DAY, -18, @Now), @By, 0),
    (NEWID(), N'Wallet updated',
     N'Your team split of 4500.00 USD was credited to your wallet.', 19,
     N'/wallet', N'{"amount":4500.00,"currency":"USD"}',
     1, DATEADD(DAY, -3, @Now), @Dev8, NULL, @Dp8, @Project6, NULL, @Team8, NULL, NULL, NULL,
     DATEADD(DAY, -3, @Now), @By, 0),
    (NEWID(), N'Review reminder',
     N'Please leave a review for the completed Shopify store optimization project.', 17,
     N'/projects/90909090-9090-9090-9090-000000000006/review', NULL,
     0, NULL, @Client6, @Cp6, NULL, @Project6, NULL, NULL, NULL, NULL, NULL,
     DATEADD(DAY, -1, @Now), @By, 0),
    (NEWID(), N'New chat message',
     N'You have a new message about the E-commerce platform revamp.', 16,
     N'/messages/a7a7a7a7-a7a7-a7a7-a7a7-000000000021', NULL,
     1, DATEADD(DAY, -11, @Now), @Client1, @Cp1, NULL, @Project1, NULL, NULL, NULL, @RoomProp1, @Msg1,
     DATEADD(DAY, -11, @Now), @By, 0),
    (NEWID(), N'Milestone plan proposed',
     N'API Forge proposed a milestone plan for the Booking platform backend.', 7,
     N'/projects/90909090-9090-9090-9090-000000000004/plan', NULL,
     0, NULL, @Client4, @Cp4, NULL, @Project4, @Proposal6, @Team4, NULL, NULL, NULL,
     DATEADD(DAY, -8, @Now), @By, 0);

/* -------------------------------------------------------------------------- */
/* 31. PORTFOLIO PROJECTS (portfolio)                                          */
/* -------------------------------------------------------------------------- */

DECLARE @Portfolio1 UNIQUEIDENTIFIER = 'a4a4a4a4-a4a4-a4a4-a4a4-000000000001';
DECLARE @Portfolio2 UNIQUEIDENTIFIER = 'a4a4a4a4-a4a4-a4a4-a4a4-000000000002';
DECLARE @Portfolio3 UNIQUEIDENTIFIER = 'a4a4a4a4-a4a4-a4a4-a4a4-000000000003';
DECLARE @Portfolio4 UNIQUEIDENTIFIER = 'a4a4a4a4-a4a4-a4a4-a4a4-000000000004';
DECLARE @Portfolio5 UNIQUEIDENTIFIER = 'a4a4a4a4-a4a4-a4a4-a4a4-000000000005';
DECLARE @Portfolio6 UNIQUEIDENTIFIER = 'a4a4a4a4-a4a4-a4a4-a4a4-000000000006';
DECLARE @Portfolio7 UNIQUEIDENTIFIER = 'a4a4a4a4-a4a4-a4a4-a4a4-000000000007';
DECLARE @Portfolio8 UNIQUEIDENTIFIER = 'a4a4a4a4-a4a4-a4a4-a4a4-000000000008';

INSERT INTO [portfolio].[PortfolioProjects]
    ([Id], [OwnerType], [OwnerUserId], [OwnerTeamId], [Title], [Description], [Budget],
     [ImageCover], [ProjectUrl], [PrototypeUrl], [CompletionDate], [CategoryId], [Visibility],
     [Challenge], [Solution], [DurationLabel], [Industry], [TeamLeads],
     [TestimonialQuote], [TestimonialAuthorName], [TestimonialAuthorTitle], [TestimonialAuthorAvatarUrl],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@Portfolio1, N'User', @Dev1, NULL,
     N'E-commerce Storefront Revamp',
     N'Full storefront rebuild: faster checkout, new product pages and an admin dashboard.',
     4500.00,
     N'https://cdn.example.com/seed/pf1-cover.png',
     N'https://nilecode.example.com/work/ecommerce', NULL,
     DATEADD(DAY, -30, @Now), @CatEcom, N'Public',
     N'Legacy checkout was losing conversions and the admin tooling was manual.',
     N'Component-driven storefront refactor with a clean API and role-based admin panel.',
     N'7 weeks', N'Retail & E-commerce', NULL,
     N'Checkout conversions improved by 32% after launch.',
     N'Laila Hassan', N'Operations Director', N'https://cdn.example.com/seed/avatar-client1.png',
     @Now, @By, 0),
    (@Portfolio2, N'User', @Dev2, NULL,
     N'SaaS Analytics Dashboard',
     N'B2B analytics dashboard MVP with auth, role management and reporting.',
     6000.00,
     N'https://cdn.example.com/seed/pf2-cover.png',
     N'https://pixelcrafters.example.com/work/saas', NULL,
     DATEADD(DAY, -20, @Now), @CatSaas, N'Public',
     N'Client needed a white-label dashboard their customers could use daily.',
     N'Design system first, then auth, roles and reporting modules.',
     N'5 weeks', N'SaaS / B2B', NULL,
     NULL, NULL, NULL, NULL,
     @Now, @By, 0),
    (@Portfolio3, N'User', @Dev3, NULL,
     N'Cross-Platform Fitness App',
     N'Fitness tracking app for iOS and Android with workout plans and offline mode.',
     7000.00,
     N'https://cdn.example.com/seed/pf3-cover.png',
     N'https://mobiflex.example.com/cases/fitness', NULL,
     DATEADD(DAY, -15, @Now), @CatMobile, N'Public',
     N'Users needed offline tracking when training in remote areas.',
     N'Shared Flutter codebase with an offline-first sync engine.',
     N'8 weeks', N'Health & Fitness', NULL,
     N'Rated 4.8 on both app stores in the first month.',
     N'Omar Khalil', N'Product Manager', N'https://cdn.example.com/seed/avatar-client3.png',
     @Now, @By, 0),
    (@Portfolio4, N'Team', NULL, @Team1,
     N'Multi-Tenant SaaS Platform',
     N'End-to-end SaaS platform with tenant isolation, billing and usage metering.',
     18000.00,
     N'https://cdn.example.com/seed/pf4-cover.png',
     N'https://nilecode.example.com/work/saas-platform', NULL,
     DATEADD(DAY, -60, @Now), @CatSaas, N'Public',
     N'Growing startup needed a reliable multi-tenant foundation.',
     N'Full-stack delivery with tenant-scoped data model and subscription billing.',
     N'12 weeks', N'SaaS / B2B', N'Salma (lead), Karim, Nour',
     N'The platform handled 4x growth without infrastructure changes.',
     N'Yara Mostafa', N'CEO', N'https://cdn.example.com/seed/avatar-client4.png',
     @Now, @By, 0),
    (@Portfolio5, N'Team', NULL, @Team2,
     N'Design System for Fintech Suite',
     N'Token-based design system powering a suite of fintech products.',
     3500.00,
     N'https://cdn.example.com/seed/pf5-cover.png',
     N'https://pixelcrafters.example.com/work/fintech', NULL,
     DATEADD(DAY, -25, @Now), @CatUiux, N'Public',
     N'Three products shipped with inconsistent components.',
     N'Token-based library with accessibility baked in.',
     N'4 weeks', N'Fintech', NULL,
     NULL, NULL, NULL, NULL,
     @Now, @By, 0),
    (@Portfolio6, N'User', @Dev5, NULL,
     N'QA Automation for Logistics Platform',
     N'End-to-end automated test suite for a logistics tracking platform.',
     2800.00,
     N'https://cdn.example.com/seed/pf6-cover.png',
     N'https://qflabs.example.com/work/logistics', NULL,
     DATEADD(DAY, -18, @Now), @CatQa, N'Private',
     N'Manual regression runs took two days per release.',
     N'Cypress E2E suite wired into CI with flake detection.',
     N'3 weeks', N'Logistics', NULL,
     NULL, NULL, NULL, NULL,
     @Now, @By, 0),
    (@Portfolio7, N'Team', NULL, @Team8,
     N'Churn Prediction Service',
     N'ML service predicting customer churn from usage telemetry with a clean API.',
     8000.00,
     N'https://cdn.example.com/seed/pf7-cover.png',
     N'https://dataminds.example.com/work/churn', NULL,
     DATEADD(DAY, -40, @Now), @CatDataai, N'Public',
     N'Churn was detected too late to act on it.',
     N'ETL pipeline feeding a validated churn model behind a documented API.',
     N'9 weeks', N'Data & Analytics', N'Dina (lead), Hany',
     N'Retention team now acts on predictions two weeks earlier.',
     N'Rania Adel', N'VP Engineering', N'https://cdn.example.com/seed/avatar-client6.png',
     @Now, @By, 0),
    (@Portfolio8, N'Team', NULL, @Team6,
     N'Kubernetes Migration & CI/CD',
     N'Containerized migration of a production platform with automated delivery.',
     5000.00,
     N'https://cdn.example.com/seed/pf8-cover.png',
     N'https://cloudnest.example.com/work/k8s', NULL,
     DATEADD(DAY, -12, @Now), @CatDevops, N'Public',
     N'Deployments were manual and rollbacks were painful.',
     N'Assessment, containerization, CI/CD wiring and gradual traffic shift.',
     N'4 weeks', N'DevOps / Cloud', NULL,
     NULL, NULL, NULL, NULL,
     @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 32. PORTFOLIO IMAGES / SKILLS                                               */
/* -------------------------------------------------------------------------- */

INSERT INTO [portfolio].[PortfolioImages]
    ([Id], [PortfolioProjectId], [ImageUrl], [SortOrder], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Portfolio1, N'https://cdn.example.com/seed/pf1-1.png', 1, @Now, @By, 0),
    (NEWID(), @Portfolio1, N'https://cdn.example.com/seed/pf1-2.png', 2, @Now, @By, 0),
    (NEWID(), @Portfolio2, N'https://cdn.example.com/seed/pf2-1.png', 1, @Now, @By, 0),
    (NEWID(), @Portfolio2, N'https://cdn.example.com/seed/pf2-2.png', 2, @Now, @By, 0),
    (NEWID(), @Portfolio3, N'https://cdn.example.com/seed/pf3-1.png', 1, @Now, @By, 0),
    (NEWID(), @Portfolio3, N'https://cdn.example.com/seed/pf3-2.png', 2, @Now, @By, 0),
    (NEWID(), @Portfolio4, N'https://cdn.example.com/seed/pf4-1.png', 1, @Now, @By, 0),
    (NEWID(), @Portfolio4, N'https://cdn.example.com/seed/pf4-2.png', 2, @Now, @By, 0),
    (NEWID(), @Portfolio5, N'https://cdn.example.com/seed/pf5-1.png', 1, @Now, @By, 0),
    (NEWID(), @Portfolio6, N'https://cdn.example.com/seed/pf6-1.png', 1, @Now, @By, 0),
    (NEWID(), @Portfolio7, N'https://cdn.example.com/seed/pf7-1.png', 1, @Now, @By, 0),
    (NEWID(), @Portfolio7, N'https://cdn.example.com/seed/pf7-2.png', 2, @Now, @By, 0),
    (NEWID(), @Portfolio8, N'https://cdn.example.com/seed/pf8-1.png', 1, @Now, @By, 0),
    (NEWID(), @Portfolio8, N'https://cdn.example.com/seed/pf8-2.png', 2, @Now, @By, 0);

INSERT INTO [portfolio].[PortfolioSkills]
    ([Id], [PortfolioProjectId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Portfolio1, @SkillReact,   @Now, @By, 0),
    (NEWID(), @Portfolio1, @SkillTs,      @Now, @By, 0),
    (NEWID(), @Portfolio2, @SkillReact,   @Now, @By, 0),
    (NEWID(), @Portfolio2, @SkillCsharp,  @Now, @By, 0),
    (NEWID(), @Portfolio3, @SkillFlutter, @Now, @By, 0),
    (NEWID(), @Portfolio3, @SkillDart,    @Now, @By, 0),
    (NEWID(), @Portfolio4, @SkillAsp,     @Now, @By, 0),
    (NEWID(), @Portfolio4, @SkillSql,     @Now, @By, 0),
    (NEWID(), @Portfolio5, @SkillFigma,   @Now, @By, 0),
    (NEWID(), @Portfolio6, @SkillCypress, @Now, @By, 0),
    (NEWID(), @Portfolio6, @SkillPostman, @Now, @By, 0),
    (NEWID(), @Portfolio7, @SkillPython,  @Now, @By, 0),
    (NEWID(), @Portfolio7, @SkillPandas,  @Now, @By, 0),
    (NEWID(), @Portfolio8, @SkillDocker,  @Now, @By, 0),
    (NEWID(), @Portfolio8, @SkillK8s,     @Now, @By, 0),
    (NEWID(), @Portfolio8, @SkillAws,     @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 33. PORTFOLIO ROADMAP STEPS / METRICS                                       */
/* -------------------------------------------------------------------------- */

INSERT INTO [portfolio].[PortfolioRoadmapSteps]
    ([Id], [PortfolioProjectId], [Title], [SortOrder], [IsDone])
VALUES
    (NEWID(), @Portfolio1, N'Discovery & scoping',       1, 1),
    (NEWID(), @Portfolio1, N'Storefront rebuild',        2, 1),
    (NEWID(), @Portfolio1, N'Admin dashboard',           3, 1),
    (NEWID(), @Portfolio1, N'Performance & launch',      4, 1),
    (NEWID(), @Portfolio2, N'Auth & roles',              1, 1),
    (NEWID(), @Portfolio2, N'Dashboard widgets',         2, 1),
    (NEWID(), @Portfolio3, N'Flutter foundation',        1, 1),
    (NEWID(), @Portfolio3, N'Workout engine',            2, 1),
    (NEWID(), @Portfolio3, N'Offline sync',              3, 1),
    (NEWID(), @Portfolio7, N'Data pipeline',             1, 1),
    (NEWID(), @Portfolio7, N'Model training',            2, 1),
    (NEWID(), @Portfolio7, N'API & handoff',             3, 1);

INSERT INTO [portfolio].[PortfolioMetrics]
    ([Id], [PortfolioProjectId], [Value], [Label], [SortOrder])
VALUES
    (NEWID(), @Portfolio1, N'32%',    N'Checkout conversion lift', 1),
    (NEWID(), @Portfolio1, N'8 weeks', N'Time to launch',          2),
    (NEWID(), @Portfolio2, N'50+',    N'Dashboard users',          1),
    (NEWID(), @Portfolio3, N'4.8',    N'App store rating',         1),
    (NEWID(), @Portfolio3, N'2',      N'Platforms shipped',        2),
    (NEWID(), @Portfolio4, N'4x',     N'Growth handled',           1),
    (NEWID(), @Portfolio6, N'2 days', N'Regression time saved',    1),
    (NEWID(), @Portfolio7, N'2 wks',  N'Earlier churn detection',  1),
    (NEWID(), @Portfolio8, N'10 min', N'Deploy time',              1);

/* -------------------------------------------------------------------------- */
/* 34. PORTFOLIO FEEDBACKS / RECENTLY VIEWED                                    */
/* -------------------------------------------------------------------------- */

INSERT INTO [portfolio].[PortfolioFeedbacks]
    ([Id], [PortfolioProjectId], [ReviewerUserId], [Rating], [Comment],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Portfolio1, @Client1, 5, N'Delivery was smooth and the storefront is fast.',     @Now, @By, 0),
    (NEWID(), @Portfolio2, @Client2, 4, N'Great design system, small delays on reporting.',     @Now, @By, 0),
    (NEWID(), @Portfolio3, @Client3, 5, N'Offline mode works perfectly in the field.',          @Now, @By, 0),
    (NEWID(), @Portfolio4, @Client4, 5, N'Scaled with us through four product releases.',       @Now, @By, 0),
    (NEWID(), @Portfolio5, @Client5, 4, N'Components are consistent and accessible.',           @Now, @By, 0),
    (NEWID(), @Portfolio7, @Client6, 5, N'The churn model predictions are spot on.',            @Now, @By, 0),
    (NEWID(), @Portfolio8, @Client7, 5, N'CI/CD cut our deploy times dramatically.',            @Now, @By, 0),
    (NEWID(), @Portfolio2, @Client4, 4, N'Clean APIs and solid UX work.',                       @Now, @By, 0);

INSERT INTO [portfolio].[RecentlyViewedPortfolios]
    ([Id], [UserId], [PortfolioProjectId], [ViewedAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Client2, @Portfolio1, DATEADD(DAY, -3, @Now), @Now, @By, 0),
    (NEWID(), @Client3, @Portfolio1, DATEADD(DAY, -2, @Now), @Now, @By, 0),
    (NEWID(), @Client1, @Portfolio2, DATEADD(DAY, -4, @Now), @Now, @By, 0),
    (NEWID(), @Client5, @Portfolio3, DATEADD(DAY, -1, @Now), @Now, @By, 0),
    (NEWID(), @Client6, @Portfolio4, DATEADD(DAY, -2, @Now), @Now, @By, 0),
    (NEWID(), @Client8, @Portfolio5, DATEADD(DAY, -5, @Now), @Now, @By, 0),
    (NEWID(), @Client4, @Portfolio7, DATEADD(DAY, -3, @Now), @Now, @By, 0),
    (NEWID(), @Client7, @Portfolio7, DATEADD(DAY, -1, @Now), @Now, @By, 0),
    (NEWID(), @Client3, @Portfolio8, DATEADD(DAY, -2, @Now), @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 35. PROJECT TASKS (marketplace)                                             */
/* -------------------------------------------------------------------------- */

DECLARE @Task1 UNIQUEIDENTIFIER = 'e2e2e2e2-e2e2-e2e2-e2e2-000000000001';
DECLARE @Task2 UNIQUEIDENTIFIER = 'e2e2e2e2-e2e2-e2e2-e2e2-000000000002';
DECLARE @Task3 UNIQUEIDENTIFIER = 'e2e2e2e2-e2e2-e2e2-e2e2-000000000003';
DECLARE @Task4 UNIQUEIDENTIFIER = 'e2e2e2e2-e2e2-e2e2-e2e2-000000000004';
DECLARE @Task5 UNIQUEIDENTIFIER = 'e2e2e2e2-e2e2-e2e2-e2e2-000000000005';
DECLARE @Task6 UNIQUEIDENTIFIER = 'e2e2e2e2-e2e2-e2e2-e2e2-000000000006';
DECLARE @Task7 UNIQUEIDENTIFIER = 'e2e2e2e2-e2e2-e2e2-e2e2-000000000007';
DECLARE @Task8 UNIQUEIDENTIFIER = 'e2e2e2e2-e2e2-e2e2-e2e2-000000000008';

INSERT INTO [marketplace].[ProjectTasks]
    ([Id], [MilestoneId], [Title], [Description], [Requirements], [Priority], [Status],
     [AssigneeUserId], [CreatedByUserId], [DueDate], [EstimatedHours], [SpentHours], [CompletedAt],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@Task1, @M1, N'Design system tokens',
     N'Define color, spacing and typography tokens for the dashboard.',
     N'Tokens documented and consumed by all screens.', N'Medium', N'Done',
     @Dev2, @Dev2, DATEADD(DAY, -20, @Now), 16.00, 14.50, DATEADD(DAY, -18, @Now),
     DATEADD(DAY, -25, @Now), @By, 0),
    (@Task2, @M1, N'Auth & login screens',
     N'Build login, registration and password reset flows.',
     N'All flows tested against the auth API.', N'High', N'Done',
     @Dev3, @Dev2, DATEADD(DAY, -15, @Now), 24.00, 26.00, DATEADD(DAY, -12, @Now),
     DATEADD(DAY, -24, @Now), @By, 0),
    (@Task3, @M2, N'Core dashboard widgets',
     N'Implement the main report widgets from live data.',
     N'Widgets render from the reporting API.', N'High', N'Done',
     @Dev2, @Dev2, DATEADD(DAY, -8, @Now), 30.00, 28.00, DATEADD(DAY, -6, @Now),
     DATEADD(DAY, -18, @Now), @By, 0),
    (@Task4, @M2, N'Report export service',
     N'Add CSV/PDF export for dashboard reports.',
     N'Exports match the on-screen data.', N'Medium', N'InReview',
     @Dev8, @Dev2, DATEADD(DAY, -3, @Now), 12.00, 11.00, NULL,
     DATEADD(DAY, -14, @Now), @By, 0),
    (@Task5, @M4, N'Flutter app foundation',
     N'Set up the Flutter project, navigation and theme.',
     N'Navigates all core screens on both platforms.', N'High', N'Done',
     @Dev3, @Dev3, DATEADD(DAY, -14, @Now), 20.00, 19.00, DATEADD(DAY, -16, @Now),
     DATEADD(DAY, -22, @Now), @By, 0),
    (@Task6, @M5, N'Workout plan engine',
     N'Model and render personalized workout plans.',
     N'Plans persist and sync offline.', N'High', N'InProgress',
     @Dev1, @Dev3, DATEADD(DAY, 4, @Now), 28.00, 12.00, NULL,
     DATEADD(DAY, -12, @Now), @By, 0),
    (@Task7, @M10, N'CI/CD pipeline setup',
     N'Create a build pipeline that runs on every commit.',
     N'Green build with tests on every push.', N'Critical', N'Done',
     @Dev6, @Dev6, DATEADD(DAY, -8, @Now), 18.00, 16.00, DATEADD(DAY, -5, @Now),
     DATEADD(DAY, -12, @Now), @By, 0),
    (@Task8, @M11, N'Containerized staging deploy',
     N'Deploy all services in containers to staging.',
     N'Staging fully functional behind the load balancer.', N'High', N'Todo',
     @Dev6, @Dev6, DATEADD(DAY, 10, @Now), 24.00, 0.00, NULL,
     DATEADD(DAY, -5, @Now), @By, 0);

/* -------------------------------------------------------------------------- */
/* 36. TASK CHECKLIST ITEMS / COMMENTS / ATTACHMENTS                           */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[TaskChecklistItems]
    ([Id], [TaskId], [Title], [IsCompleted], [CompletedAt], [CompletedByUserId],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Task1, N'Define palette',         1, DATEADD(DAY, -22, @Now), @Dev2, @Now, @By, 0),
    (NEWID(), @Task1, N'Define typography',      1, DATEADD(DAY, -21, @Now), @Dev2, @Now, @By, 0),
    (NEWID(), @Task1, N'Publish to Storybook',   1, DATEADD(DAY, -19, @Now), @Dev2, @Now, @By, 0),
    (NEWID(), @Task2, N'Login flow wired',       1, DATEADD(DAY, -16, @Now), @Dev3, @Now, @By, 0),
    (NEWID(), @Task2, N'Registration wired',     1, DATEADD(DAY, -14, @Now), @Dev3, @Now, @By, 0),
    (NEWID(), @Task3, N'Widget data binding',    1, DATEADD(DAY, -9, @Now),  @Dev2, @Now, @By, 0),
    (NEWID(), @Task3, N'Empty/loading states',   1, DATEADD(DAY, -7, @Now),  @Dev2, @Now, @By, 0),
    (NEWID(), @Task4, N'CSV export',             1, DATEADD(DAY, -5, @Now),  @Dev8, @Now, @By, 0),
    (NEWID(), @Task4, N'PDF export',             0, NULL, NULL,                        @Now, @By, 0),
    (NEWID(), @Task6, N'Plan data model',        1, DATEADD(DAY, -9, @Now),  @Dev1, @Now, @By, 0),
    (NEWID(), @Task6, N'Plan rendering',         0, NULL, NULL,                        @Now, @By, 0),
    (NEWID(), @Task7, N'Pipeline triggers',      1, DATEADD(DAY, -8, @Now),  @Dev6, @Now, @By, 0),
    (NEWID(), @Task7, N'Test stage',             1, DATEADD(DAY, -6, @Now),  @Dev6, @Now, @By, 0),
    (NEWID(), @Task8, N'Dockerfiles',            0, NULL, NULL,                        @Now, @By, 0),
    (NEWID(), @Task8, N'Staging deploy script',  0, NULL, NULL,                        @Now, @By, 0);

INSERT INTO [marketplace].[TaskComments]
    ([Id], [TaskId], [UserId], [Content], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Task1, @Dev2,  N'Tokens reviewed with the client, approved.',  DATEADD(DAY, -20, @Now), @By, 0),
    (NEWID(), @Task2, @Dev3,  N'Handled edge case on password reset.',        DATEADD(DAY, -15, @Now), @By, 0),
    (NEWID(), @Task2, @Dev2,  N'Looks good, merged to develop.',              DATEADD(DAY, -13, @Now), @By, 0),
    (NEWID(), @Task3, @Dev2,  N'Widgets now pull live data.',                 DATEADD(DAY, -8, @Now), @By, 0),
    (NEWID(), @Task4, @Dev8,  N'CSV export ready for review.',                DATEADD(DAY, -4, @Now), @By, 0),
    (NEWID(), @Task6, @Dev1,  N'Data model done, moving to rendering.',       DATEADD(DAY, -8, @Now), @By, 0),
    (NEWID(), @Task7, @Dev6,  N'Pipeline green on all branches now.',         DATEADD(DAY, -5, @Now), @By, 0),
    (NEWID(), @Task8, @Dev7,  N'Dockerfiles started, infra diagram attached.',DATEADD(DAY, -3, @Now), @By, 0);

INSERT INTO [marketplace].[TaskAttachments]
    ([Id], [TaskId], [UploadedByUserId], [FileName], [FileUrl],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Task1, @Dev2,  N'design-tokens.fig',     N'https://cdn.example.com/seed/t1-tokens.fig',     @Now, @By, 0),
    (NEWID(), @Task2, @Dev3,  N'auth-flows.pdf',        N'https://cdn.example.com/seed/t2-auth.pdf',        @Now, @By, 0),
    (NEWID(), @Task3, @Dev2,  N'widget-specs.md',       N'https://cdn.example.com/seed/t3-widgets.md',      @Now, @By, 0),
    (NEWID(), @Task4, @Dev8,  N'export-sample.csv',     N'https://cdn.example.com/seed/t4-export.csv',      @Now, @By, 0),
    (NEWID(), @Task5, @Dev3,  N'app-shell.zip',         N'https://cdn.example.com/seed/t5-app.zip',         @Now, @By, 0),
    (NEWID(), @Task6, @Dev1,  N'workout-model.md',      N'https://cdn.example.com/seed/t6-workout.md',      @Now, @By, 0),
    (NEWID(), @Task7, @Dev6,  N'pipeline.yaml',         N'https://cdn.example.com/seed/t7-pipeline.yaml',   @Now, @By, 0),
    (NEWID(), @Task8, @Dev7,  N'infra-diagram.png',     N'https://cdn.example.com/seed/t8-infra.png',       @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* 37. TASK SUBTASKS / TIME LOGS                                               */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[TaskSubtasks]
    ([Id], [TaskId], [Title], [Status], [AssigneeUserId], [DueDate],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Task2, N'Login API integration',     N'Done',       @Dev3, DATEADD(DAY, -16, @Now), @Now, @By, 0),
    (NEWID(), @Task2, N'Registration API',          N'Done',       @Dev3, DATEADD(DAY, -14, @Now), @Now, @By, 0),
    (NEWID(), @Task2, N'Password reset',            N'InProgress', @Dev3, DATEADD(DAY, -6, @Now),  @Now, @By, 0),
    (NEWID(), @Task3, N'Revenue widget',            N'Done',       @Dev2, DATEADD(DAY, -9, @Now),  @Now, @By, 0),
    (NEWID(), @Task3, N'User activity widget',      N'InProgress', @Dev2, DATEADD(DAY, -3, @Now),  @Now, @By, 0),
    (NEWID(), @Task6, N'Workout plan persistence',  N'Done',       @Dev1, DATEADD(DAY, -8, @Now),  @Now, @By, 0),
    (NEWID(), @Task6, N'Offline sync queue',        N'Todo',       @Dev1, DATEADD(DAY, 2, @Now),   @Now, @By, 0),
    (NEWID(), @Task7, N'Build stage',               N'Done',       @Dev6, DATEADD(DAY, -9, @Now),  @Now, @By, 0),
    (NEWID(), @Task7, N'Test stage',                N'Done',       @Dev6, DATEADD(DAY, -7, @Now),  @Now, @By, 0),
    (NEWID(), @Task8, N'Web service image',         N'Todo',       @Dev6, DATEADD(DAY, 3, @Now),   @Now, @By, 0),
    (NEWID(), @Task8, N'Worker service image',      N'Todo',       @Dev6, DATEADD(DAY, 5, @Now),   @Now, @By, 0);

INSERT INTO [marketplace].[TaskTimeLogs]
    ([Id], [TaskId], [UserId], [Hours], [Note], [WorkDate],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Task1, @Dev2, 8.00,  N'Token discovery with client.',    DATEADD(DAY, -22, @Now), @Now, @By, 0),
    (NEWID(), @Task1, @Dev2, 6.50,  N'Storybook setup.',                DATEADD(DAY, -20, @Now), @Now, @By, 0),
    (NEWID(), @Task2, @Dev3, 10.00, N'Auth flows implementation.',      DATEADD(DAY, -18, @Now), @Now, @By, 0),
    (NEWID(), @Task2, @Dev3, 8.00,  N'Reset flow and edge cases.',      DATEADD(DAY, -14, @Now), @Now, @By, 0),
    (NEWID(), @Task3, @Dev2, 9.00,  N'Widget data binding.',            DATEADD(DAY, -10, @Now), @Now, @By, 0),
    (NEWID(), @Task4, @Dev8, 6.00,  N'CSV export implementation.',      DATEADD(DAY, -5, @Now),  @Now, @By, 0),
    (NEWID(), @Task6, @Dev1, 7.00,  N'Workout plan model.',             DATEADD(DAY, -9, @Now),  @Now, @By, 0),
    (NEWID(), @Task6, @Dev1, 5.00,  N'Rendering start.',                DATEADD(DAY, -7, @Now),  @Now, @By, 0),
    (NEWID(), @Task7, @Dev6, 8.00,  N'Pipeline triggers and stages.',   DATEADD(DAY, -11, @Now), @Now, @By, 0),
    (NEWID(), @Task7, @Dev6, 8.00,  N'Test stage and flake handling.',  DATEADD(DAY, -8, @Now),  @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* DONE                                                                        */
/* -------------------------------------------------------------------------- */

COMMIT TRANSACTION;

PRINT N'Final seed completed successfully.';
PRINT N'Login with any seeded *@freegency.local account and password: Password123!';
GO
