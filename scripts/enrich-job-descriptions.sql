SET NOCOUNT ON;

UPDATE teams.TeamJobs
SET Description =
  N'We''re looking for a ' +
  CASE WHEN NULLIF(LTRIM(RTRIM(Title)), N'') IS NULL THEN N'teammate' ELSE LTRIM(RTRIM(Title)) END +
  N' to join our team on real client projects. You''ll collaborate closely, own your slice from idea to handoff, and ship quality work with clear communication. If you care about craft, teamwork, and growth - tell us about a project you''re proud of when you apply.',
  UpdatedAt = SYSUTCDATETIME()
WHERE Status = N'open'
  AND IsDeleted = 0
  AND (
    Description LIKE N'%What you''ll own%'
    OR Description LIKE N'%Who thrives here%'
    OR Description LIKE N'We''re hiring a %'
  );

SELECT @@ROWCOUNT AS UpdatedCount;
SELECT Title, LEN(Description) AS Len, Description
FROM teams.TeamJobs
WHERE Status = N'open' AND IsDeleted = 0;
