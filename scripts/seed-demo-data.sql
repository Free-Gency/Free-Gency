/*
================================================================================
 FreeGency — Demo seed script (SQL Server / SSMS)
================================================================================
 Prerequisites:
   1. Database FreeGency_DB exists.
   2. EF migrations applied (run the API once, or: dotnet ef database update).
   3. Prefer running TaxonomySeeder first (API startup). If catalog is empty,
      this script inserts a minimal taxonomy fallback.

 Idempotent:
   Skips entirely if identity.Users already has client1@freegency.local.

 Demo password (all users):  Password123!
   ASP.NET Core Identity V3 hash baked into @PasswordHash below.

 Demo logins:
   Clients:    client1@freegency.local … client3@freegency.local
   Developers: dev1@freegency.local … dev5@freegency.local

 CreatedBy: seed-sql
 User.code: NULL (avoids EncryptColumn)
================================================================================
*/

USE [FreeGency_DB];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;

IF EXISTS (
    SELECT 1
    FROM [identity].[Users]
    WHERE [NormalizedEmail] = N'CLIENT1@FREEGENCY.LOCAL'
)
BEGIN
    PRINT N'Seed already applied (client1@freegency.local found). Skipping.';
    RETURN;
END;

BEGIN TRANSACTION;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @By  NVARCHAR(256) = N'seed-sql';
DECLARE @PasswordHash NVARCHAR(MAX) =
    N'AQAAAAIAAYagAAAAEHfsErsnfAc+hYS1KXdNaS/O1H4FvDDzeB/zacTHbbT1paRNRVbLhIrLqV6HHo3YqQ==';

/* -------------------------------------------------------------------------- */
/* Taxonomy lookup / minimal fallback                                         */
/* -------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM [catalog].[Categories])
BEGIN
    PRINT N'Catalog empty — inserting minimal taxonomy fallback.';

    INSERT INTO [catalog].[Categories] ([Id], [Name], [NameEn], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        ('11111111-1111-1111-1111-000000000001', N'تطوير الويب',     N'Web Development',    @Now, @By, 0),
        ('11111111-1111-1111-1111-000000000002', N'تطوير الموبايل',   N'Mobile Development', @Now, @By, 0),
        ('11111111-1111-1111-1111-000000000005', N'الباك اند',        N'Backend & APIs',     @Now, @By, 0);

    INSERT INTO [catalog].[Specialties] ([Id], [NameEn], [NameAr], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        ('22222222-2222-2222-2222-000000000001', N'Frontend Development', N'تطوير الواجهات', @Now, @By, 0),
        ('22222222-2222-2222-2222-000000000002', N'Backend Development',  N'تطوير الباك اند', @Now, @By, 0),
        ('22222222-2222-2222-2222-000000000003', N'Full Stack Development', N'فول ستاك', @Now, @By, 0),
        ('22222222-2222-2222-2222-000000000004', N'iOS Development', N'تطوير iOS', @Now, @By, 0),
        ('22222222-2222-2222-2222-000000000005', N'Android Development', N'تطوير Android', @Now, @By, 0),
        ('22222222-2222-2222-2222-000000000006', N'Cross-Platform Mobile Development', N'موبايل متعدد المنصات', @Now, @By, 0),
        ('22222222-2222-2222-2222-000000000007', N'API Development', N'تطوير APIs', @Now, @By, 0);

    INSERT INTO [catalog].[Skills] ([Id], [Name], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        ('33333333-3333-3333-3333-000000000001', N'React',          @Now, @By, 0),
        ('33333333-3333-3333-3333-000000000002', N'TypeScript',     @Now, @By, 0),
        ('33333333-3333-3333-3333-000000000003', N'Node.js',        @Now, @By, 0),
        ('33333333-3333-3333-3333-000000000004', N'C#',             @Now, @By, 0),
        ('33333333-3333-3333-3333-000000000005', N'ASP.NET Core',   @Now, @By, 0),
        ('33333333-3333-3333-3333-000000000006', N'Flutter',        @Now, @By, 0),
        ('33333333-3333-3333-3333-000000000007', N'React Native',   @Now, @By, 0),
        ('33333333-3333-3333-3333-000000000008', N'SQL Server',     @Now, @By, 0),
        ('33333333-3333-3333-3333-000000000009', N'Docker',         @Now, @By, 0),
        ('33333333-3333-3333-3333-00000000000a', N'Figma',          @Now, @By, 0);

    INSERT INTO [catalog].[CategorySpecialties]
        ([Id], [CategoryId], [SpecialtyId], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), '11111111-1111-1111-1111-000000000001', '22222222-2222-2222-2222-000000000001', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000001', '22222222-2222-2222-2222-000000000002', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000001', '22222222-2222-2222-2222-000000000003', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000002', '22222222-2222-2222-2222-000000000004', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000002', '22222222-2222-2222-2222-000000000005', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000002', '22222222-2222-2222-2222-000000000006', @Now, @By, 0),
        (NEWID(), '11111111-1111-1111-1111-000000000005', '22222222-2222-2222-2222-000000000007', @Now, @By, 0);

    INSERT INTO [catalog].[SpecialtySkills]
        ([Id], [SpecialtyId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), '22222222-2222-2222-2222-000000000001', '33333333-3333-3333-3333-000000000001', @Now, @By, 0),
        (NEWID(), '22222222-2222-2222-2222-000000000001', '33333333-3333-3333-3333-000000000002', @Now, @By, 0),
        (NEWID(), '22222222-2222-2222-2222-000000000002', '33333333-3333-3333-3333-000000000003', @Now, @By, 0),
        (NEWID(), '22222222-2222-2222-2222-000000000002', '33333333-3333-3333-3333-000000000004', @Now, @By, 0),
        (NEWID(), '22222222-2222-2222-2222-000000000002', '33333333-3333-3333-3333-000000000005', @Now, @By, 0),
        (NEWID(), '22222222-2222-2222-2222-000000000006', '33333333-3333-3333-3333-000000000006', @Now, @By, 0),
        (NEWID(), '22222222-2222-2222-2222-000000000006', '33333333-3333-3333-3333-000000000007', @Now, @By, 0),
        (NEWID(), '22222222-2222-2222-2222-000000000007', '33333333-3333-3333-3333-000000000008', @Now, @By, 0);
END;

DECLARE @CatWeb    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Web Development');
DECLARE @CatMobile UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Mobile Development');
DECLARE @CatBackend UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Backend & APIs');

IF @CatWeb IS NULL OR @CatMobile IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50001, N'Required categories (Web Development / Mobile Development) not found. Run API taxonomy seed first.', 1;
END;

IF @CatBackend IS NULL SET @CatBackend = @CatWeb;

DECLARE @SpecFrontend UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Frontend Development');
DECLARE @SpecBackend  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Backend Development');
DECLARE @SpecFullStack UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Full Stack Development');
DECLARE @SpecCrossMobile UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'Cross-Platform Mobile Development');
DECLARE @SpecApi UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] WHERE [NameEn] = N'API Development');

IF @SpecFrontend IS NULL SET @SpecFrontend = (SELECT TOP 1 [Id] FROM [catalog].[Specialties] ORDER BY [NameEn]);
IF @SpecBackend  IS NULL SET @SpecBackend  = @SpecFrontend;
IF @SpecFullStack IS NULL SET @SpecFullStack = @SpecFrontend;
IF @SpecCrossMobile IS NULL SET @SpecCrossMobile = @SpecFrontend;
IF @SpecApi IS NULL SET @SpecApi = @SpecBackend;

DECLARE @SkillReact UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'React');
DECLARE @SkillTs    UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'TypeScript');
DECLARE @SkillNode  UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Node.js');
DECLARE @SkillCsharp UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'C#');
DECLARE @SkillAsp   UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'ASP.NET Core');
DECLARE @SkillFlutter UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Flutter');
DECLARE @SkillSql   UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'SQL Server');
DECLARE @SkillDocker UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Skills] WHERE [Name] = N'Docker');

IF @SkillReact IS NULL SET @SkillReact = (SELECT TOP 1 [Id] FROM [catalog].[Skills] ORDER BY [Name]);
IF @SkillTs IS NULL SET @SkillTs = @SkillReact;
IF @SkillNode IS NULL SET @SkillNode = @SkillReact;
IF @SkillCsharp IS NULL SET @SkillCsharp = @SkillReact;
IF @SkillAsp IS NULL SET @SkillAsp = @SkillReact;
IF @SkillFlutter IS NULL SET @SkillFlutter = @SkillReact;
IF @SkillSql IS NULL SET @SkillSql = @SkillReact;
IF @SkillDocker IS NULL SET @SkillDocker = @SkillReact;

/* -------------------------------------------------------------------------- */
/* Deterministic IDs                                                          */
/* -------------------------------------------------------------------------- */

DECLARE @Client1 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001';
DECLARE @Client2 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002';
DECLARE @Client3 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003';
DECLARE @Dev1    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000011';
DECLARE @Dev2    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000012';
DECLARE @Dev3    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000013';
DECLARE @Dev4    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000014';
DECLARE @Dev5    UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000015';

DECLARE @Cp1 UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000001';
DECLARE @Cp2 UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000002';
DECLARE @Cp3 UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000003';
DECLARE @Dp1 UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000011';
DECLARE @Dp2 UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000012';
DECLARE @Dp3 UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000013';
DECLARE @Dp4 UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000014';
DECLARE @Dp5 UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-000000000015';

DECLARE @TeamWeb    UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000001';
DECLARE @TeamMobile UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000002';

DECLARE @ProjDraft      UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000001';
DECLARE @ProjOpen       UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000002';
DECLARE @ProjInProgress UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000003';
DECLARE @ProjCompleted  UNIQUEIDENTIFIER = 'dddddddd-dddd-dddd-dddd-000000000004';

DECLARE @PropTeamOpen   UNIQUEIDENTIFIER = 'eeeeeeee-eeee-eeee-eeee-000000000001';
DECLARE @PropUserOpen   UNIQUEIDENTIFIER = 'eeeeeeee-eeee-eeee-eeee-000000000002';
DECLARE @PropTeamActive UNIQUEIDENTIFIER = 'eeeeeeee-eeee-eeee-eeee-000000000003';
DECLARE @PropTeamDone   UNIQUEIDENTIFIER = 'eeeeeeee-eeee-eeee-eeee-000000000004';

DECLARE @MsActive1 UNIQUEIDENTIFIER = 'ffffffff-ffff-ffff-ffff-000000000001';
DECLARE @MsActive2 UNIQUEIDENTIFIER = 'ffffffff-ffff-ffff-ffff-000000000002';
DECLARE @MsDone1   UNIQUEIDENTIFIER = 'ffffffff-ffff-ffff-ffff-000000000003';
DECLARE @MsDone2   UNIQUEIDENTIFIER = 'ffffffff-ffff-ffff-ffff-000000000004';

DECLARE @WalletC1 UNIQUEIDENTIFIER = '10101010-1010-1010-1010-000000000001';
DECLARE @WalletC2 UNIQUEIDENTIFIER = '10101010-1010-1010-1010-000000000002';
DECLARE @WalletC3 UNIQUEIDENTIFIER = '10101010-1010-1010-1010-000000000003';
DECLARE @WalletD1 UNIQUEIDENTIFIER = '10101010-1010-1010-1010-000000000011';
DECLARE @WalletD2 UNIQUEIDENTIFIER = '10101010-1010-1010-1010-000000000012';
DECLARE @WalletD3 UNIQUEIDENTIFIER = '10101010-1010-1010-1010-000000000013';
DECLARE @WalletD4 UNIQUEIDENTIFIER = '10101010-1010-1010-1010-000000000014';
DECLARE @WalletD5 UNIQUEIDENTIFIER = '10101010-1010-1010-1010-000000000015';
DECLARE @WalletT1 UNIQUEIDENTIFIER = '10101010-1010-1010-1010-000000000101';
DECLARE @WalletT2 UNIQUEIDENTIFIER = '10101010-1010-1010-1010-000000000102';

DECLARE @JobWeb UNIQUEIDENTIFIER = '20202020-2020-2020-2020-000000000001';
DECLARE @JobMob UNIQUEIDENTIFIER = '20202020-2020-2020-2020-000000000002';

DECLARE @ChatTeamWeb UNIQUEIDENTIFIER = '30303030-3030-3030-3030-000000000001';
DECLARE @ChatTeamMob UNIQUEIDENTIFIER = '30303030-3030-3030-3030-000000000002';
DECLARE @ChatPropOpen UNIQUEIDENTIFIER = '30303030-3030-3030-3030-000000000011';
DECLARE @ChatProjActive UNIQUEIDENTIFIER = '30303030-3030-3030-3030-000000000021';
DECLARE @ChatProjDone UNIQUEIDENTIFIER = '30303030-3030-3030-3030-000000000022';

DECLARE @Msg1 UNIQUEIDENTIFIER = '40404040-4040-4040-4040-000000000001';
DECLARE @MsgProp UNIQUEIDENTIFIER = '40404040-4040-4040-4040-000000000011';
DECLARE @PortUser UNIQUEIDENTIFIER = '50505050-5050-5050-5050-000000000001';
DECLARE @PortTeam UNIQUEIDENTIFIER = '50505050-5050-5050-5050-000000000002';

/* -------------------------------------------------------------------------- */
/* Users                                                                      */
/* -------------------------------------------------------------------------- */

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
 N'Mona', N'Zaki', 1, 1, N'Developer', N'Egypt', NULL, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Profiles                                                                   */
/* -------------------------------------------------------------------------- */

INSERT INTO [identity].[ClientProfiles]
    ([Id], [UserId], [Bio], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(@Cp1, @Client1, N'Product founder looking for solid delivery teams.', 4.50, 2, @Now, @By, 0),
(@Cp2, @Client2, N'Startup CTO outsourcing product builds.', 4.80, 1, @Now, @By, 0),
(@Cp3, @Client3, N'Agency client for web and mobile apps.', 5.00, 1, @Now, @By, 0);

INSERT INTO [identity].[DeveloperProfiles]
    ([Id], [UserId], [Bio], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(@Dp1, @Dev1, N'Full-stack lead. React + .NET.', 4.90, 3, @Now, @By, 0),
(@Dp2, @Dev2, N'Frontend engineer specializing in React/TypeScript.', 4.70, 2, @Now, @By, 0),
(@Dp3, @Dev3, N'Mobile engineer — Flutter & React Native.', 4.60, 2, @Now, @By, 0),
(@Dp4, @Dev4, N'Backend / API engineer.', 4.40, 1, @Now, @By, 0),
(@Dp5, @Dev5, N'QA + DevOps hybrid.', 4.20, 1, @Now, @By, 0);

INSERT INTO [identity].[UserInterests]
    ([Id], [CategoryId], [ClientProfileId], [DeveloperProfileId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @CatWeb, @Cp1, NULL, @Now, @By, 0),
(NEWID(), @CatMobile, @Cp1, NULL, @Now, @By, 0),
(NEWID(), @CatWeb, @Cp2, NULL, @Now, @By, 0),
(NEWID(), @CatBackend, @Cp3, NULL, @Now, @By, 0),
(NEWID(), @CatWeb, NULL, @Dp1, @Now, @By, 0),
(NEWID(), @CatBackend, NULL, @Dp1, @Now, @By, 0),
(NEWID(), @CatWeb, NULL, @Dp2, @Now, @By, 0),
(NEWID(), @CatMobile, NULL, @Dp3, @Now, @By, 0),
(NEWID(), @CatBackend, NULL, @Dp4, @Now, @By, 0),
(NEWID(), @CatWeb, NULL, @Dp5, @Now, @By, 0);

INSERT INTO [identity].[UserSpecialties]
    ([Id], [SpecialtyId], [ClientProfileId], [DeveloperProfileId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @SpecFullStack, @Cp1, NULL, @Now, @By, 0),
(NEWID(), @SpecFrontend, @Cp2, NULL, @Now, @By, 0),
(NEWID(), @SpecApi, @Cp3, NULL, @Now, @By, 0),
(NEWID(), @SpecFullStack, NULL, @Dp1, @Now, @By, 0),
(NEWID(), @SpecFrontend, NULL, @Dp2, @Now, @By, 0),
(NEWID(), @SpecCrossMobile, NULL, @Dp3, @Now, @By, 0),
(NEWID(), @SpecBackend, NULL, @Dp4, @Now, @By, 0),
(NEWID(), @SpecApi, NULL, @Dp5, @Now, @By, 0);

INSERT INTO [identity].[UserSkills]
    ([Id], [SkillId], [ClientProfileId], [DeveloperProfileId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @SkillReact, @Cp1, NULL, @Now, @By, 0),
(NEWID(), @SkillCsharp, @Cp2, NULL, @Now, @By, 0),
(NEWID(), @SkillReact, NULL, @Dp1, @Now, @By, 0),
(NEWID(), @SkillTs, NULL, @Dp1, @Now, @By, 0),
(NEWID(), @SkillAsp, NULL, @Dp1, @Now, @By, 0),
(NEWID(), @SkillReact, NULL, @Dp2, @Now, @By, 0),
(NEWID(), @SkillTs, NULL, @Dp2, @Now, @By, 0),
(NEWID(), @SkillFlutter, NULL, @Dp3, @Now, @By, 0),
(NEWID(), @SkillCsharp, NULL, @Dp4, @Now, @By, 0),
(NEWID(), @SkillSql, NULL, @Dp4, @Now, @By, 0),
(NEWID(), @SkillDocker, NULL, @Dp5, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Teams                                                                      */
/* -------------------------------------------------------------------------- */

INSERT INTO [teams].[Teams]
    ([Id], [OwnerUserId], [Name], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(@TeamWeb, @Dev1, N'WebSquad', N'WEBSQUAD', N'Web & SaaS delivery squad.', 4.80, 4, @Now, @By, 0),
(@TeamMobile, @Dev3, N'MobileForge', N'MOBFORGE', N'Cross-platform mobile specialists.', 4.50, 2, @Now, @By, 0);

INSERT INTO [teams].[TeamMembers]
    ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @TeamWeb, @Dev1, N'TeamLeader', N'Tech Lead', N'2025-01-10', @Now, @By, 0),
(NEWID(), @TeamWeb, @Dev2, N'TeamMember', N'Frontend', N'2025-02-01', @Now, @By, 0),
(NEWID(), @TeamWeb, @Dev4, N'TeamMember', N'Backend', N'2025-02-15', @Now, @By, 0),
(NEWID(), @TeamMobile, @Dev3, N'TeamLeader', N'Mobile Lead', N'2025-01-20', @Now, @By, 0),
(NEWID(), @TeamMobile, @Dev5, N'TeamMember', N'QA/DevOps', N'2025-03-01', @Now, @By, 0);

INSERT INTO [teams].[TeamCategories]
    ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @TeamWeb, @CatWeb, 1, @Now, @By, 0),
(NEWID(), @TeamMobile, @CatMobile, 1, @Now, @By, 0);

INSERT INTO [teams].[TeamSpecialties]
    ([Id], [TeamId], [SpecialtyId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @TeamWeb, @SpecFullStack, @Now, @By, 0),
(NEWID(), @TeamWeb, @SpecFrontend, @Now, @By, 0),
(NEWID(), @TeamWeb, @SpecBackend, @Now, @By, 0),
(NEWID(), @TeamMobile, @SpecCrossMobile, @Now, @By, 0);

INSERT INTO [teams].[TeamSkills]
    ([Id], [TeamId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @TeamWeb, @SkillReact, @Now, @By, 0),
(NEWID(), @TeamWeb, @SkillTs, @Now, @By, 0),
(NEWID(), @TeamWeb, @SkillAsp, @Now, @By, 0),
(NEWID(), @TeamWeb, @SkillCsharp, @Now, @By, 0),
(NEWID(), @TeamMobile, @SkillFlutter, @Now, @By, 0),
(NEWID(), @TeamMobile, @SkillDocker, @Now, @By, 0);

INSERT INTO [teams].[TeamJobs]
    ([Id], [TeamId], [Title], [Description], [Status], [CreatedByUserId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(@JobWeb, @TeamWeb, N'React Frontend Developer', N'Looking for a React specialist to join WebSquad.', N'open', @Dev1, @Now, @By, 0),
(@JobMob, @TeamMobile, N'Flutter Engineer', N'Help ship Flutter apps for MobileForge.', N'open', @Dev3, @Now, @By, 0);

INSERT INTO [teams].[TeamJobSkills]
    ([Id], [TeamJobId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @JobWeb, @SkillReact, @Now, @By, 0),
(NEWID(), @JobWeb, @SkillTs, @Now, @By, 0),
(NEWID(), @JobMob, @SkillFlutter, @Now, @By, 0);

INSERT INTO [teams].[TeamJoinRequests]
    ([Id], [TeamId], [TeamJobId], [UserId], [CoverLetter], [Job], [Status], [RequestedAt], [ResponseAt], [RespondedByUserId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @TeamWeb, @JobWeb, @Dev5, N'I can help with QA and CI for your frontend work.', N'QA', N'pending', @Now, NULL, NULL, @Now, @By, 0),
(NEWID(), @TeamMobile, @JobMob, @Dev2, N'Interested in cross-platform UI work.', N'UI', N'Rejected', DATEADD(DAY, -7, @Now), DATEADD(DAY, -5, @Now), CONVERT(NVARCHAR(36), @Dev3), @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Wallets                                                                    */
/* -------------------------------------------------------------------------- */

INSERT INTO [finance].[Wallets]
    ([Id], [OwnerType], [OwnerUserId], [OwnerTeamId], [Currency], [Available], [Reserved], [Pending], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(@WalletC1, N'User', @Client1, NULL, N'USD', 5000.00, 1500.00, 0, @Now, @By, 0),
(@WalletC2, N'User', @Client2, NULL, N'USD', 8000.00, 3000.00, 0, @Now, @By, 0),
(@WalletC3, N'User', @Client3, NULL, N'USD', 2000.00, 0, 0, @Now, @By, 0),
(@WalletD1, N'User', @Dev1, NULL, N'USD', 1200.00, 0, 200.00, @Now, @By, 0),
(@WalletD2, N'User', @Dev2, NULL, N'USD', 400.00, 0, 0, @Now, @By, 0),
(@WalletD3, N'User', @Dev3, NULL, N'USD', 650.00, 0, 0, @Now, @By, 0),
(@WalletD4, N'User', @Dev4, NULL, N'USD', 300.00, 0, 0, @Now, @By, 0),
(@WalletD5, N'User', @Dev5, NULL, N'USD', 150.00, 0, 0, @Now, @By, 0),
(@WalletT1, N'Team', NULL, @TeamWeb, N'USD', 4500.00, 0, 500.00, @Now, @By, 0),
(@WalletT2, N'Team', NULL, @TeamMobile, N'USD', 900.00, 0, 0, @Now, @By, 0);

INSERT INTO [finance].[LedgerEntries]
    ([Id], [WalletId], [EntryType], [Amount], [Currency], [ProjectId], [MilestoneId], [IdempotencyKey], [PaymentProviderRef], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @WalletC1, N'TopUp', 6500.00, N'USD', NULL, NULL, N'seed-topup-c1', N'stripe_seed_c1', @Now, @By, 0),
(NEWID(), @WalletC2, N'TopUp', 11000.00, N'USD', NULL, NULL, N'seed-topup-c2', N'stripe_seed_c2', @Now, @By, 0),
(NEWID(), @WalletC3, N'TopUp', 5000.00, N'USD', NULL, NULL, N'seed-topup-c3', N'stripe_seed_c3', @Now, @By, 0),
(NEWID(), @WalletD1, N'TopUp', 1000.00, N'USD', NULL, NULL, N'seed-topup-d1', NULL, @Now, @By, 0),
(NEWID(), @WalletT1, N'TopUp', 2000.00, N'USD', NULL, NULL, N'seed-topup-t1', NULL, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Projects                                                                   */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[Projects]
(
    [Id], [Title], [Description], [ClientId], [CategoryId], [IsFixedPrice],
    [BudgetMin], [BudgetMax], [Currency], [Deadline], [EstimatedDurationDays],
    [Status], [AssignedTeamId], [AssignedUserId], [CompletedAt],
    [CreatedAt], [CreatedBy], [IsDeleted]
)
VALUES
(@ProjDraft, N'Internal admin dashboard (draft)', N'Draft brief for an internal ops dashboard.',
 @Client1, @CatWeb, 1, 2000, 3500, N'USD', DATEADD(DAY, 45, @Now), 30,
 N'Draft', NULL, NULL, NULL, @Now, @By, 0),

(@ProjOpen, N'Marketing landing page rebuild', N'Rebuild marketing site with Next.js and CMS.',
 @Client1, @CatWeb, 1, 1500, 2500, N'USD', DATEADD(DAY, 30, @Now), 21,
 N'Open', NULL, NULL, NULL, @Now, @By, 0),

(@ProjInProgress, N'Saas billing portal', N'Build client billing portal with subscriptions.',
 @Client2, @CatWeb, 1, 5000, 8000, N'USD', DATEADD(DAY, 60, @Now), 45,
 N'InProgress', @TeamWeb, NULL, NULL, DATEADD(DAY, -10, @Now), @By, 0),

(@ProjCompleted, N'Company portfolio site', N'Responsive portfolio delivered and accepted.',
 @Client3, @CatWeb, 1, 1200, 1800, N'USD', DATEADD(DAY, -5, @Now), 14,
 N'Completed', @TeamWeb, NULL, DATEADD(DAY, -2, @Now), DATEADD(DAY, -40, @Now), @By, 0);

INSERT INTO [marketplace].[ProjectSpecialties]
    ([Id], [ProjectId], [SpecialtyId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @ProjDraft, @SpecFrontend, @Now, @By, 0),
(NEWID(), @ProjOpen, @SpecFrontend, @Now, @By, 0),
(NEWID(), @ProjOpen, @SpecFullStack, @Now, @By, 0),
(NEWID(), @ProjInProgress, @SpecFullStack, @Now, @By, 0),
(NEWID(), @ProjInProgress, @SpecBackend, @Now, @By, 0),
(NEWID(), @ProjCompleted, @SpecFrontend, @Now, @By, 0);

INSERT INTO [marketplace].[ProjectSkills]
    ([Id], [ProjectId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @ProjOpen, @SkillReact, @Now, @By, 0),
(NEWID(), @ProjOpen, @SkillTs, @Now, @By, 0),
(NEWID(), @ProjInProgress, @SkillReact, @Now, @By, 0),
(NEWID(), @ProjInProgress, @SkillAsp, @Now, @By, 0),
(NEWID(), @ProjInProgress, @SkillSql, @Now, @By, 0),
(NEWID(), @ProjCompleted, @SkillReact, @Now, @By, 0);

INSERT INTO [marketplace].[SavedProjects]
    ([Id], [UserId], [ProjectId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @Dev1, @ProjOpen, @Now, @By, 0),
(NEWID(), @Dev2, @ProjOpen, @Now, @By, 0),
(NEWID(), @Dev3, @ProjInProgress, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Proposals                                                                  */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[ProjectProposals]
(
    [Id], [ProjectId], [ApplicantType], [TeamId], [UserId], [CoverLetter],
    [ProposedBudget], [Status], [AppliedAt], [ResponseAt],
    [CreatedAt], [CreatedBy], [IsDeleted]
)
VALUES
(@PropTeamOpen, @ProjOpen, N'Team', @TeamWeb, NULL,
 N'WebSquad can deliver the landing rebuild in 3 weeks.', 2200.00, N'Pending', @Now, NULL, @Now, @By, 0),

(@PropUserOpen, @ProjOpen, N'User', NULL, @Dev3,
 N'Solo Flutter/web hybrid proposal for your landing needs.', 1900.00, N'Pending', @Now, NULL, @Now, @By, 0),

(@PropTeamActive, @ProjInProgress, N'Team', @TeamWeb, NULL,
 N'Accepted proposal for SaaS billing portal.', 6500.00, N'Accepted', DATEADD(DAY, -12, @Now), DATEADD(DAY, -10, @Now), DATEADD(DAY, -12, @Now), @By, 0),

(@PropTeamDone, @ProjCompleted, N'Team', @TeamWeb, NULL,
 N'Delivered portfolio site successfully.', 1500.00, N'Accepted', DATEADD(DAY, -35, @Now), DATEADD(DAY, -33, @Now), DATEADD(DAY, -35, @Now), @By, 0);

INSERT INTO [marketplace].[ProposalAttachments]
    ([Id], [ProposalId], [FileName], [FileUrl], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @PropTeamOpen, N'websquad-case-study.pdf', N'https://cdn.example.com/seed/websquad-case-study.pdf', @Now, @By, 0),
(NEWID(), @PropTeamActive, N'billing-architecture.pdf', N'https://cdn.example.com/seed/billing-architecture.pdf', @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Project members / milestones / files / events                              */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[ProjectMembers]
    ([Id], [ProjectId], [UserId], [RoleInProject], [AssignedByUserId], [AssignedAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @ProjInProgress, @Dev1, N'Tech Lead', @Client2, DATEADD(DAY, -10, @Now), @Now, @By, 0),
(NEWID(), @ProjInProgress, @Dev2, N'Frontend', @Client2, DATEADD(DAY, -10, @Now), @Now, @By, 0),
(NEWID(), @ProjInProgress, @Dev4, N'Backend', @Client2, DATEADD(DAY, -9, @Now), @Now, @By, 0),
(NEWID(), @ProjCompleted, @Dev1, N'Tech Lead', @Client3, DATEADD(DAY, -33, @Now), @Now, @By, 0),
(NEWID(), @ProjCompleted, @Dev2, N'Frontend', @Client3, DATEADD(DAY, -33, @Now), @Now, @By, 0);

INSERT INTO [marketplace].[Milestones]
(
    [Id], [ProjectId], [Title], [Description], [Amount], [ReleasedAmount], [SortOrder],
    [ReleaseStatus], [WorkStatus], [ProposedByUserId], [SubmittedAt], [AvailableAt], [ReleasedAt],
    [CreatedAt], [CreatedBy], [IsDeleted]
)
VALUES
(@MsActive1, @ProjInProgress, N'Discovery & plan', N'Requirements and milestone plan.', 1500.00, 1500.00, 1,
 N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev1), DATEADD(DAY, -8, @Now), DATEADD(DAY, -7, @Now), DATEADD(DAY, -7, @Now), @Now, @By, 0),

(@MsActive2, @ProjInProgress, N'Billing UI + API', N'Implement billing screens and APIs.', 3000.00, 0, 2,
 N'Locked', N'InProgress', CONVERT(NVARCHAR(36), @Dev1), NULL, NULL, NULL, @Now, @By, 0),

(@MsDone1, @ProjCompleted, N'Design + build', N'UI and frontend implementation.', 900.00, 900.00, 1,
 N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev1), DATEADD(DAY, -20, @Now), DATEADD(DAY, -18, @Now), DATEADD(DAY, -18, @Now), @Now, @By, 0),

(@MsDone2, @ProjCompleted, N'Launch & handoff', N'Deploy and documentation.', 600.00, 600.00, 2,
 N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev1), DATEADD(DAY, -5, @Now), DATEADD(DAY, -3, @Now), DATEADD(DAY, -3, @Now), @Now, @By, 0);

INSERT INTO [marketplace].[ProjectFiles]
    ([Id], [ProjectId], [MilestoneId], [UploadedByUserId], [FileName], [FileUrl], [FileKind], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @ProjOpen, NULL, @Client1, N'brief.pdf', N'https://cdn.example.com/seed/open-brief.pdf', N'Brief', @Now, @By, 0),
(NEWID(), @ProjInProgress, @MsActive1, @Dev1, N'plan.pdf', N'https://cdn.example.com/seed/active-plan.pdf', N'Deliverable', @Now, @By, 0),
(NEWID(), @ProjInProgress, @MsActive2, @Dev2, N'ui-wip.zip', N'https://cdn.example.com/seed/ui-wip.zip', N'Deliverable', @Now, @By, 0),
(NEWID(), @ProjCompleted, @MsDone2, @Dev1, N'final-handoff.zip', N'https://cdn.example.com/seed/final-handoff.zip', N'Deliverable', @Now, @By, 0);

INSERT INTO [marketplace].[ProjectEvents]
    ([Id], [ProjectId], [MilestoneId], [ActorUserId], [EventType], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @ProjInProgress, NULL, @Client2, N'ProposalAccepted', DATEADD(DAY, -10, @Now), @By, 0),
(NEWID(), @ProjInProgress, @MsActive1, @Dev1, N'MilestoneSubmitted', DATEADD(DAY, -8, @Now), @By, 0),
(NEWID(), @ProjInProgress, @MsActive1, @Client2, N'MilestoneApproved', DATEADD(DAY, -7, @Now), @By, 0),
(NEWID(), @ProjCompleted, NULL, @Client3, N'ProposalAccepted', DATEADD(DAY, -33, @Now), @By, 0),
(NEWID(), @ProjCompleted, NULL, @Client3, N'ProjectCompleted', DATEADD(DAY, -2, @Now), @By, 0);

/* -------------------------------------------------------------------------- */
/* Finance linked to projects                                                 */
/* -------------------------------------------------------------------------- */

INSERT INTO [finance].[EscrowHolds]
    ([Id], [ProjectId], [TotalAmount], [TotalReleased], [FundingStatus], [planStatus], [LockedAt], [PlanAgreedAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @ProjInProgress, 6500.00, 1500.00, N'Locked', N'PlanAgreed', DATEADD(DAY, -10, @Now), DATEADD(DAY, -9, @Now), @Now, @By, 0),
(NEWID(), @ProjCompleted, 1500.00, 1500.00, N'Completed', N'PlanAgreed', DATEADD(DAY, -33, @Now), DATEADD(DAY, -32, @Now), @Now, @By, 0);

INSERT INTO [finance].[LedgerEntries]
    ([Id], [WalletId], [EntryType], [Amount], [Currency], [ProjectId], [MilestoneId], [IdempotencyKey], [PaymentProviderRef], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @WalletC2, N'EscrowLock', 6500.00, N'USD', @ProjInProgress, NULL, N'seed-escrow-lock-active', NULL, DATEADD(DAY, -10, @Now), @By, 0),
(NEWID(), @WalletT1, N'EscrowRelease', 1500.00, N'USD', @ProjInProgress, @MsActive1, N'seed-escrow-rel-active-m1', NULL, DATEADD(DAY, -7, @Now), @By, 0),
(NEWID(), @WalletT1, N'AvailableCredit', 1500.00, N'USD', @ProjInProgress, @MsActive1, N'seed-avail-active-m1', NULL, DATEADD(DAY, -7, @Now), @By, 0),
(NEWID(), @WalletC3, N'EscrowLock', 1500.00, N'USD', @ProjCompleted, NULL, N'seed-escrow-lock-done', NULL, DATEADD(DAY, -33, @Now), @By, 0),
(NEWID(), @WalletT1, N'EscrowRelease', 1500.00, N'USD', @ProjCompleted, @MsDone2, N'seed-escrow-rel-done', NULL, DATEADD(DAY, -3, @Now), @By, 0),
(NEWID(), @WalletD1, N'TeamSplit', 600.00, N'USD', @ProjCompleted, @MsDone2, N'seed-split-d1-done', NULL, DATEADD(DAY, -3, @Now), @By, 0),
(NEWID(), @WalletD2, N'TeamSplit', 450.00, N'USD', @ProjCompleted, @MsDone2, N'seed-split-d2-done', NULL, DATEADD(DAY, -3, @Now), @By, 0),
(NEWID(), @WalletT1, N'PlatformFee', 150.00, N'USD', @ProjCompleted, NULL, N'seed-fee-done', NULL, DATEADD(DAY, -3, @Now), @By, 0);

INSERT INTO [finance].[TeamPayoutSplits]
    ([Id], [TeamId], [ProjectId], [UserId], [SplitType], [Value], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @TeamWeb, @ProjInProgress, @Dev1, N'Percent', 40, @Now, @By, 0),
(NEWID(), @TeamWeb, @ProjInProgress, @Dev2, N'Percent', 30, @Now, @By, 0),
(NEWID(), @TeamWeb, @ProjInProgress, @Dev4, N'Percent', 30, @Now, @By, 0),
(NEWID(), @TeamWeb, @ProjCompleted, @Dev1, N'Percent', 50, @Now, @By, 0),
(NEWID(), @TeamWeb, @ProjCompleted, @Dev2, N'Percent', 50, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Reviews                                                                    */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[Reviews]
    ([Id], [ProjectId], [ReviewerUserId], [RevieweeType], [RevieweeTeamId], [RevieweeUserId], [Rating], [Comment], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @ProjCompleted, @Client3, N'Team', @TeamWeb, NULL, 5, N'Great delivery and communication.', DATEADD(DAY, -1, @Now), @By, 0),
(NEWID(), @ProjCompleted, @Dev1, N'User', NULL, @Client3, 5, N'Clear requirements and fast feedback.', DATEADD(DAY, -1, @Now), @By, 0);

/* -------------------------------------------------------------------------- */
/* Chat                                                                       */
/* -------------------------------------------------------------------------- */

INSERT INTO [chat].[ChatRooms]
    ([Id], [RoomType], [TeamId], [ProjectId], [ProposalId], [CreatedByUserId], [Title], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(@ChatTeamWeb, N'TeamMain', @TeamWeb, NULL, NULL, @Dev1, N'WebSquad Main', @Now, @By, 0),
(@ChatTeamMob, N'TeamMain', @TeamMobile, NULL, NULL, @Dev3, N'MobileForge Main', @Now, @By, 0),
(@ChatPropOpen, N'Proposal', NULL, NULL, @PropTeamOpen, @Dev1, N'Proposal: Landing rebuild', @Now, @By, 0),
(@ChatProjActive, N'Project', NULL, @ProjInProgress, NULL, @Client2, N'Project: SaaS billing', @Now, @By, 0),
(@ChatProjDone, N'Project', NULL, @ProjCompleted, NULL, @Client3, N'Project: Portfolio site', @Now, @By, 0);

INSERT INTO [chat].[ChatRoomMembers]
    ([Id], [ChatRoomId], [UserId], [JoinedAt], [LastReadAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @ChatTeamWeb, @Dev1, @Now, @Now, @Now, @By, 0),
(NEWID(), @ChatTeamWeb, @Dev2, @Now, @Now, @Now, @By, 0),
(NEWID(), @ChatTeamWeb, @Dev4, @Now, NULL, @Now, @By, 0),
(NEWID(), @ChatTeamMob, @Dev3, @Now, @Now, @Now, @By, 0),
(NEWID(), @ChatTeamMob, @Dev5, @Now, NULL, @Now, @By, 0),
(NEWID(), @ChatPropOpen, @Client1, @Now, @Now, @Now, @By, 0),
(NEWID(), @ChatPropOpen, @Dev1, @Now, @Now, @Now, @By, 0),
(NEWID(), @ChatPropOpen, @Dev2, @Now, NULL, @Now, @By, 0),
(NEWID(), @ChatProjActive, @Client2, @Now, @Now, @Now, @By, 0),
(NEWID(), @ChatProjActive, @Dev1, @Now, @Now, @Now, @By, 0),
(NEWID(), @ChatProjActive, @Dev2, @Now, @Now, @Now, @By, 0),
(NEWID(), @ChatProjActive, @Dev4, @Now, NULL, @Now, @By, 0),
(NEWID(), @ChatProjDone, @Client3, @Now, @Now, @Now, @By, 0),
(NEWID(), @ChatProjDone, @Dev1, @Now, @Now, @Now, @By, 0),
(NEWID(), @ChatProjDone, @Dev2, @Now, @Now, @Now, @By, 0);

INSERT INTO [chat].[Messages]
    ([Id], [ChatRoomId], [SenderUserId], [Text], [FileUrl], [FileName], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(@Msg1, @ChatTeamWeb, @Dev1, N'Welcome to WebSquad — kickoff notes in the shared drive.', NULL, NULL, @Now, @By, 0),
(NEWID(), @ChatTeamWeb, @Dev2, N'Ready on the landing page components.', NULL, NULL, DATEADD(MINUTE, 5, @Now), @By, 0),
(NEWID(), @ChatPropOpen, @Dev1, N'We can start next Monday if the proposal is accepted.', NULL, NULL, @Now, @By, 0),
(@MsgProp, @ChatPropOpen, @Client1, N'Thanks — reviewing budget today.', NULL, NULL, DATEADD(MINUTE, 10, @Now), @By, 0),
(NEWID(), @ChatProjActive, @Client2, N'Please prioritize invoice PDF export this sprint.', NULL, NULL, @Now, @By, 0),
(NEWID(), @ChatProjActive, @Dev1, N'On it — API draft landing tomorrow.', NULL, NULL, DATEADD(MINUTE, 15, @Now), @By, 0),
(NEWID(), @ChatProjDone, @Client3, N'Looks great. Final payment released.', NULL, NULL, DATEADD(DAY, -2, @Now), @By, 0);

/* -------------------------------------------------------------------------- */
/* Portfolio + social links                                                   */
/* -------------------------------------------------------------------------- */

INSERT INTO [portfolio].[PortfolioProjects]
(
    [Id], [OwnerType], [OwnerUserId], [OwnerTeamId], [Title], [Description],
    [Budget], [ImageCover], [ProjectUrl], [CompletionDate], [CategoryId], [Visibility],
    [CreatedAt], [CreatedBy], [IsDeleted]
)
VALUES
(@PortUser, N'User', @Dev1, NULL, N'Personal design system kit', N'Open-source React design system.',
 0, N'https://cdn.example.com/seed/port-user-cover.jpg', N'https://github.com/example/ds', DATEADD(MONTH, -2, @Now), @CatWeb, N'Public',
 @Now, @By, 0),

(@PortTeam, N'Team', NULL, @TeamWeb, N'Fintech dashboard', N'Dashboard delivered for a fintech client.',
 12000, N'https://cdn.example.com/seed/port-team-cover.jpg', N'https://example.com/fintech', DATEADD(MONTH, -1, @Now), @CatWeb, N'Public',
 @Now, @By, 0);

INSERT INTO [portfolio].[PortfolioImages]
    ([Id], [PortfolioProjectId], [ImageUrl], [SortOrder], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @PortUser, N'https://cdn.example.com/seed/port-user-1.jpg', 0, @Now, @By, 0),
(NEWID(), @PortTeam, N'https://cdn.example.com/seed/port-team-1.jpg', 0, @Now, @By, 0),
(NEWID(), @PortTeam, N'https://cdn.example.com/seed/port-team-2.jpg', 1, @Now, @By, 0);

INSERT INTO [portfolio].[PortfolioSkills]
    ([Id], [PortfolioProjectId], [SkillId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @PortUser, @SkillReact, @Now, @By, 0),
(NEWID(), @PortUser, @SkillTs, @Now, @By, 0),
(NEWID(), @PortTeam, @SkillReact, @Now, @By, 0),
(NEWID(), @PortTeam, @SkillAsp, @Now, @By, 0);

INSERT INTO [core].[SocialLinks]
    ([Id], [OwnerType], [OwnerUserId], [OwnerTeamId], [Platform], [Url], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), N'User', @Dev1, NULL, N'GitHub', N'https://github.com/youssef-seed', @Now, @By, 0),
(NEWID(), N'User', @Dev1, NULL, N'LinkedIn', N'https://linkedin.com/in/youssef-seed', @Now, @By, 0),
(NEWID(), N'Team', NULL, @TeamWeb, N'Website', N'https://websquad.example.com', @Now, @By, 0),
(NEWID(), N'Team', NULL, @TeamMobile, N'GitHub', N'https://github.com/mobileforge-seed', @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Notifications (Type stored as int)                                         */
/* -------------------------------------------------------------------------- */

INSERT INTO [dbo].[Notifications]
(
    [Id], [Title], [Body], [Type], [ImageUrl], [ActionUrl], [Data], [IsRead], [ReadAt],
    [UserId], [ProjectId], [ProjectProposalId], [TeamId], [MilestoneId], [ChatRoomId], [MessageId],
    [CreatedAt], [CreatedBy], [IsDeleted]
)
VALUES
(NEWID(), N'New proposal received', N'WebSquad submitted a proposal on your open project.',
 1 /* NewProposal */, NULL, N'/projects/' + CONVERT(NVARCHAR(36), @ProjOpen), NULL, 0, NULL,
 @Client1, @ProjOpen, @PropTeamOpen, NULL, NULL, NULL, NULL, @Now, @By, 0),

(NEWID(), N'Proposal accepted', N'Your team proposal for SaaS billing was accepted.',
 2 /* ProposalAccepted */, NULL, N'/projects/' + CONVERT(NVARCHAR(36), @ProjInProgress), NULL, 1, DATEADD(DAY, -10, @Now),
 @Dev1, @ProjInProgress, @PropTeamActive, @TeamWeb, NULL, NULL, NULL, DATEADD(DAY, -10, @Now), @By, 0),

(NEWID(), N'Join request', N'dev5 requested to join WebSquad.',
 4 /* JoinRequestReceived */, NULL, N'/teams/' + CONVERT(NVARCHAR(36), @TeamWeb), NULL, 0, NULL,
 @Dev1, NULL, NULL, @TeamWeb, NULL, NULL, NULL, @Now, @By, 0),

(NEWID(), N'New chat message', N'Ahmed replied in the proposal chat.',
 15 /* NewChatMessage */, NULL, N'/chat/' + CONVERT(NVARCHAR(36), @ChatPropOpen), NULL, 0, NULL,
 @Dev1, @ProjOpen, @PropTeamOpen, NULL, NULL, @ChatPropOpen, @MsgProp, @Now, @By, 0),

(NEWID(), N'Project completed', N'Your portfolio project was marked completed.',
 0 /* System */, NULL, N'/projects/' + CONVERT(NVARCHAR(36), @ProjCompleted), NULL, 1, DATEADD(DAY, -1, @Now),
 @Client3, @ProjCompleted, NULL, @TeamWeb, NULL, NULL, NULL, DATEADD(DAY, -2, @Now), @By, 0);

COMMIT TRANSACTION;

PRINT N'Demo seed completed successfully.';
PRINT N'Login with any *@freegency.local account and password: Password123!';
GO
