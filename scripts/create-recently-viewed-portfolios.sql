/*
================================================================================
 FreeGency — Create portfolio.RecentlyViewedPortfolios
================================================================================
 Safe to re-run: creates table only if missing.
================================================================================
*/

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'portfolio' AND t.name = N'RecentlyViewedPortfolios'
)
BEGIN
    CREATE TABLE [portfolio].[RecentlyViewedPortfolios]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_RecentlyViewedPortfolios_CreatedAt] DEFAULT (GETUTCDATE()),
        [CreatedBy] NVARCHAR(256) NOT NULL CONSTRAINT [DF_RecentlyViewedPortfolios_CreatedBy] DEFAULT (N'system'),
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(256) NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_RecentlyViewedPortfolios_IsDeleted] DEFAULT (0),
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(256) NULL,
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [PortfolioProjectId] UNIQUEIDENTIFIER NOT NULL,
        [ViewedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_RecentlyViewedPortfolios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RecentlyViewedPortfolios_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RecentlyViewedPortfolios_PortfolioProjects_PortfolioProjectId]
            FOREIGN KEY ([PortfolioProjectId]) REFERENCES [portfolio].[PortfolioProjects] ([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_RecentlyViewedPortfolios_UserId_PortfolioProjectId]
        ON [portfolio].[RecentlyViewedPortfolios] ([UserId], [PortfolioProjectId]);

    CREATE INDEX [IX_RecentlyViewedPortfolios_UserId_ViewedAt]
        ON [portfolio].[RecentlyViewedPortfolios] ([UserId], [ViewedAt]);

    SELECT N'recently_viewed_table_created' AS Result;
END
ELSE
    SELECT N'recently_viewed_table_already_exists' AS Result;
GO
