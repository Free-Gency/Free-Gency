# FreeGency — Team seed scripts (Client Home)

## One command for the whole team

Use **`seed-team-client-home.sql`** (SQLCMD orchestrator).

### Local

```bat
cd Free-Gency\scripts
sqlcmd -S . -d FreeGency_DB -E -i seed-team-client-home.sql
```

### Deployed (example)

```bat
cd Free-Gency\scripts
sqlcmd -S db60614.public.databaseasp.net -d db60614 -U db60614 -P "YOUR_PASSWORD" -C -i seed-team-client-home.sql
```

### SSMS

1. Connect to local **or** deployed
2. Select the database
3. **Query → SQLCMD Mode**
4. Open `seed-team-client-home.sql` → Execute

## What it seeds

| Step | Script | Content |
|------|--------|---------|
| 1 | `seed-inspiration-portfolio.sql` | 6 public Inspiration portfolios |
| 2 | `seed-portfolio-details-enrich.sql` | Gallery images + **Reviews** + ratings |
| 3 | `seed-filter-projects.sql` | 16 My Projects / filter samples |
| 4 | `seed-filter-projects-enrich.sql` | Teams / proposals / covers |

## Not in SQL (use EF) — or SQL fallback

**Recently Viewed** table — migration `AddRecentlyViewedPortfolios`

**Portfolio Feedbacks** (reviews on inspiration details) — migration `AddPortfolioFeedbacks`

```powershell
Update-Database
```

SQL fallback (either table):

```bat
sqlcmd -S . -d FreeGency_DB -E -i create-recently-viewed-portfolios.sql
sqlcmd -S . -d FreeGency_DB -E -i create-portfolio-feedbacks.sql
```

## Demo login

- Email: `client3@freegency.local`
- Password: `Password123!`

## Prerequisites

- `seed-demo-data.sql` (or equivalent users/teams/categories) already applied
- Frontend cover assets under `/assets/client-home/`
