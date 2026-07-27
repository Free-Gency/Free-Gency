/*
================================================================================
 FreeGency — TEAM ONE-SHOT SEED (Client Home / Inspiration / My Projects)
================================================================================
 Safe to re-run (skips existing rows). Works on LOCAL and DEPLOYED — you only
 change the sqlcmd connection, not this file.

 WHAT THIS RUNS (in order):
   1) Inspiration portfolio projects (6 public items)
   2) Multi-image galleries + owner Reviews + rating refresh
   3) Filter / My Projects sample (16 projects + teams/proposals/covers)
   4) Enrich filter projects if needed

 PREREQUISITES:
   - Base demo users already exist (seed-demo-data.sql OR your env users)
   - Migrations applied (including RecentlyViewedPortfolios via Update-Database)
   - Frontend assets under /assets/client-home/ (covers)

 RECENTLY VIEWED TABLE:
   Do NOT create via SQL. Run EF:
     Update-Database
   (migration: AddRecentlyViewedPortfolios)

--------------------------------------------------------------------------------
 HOW TO RUN
--------------------------------------------------------------------------------
 From folder: Free-Gency/scripts

 LOCAL (Windows Integrated):
   sqlcmd -S . -d FreeGency_DB -E -i seed-team-client-home.sql

 DEPLOYED (DatabaseAsp example — replace password):
   sqlcmd -S db60614.public.databaseasp.net -d db60614 -U db60614 -P "YOUR_PASSWORD" -C -i seed-team-client-home.sql

 SSMS:
   1) Connect to local OR deployed
   2) Select the correct database
   3) Open this file and Enable SQLCMD Mode (Query > SQLCMD Mode)
   4) Execute

 DEMO LOGIN (My Projects / client home):
   client3@freegency.local  /  Password123!
================================================================================
*/

:on error exit
SET NOCOUNT ON;

PRINT N'========== [1/4] Inspiration portfolios ==========';
:r seed-inspiration-portfolio.sql

PRINT N'========== [2/4] Galleries + Reviews ==========';
:r seed-portfolio-details-enrich.sql

PRINT N'========== [3/4] Filter / My Projects ==========';
:r seed-filter-projects.sql

PRINT N'========== [4/4] Filter enrich (teams/proposals/covers) ==========';
:r seed-filter-projects-enrich.sql

PRINT N'========== DONE ==========';
PRINT N'Remember: RecentlyViewedPortfolios comes from EF Update-Database, not this script.';
PRINT N'Demo client: client3@freegency.local / Password123!';
GO
