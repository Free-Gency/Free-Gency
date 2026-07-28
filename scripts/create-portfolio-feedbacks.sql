/*
================================================================================
 FreeGency — Create portfolio.PortfolioFeedbacks (if migration not applied)
================================================================================
 Safe to re-run.
================================================================================
*/

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'portfolio' AND t.name = N'PortfolioFeedbacks'
)
BEGIN
    CREATE TABLE [portfolio].[PortfolioFeedbacks]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PortfolioFeedbacks_CreatedAt] DEFAULT (GETUTCDATE()),
        [CreatedBy] NVARCHAR(256) NOT NULL CONSTRAINT [DF_PortfolioFeedbacks_CreatedBy] DEFAULT (N'system'),
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(256) NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_PortfolioFeedbacks_IsDeleted] DEFAULT (0),
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(256) NULL,
        [PortfolioProjectId] UNIQUEIDENTIFIER NOT NULL,
        [ReviewerUserId] UNIQUEIDENTIFIER NOT NULL,
        [Rating] INT NOT NULL,
        [Comment] NVARCHAR(500) NULL,
        CONSTRAINT [PK_PortfolioFeedbacks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PortfolioFeedbacks_PortfolioProjects_PortfolioProjectId]
            FOREIGN KEY ([PortfolioProjectId])
            REFERENCES [portfolio].[PortfolioProjects] ([Id])
            ON DELETE CASCADE,
        CONSTRAINT [FK_PortfolioFeedbacks_Users_ReviewerUserId]
            FOREIGN KEY ([ReviewerUserId])
            REFERENCES [identity].[Users] ([Id])
            ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_PortfolioFeedbacks_PortfolioProjectId_ReviewerUserId]
        ON [portfolio].[PortfolioFeedbacks] ([PortfolioProjectId], [ReviewerUserId]);

    CREATE INDEX [IX_PortfolioFeedbacks_PortfolioProjectId_CreatedAt]
        ON [portfolio].[PortfolioFeedbacks] ([PortfolioProjectId], [CreatedAt]);

    CREATE INDEX [IX_PortfolioFeedbacks_ReviewerUserId]
        ON [portfolio].[PortfolioFeedbacks] ([ReviewerUserId]);

    PRINT N'Created portfolio.PortfolioFeedbacks';
END
ELSE
BEGIN
    PRINT N'portfolio.PortfolioFeedbacks already exists';
END
GO
