# Free-gency — API Endpoints (MVP)

> Base: `/api/v1` · Header: `Authorization: Bearer {jwt}`  
> Sources: `Free-Gency-Backend/docs/DOMAIN-MODEL.md` · `API-ENDPOINTS.md` · `FEATURES-TABLES.md`  
> Out of scope: Contracts · real payment gateway (top-up = mock) · Disputes/Cancel · cold DMs

**Rules**

- One Client + one Developer profile per account · duplicate `POST` → `409`
- Session: `Users.ActiveProfileMode` = `Client` | `Developer` | `null`
- Social + Portfolio: under **Developer** or **Team** only
- `Projects` = marketplace · `PortfolioProjects` = showcase

---

## Controller map


| Controller | Feature |
| ---------- | ------- |
| `AuthController` | Auth & session |
| `ProfilesController` | Own Client / Developer profiles |
| `ClientsController` / `DevelopersController` | Public profiles |
| `TaxonomyController` | Categories · Specialties · Skills |
| `SocialLinksController` | Developer / Team social links |
| `PortfolioController` | Developer / Team portfolio |
| `TeamsController` | Teams · members · taxonomy |
| `TeamJobsController` | Team openings |
| `JoinRequestsController` | Join via job or team code |
| `ProjectsController` | Marketplace projects |
| `ProposalsController` | Apply · accept · reject |
| `WalletsController` | Wallet · top-up · ledger |
| `MilestonesController` | Plan · escrow · milestone actions |
| `PayoutSplitsController` | Team payout ratios |
| `ProjectMembersController` | Project roster |
| `TasksController` | Project tasks |
| `FilesController` | Project files |
| `ProjectEventsController` | Project timeline |
| `ReviewsController` | Post-completion reviews |
| `ChatController` | Rooms · messages · team groups |
| `NotificationsController` | In-app bell |
| `AiController` | Matching · moderation · index |
| `AdminController` | Admin panel |

---

## 1. AuthController — Auth & Session

| Method | Path | Notes |
| ------ | ---- | ----- |
| POST | `/auth/register` | User + wallet |
| POST | `/auth/login` | Tokens + session |
| POST | `/auth/refresh` | Rotate tokens |
| POST | `/auth/logout` | Revoke refresh |
| GET | `/auth/me` | Account + mode + active profile |
| POST | `/auth/switch-profile` | Body: `{ "mode": "Client" \| "Developer" }` |

---

## 2. ProfilesController — Own profiles

| Method | Path | Notes |
| ------ | ---- | ----- |
| POST | `/profiles/client` | Activate once |
| GET / PUT | `/profiles/client/me` | Read / update |
| POST | `/profiles/developer` | Activate once |
| GET / PUT | `/profiles/developer/me` | Read / update |
| PUT | `/profiles/developer/me/skills` | Replace skills |
| PUT | `/profiles/developer/me/interests` | Replace interests |

---

## 3. ClientsController / DevelopersController — Public profiles

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET | `/clients/{userId}` | Public client |
| GET | `/developers/{userId}` | Public developer (+ social/portfolio summary) |

---

## 4. TaxonomyController — Categories & Skills

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET | `/categories` | List |
| GET | `/categories/{id}` | Detail |
| GET | `/categories/{id}/specialties` | Under category |
| GET | `/specialties/{id}` | Detail |
| GET | `/skills` | List |
| GET | `/skills/search?q=` | Search picker |

---

## 5. SocialLinksController — Social links

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET / POST | `/profiles/developer/me/social-links` | Own |
| PUT / DELETE | `/profiles/developer/me/social-links/{linkId}` | Own |
| GET / POST | `/teams/{teamId}/social-links` | Leader |
| PUT / DELETE | `/teams/{teamId}/social-links/{linkId}` | Leader |

---

## 6. PortfolioController — Portfolio showcase

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET / POST | `/profiles/developer/me/portfolio-projects` | Own |
| GET / PUT / DELETE | `/profiles/developer/me/portfolio-projects/{id}` | Own |
| POST / DELETE | `.../portfolio-projects/{id}/images` · `.../images/{imageId}` | Own |
| PUT | `.../portfolio-projects/{id}/skills` | Own |
| GET | `/developers/{userId}/portfolio-projects` | Public |
| GET / POST | `/teams/{teamId}/portfolio-projects` | Leader CRUD |
| GET / PUT / DELETE | `/teams/{teamId}/portfolio-projects/{id}` | Leader |
| POST / DELETE | `.../images` · `.../images/{imageId}` | Leader |
| PUT | `.../portfolio-projects/{id}/skills` | Leader |

---

## 7. TeamsController — Teams & members

| Method | Path | Notes |
| ------ | ---- | ----- |
| POST | `/teams` | Create (+ wallet + TeamMain chat) |
| GET | `/teams` · `/teams/{id}` · `/teams/mine` | Browse / detail / mine |
| PUT | `/teams/{id}` | Update |
| PUT | `/teams/{id}/categories` · `/teams/{id}/skills` | Taxonomy |
| GET | `/teams/{id}/members` | Roster |
| DELETE | `/teams/{id}/members/{userId}` | Remove |
| PATCH | `/teams/{id}/members/{userId}/role` | Leader / Member |
| GET | `/teams/by-code/{teamCode}` | Lookup before join |

---

## 8. TeamJobsController — Team openings

| Method | Path | Notes |
| ------ | ---- | ----- |
| POST / GET | `/teams/{id}/jobs` | Create / list for team |
| GET | `/jobs` · `/jobs/{id}` | Public browse |
| PUT | `/jobs/{id}` · `/jobs/{id}/skills` | Update |
| POST | `/jobs/{id}/close` | Close opening |

---

## 9. JoinRequestsController — Join team

| Method | Path | Notes |
| ------ | ---- | ----- |
| POST | `/jobs/{id}/join-requests` | Apply via job |
| POST | `/teams/join-by-code` | Apply via code |
| GET | `/teams/{id}/join-requests` | Leader inbox |
| GET | `/join-requests/mine` | My requests |
| POST | `/join-requests/{id}/accept` · `/reject` | Leader decides |

---

## 10. ProjectsController — Marketplace

`Status:` `Draft` → `Open` → `InProgress` → `Completed`

| Method | Path | Notes |
| ------ | ---- | ----- |
| POST | `/projects` | Draft (Client) |
| GET | `/projects` · `/projects/{id}` | Browse / detail |
| PUT / DELETE | `/projects/{id}` | Edit / delete draft |
| POST | `/projects/{id}/publish` | Draft → Open |
| GET | `/projects/mine/as-client` · `/as-assignee` | My lists |
| PUT | `/projects/{id}/skills` | Replace skills |
| POST / DELETE | `/projects/{id}/save` | Bookmark |
| GET | `/projects/saved` | Saved list |

---

## 11. ProposalsController — Proposals

`Status:` `Pending` → `Accepted` | `Rejected` · `ApplicantType:` `User` | `Team`

| Method | Path | Notes |
| ------ | ---- | ----- |
| POST | `/projects/{id}/proposals` | Submit (+ Proposal chat) |
| GET | `/projects/{id}/proposals` · `/proposals/{id}` · `/proposals/mine` | Lists |
| POST | `/proposals/{id}/accept` | Assignee + EscrowHold `Unlocked` + Project chat |
| POST | `/proposals/{id}/reject` | Reject |
| POST / DELETE | `/proposals/{id}/attachments` · `.../{aid}` | Files |

---

## 12. WalletsController — Wallets

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET | `/wallets/me` · `/wallets/me/ledger` | User wallet |
| POST | `/wallets/me/top-up` | Mock top-up |
| GET | `/teams/{teamId}/wallet` · `.../ledger` | Team wallet |

---

## 13. MilestonesController — Plan & escrow

**Money moves only on Agree (lock) and Approve (release).**

| Method | Path | Notes |
| ------ | ---- | ----- |
| POST / PUT / GET | `/projects/{id}/milestone-plan` | Propose / revise / read |
| POST | `/projects/{id}/milestone-plan/agree` | Lock escrow (client pay) |
| POST | `/projects/{id}/milestone-plan/request-changes` | Client revision |
| GET | `/projects/{id}/milestones` · `/milestones/{id}` | List / detail |
| GET | `/projects/{id}/escrow` | EscrowHold summary |
| POST | `/milestones/{id}/start` | Work → InProgress |
| POST | `/milestones/{id}/submit` | Work → Submitted |
| POST | `/milestones/{id}/approve` | Release funds |
| POST | `/milestones/{id}/request-changes` | Reject submission |

---

## 14. PayoutSplitsController — Team splits

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET / PUT | `/teams/{teamId}/payout-splits` | Default team ratios |
| GET / PUT | `/projects/{id}/payout-splits` | Per-project override |

Ratios only — applied on milestone release, not a transfer API.

---

## 15. ProjectMembersController — Roster

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET / POST | `/projects/{id}/members` | List / add |
| PUT / DELETE | `/projects/{id}/members/{userId}` | Update / remove |

---

## 16. TasksController — Tasks

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET / POST | `/projects/{id}/tasks` | List / create |
| GET / PUT / DELETE | `/tasks/{id}` | Detail / edit / delete |
| PATCH | `/tasks/{id}/status` | Status only |
| GET | `/tasks/mine` | Assigned to me |

---

## 17. FilesController — Project files

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET / POST | `/projects/{id}/files` | List / upload |
| DELETE | `/files/{id}` | Remove |

`FileKind:` `Brief` | `Deliverable` | `Shared`

---

## 18. ProjectEventsController — Timeline

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET | `/projects/{id}/events` | Read-only event log |

---

## 19. ReviewsController — Reviews

Only when `Projects.Status = Completed`.

| Method | Path | Notes |
| ------ | ---- | ----- |
| POST / GET | `/projects/{id}/reviews` | Leave / list for project |
| GET | `/clients/{id}/reviews` | Public |
| GET | `/developers/{id}/reviews` | Public |
| GET | `/teams/{id}/reviews` | Public |

---

## 20. ChatController — Chat

Auto rooms: Team create → `TeamMain` · Proposal submit → `Proposal` · Accept → `Project`  
No cold DMs in MVP.

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET | `/chat/rooms` · `/chat/rooms/{id}` | My rooms / detail |
| GET | `/chat/rooms/by-project/{id}` · `/by-proposal/{id}` · `/by-team/{id}` | Lookup |
| GET / POST | `/chat/rooms/{id}/messages` | History / send |
| POST | `/chat/rooms/{id}/read` | Mark read |
| POST | `/teams/{id}/chat-groups` | Leader creates TeamGroup |
| PUT / DELETE | `/teams/{id}/chat-groups/{groupId}` | Update / delete |
| POST / DELETE | `/teams/{id}/chat-groups/{groupId}/members` | Manage members |

---

## 21. NotificationsController — Notifications

Inbox is **profile-scoped** (active `ClientProfile` or `DeveloperProfile`), same pattern as chat — not `UserId`.

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET | `/notifications` · `/notifications/unread-count` | Inbox for active profile |
| POST | `/notifications/{id}/read` · `/notifications/read-all` | Mark read |
| DELETE | `/notifications/{id}` | Dismiss |

---

## 22. AiController — AI

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET | `/ai/health` | Health |
| POST | `/ai/match/projects` · `/teams` · `/team-jobs` | Matching |
| POST | `/ai/index/projects/{id}` · `/ai/index/rebuild` | Index |
| POST | `/ai/moderate-text` | Chat gate |

---

## 23. AdminController — Admin

| Method | Path | Notes |
| ------ | ---- | ----- |
| GET | `/admin/users` · `/admin/projects` | Lists |
| PATCH | `/admin/users/{id}/role` | `user` / `admin` |
| POST | `/admin/ai/index/rebuild` | Force reindex |

---

## Happy paths (short)

**Client:** `register` → `login` → `POST /profiles/client` → top-up → `POST /projects` → publish → accept proposal → agree plan → approve milestones → reviews

**Developer solo:** `POST /profiles/developer` → switch → portfolio → propose → submit milestones → wallet

**Team:** `POST /teams` → payout-splits → propose as Team → plan → release → team wallet → member splits

**Switch:** `GET /auth/me` → `POST /auth/switch-profile`

---

## Status cheat-sheet


| Entity | Field | Values |
| ------ | ----- | ------ |
| User session | `ActiveProfileMode` | `Client` \| `Developer` \| `null` |
| User | `Role` | `user` \| `admin` |
| Project | `Status` | `Draft` → `Open` → `InProgress` → `Completed` |
| Proposal | `Status` | `Pending` \| `Accepted` \| `Rejected` |
| Proposal | `ApplicantType` | `User` \| `Team` |
| EscrowHold | `FundingStatus` | `Unlocked` → `Locked` → `Completed` |
| EscrowHold | `PlanStatus` | `AwaitingPlan` → `PlanSubmitted` ↔ `PlanRevisionRequested` → `PlanAgreed` |
| Milestone | `WorkStatus` | `NotStarted` → `InProgress` → `Submitted` → `Approved` |
| Milestone | `ReleaseStatus` | `Locked` → `InReview` → `Pending` → `Released` |
| TeamMembers | `TeamRole` | `Leader` \| `Member` |
| ChatRoom | `RoomType` | `TeamMain` \| `TeamGroup` \| `Proposal` \| `Project` |

---

*Jul 2026 · Concise controller map for Free-Gency (.NET). Full money/SQL detail remains in `Free-Gency-Backend/docs/API-ENDPOINTS.md` §9.*
