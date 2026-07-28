/*
================================================================================
 FreeGency — Enrich Inspiration details (galleries + owner Reviews)
================================================================================
 Safe to re-run.
 Works on LOCAL and DEPLOYED — select DB via sqlcmd -d / SSMS connection.

 Adds:
   - Up to 4 gallery images per inspiration portfolio (66666666-…)
   - Sample Reviews for each distinct portfolio owner (User/Team)
   - Refreshes DeveloperProfiles / Teams AverageRating + RatingCount

 Reviews note:
   Unique index = (ProjectId, ReviewerUserId, RevieweeType)
   so each owner gets a different marketplace ProjectId when possible.
================================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @By  NVARCHAR(256) = N'system';

/* -------------------------------------------------------------------------- */
/* 1) Galleries                                                               */
/* -------------------------------------------------------------------------- */

DECLARE @Assets TABLE (SortOrder INT, Url NVARCHAR(500));
INSERT INTO @Assets (SortOrder, Url) VALUES
(0, N'/assets/client-home/nomad-finance-app.jpg'),
(1, N'/assets/client-home/e-learning-dashboard.jpg'),
(2, N'/assets/client-home/cyber-sentinel-lp.jpg'),
(3, N'/assets/client-home/social-map-interface.jpg'),
(4, N'/assets/client-home/brutalist-portfolio.jpg'),
(5, N'/assets/client-home/stories-app-ui-kit.jpg');

;WITH Targets AS (
    SELECT
        p.[Id] AS PortfolioProjectId,
        a.Url,
        ((a.SortOrder + ABS(CHECKSUM(p.[Id]))) % 6) AS SortOrder,
        ROW_NUMBER() OVER (
            PARTITION BY p.[Id]
            ORDER BY ((a.SortOrder + ABS(CHECKSUM(p.[Id]))) % 6)
        ) AS rn
    FROM [portfolio].[PortfolioProjects] p
    CROSS JOIN @Assets a
    WHERE p.[Id] IN (
        '66666666-6666-6666-6666-000000000001',
        '66666666-6666-6666-6666-000000000002',
        '66666666-6666-6666-6666-000000000003',
        '66666666-6666-6666-6666-000000000004',
        '66666666-6666-6666-6666-000000000005',
        '66666666-6666-6666-6666-000000000006'
    )
)
INSERT INTO [portfolio].[PortfolioImages]
    ([Id], [PortfolioProjectId], [ImageUrl], [SortOrder], [CreatedAt], [CreatedBy], [IsDeleted])
SELECT NEWID(), t.PortfolioProjectId, t.Url, t.SortOrder, @Now, @By, 0
FROM Targets t
WHERE t.rn <= 4
  AND NOT EXISTS (
      SELECT 1
      FROM [portfolio].[PortfolioImages] i
      WHERE i.[PortfolioProjectId] = t.PortfolioProjectId
        AND i.[ImageUrl] = t.Url
        AND i.[IsDeleted] = 0
  );

PRINT N'Gallery images upserted for inspiration portfolios.';

/* -------------------------------------------------------------------------- */
/* 2) Reviews for portfolio owners                                            */
/* -------------------------------------------------------------------------- */

DECLARE @ClientId UNIQUEIDENTIFIER = (
    SELECT TOP 1 u.[Id]
    FROM [identity].[Users] u
    INNER JOIN [identity].[ClientProfiles] c ON c.[UserId] = u.[Id]
    ORDER BY u.[CreatedAt]
);

IF @ClientId IS NULL
BEGIN
    PRINT N'SKIP reviews: no client profile found.';
END
ELSE IF NOT EXISTS (SELECT 1 FROM [marketplace].[Projects] WHERE [IsDeleted] = 0)
BEGIN
    PRINT N'SKIP reviews: no marketplace projects found.';
END
ELSE
BEGIN
    ;WITH Owners AS (
        SELECT
            ROW_NUMBER() OVER (ORDER BY OwnerKey) AS rn,
            RevieweeType,
            OwnerUserId,
            OwnerTeamId
        FROM (
            SELECT DISTINCT
                CAST(p.[OwnerUserId] AS NVARCHAR(36)) AS OwnerKey,
                N'User' AS RevieweeType,
                p.[OwnerUserId] AS OwnerUserId,
                CAST(NULL AS UNIQUEIDENTIFIER) AS OwnerTeamId
            FROM [portfolio].[PortfolioProjects] p
            WHERE p.[Id] LIKE '66666666-6666-6666-6666-%'
              AND p.[OwnerType] = N'User'
              AND p.[OwnerUserId] IS NOT NULL
              AND p.[IsDeleted] = 0

            UNION ALL

            SELECT DISTINCT
                CAST(p.[OwnerTeamId] AS NVARCHAR(36)),
                N'Team',
                NULL,
                p.[OwnerTeamId]
            FROM [portfolio].[PortfolioProjects] p
            WHERE p.[Id] LIKE '66666666-6666-6666-6666-%'
              AND p.[OwnerType] = N'Team'
              AND p.[OwnerTeamId] IS NOT NULL
              AND p.[IsDeleted] = 0
        ) x
    ),
    Projects AS (
        SELECT
            ROW_NUMBER() OVER (ORDER BY [CreatedAt], [Id]) AS rn,
            [Id] AS ProjectId
        FROM [marketplace].[Projects]
        WHERE [IsDeleted] = 0
    )
    INSERT INTO [marketplace].[Reviews]
    (
        [Id], [CreatedAt], [CreatedBy], [IsDeleted],
        [ProjectId], [ReviewerUserId], [RevieweeType],
        [RevieweeUserId], [RevieweeTeamId], [Rating], [Comment]
    )
    SELECT
        NEWID(),
        DATEADD(DAY, -2 - o.rn, @Now),
        @By,
        0,
        p.ProjectId,
        @ClientId,
        o.RevieweeType,
        o.OwnerUserId,
        o.OwnerTeamId,
        CASE WHEN (o.rn % 2) = 0 THEN 4 ELSE 5 END,
        CASE
            WHEN o.RevieweeType = N'User'
                THEN N'Clear communication and polished delivery on the portfolio work.'
            ELSE N'The team handled scope changes smoothly and shipped on time.'
        END
    FROM Owners o
    INNER JOIN Projects p ON p.rn = o.rn
    WHERE NOT EXISTS (
        SELECT 1
        FROM [marketplace].[Reviews] r
        WHERE r.[ProjectId] = p.ProjectId
          AND r.[ReviewerUserId] = @ClientId
          AND r.[RevieweeType] = o.RevieweeType
          AND r.[IsDeleted] = 0
    )
    AND NOT EXISTS (
        SELECT 1
        FROM [marketplace].[Reviews] r
        WHERE r.[ReviewerUserId] = @ClientId
          AND r.[RevieweeType] = o.RevieweeType
          AND r.[IsDeleted] = 0
          AND (
                (o.RevieweeType = N'User' AND r.[RevieweeUserId] = o.OwnerUserId)
             OR (o.RevieweeType = N'Team' AND r.[RevieweeTeamId] = o.OwnerTeamId)
          )
          AND r.[Comment] IN (
              N'Clear communication and polished delivery on the portfolio work.',
              N'The team handled scope changes smoothly and shipped on time.'
          )
    );

    PRINT N'Owner reviews upserted (when unique index allowed).';
END

/* -------------------------------------------------------------------------- */
/* 3) Refresh denormalized ratings                                            */
/* -------------------------------------------------------------------------- */

UPDATE dp
SET
    dp.[AverageRating] = ISNULL(x.AvgRating, dp.[AverageRating]),
    dp.[RatingCount] = ISNULL(x.Cnt, dp.[RatingCount])
FROM [identity].[DeveloperProfiles] dp
OUTER APPLY (
    SELECT
        AVG(CAST(r.[Rating] AS DECIMAL(9, 2))) AS AvgRating,
        COUNT(*) AS Cnt
    FROM [marketplace].[Reviews] r
    WHERE r.[RevieweeType] = N'User'
      AND r.[RevieweeUserId] = dp.[UserId]
      AND r.[IsDeleted] = 0
) x
WHERE EXISTS (
    SELECT 1
    FROM [portfolio].[PortfolioProjects] p
    WHERE p.[OwnerUserId] = dp.[UserId]
      AND p.[Id] LIKE '66666666-6666-6666-6666-%'
);

UPDATE t
SET
    t.[AverageRating] = ISNULL(x.AvgRating, t.[AverageRating]),
    t.[RatingCount] = ISNULL(x.Cnt, t.[RatingCount])
FROM [teams].[Teams] t
OUTER APPLY (
    SELECT
        AVG(CAST(r.[Rating] AS DECIMAL(9, 2))) AS AvgRating,
        COUNT(*) AS Cnt
    FROM [marketplace].[Reviews] r
    WHERE r.[RevieweeType] = N'Team'
      AND r.[RevieweeTeamId] = t.[Id]
      AND r.[IsDeleted] = 0
) x
WHERE EXISTS (
    SELECT 1
    FROM [portfolio].[PortfolioProjects] p
    WHERE p.[OwnerTeamId] = t.[Id]
      AND p.[Id] LIKE '66666666-6666-6666-6666-%'
);

SELECT
    (SELECT COUNT(*)
     FROM [portfolio].[PortfolioImages]
     WHERE [PortfolioProjectId] LIKE '66666666-6666-6666-6666-%'
       AND [IsDeleted] = 0) AS InspirationImages,
    (SELECT COUNT(*)
     FROM [marketplace].[Reviews]
     WHERE [IsDeleted] = 0) AS ReviewsTotal,
    (SELECT COUNT(*)
     FROM [portfolio].[PortfolioProjects]
     WHERE [Id] LIKE '66666666-6666-6666-6666-%'
       AND [IsDeleted] = 0) AS InspirationProjects;

PRINT N'seed-portfolio-details-enrich finished.';
GO
