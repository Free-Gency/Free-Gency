using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class BackfillAcceptedProposalsAndNotificationActionUrls : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            SET QUOTED_IDENTIFIER ON;

            -- 1) Proposals linked to an Accepted milestone plan → Accepted
            UPDATE pp
            SET
                pp.[Status] = N'Accepted',
                pp.[ResponseAt] = ISNULL(pp.[ResponseAt], SYSUTCDATETIME()),
                pp.[UpdatedAt] = SYSUTCDATETIME(),
                pp.[UpdatedBy] = N'system-backfill'
            FROM [marketplace].[ProjectProposals] AS pp
            INNER JOIN [marketplace].[MilestonePlanVersions] AS mpv
                ON mpv.[ProposalId] = pp.[Id]
               AND mpv.[Status] = N'Accepted'
               AND mpv.[IsDeleted] = 0
            WHERE pp.[IsDeleted] = 0
              AND pp.[Status] <> N'Accepted';

            -- 2) Team-assigned projects: matching team proposal → Accepted
            UPDATE pp
            SET
                pp.[Status] = N'Accepted',
                pp.[ResponseAt] = ISNULL(pp.[ResponseAt], SYSUTCDATETIME()),
                pp.[UpdatedAt] = SYSUTCDATETIME(),
                pp.[UpdatedBy] = N'system-backfill'
            FROM [marketplace].[ProjectProposals] AS pp
            INNER JOIN [marketplace].[Projects] AS p
                ON p.[Id] = pp.[ProjectId]
            WHERE pp.[IsDeleted] = 0
              AND p.[IsDeleted] = 0
              AND p.[AssignedTeamId] IS NOT NULL
              AND pp.[ApplicantType] = N'Team'
              AND pp.[TeamId] = p.[AssignedTeamId]
              AND pp.[Status] IN (N'Pending', N'Viewed', N'InDiscussion');

            -- 3) User-assigned projects: matching user proposal → Accepted
            UPDATE pp
            SET
                pp.[Status] = N'Accepted',
                pp.[ResponseAt] = ISNULL(pp.[ResponseAt], SYSUTCDATETIME()),
                pp.[UpdatedAt] = SYSUTCDATETIME(),
                pp.[UpdatedBy] = N'system-backfill'
            FROM [marketplace].[ProjectProposals] AS pp
            INNER JOIN [marketplace].[Projects] AS p
                ON p.[Id] = pp.[ProjectId]
            WHERE pp.[IsDeleted] = 0
              AND p.[IsDeleted] = 0
              AND p.[AssignedUserId] IS NOT NULL
              AND pp.[ApplicantType] = N'User'
              AND pp.[UserId] = p.[AssignedUserId]
              AND pp.[Status] IN (N'Pending', N'Viewed', N'InDiscussion');

            -- 4) On hired projects, reject every other still-open proposal
            UPDATE pp
            SET
                pp.[Status] = N'Rejected',
                pp.[RejectReason] = ISNULL(pp.[RejectReason], N'Client hired another candidate for this project'),
                pp.[ResponseAt] = ISNULL(pp.[ResponseAt], SYSUTCDATETIME()),
                pp.[UpdatedAt] = SYSUTCDATETIME(),
                pp.[UpdatedBy] = N'system-backfill'
            FROM [marketplace].[ProjectProposals] AS pp
            INNER JOIN [marketplace].[Projects] AS p
                ON p.[Id] = pp.[ProjectId]
            WHERE pp.[IsDeleted] = 0
              AND p.[IsDeleted] = 0
              AND (p.[AssignedUserId] IS NOT NULL OR p.[AssignedTeamId] IS NOT NULL)
              AND pp.[Status] IN (N'Pending', N'Viewed', N'InDiscussion');

            -- 5) Normalize notification ActionUrl values
            UPDATE [core].[Notifications]
            SET [ActionUrl] = CONCAT(N'/chat?room=', SUBSTRING([ActionUrl], 7, 36))
            WHERE [ActionUrl] LIKE N'/chat/%'
              AND [ActionUrl] NOT LIKE N'/chat?%';

            UPDATE [core].[Notifications]
            SET [ActionUrl] = CONCAT(N'/chat?room=', SUBSTRING([ActionUrl], CHARINDEX(N'/rooms/', [ActionUrl]) + 7, 36))
            WHERE [ActionUrl] LIKE N'%/api/v1/Chat/rooms/%';

            UPDATE [core].[Notifications]
            SET [ActionUrl] = CONCAT(N'/developer/teams/', SUBSTRING([ActionUrl], 8, 36), N'?tab=jobs')
            WHERE [ActionUrl] LIKE N'/teams/%/join-requests%';

            UPDATE [core].[Notifications]
            SET [ActionUrl] = CONCAT(N'/developer/teams/', SUBSTRING([ActionUrl], 8, 36))
            WHERE [ActionUrl] LIKE N'/teams/%'
              AND [ActionUrl] NOT LIKE N'/teams/%/join-requests%'
              AND [ActionUrl] NOT LIKE N'/developer/teams/%';

            UPDATE [core].[Notifications]
            SET [ActionUrl] = CONCAT(
                    N'/projects/',
                    SUBSTRING([ActionUrl], 11, 36),
                    N'?tab=proposals')
            WHERE [ActionUrl] LIKE N'/projects/%/proposals%';

            UPDATE [core].[Notifications]
            SET [ActionUrl] = CONCAT(
                    N'/projects/',
                    SUBSTRING([ActionUrl], 11, 36),
                    N'?tab=milestones')
            WHERE [ActionUrl] LIKE N'/projects/%/milestones%';
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Data backfill is intentionally not reversed.
    }
}
