/*
================================================================================
 FreeGency — Enrich filter projects: teams + proposals + cover images
================================================================================
 Use when the 16 filter projects (Id 44444444-…) already exist but lack
 teams / proposals / ProjectFiles covers.

 Safe to re-run: skips rows that already exist.
================================================================================
*/

/* Target DB via sqlcmd -d (FreeGency_DB locally, or deployed catalog name). */

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;

BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT 1 FROM [marketplace].[Projects]
    WHERE [Id] = '44444444-4444-4444-4444-000000000001'
)
BEGIN
    SELECT N'filter_projects_missing_run_seed_filter_projects_first' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @By  NVARCHAR(256) = N'system';

DECLARE @ClientId UNIQUEIDENTIFIER =
    (SELECT TOP 1 [ClientId] FROM [marketplace].[Projects]
     WHERE [Id] = '44444444-4444-4444-4444-000000000001');

DECLARE @Dev1 UNIQUEIDENTIFIER = (
    SELECT TOP 1 [UserId] FROM [identity].[DeveloperProfiles] ORDER BY [CreatedAt]);
DECLARE @Dev2 UNIQUEIDENTIFIER = (
    SELECT [UserId] FROM (
        SELECT [UserId], ROW_NUMBER() OVER (ORDER BY [CreatedAt]) AS rn
        FROM [identity].[DeveloperProfiles]
    ) d WHERE d.rn = 2);
DECLARE @Dev3 UNIQUEIDENTIFIER = (
    SELECT [UserId] FROM (
        SELECT [UserId], ROW_NUMBER() OVER (ORDER BY [CreatedAt]) AS rn
        FROM [identity].[DeveloperProfiles]
    ) d WHERE d.rn = 3);
DECLARE @Dev4 UNIQUEIDENTIFIER = (
    SELECT [UserId] FROM (
        SELECT [UserId], ROW_NUMBER() OVER (ORDER BY [CreatedAt]) AS rn
        FROM [identity].[DeveloperProfiles]
    ) d WHERE d.rn = 4);
DECLARE @Dev5 UNIQUEIDENTIFIER = (
    SELECT [UserId] FROM (
        SELECT [UserId], ROW_NUMBER() OVER (ORDER BY [CreatedAt]) AS rn
        FROM [identity].[DeveloperProfiles]
    ) d WHERE d.rn = 5);

IF @Dev2 IS NULL SET @Dev2 = @Dev1;
IF @Dev3 IS NULL SET @Dev3 = @Dev1;
IF @Dev4 IS NULL SET @Dev4 = @Dev1;
IF @Dev5 IS NULL SET @Dev5 = @Dev1;

IF @Dev1 IS NULL OR @ClientId IS NULL
BEGIN
    SELECT N'missing_client_or_developer' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @CatUiux UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'UI/UX for Software');
DECLARE @CatDevops UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'DevOps & Cloud');
DECLARE @CatSecurity UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Cybersecurity');
DECLARE @CatDataAi UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Data & AI');
DECLARE @CatEcommerce UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'E-commerce Development');
DECLARE @CatMobile UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Mobile Development');
DECLARE @CatWeb UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Web Development');

DECLARE @TeamWeb UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000001';
DECLARE @TeamMobile UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000002';
DECLARE @TeamDesign UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000003';
DECLARE @TeamCloud UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000004';
DECLARE @TeamSecure UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000005';
DECLARE @TeamData UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000006';
DECLARE @TeamShop UNIQUEIDENTIFIER = 'cccccccc-cccc-cccc-cccc-000000000007';

/* Prefer distinct developers for second seats when few profiles exist */
DECLARE @Alt2 UNIQUEIDENTIFIER = CASE WHEN @Dev2 <> @Dev1 THEN @Dev2 ELSE COALESCE(NULLIF(@Dev3, @Dev1), NULLIF(@Dev4, @Dev1), NULLIF(@Dev5, @Dev1), @Dev1) END;
DECLARE @Alt3 UNIQUEIDENTIFIER = CASE WHEN @Dev3 <> @Dev1 THEN @Dev3 ELSE COALESCE(NULLIF(@Dev2, @Dev1), NULLIF(@Dev4, @Dev1), NULLIF(@Dev5, @Dev1), @Dev1) END;
DECLARE @Alt4 UNIQUEIDENTIFIER = CASE WHEN @Dev4 <> @Dev1 THEN @Dev4 ELSE COALESCE(NULLIF(@Dev2, @Dev1), NULLIF(@Dev3, @Dev1), NULLIF(@Dev5, @Dev1), @Dev1) END;
DECLARE @Alt5 UNIQUEIDENTIFIER = CASE WHEN @Dev5 <> @Dev1 THEN @Dev5 ELSE COALESCE(NULLIF(@Dev2, @Dev1), NULLIF(@Dev3, @Dev1), NULLIF(@Dev4, @Dev1), @Dev1) END;

/* Create teams if missing (by Id or TeamCode) */
IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamWeb OR [TeamCode] = N'WEBSQUAD')
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (@TeamWeb, @Dev1, N'WebSquad', NULL, N'WEBSQUAD', N'Web & SaaS delivery squad.', 4.80, 4, @Now, @By, 0);
    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamWeb, @Dev1, N'TeamLeader', N'Tech Lead', CAST(@Now AS date), @Now, @By, 0);
    IF @Alt2 <> @Dev1
        INSERT INTO [teams].[TeamMembers]
            ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamWeb, @Alt2, N'TeamMember', N'Frontend', CAST(@Now AS date), @Now, @By, 0);
    IF @CatWeb IS NOT NULL
        INSERT INTO [teams].[TeamCategories] ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamWeb, @CatWeb, 1, @Now, @By, 0);
END
ELSE
    SET @TeamWeb = COALESCE(
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [Id] = @TeamWeb),
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [TeamCode] = N'WEBSQUAD'));

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamMobile OR [TeamCode] = N'MOBFORGE')
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (@TeamMobile, @Alt3, N'MobileForge', NULL, N'MOBFORGE', N'Cross-platform mobile specialists.', 4.50, 2, @Now, @By, 0);
    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamMobile, @Alt3, N'TeamLeader', N'Mobile Lead', CAST(@Now AS date), @Now, @By, 0);
    IF @Alt5 <> @Alt3
        INSERT INTO [teams].[TeamMembers]
            ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamMobile, @Alt5, N'TeamMember', N'QA/DevOps', CAST(@Now AS date), @Now, @By, 0);
    IF @CatMobile IS NOT NULL
        INSERT INTO [teams].[TeamCategories] ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamMobile, @CatMobile, 1, @Now, @By, 0);
END
ELSE
    SET @TeamMobile = COALESCE(
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [Id] = @TeamMobile),
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [TeamCode] = N'MOBFORGE'));

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamDesign OR [TeamCode] = N'PIXEL01')
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (@TeamDesign, @Alt2, N'PixelCraft', N'/assets/client-home/brutalist-portfolio.jpg', N'PIXEL01', N'UI/UX and design systems studio.', 4.70, 3, @Now, @By, 0);
    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamDesign, @Alt2, N'TeamLeader', N'Design Lead', CAST(@Now AS date), @Now, @By, 0);
    IF @Alt5 <> @Alt2
        INSERT INTO [teams].[TeamMembers]
            ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamDesign, @Alt5, N'TeamMember', N'Product Designer', CAST(@Now AS date), @Now, @By, 0);
    IF @CatUiux IS NOT NULL
        INSERT INTO [teams].[TeamCategories] ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamDesign, @CatUiux, 1, @Now, @By, 0);
END
ELSE
    SET @TeamDesign = COALESCE(
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [Id] = @TeamDesign),
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [TeamCode] = N'PIXEL01'));

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamCloud OR [TeamCode] = N'CLOUDOPS')
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (@TeamCloud, @Alt5, N'CloudOps', N'/assets/client-home/cyber-sentinel-lp.jpg', N'CLOUDOPS', N'CI/CD, Azure, and Kubernetes delivery.', 4.60, 2, @Now, @By, 0);
    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamCloud, @Alt5, N'TeamLeader', N'DevOps Lead', CAST(@Now AS date), @Now, @By, 0);
    IF @Alt4 <> @Alt5
        INSERT INTO [teams].[TeamMembers]
            ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamCloud, @Alt4, N'TeamMember', N'Cloud Engineer', CAST(@Now AS date), @Now, @By, 0);
    IF @CatDevops IS NOT NULL
        INSERT INTO [teams].[TeamCategories] ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamCloud, @CatDevops, 1, @Now, @By, 0);
END
ELSE
    SET @TeamCloud = COALESCE(
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [Id] = @TeamCloud),
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [TeamCode] = N'CLOUDOPS'));

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamSecure OR [TeamCode] = N'SECURE01')
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (@TeamSecure, @Alt4, N'SecureShield', N'/assets/client-home/stories-app-ui-kit.jpg', N'SECURE01', N'AppSec and penetration testing.', 4.90, 5, @Now, @By, 0);
    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamSecure, @Alt4, N'TeamLeader', N'Security Lead', CAST(@Now AS date), @Now, @By, 0);
    IF @Dev1 <> @Alt4
        INSERT INTO [teams].[TeamMembers]
            ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamSecure, @Dev1, N'TeamMember', N'Secure Code Reviewer', CAST(@Now AS date), @Now, @By, 0);
    IF @CatSecurity IS NOT NULL
        INSERT INTO [teams].[TeamCategories] ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamSecure, @CatSecurity, 1, @Now, @By, 0);
END
ELSE
    SET @TeamSecure = COALESCE(
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [Id] = @TeamSecure),
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [TeamCode] = N'SECURE01'));

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamData OR [TeamCode] = N'DATANEST')
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (@TeamData, @Dev1, N'DataNest', N'/assets/client-home/e-learning-dashboard.jpg', N'DATANEST', N'Data, BI, and LLM solutions.', 4.75, 3, @Now, @By, 0);
    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamData, @Dev1, N'TeamLeader', N'Data Lead', CAST(@Now AS date), @Now, @By, 0);
    IF @Alt2 <> @Dev1
        INSERT INTO [teams].[TeamMembers]
            ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamData, @Alt2, N'TeamMember', N'ML Engineer', CAST(@Now AS date), @Now, @By, 0);
    IF @CatDataAi IS NOT NULL
        INSERT INTO [teams].[TeamCategories] ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamData, @CatDataAi, 1, @Now, @By, 0);
END
ELSE
    SET @TeamData = COALESCE(
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [Id] = @TeamData),
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [TeamCode] = N'DATANEST'));

IF NOT EXISTS (SELECT 1 FROM [teams].[Teams] WHERE [Id] = @TeamShop OR [TeamCode] = N'SHOPFORGE')
BEGIN
    INSERT INTO [teams].[Teams]
        ([Id], [OwnerUserId], [Name], [Logo], [TeamCode], [AboutUs], [AverageRating], [RatingCount], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (@TeamShop, @Alt3, N'ShopForge', N'/assets/client-home/nomad-finance-app.jpg', N'SHOPFORGE', N'Shopify and e-commerce specialists.', 4.40, 2, @Now, @By, 0);
    INSERT INTO [teams].[TeamMembers]
        ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (NEWID(), @TeamShop, @Alt3, N'TeamLeader', N'Commerce Lead', CAST(@Now AS date), @Now, @By, 0);
    IF @Alt2 <> @Alt3
        INSERT INTO [teams].[TeamMembers]
            ([Id], [TeamId], [UserId], [TeamRole], [Job], [JoinedAt], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamShop, @Alt2, N'TeamMember', N'Storefront Dev', CAST(@Now AS date), @Now, @By, 0);
    IF @CatEcommerce IS NOT NULL
        INSERT INTO [teams].[TeamCategories] ([Id], [TeamId], [CategoryId], [IsPrimary], [CreatedAt], [CreatedBy], [IsDeleted])
        VALUES (NEWID(), @TeamShop, @CatEcommerce, 1, @Now, @By, 0);
END
ELSE
    SET @TeamShop = COALESCE(
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [Id] = @TeamShop),
        (SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [TeamCode] = N'SHOPFORGE'));

DECLARE @P1  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000001';
DECLARE @P2  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000002';
DECLARE @P3  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000003';
DECLARE @P4  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000004';
DECLARE @P5  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000005';
DECLARE @P6  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000006';
DECLARE @P7  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000007';
DECLARE @P8  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000008';
DECLARE @P9  UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000009';
DECLARE @P10 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000010';
DECLARE @P11 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000011';
DECLARE @P12 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000012';
DECLARE @P13 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000013';
DECLARE @P14 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000014';
DECLARE @P15 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000015';
DECLARE @P16 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-000000000016';

/* Assign teams on InProgress / Completed */
UPDATE [marketplace].[Projects] SET [AssignedTeamId] = @TeamMobile WHERE [Id] = @P14 AND ([AssignedTeamId] IS NULL OR 1 = 1);
UPDATE [marketplace].[Projects] SET [AssignedTeamId] = @TeamData WHERE [Id] = @P15 AND ([AssignedTeamId] IS NULL OR 1 = 1);

/* Cover images */
;WITH Covers AS (
    SELECT * FROM (VALUES
        (@P1,  N'cover-e-learning-dashboard.jpg', N'/assets/client-home/e-learning-dashboard.jpg'),
        (@P2,  N'cover-cyber-sentinel-lp.jpg', N'/assets/client-home/cyber-sentinel-lp.jpg'),
        (@P3,  N'cover-nomad-finance-app.jpg', N'/assets/client-home/nomad-finance-app.jpg'),
        (@P4,  N'cover-social-map-interface.jpg', N'/assets/client-home/social-map-interface.jpg'),
        (@P5,  N'cover-stories-app-ui-kit.jpg', N'/assets/client-home/stories-app-ui-kit.jpg'),
        (@P6,  N'cover-brutalist-portfolio.jpg', N'/assets/client-home/brutalist-portfolio.jpg'),
        (@P7,  N'cover-cyber-sentinel-lp.jpg', N'/assets/client-home/cyber-sentinel-lp.jpg'),
        (@P8,  N'cover-stories-app-ui-kit.jpg', N'/assets/client-home/stories-app-ui-kit.jpg'),
        (@P9,  N'cover-nomad-finance-app.jpg', N'/assets/client-home/nomad-finance-app.jpg'),
        (@P10, N'cover-cyber-sentinel-lp.jpg', N'/assets/client-home/cyber-sentinel-lp.jpg'),
        (@P11, N'cover-stories-app-ui-kit.jpg', N'/assets/client-home/stories-app-ui-kit.jpg'),
        (@P12, N'cover-e-learning-dashboard.jpg', N'/assets/client-home/e-learning-dashboard.jpg'),
        (@P13, N'cover-brutalist-portfolio.jpg', N'/assets/client-home/brutalist-portfolio.jpg'),
        (@P14, N'cover-social-map-interface.jpg', N'/assets/client-home/social-map-interface.jpg'),
        (@P15, N'cover-e-learning-dashboard.jpg', N'/assets/client-home/e-learning-dashboard.jpg'),
        (@P16, N'cover-nomad-finance-app.jpg', N'/assets/client-home/nomad-finance-app.jpg')
    ) v(ProjectId, FileName, FileUrl)
)
INSERT INTO [marketplace].[ProjectFiles]
    ([Id], [ProjectId], [MilestoneId], [UploadedByUserId], [FileName], [FileUrl], [FileKind], [CreatedAt], [CreatedBy], [IsDeleted])
SELECT NEWID(), c.ProjectId, NULL, @ClientId, c.FileName, c.FileUrl, N'Brief', @Now, @By, 0
FROM Covers c
WHERE EXISTS (SELECT 1 FROM [marketplace].[Projects] p WHERE p.[Id] = c.ProjectId)
  AND NOT EXISTS (
      SELECT 1 FROM [marketplace].[ProjectFiles] f
      WHERE f.[ProjectId] = c.ProjectId AND f.[FileUrl] = c.FileUrl AND f.[IsDeleted] = 0
  );

/* Proposals (skip if seed proposal ids already present) */
IF NOT EXISTS (
    SELECT 1 FROM [marketplace].[ProjectProposals]
    WHERE [Id] = '55555555-5555-5555-5555-000000000001'
)
BEGIN
    INSERT INTO [marketplace].[ProjectProposals]
    (
        [Id], [ProjectId], [ApplicantType], [TeamId], [UserId], [CoverLetter],
        [ProposedBudget], [Status], [AppliedAt], [ResponseAt],
        [CreatedAt], [CreatedBy], [IsDeleted]
    )
    VALUES
    ('55555555-5555-5555-5555-000000000001', @P1,  N'Team', @TeamWeb,    NULL,  N'WebSquad can deliver the analytics dashboard with React + charts in 4 weeks.', 1400.00, N'Pending', DATEADD(HOUR, -20, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000002', @P1,  N'User', NULL,         @Dev4, N'Solo proposal focused on role-based dashboards and performance.', 1250.00, N'Pending', DATEADD(HOUR, -18, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000003', @P2,  N'Team', @TeamWeb,    NULL,  N'Fast Next.js landing with SEO and conversion-focused sections.', 520.00, N'Pending', DATEADD(HOUR, -16, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000004', @P3,  N'Team', @TeamWeb,    NULL,  N'NestJS inventory API with auth, pagination, and Postgres.', 3800.00, N'Pending', DATEADD(HOUR, -15, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000005', @P4,  N'Team', @TeamMobile, NULL,  N'MobileForge Flutter delivery tracker with realtime updates.', 8200.00, N'Pending', DATEADD(HOUR, -14, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000006', @P4,  N'User', NULL,         @Dev3, N'Solo Flutter specialist available for the delivery app MVP.', 7000.00, N'Pending', DATEADD(HOUR, -13, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000007', @P5,  N'Team', @TeamWeb,    NULL,  N'SaaS admin MVP with ASP.NET Core + React reporting.', 6000.00, N'Pending', DATEADD(HOUR, -12, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000008', @P6,  N'Team', @TeamDesign, NULL,  N'PixelCraft fintech design system in Figma with tokens and components.', 2100.00, N'Pending', DATEADD(HOUR, -11, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000009', @P7,  N'Team', @TeamCloud,  NULL,  N'CloudOps Azure CI/CD with Docker staging/production pipelines.', 2600.00, N'Pending', DATEADD(HOUR, -10, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000010', @P8,  N'Team', @TeamCloud,  NULL,  N'Playwright E2E suite covering auth, checkout, and regressions.', 1500.00, N'Pending', DATEADD(HOUR, -9, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000011', @P9,  N'Team', @TeamData,   NULL,  N'DataNest bilingual LLM support chatbot with LangChain.', 7200.00, N'Pending', DATEADD(HOUR, -8, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000012', @P10, N'Team', @TeamSecure, NULL,  N'SecureShield OWASP review, pentest, and remediation report.', 4200.00, N'Pending', DATEADD(HOUR, -7, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000013', @P11, N'Team', @TeamShop,   NULL,  N'ShopForge Shopify theme customization and Stripe checkout UX.', 28000.00, N'Pending', DATEADD(HOUR, -6, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000014', @P12, N'User', NULL,         @Dev2, N'Hourly Angular portal enhancements with RxJS expertise.', 1200.00, N'Pending', DATEADD(HOUR, -5, @Now), NULL, @Now, @By, 0),
    ('55555555-5555-5555-5555-000000000015', @P14, N'Team', @TeamMobile, NULL,  N'Accepted: MobileForge owns the iOS fitness tracker delivery.', 8500.00, N'Accepted', DATEADD(DAY, -12, @Now), DATEADD(DAY, -10, @Now), DATEADD(DAY, -12, @Now), @By, 0),
    ('55555555-5555-5555-5555-000000000016', @P14, N'User', NULL,         @Dev5, N'Rejected alternate solo bid for the fitness tracker.', 7800.00, N'Rejected', DATEADD(DAY, -11, @Now), DATEADD(DAY, -10, @Now), DATEADD(DAY, -11, @Now), @By, 0),
    ('55555555-5555-5555-5555-000000000017', @P15, N'Team', @TeamData,   NULL,  N'Accepted: DataNest delivered Power BI sales dashboards.', 1800.00, N'Accepted', DATEADD(DAY, -25, @Now), DATEADD(DAY, -23, @Now), DATEADD(DAY, -25, @Now), @By, 0),
    ('55555555-5555-5555-5555-000000000018', @P16, N'Team', @TeamShop,   NULL,  N'Withdrawn after Magento marketplace was cancelled.', 12000.00, N'Withdrawn', DATEADD(DAY, -30, @Now), DATEADD(DAY, -28, @Now), DATEADD(DAY, -30, @Now), @By, 0);
END;

COMMIT TRANSACTION;

SELECT N'enriched_ok' AS Result;
SELECT COUNT(*) AS Teams FROM [teams].[Teams] WHERE [TeamCode] IN (N'WEBSQUAD', N'MOBFORGE', N'PIXEL01', N'CLOUDOPS', N'SECURE01', N'DATANEST', N'SHOPFORGE');
SELECT COUNT(*) AS Proposals FROM [marketplace].[ProjectProposals] WHERE [Id] LIKE '55555555-5555-5555-5555-%';
SELECT COUNT(*) AS CoverFiles FROM [marketplace].[ProjectFiles] WHERE [FileUrl] LIKE N'/assets/client-home/%';
GO
