/*
================================================================================
 FreeGency — Omar Nabil milestones demo seed
================================================================================
 Adds InProgress projects (with milestone plans + escrow) owned by Omar Nabil
 (client3), assigned to developer Youssef Kamal (dev1) solo AND his team
 Nile Code Studio (Team1). Also adds an Open Omar project that the same team
 applied to.

 Idempotent: skips when project a5555555-a555-a555-a555-000000000001 exists.

 Prerequisites:
   - Users/teams from final-seed-data.sql (or equivalent demo seed)
   - Omar:  client3@freegency.local  (aaaaaaaa-aaaa-aaaa-aaaa-000000000003)
   - Dev:   dev1@freegency.local     (aaaaaaaa-aaaa-aaaa-aaaa-000000000011)
   - Team:  WebSquad (or any team owned by Dev1)

 Demo password: Password123!

 After seed:
   Client login  → client3@freegency.local  (Client mode) → Manage Work → Milestones
   Developer login → dev1@freegency.local   (Developer mode) → My Milestones
================================================================================
*/

USE [FreeGency_DB];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;

BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1
    FROM [marketplace].[Projects]
    WHERE [Id] = 'a5555555-a555-a555-a555-000000000001'
)
BEGIN
    SELECT N'already_seeded' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @ClientId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003'; /* Omar Nabil */
DECLARE @Dev1     UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000011'; /* Youssef Kamal */
DECLARE @Dev2     UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000012';

IF NOT EXISTS (SELECT 1 FROM [identity].[Users] WHERE [Id] = @ClientId)
BEGIN
    SELECT N'omar_missing' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM [identity].[Users] WHERE [Id] = @Dev1)
BEGIN
    SELECT N'dev1_missing' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

/* Prefer WebSquad (filter seed), else any team owned by Dev1 */
DECLARE @Team1 UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id]
    FROM [teams].[Teams]
    WHERE [IsDeleted] = 0
      AND (
            [Id] = 'cccccccc-cccc-cccc-cccc-000000000001'
         OR [OwnerUserId] = @Dev1
      )
    ORDER BY CASE WHEN [Id] = 'cccccccc-cccc-cccc-cccc-000000000001' THEN 0 ELSE 1 END,
             [CreatedAt]
);

IF @Team1 IS NULL
BEGIN
    SELECT N'dev1_team_missing' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @By  NVARCHAR(256) = N'seed-omar-milestones';

DECLARE @CatWeb UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Web Development'
);
DECLARE @CatMobile UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id] FROM [catalog].[Categories] WHERE [NameEn] = N'Mobile Development'
);
DECLARE @CatSaas UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id] FROM [catalog].[Categories]
    WHERE [NameEn] LIKE N'%SaaS%' OR [NameEn] = N'Custom Software / SaaS'
);

IF @CatWeb IS NULL SET @CatWeb = (SELECT TOP 1 [Id] FROM [catalog].[Categories] ORDER BY [NameEn]);
IF @CatMobile IS NULL SET @CatMobile = @CatWeb;
IF @CatSaas IS NULL SET @CatSaas = @CatWeb;

DECLARE @P1 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000001'; /* Lumina — solo Dev1 */
DECLARE @P2 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000002'; /* Flutter — Team1 */
DECLARE @P3 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000003'; /* Open — Team1 applied */

DECLARE @Prop1 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000011';
DECLARE @Prop2 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000012';
DECLARE @Prop3 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000013';

DECLARE @Plan1 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000021';
DECLARE @Plan2 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000022';

DECLARE @M11 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000031';
DECLARE @M12 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000032';
DECLARE @M13 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000033';
DECLARE @M14 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000034';
DECLARE @M15 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000035';

DECLARE @M21 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000041';
DECLARE @M22 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000042';
DECLARE @M23 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000043';

/* -------------------------------------------------------------------------- */
/* Projects                                                                    */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[Projects]
    ([Id], [Title], [Description], [ClientId], [CategoryId], [IsFixedPrice],
     [BudgetMin], [BudgetMax], [Currency], [Deadline], [EstimatedDurationDays],
     [Status], [AssignedTeamId], [AssignedUserId], [CompletedAt],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@P1,
     N'Lumina Finance Platform',
     N'Fintech web platform: dashboard, payments ledger, and API integrations for Lumina Tech.',
     @ClientId, @CatSaas, 1,
     10000.00, 12500.00, N'USD', DATEADD(DAY, 75, CAST(@Now AS date)), 90,
     N'InProgress', NULL, @Dev1, NULL,
     DATEADD(DAY, -40, @Now), @By, 0),
    (@P2,
     N'Flutter Mobile App Development',
     N'Cross-platform consumer mobile app with auth, catalog, and offline sync.',
     @ClientId, @CatMobile, 1,
     4500.00, 5000.00, N'USD', DATEADD(DAY, 60, CAST(@Now AS date)), 70,
     N'InProgress', @Team1, NULL, NULL,
     DATEADD(DAY, -20, @Now), @By, 0),
    (@P3,
     N'React Admin Portal Refresh',
     N'Redesign and rebuild an internal admin portal with role-based access and reporting.',
     @ClientId, @CatWeb, 1,
     3000.00, 4500.00, N'USD', DATEADD(DAY, 45, CAST(@Now AS date)), 40,
     N'Open', NULL, NULL, NULL,
     DATEADD(DAY, -5, @Now), @By, 0);

/* -------------------------------------------------------------------------- */
/* Proposals                                                                   */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[ProjectProposals]
    ([Id], [ProjectId], [ApplicantType], [TeamId], [UserId], [CoverLetter], [Approach],
     [ProposedTimeline], [SimilarLinksUrl], [ProposedBudget], [Status], [RejectReason],
     [AppliedAt], [ResponseAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@Prop1, @P1, N'User', NULL, @Dev1,
     N'Solo full-stack delivery for Lumina with weekly demos.',
     N'Milestone-based delivery: UX shell, API, payments, polish.',
     N'12 weeks', N'https://github.com/youssef-kamal', 12500.00, N'InDiscussion', NULL,
     DATEADD(DAY, -38, @Now), DATEADD(DAY, -36, @Now), @Now, @By, 0),
    (@Prop2, @P2, N'Team', @Team1, @Dev1,
     N'Nile Code Studio can ship the Flutter app end-to-end.',
     N'Shared Flutter codebase, CI builds, and staged store releases.',
     N'10 weeks', N'https://nilecode.example.com/portfolio', 5000.00, N'InDiscussion', NULL,
     DATEADD(DAY, -18, @Now), DATEADD(DAY, -16, @Now), @Now, @By, 0),
    (@Prop3, @P3, N'Team', @Team1, @Dev1,
     N'Our team already owns similar admin portal rebuilds.',
     N'Design system first, then modules and role matrix.',
     N'6 weeks', N'https://nilecode.example.com/admin-cases', 4200.00, N'Pending', NULL,
     DATEADD(DAY, -2, @Now), NULL, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Plans + items                                                               */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[MilestonePlanVersions]
    ([Id], [ProjectId], [ProposalId], [Version], [Status], [ChangeComment], [ProposedByUserId],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@Plan1, @P1, @Prop1, 1, N'Accepted', N'Agreed 5-milestone plan.', @Dev1, DATEADD(DAY, -35, @Now), @By, 0),
    (@Plan2, @P2, @Prop2, 1, N'Accepted', NULL, @Dev1, DATEADD(DAY, -15, @Now), @By, 0);

INSERT INTO [marketplace].[MilestonePlanItems]
    ([Id], [PlanVersionId], [Title], [DefinitionOfDone], [Amount], [DueDate], [SortOrder], [ChangeTag],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @Plan1, N'Design system & shell', N'App shell, nav, and design tokens live on staging.', 2000.00, DATEADD(DAY, -20, @Now), 1, NULL, @Now, @By, 0),
    (NEWID(), @Plan1, N'Auth & roles', N'Login, MFA hooks, and role matrix work.', 2500.00, DATEADD(DAY, -5, @Now), 2, NULL, @Now, @By, 0),
    (NEWID(), @Plan1, N'API Integration', N'Core ledger APIs integrated and documented.', 2500.00, DATEADD(DAY, 4, @Now), 3, NULL, @Now, @By, 0),
    (NEWID(), @Plan1, N'Payments & reports', N'Payouts flow and reporting screens complete.', 3000.00, DATEADD(DAY, 30, @Now), 4, NULL, @Now, @By, 0),
    (NEWID(), @Plan1, N'Launch polish', N'Perf budget met and production deploy.', 2500.00, DATEADD(DAY, 55, @Now), 5, NULL, @Now, @By, 0),
    (NEWID(), @Plan2, N'App foundation', N'Navigation and auth screens ready.', 1500.00, DATEADD(DAY, 10, @Now), 1, NULL, @Now, @By, 0),
    (NEWID(), @Plan2, N'Catalog & cart', N'Product catalog and cart flows work offline.', 2000.00, DATEADD(DAY, 30, @Now), 2, NULL, @Now, @By, 0),
    (NEWID(), @Plan2, N'Store release', N'Store builds submitted with release notes.', 1500.00, DATEADD(DAY, 50, @Now), 3, NULL, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Escrow                                                                      */
/* -------------------------------------------------------------------------- */

INSERT INTO [finance].[EscrowHolds]
    ([Id], [ProjectId], [TotalAmount], [TotalReleased], [FundingStatus], [planStatus],
     [LockedAt], [PlanAgreedAt], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    /* 2 released + 1 funded active = 7000 held total so far; 4500 released */
    (NEWID(), @P1, 7000.00, 4500.00, N'Locked', N'PlanAgreed',
     DATEADD(DAY, -34, @Now), DATEADD(DAY, -34, @Now), @Now, @By, 0),
    /* First milestone funded only */
    (NEWID(), @P2, 1500.00, 0.00, N'Locked', N'PlanAgreed',
     DATEADD(DAY, -14, @Now), DATEADD(DAY, -14, @Now), @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Live milestones                                                             */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[Milestones]
    ([Id], [ProjectId], [Title], [Description], [Amount], [ReleasedAmount], [SortOrder], [DueDate],
     [IsFunded], [ReleaseStatus], [WorkStatus], [ProposedByUserId], [SubmittedAt], [AvailableAt], [ReleasedAt],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    /* P1 — 2 done, current API Integration active, 2 queued */
    (@M11, @P1, N'Design system & shell',
     N'App shell, nav, and design tokens.', 2000.00, 2000.00, 1, DATEADD(DAY, -20, @Now),
     1, N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev1),
     DATEADD(DAY, -28, @Now), DATEADD(DAY, -26, @Now), DATEADD(DAY, -25, @Now), @Now, @By, 0),
    (@M12, @P1, N'Auth & roles',
     N'Login and role matrix.', 2500.00, 2500.00, 2, DATEADD(DAY, -5, @Now),
     1, N'Released', N'Approved', CONVERT(NVARCHAR(36), @Dev1),
     DATEADD(DAY, -12, @Now), DATEADD(DAY, -10, @Now), DATEADD(DAY, -9, @Now), @Now, @By, 0),
    (@M13, @P1, N'API Integration',
     N'Core ledger APIs integrated.', 2500.00, 0.00, 3, DATEADD(DAY, 4, @Now),
     1, N'Locked', N'InProgress', CONVERT(NVARCHAR(36), @Dev1),
     NULL, NULL, NULL, @Now, @By, 0),
    (@M14, @P1, N'Payments & reports',
     N'Payouts and reporting.', 3000.00, 0.00, 4, DATEADD(DAY, 30, @Now),
     0, N'Locked', N'NotStarted', CONVERT(NVARCHAR(36), @Dev1),
     NULL, NULL, NULL, @Now, @By, 0),
    (@M15, @P1, N'Launch polish',
     N'Perf and production deploy.', 2500.00, 0.00, 5, DATEADD(DAY, 55, @Now),
     0, N'Locked', N'NotStarted', CONVERT(NVARCHAR(36), @Dev1),
     NULL, NULL, NULL, @Now, @By, 0),

    /* P2 — funded active MS1; client can later fund next after release */
    (@M21, @P2, N'App foundation',
     N'Navigation and auth screens.', 1500.00, 0.00, 1, DATEADD(DAY, 10, @Now),
     1, N'Locked', N'InProgress', CONVERT(NVARCHAR(36), @Dev1),
     NULL, NULL, NULL, @Now, @By, 0),
    (@M22, @P2, N'Catalog & cart',
     N'Catalog and offline cart.', 2000.00, 0.00, 2, DATEADD(DAY, 30, @Now),
     0, N'Locked', N'NotStarted', CONVERT(NVARCHAR(36), @Dev1),
     NULL, NULL, NULL, @Now, @By, 0),
    (@M23, @P2, N'Store release',
     N'Store submission package.', 1500.00, 0.00, 3, DATEADD(DAY, 50, @Now),
     0, N'Locked', N'NotStarted', CONVERT(NVARCHAR(36), @Dev1),
     NULL, NULL, NULL, @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Project members                                                             */
/* -------------------------------------------------------------------------- */

INSERT INTO [marketplace].[ProjectMembers]
    ([Id], [ProjectId], [UserId], [RoleInProject], [AssignedByUserId], [AssignedAt],
     [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @P1, @Dev1, N'Solo Developer', @ClientId, DATEADD(DAY, -36, @Now), @Now, @By, 0),
    (NEWID(), @P2, @Dev1, N'Team Lead',      @ClientId, DATEADD(DAY, -16, @Now), @Now, @By, 0),
    (NEWID(), @P2, @Dev2, N'Frontend',       @ClientId, DATEADD(DAY, -16, @Now), @Now, @By, 0);

/* -------------------------------------------------------------------------- */
/* Project chat rooms (hire groups)                                            */
/* -------------------------------------------------------------------------- */

DECLARE @Cp3 UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id] FROM [identity].[ClientProfiles]
    WHERE [UserId] = @ClientId AND [IsDeleted] = 0
);
DECLARE @Dp1 UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id] FROM [identity].[DeveloperProfiles]
    WHERE [UserId] = @Dev1 AND [IsDeleted] = 0
);
DECLARE @Dp2 UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id] FROM [identity].[DeveloperProfiles]
    WHERE [UserId] = @Dev2 AND [IsDeleted] = 0
);
DECLARE @RoomP1 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000101';
DECLARE @RoomP2 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000102';
DECLARE @RoomProp3 UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000103';

IF @Cp3 IS NULL OR @Dp1 IS NULL
BEGIN
    SELECT N'profiles_missing_for_chat' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

UPDATE [marketplace].[ProjectProposals]
SET [Status] = N'Accepted',
    [ResponseAt] = ISNULL([ResponseAt], DATEADD(DAY, -34, @Now))
WHERE [Id] IN (@Prop1, @Prop2);

INSERT INTO [chat].[ChatRooms]
    ([Id], [RoomType], [Status], [TeamId], [ProjectId], [ProposalId], [SourceProposalRoomId],
     [CreatedByUserId], [Title], [Logo], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (@RoomP1, N'Project', N'Active', NULL, @P1, NULL, NULL,
     @ClientId, N'Lumina Finance Platform', NULL, DATEADD(DAY, -34, @Now), @By, 0),
    (@RoomP2, N'Project', N'Active', @Team1, @P2, NULL, NULL,
     @ClientId, N'Flutter Mobile App Development', NULL, DATEADD(DAY, -14, @Now), @By, 0),
    (@RoomProp3, N'Proposal', N'Active', @Team1, NULL, @Prop3, NULL,
     @Dev1, N'Proposal — React Admin Portal Refresh', NULL, DATEADD(DAY, -2, @Now), @By, 0);

INSERT INTO [chat].[ChatRoomMembers]
    ([Id], [ChatRoomId], [ClientProfileId], [DeveloperProfileId], [JoinedAt], [LastReadAt],
     [RoleLabel], [CanSend], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @RoomP1, @Cp3, NULL, DATEADD(DAY, -34, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomP1, NULL, @Dp1, DATEADD(DAY, -34, @Now), DATEADD(DAY, -1, @Now), N'Solo Developer', 1, @Now, @By, 0),
    (NEWID(), @RoomP2, @Cp3, NULL, DATEADD(DAY, -14, @Now), DATEADD(DAY, -1, @Now), N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomP2, NULL, @Dp1, DATEADD(DAY, -14, @Now), DATEADD(DAY, -1, @Now), N'Team Leader', 1, @Now, @By, 0),
    (NEWID(), @RoomProp3, @Cp3, NULL, DATEADD(DAY, -2, @Now), NULL, N'Client', 1, @Now, @By, 0),
    (NEWID(), @RoomProp3, NULL, @Dp1, DATEADD(DAY, -2, @Now), DATEADD(DAY, -1, @Now), N'Applicant', 1, @Now, @By, 0);

IF @Dp2 IS NOT NULL
BEGIN
    INSERT INTO [chat].[ChatRoomMembers]
        ([Id], [ChatRoomId], [ClientProfileId], [DeveloperProfileId], [JoinedAt], [LastReadAt],
         [RoleLabel], [CanSend], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @RoomP2, NULL, @Dp2, DATEADD(DAY, -14, @Now), DATEADD(DAY, -1, @Now), N'Member', 1, @Now, @By, 0);
END

INSERT INTO [chat].[Messages]
    ([Id], [ChatRoomId], [SenderClientProfileId], [SenderDeveloperProfileId], [MessageType],
     [Text], [FileUrl], [FileName], [PlanVersionId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES
    (NEWID(), @RoomP1, NULL, NULL, N'System',
     N'Project started|Lumina Finance Platform|Milestone plan agreed',
     NULL, NULL, NULL, DATEADD(DAY, -34, @Now), @By, 0),
    (NEWID(), @RoomP2, NULL, NULL, N'System',
     N'Project started|Flutter Mobile App Development|Milestone plan agreed · Team leaders can add working members',
     NULL, NULL, NULL, DATEADD(DAY, -14, @Now), @By, 0);

UPDATE [marketplace].[ProjectProposals]
SET [Status] = N'InDiscussion',
    [ResponseAt] = ISNULL([ResponseAt], DATEADD(DAY, -2, @Now))
WHERE [Id] = @Prop3;

/* -------------------------------------------------------------------------- */
/* Ensure Omar can fund next milestones                                        */
/* -------------------------------------------------------------------------- */

UPDATE [finance].[Wallets]
SET [Available] = CASE WHEN [Available] < 20000.00 THEN 20000.00 ELSE [Available] END,
    /* Seeded funded-but-unreleased milestones must have matching Reserved. */
    [Reserved] = CASE
        WHEN [Reserved] < 4000.00 THEN 4000.00
        ELSE [Reserved]
    END,
    [UpdatedAt] = @Now,
    [UpdatedBy] = @By
WHERE [OwnerType] = N'User'
  AND [OwnerUserId] = @ClientId
  AND [IsDeleted] = 0;

COMMIT TRANSACTION;

SELECT N'seeded_ok' AS Result,
       @P1 AS LuminaProjectId,
       @P2 AS FlutterProjectId,
       @P3 AS OpenProjectId,
       N'client3@freegency.local' AS ClientLogin,
       N'dev1@freegency.local' AS DeveloperLogin,
       N'Password123!' AS Password;
GO
