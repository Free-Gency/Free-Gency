/*
================================================================================
 FreeGency — Seed public portfolio items for Inspiration (home design covers)
================================================================================
 Safe to re-run: skips when inspiration portfolio ids already exist.

 Target DB via connection (local FreeGency_DB or deployed) — no hard-coded DB name.
 Prefer running via seed-team-client-home.sql for the full team pack.
================================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1 FROM [portfolio].[PortfolioProjects]
    WHERE [Id] = '66666666-6666-6666-6666-000000000001'
)
BEGIN
    SELECT N'inspiration_portfolio_already_seeded' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @By NVARCHAR(256) = N'system';

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

IF @Dev2 IS NULL SET @Dev2 = @Dev1;
IF @Dev3 IS NULL SET @Dev3 = @Dev1;

DECLARE @TeamWeb UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [TeamCode] = N'WEBSQUAD');
DECLARE @TeamMobile UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [TeamCode] = N'MOBFORGE');
DECLARE @TeamDesign UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id] FROM [teams].[Teams] WHERE [TeamCode] = N'PIXEL01');

DECLARE @CatWeb UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Web Development');
DECLARE @CatMobile UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Mobile Development');
DECLARE @CatUiux UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'UI/UX for Software');
DECLARE @CatSaas UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Custom Software / SaaS');

IF @Dev1 IS NULL OR @CatWeb IS NULL
BEGIN
    SELECT N'missing_developer_or_category' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @P1 UNIQUEIDENTIFIER = '66666666-6666-6666-6666-000000000001';
DECLARE @P2 UNIQUEIDENTIFIER = '66666666-6666-6666-6666-000000000002';
DECLARE @P3 UNIQUEIDENTIFIER = '66666666-6666-6666-6666-000000000003';
DECLARE @P4 UNIQUEIDENTIFIER = '66666666-6666-6666-6666-000000000004';
DECLARE @P5 UNIQUEIDENTIFIER = '66666666-6666-6666-6666-000000000005';
DECLARE @P6 UNIQUEIDENTIFIER = '66666666-6666-6666-6666-000000000006';

INSERT INTO [portfolio].[PortfolioProjects]
(
    [Id], [OwnerType], [OwnerUserId], [OwnerTeamId], [Title], [Description],
    [Budget], [ImageCover], [ProjectUrl], [CompletionDate], [CategoryId], [Visibility],
    [CreatedAt], [CreatedBy], [IsDeleted]
)
VALUES
(@P1, N'User', @Dev2, NULL,
 N'Nomad Finance App', N'Mobile banking UI with cards, transfers, and spending insights.',
 4500, N'/assets/client-home/nomad-finance-app.jpg', NULL, DATEADD(DAY, -40, @Now),
 COALESCE(@CatMobile, @CatWeb), N'Public', @Now, @By, 0),

(@P2, CASE WHEN COALESCE(@TeamWeb, @TeamDesign) IS NULL THEN N'User' ELSE N'Team' END,
 CASE WHEN COALESCE(@TeamWeb, @TeamDesign) IS NULL THEN @Dev1 ELSE NULL END,
 COALESCE(@TeamWeb, @TeamDesign),
 N'E-Learning Dashboard', N'Instructor analytics dashboard for courses and engagement.',
 6200, N'/assets/client-home/e-learning-dashboard.jpg', NULL, DATEADD(DAY, -28, @Now),
 COALESCE(@CatSaas, @CatWeb), N'Public', @Now, @By, 0),

(@P3, N'User', @Dev1, NULL,
 N'Cyber Sentinel LP', N'Security product landing page with dark cyber aesthetics.',
 1800, N'/assets/client-home/cyber-sentinel-lp.jpg', NULL, DATEADD(DAY, -21, @Now),
 @CatWeb, N'Public', @Now, @By, 0),

(@P4, CASE WHEN COALESCE(@TeamMobile, @TeamWeb) IS NULL THEN N'User' ELSE N'Team' END,
 CASE WHEN COALESCE(@TeamMobile, @TeamWeb) IS NULL THEN @Dev2 ELSE NULL END,
 COALESCE(@TeamMobile, @TeamWeb),
 N'Social Map Interface', N'Mobile map interface for discovering nearby social events.',
 5100, N'/assets/client-home/social-map-interface.jpg', NULL, DATEADD(DAY, -14, @Now),
 COALESCE(@CatMobile, @CatWeb), N'Public', @Now, @By, 0),

(@P5, CASE WHEN COALESCE(@TeamDesign, @TeamWeb) IS NULL THEN N'User' ELSE N'Team' END,
 CASE WHEN COALESCE(@TeamDesign, @TeamWeb) IS NULL THEN @Dev2 ELSE NULL END,
 COALESCE(@TeamDesign, @TeamWeb),
 N'Brutalist Portfolio', N'Bold branding portfolio site with expressive typography.',
 2400, N'/assets/client-home/brutalist-portfolio.jpg', NULL, DATEADD(DAY, -10, @Now),
 COALESCE(@CatUiux, @CatWeb), N'Public', @Now, @By, 0),

(@P6, N'User', @Dev3, NULL,
 N'Stories App UI Kit', N'Component kit for vertical stories and social media apps.',
 3200, N'/assets/client-home/stories-app-ui-kit.jpg', NULL, DATEADD(DAY, -7, @Now),
 COALESCE(@CatUiux, @CatMobile, @CatWeb), N'Public', @Now, @By, 0);

INSERT INTO [portfolio].[PortfolioImages]
    ([Id], [PortfolioProjectId], [ImageUrl], [SortOrder], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
(NEWID(), @P1, N'/assets/client-home/nomad-finance-app.jpg', 0, @Now, @By, 0),
(NEWID(), @P2, N'/assets/client-home/e-learning-dashboard.jpg', 0, @Now, @By, 0),
(NEWID(), @P3, N'/assets/client-home/cyber-sentinel-lp.jpg', 0, @Now, @By, 0),
(NEWID(), @P4, N'/assets/client-home/social-map-interface.jpg', 0, @Now, @By, 0),
(NEWID(), @P5, N'/assets/client-home/brutalist-portfolio.jpg', 0, @Now, @By, 0),
(NEWID(), @P6, N'/assets/client-home/stories-app-ui-kit.jpg', 0, @Now, @By, 0);

/* Point older seed covers at local home assets when still using example CDN */
UPDATE [portfolio].[PortfolioProjects]
SET [ImageCover] = N'/assets/client-home/brutalist-portfolio.jpg'
WHERE [ImageCover] LIKE N'https://cdn.example.com/seed/port-user%';

UPDATE [portfolio].[PortfolioProjects]
SET [ImageCover] = N'/assets/client-home/e-learning-dashboard.jpg'
WHERE [ImageCover] LIKE N'https://cdn.example.com/seed/port-team%';

COMMIT TRANSACTION;

SELECT N'inspiration_portfolio_seeded' AS Result;
SELECT COUNT(*) AS PublicPortfolio
FROM [portfolio].[PortfolioProjects]
WHERE [Visibility] = N'Public' AND [IsDeleted] = 0;
GO
