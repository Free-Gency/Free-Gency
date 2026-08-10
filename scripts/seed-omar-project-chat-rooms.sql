/*
================================================================================
 FreeGency — Omar project chat rooms (Project groups)
================================================================================
 Creates chat.ChatRooms (RoomType = Project) + members for every InProgress
 project owned by Omar Nabil (client3) that is missing a Project room.

 Also opens a Proposal room for Omar's Open "React Admin Portal Refresh" demo
 proposal when missing.

 Idempotent.

 Demo login: client3@freegency.local / Password123!
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

DECLARE @ClientId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003'; /* Omar */
DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @By  NVARCHAR(256) = N'seed-omar-project-chats';

DECLARE @Cp3 UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id]
    FROM [identity].[ClientProfiles]
    WHERE [UserId] = @ClientId AND [IsDeleted] = 0
);

IF @Cp3 IS NULL
BEGIN
    SELECT N'omar_client_profile_missing' AS Result;
    ROLLBACK TRANSACTION;
    RETURN;
END;

DECLARE @CreatedRooms INT = 0;

/* -------------------------------------------------------------------------- */
/* Project rooms for all Omar InProgress projects without one                  */
/* -------------------------------------------------------------------------- */

DECLARE @ProjectId UNIQUEIDENTIFIER;
DECLARE @Title NVARCHAR(300);
DECLARE @TeamId UNIQUEIDENTIFIER;
DECLARE @AssignedUserId UNIQUEIDENTIFIER;
DECLARE @RoomId UNIQUEIDENTIFIER;

DECLARE project_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT p.[Id], p.[Title], p.[AssignedTeamId], p.[AssignedUserId]
    FROM [marketplace].[Projects] p
    WHERE p.[ClientId] = @ClientId
      AND p.[IsDeleted] = 0
      AND p.[Status] = N'InProgress'
      AND NOT EXISTS (
          SELECT 1
          FROM [chat].[ChatRooms] r
          WHERE r.[ProjectId] = p.[Id]
            AND r.[RoomType] = N'Project'
            AND r.[IsDeleted] = 0
      );

OPEN project_cursor;
FETCH NEXT FROM project_cursor INTO @ProjectId, @Title, @TeamId, @AssignedUserId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @RoomId = NEWID();

    /* Free unique IX_ChatRooms_ProjectId — Proposal negotiation rooms must release it. */
    UPDATE [chat].[ChatRooms]
    SET [ProjectId] = NULL,
        [Status] = CASE WHEN [RoomType] = N'Proposal' THEN N'Archived' ELSE [Status] END,
        [ArchivedAt] = CASE WHEN [RoomType] = N'Proposal' THEN ISNULL([ArchivedAt], @Now) ELSE [ArchivedAt] END,
        [UpdatedAt] = @Now,
        [UpdatedBy] = @By
    WHERE [ProjectId] = @ProjectId
      AND [IsDeleted] = 0
      AND [RoomType] <> N'Project';

    INSERT INTO [chat].[ChatRooms]
        ([Id], [RoomType], [Status], [TeamId], [ProjectId], [ProposalId], [SourceProposalRoomId],
         [CreatedByUserId], [Title], [Logo], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (@RoomId, N'Project', N'Active', @TeamId, @ProjectId, NULL, NULL,
         @ClientId, @Title, NULL, @Now, @By, 0);

    /* Client always in the room */
    INSERT INTO [chat].[ChatRoomMembers]
        ([Id], [ChatRoomId], [ClientProfileId], [DeveloperProfileId], [JoinedAt], [LastReadAt],
         [RoleLabel], [CanSend], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @RoomId, @Cp3, NULL, @Now, @Now, N'Client', 1, @Now, @By, 0);

    /* Solo assignee */
    IF @AssignedUserId IS NOT NULL
    BEGIN
        DECLARE @SoloDp UNIQUEIDENTIFIER = (
            SELECT TOP 1 [Id]
            FROM [identity].[DeveloperProfiles]
            WHERE [UserId] = @AssignedUserId AND [IsDeleted] = 0
        );

        IF @SoloDp IS NOT NULL
        AND NOT EXISTS (
            SELECT 1 FROM [chat].[ChatRoomMembers]
            WHERE [ChatRoomId] = @RoomId AND [DeveloperProfileId] = @SoloDp AND [IsDeleted] = 0
        )
        BEGIN
            INSERT INTO [chat].[ChatRoomMembers]
                ([Id], [ChatRoomId], [ClientProfileId], [DeveloperProfileId], [JoinedAt], [LastReadAt],
                 [RoleLabel], [CanSend], [CreatedAt], [CreatedBy], [IsDeleted])
            VALUES
                (NEWID(), @RoomId, NULL, @SoloDp, @Now, @Now, N'Solo Developer', 1, @Now, @By, 0);
        END
    END

    /* Team leaders + project members */
    IF @TeamId IS NOT NULL
    BEGIN
        INSERT INTO [chat].[ChatRoomMembers]
            ([Id], [ChatRoomId], [ClientProfileId], [DeveloperProfileId], [JoinedAt], [LastReadAt],
             [RoleLabel], [CanSend], [CreatedAt], [CreatedBy], [IsDeleted])
        SELECT
            NEWID(),
            @RoomId,
            NULL,
            dp.[Id],
            @Now,
            @Now,
            CASE WHEN tm.[TeamRole] IN (N'Leader', N'TeamLeader', N'Team Leader') THEN N'Team Leader' ELSE N'Member' END,
            1,
            @Now,
            @By,
            0
        FROM [teams].[TeamMembers] tm
        INNER JOIN [identity].[DeveloperProfiles] dp
            ON dp.[UserId] = tm.[UserId] AND dp.[IsDeleted] = 0
        WHERE tm.[TeamId] = @TeamId
          AND tm.[IsDeleted] = 0
          AND NOT EXISTS (
              SELECT 1
              FROM [chat].[ChatRoomMembers] m
              WHERE m.[ChatRoomId] = @RoomId
                AND m.[DeveloperProfileId] = dp.[Id]
                AND m.[IsDeleted] = 0
          );

        INSERT INTO [chat].[ChatRoomMembers]
            ([Id], [ChatRoomId], [ClientProfileId], [DeveloperProfileId], [JoinedAt], [LastReadAt],
             [RoleLabel], [CanSend], [CreatedAt], [CreatedBy], [IsDeleted])
        SELECT
            NEWID(),
            @RoomId,
            NULL,
            dp.[Id],
            @Now,
            @Now,
            ISNULL(pm.[RoleInProject], N'Member'),
            1,
            @Now,
            @By,
            0
        FROM [marketplace].[ProjectMembers] pm
        INNER JOIN [identity].[DeveloperProfiles] dp
            ON dp.[UserId] = pm.[UserId] AND dp.[IsDeleted] = 0
        WHERE pm.[ProjectId] = @ProjectId
          AND pm.[IsDeleted] = 0
          AND NOT EXISTS (
              SELECT 1
              FROM [chat].[ChatRoomMembers] m
              WHERE m.[ChatRoomId] = @RoomId
                AND m.[DeveloperProfileId] = dp.[Id]
                AND m.[IsDeleted] = 0
          );
    END

    INSERT INTO [chat].[Messages]
        ([Id], [ChatRoomId], [SenderClientProfileId], [SenderDeveloperProfileId], [MessageType],
         [Text], [FileUrl], [FileName], [PlanVersionId], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @RoomId, NULL, NULL, N'System',
         N'Project started|' + @Title + N'|Milestone plan agreed',
         NULL, NULL, NULL, @Now, @By, 0);

    SET @CreatedRooms = @CreatedRooms + 1;

    FETCH NEXT FROM project_cursor INTO @ProjectId, @Title, @TeamId, @AssignedUserId;
END

CLOSE project_cursor;
DEALLOCATE project_cursor;

/* Mark Omar hired demo proposals Accepted when project is InProgress */
UPDATE pp
SET [Status] = N'Accepted',
    [ResponseAt] = ISNULL(pp.[ResponseAt], @Now),
    [UpdatedAt] = @Now,
    [UpdatedBy] = @By
FROM [marketplace].[ProjectProposals] pp
INNER JOIN [marketplace].[Projects] p ON p.[Id] = pp.[ProjectId]
WHERE p.[ClientId] = @ClientId
  AND p.[Status] = N'InProgress'
  AND pp.[IsDeleted] = 0
  AND pp.[Status] IN (N'InDiscussion', N'Pending');

/* -------------------------------------------------------------------------- */
/* Proposal room for Open Admin Portal demo (a555…000003)                      */
/* -------------------------------------------------------------------------- */

DECLARE @OpenProjectId UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000003';
DECLARE @OpenPropId UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000013';
DECLARE @Dev1 UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000011';
DECLARE @Dp1 UNIQUEIDENTIFIER = (
    SELECT TOP 1 [Id]
    FROM [identity].[DeveloperProfiles]
    WHERE [UserId] = @Dev1 AND [IsDeleted] = 0
);
DECLARE @OpenTeamId UNIQUEIDENTIFIER = (
    SELECT TOP 1 pp.[TeamId]
    FROM [marketplace].[ProjectProposals] pp
    WHERE pp.[Id] = @OpenPropId AND pp.[IsDeleted] = 0
);

IF EXISTS (SELECT 1 FROM [marketplace].[Projects] WHERE [Id] = @OpenProjectId AND [IsDeleted] = 0)
AND @Dp1 IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM [chat].[ChatRooms]
    WHERE [ProposalId] = @OpenPropId AND [RoomType] = N'Proposal' AND [IsDeleted] = 0
)
BEGIN
    DECLARE @RoomProp UNIQUEIDENTIFIER = 'a5555555-a555-a555-a555-000000000103';

    INSERT INTO [chat].[ChatRooms]
        ([Id], [RoomType], [Status], [TeamId], [ProjectId], [ProposalId], [SourceProposalRoomId],
         [CreatedByUserId], [Title], [Logo], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (@RoomProp, N'Proposal', N'Active', @OpenTeamId, NULL, @OpenPropId, NULL,
         @Dev1, N'Proposal — React Admin Portal Refresh', NULL, @Now, @By, 0);

    INSERT INTO [chat].[ChatRoomMembers]
        ([Id], [ChatRoomId], [ClientProfileId], [DeveloperProfileId], [JoinedAt], [LastReadAt],
         [RoleLabel], [CanSend], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES
        (NEWID(), @RoomProp, @Cp3, NULL, @Now, NULL, N'Client', 1, @Now, @By, 0),
        (NEWID(), @RoomProp, NULL, @Dp1, @Now, @Now, N'Applicant', 1, @Now, @By, 0);

    UPDATE [marketplace].[ProjectProposals]
    SET [Status] = N'InDiscussion',
        [ResponseAt] = ISNULL([ResponseAt], @Now),
        [UpdatedAt] = @Now,
        [UpdatedBy] = @By
    WHERE [Id] = @OpenPropId
      AND [IsDeleted] = 0
      AND [Status] = N'Pending';

    SET @CreatedRooms = @CreatedRooms + 1;
END;

COMMIT TRANSACTION;

SELECT N'seeded_omar_chats_ok' AS Result,
       @CreatedRooms AS RoomsCreatedThisRun,
       (
           SELECT COUNT(*)
           FROM [marketplace].[Projects] p
           INNER JOIN [chat].[ChatRooms] r
               ON r.[ProjectId] = p.[Id] AND r.[RoomType] = N'Project' AND r.[IsDeleted] = 0
           WHERE p.[ClientId] = @ClientId
             AND p.[Status] = N'InProgress'
             AND p.[IsDeleted] = 0
       ) AS OmarInProgressProjectRooms;
GO
