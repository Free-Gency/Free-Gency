# Milestones — Endpoints Status

> Source of truth for **implemented** routes: `MilestonesController` + related controllers
> Compared against: `docs/API-ENDPOINTS.md` §13–14 and `DOMAIN-MODEL.md` §5.6–5.8
> Last checked: Aug 2026

**Base path:** `/api/v1`
**Auth:** all endpoints require `[Authorize]`

### Product rules (MVP) — decided


| Topic              | Decision                                                                                              |
| ------------------ | ----------------------------------------------------------------------------------------------------- |
| **Money**          | **Fund milestone-by-milestone** via `fund-next`. **No** full-project escrow lock.                     |
| **Start**          | No separate `start` endpoint — **fund = fund + start work** (`IsFunded` + `WorkStatus = InProgress`). |
| **Plan**           | Client: `Request changes` **or** `Accept / Agree` (hire).                                             |
| **Submitted work** | Client: `Request work changes` **or** `Approve & release`.                                            |
| **Disputes**       | `Disputed` / `Refunded` → **Phase 2**.                                                                |


---



## Legend


| Mark | Meaning                                                  |
| ---- | -------------------------------------------------------- |
| ✅    | Implemented and callable                                 |
| ⚠️   | Backend exists, but Frontend UI incomplete / not wired   |
| ❌    | Not implemented (or only mentioned in old docs / domain) |
| 🔀   | Exists under a **different path/name** than the docs     |
| ⏸️   | Deferred to **Phase 2**                                  |


---



## 1. Working endpoints (Backend ✅)



### 1.1 Plan negotiation


| Method | Path                                           | Who                  | Purpose                    | Frontend                    |
| ------ | ---------------------------------------------- | -------------------- | -------------------------- | --------------------------- |
| `GET`  | `/projects/{projectId}/milestone-plans`        | Client / Developer   | List all plan versions     | ✅ Project milestones + Chat |
| `GET`  | `/projects/{projectId}/milestone-plans/latest` | Client / Developer   | Latest plan version        | ✅ Project milestones + Chat |
| `POST` | `/milestone-plans`                             | Developer (assignee) | Propose / revise full plan | ✅ Chat discussion modal     |
| `POST` | `/milestone-plans/request-changes`             | Client               | Request plan revisions     | ✅ Project milestones + Chat |
| `POST` | `/milestone-plans/{planVersionId}/accept`      | Client               | Accept plan = **Hire**     | ✅ Project milestones + Chat |




### 1.2 Milestones list & money actions


| Method | Path                                             | Who                | Purpose                                                                | Frontend                                     |
| ------ | ------------------------------------------------ | ------------------ | ---------------------------------------------------------------------- | -------------------------------------------- |
| `GET`  | `/projects/{projectId}/milestones`               | Client / Developer | List project milestones                                                | ✅ Project milestones tab                     |
| `GET`  | `/milestones/mine`                               | Developer          | Developer’s milestones across projects                                 | ✅ My Milestones                              |
| `POST` | `/projects/{projectId}/milestones/fund-next`     | Client             | Fund **next** MS only (+ starts work). One funded-unreleased at a time | ✅ Project milestones                         |
| `POST` | `/milestones/{milestoneId}/submit`               | Developer          | Submit work for review                                                 | ✅ My Milestones                              |
| `POST` | `/milestones/{milestoneId}/approve-release`      | Client             | Approve + release funds                                                | ✅ Project milestones                         |
| `POST` | `/milestones/{milestoneId}/request-work-changes` | Client             | Reject submission → `ChangesRequested`                                 | ⚠️ API + FE service exist — **no UI button** |




### 1.3 Related (used by milestone flow)


| Method   | Path                              | Who                | Purpose                                     | Frontend                                             |
| -------- | --------------------------------- | ------------------ | ------------------------------------------- | ---------------------------------------------------- |
| `GET`    | `/projects/{projectId}/escrow`    | Client / Developer | EscrowHold summary                          | ✅ Project milestones                                 |
| `GET`    | `/projects/{projectId}/files`     | Both               | List project files (`?kind=`)               | ✅ Files tab (project-level)                          |
| `POST`   | `/projects/{projectId}/files`     | Both               | Upload (`fileKind`, optional `milestoneId`) | ⚠️ Upload works — `milestoneId` **not sent from UI** |
| `DELETE` | `/files/{id}`                     | Both               | Delete file                                 | ✅ Files tab                                          |
| `GET`    | `/projects/{projectId}/events`    | Both               | Project timeline                            | ✅ Events UI (events often empty for MS actions)      |
| `GET`    | `/milestones/{milestoneId}/tasks` | Developer          | Tasks under a milestone                     | ✅ Tasks feature                                      |
| `POST`   | `/milestones/{milestoneId}/tasks` | Developer          | Create task on milestone                    | ✅ Tasks feature                                      |
| `GET`    | `/projects/{projectId}/tasks`     | Both               | All project tasks                           | ✅                                                    |
| `GET`    | `/tasks/mine`                     | Developer          | My assigned tasks                           | ✅                                                    |


---



## 2. Documented in `API-ENDPOINTS.md` but missing / renamed

Old docs §13 paths ≠ current controller. Map:


| Docs path (old)                                      | Status | Actual / notes                                                                              |
| ---------------------------------------------------- | ------ | ------------------------------------------------------------------------------------------- |
| `POST/PUT/GET /projects/{id}/milestone-plan`         | 🔀     | Use `/milestone-plans`, `/milestone-plans/latest`, `GET …/milestone-plans`                  |
| `POST /projects/{id}/milestone-plan/agree`           | 🔀     | Use `POST /milestone-plans/{planVersionId}/accept` — hire only; money via later `fund-next` |
| `POST /projects/{id}/milestone-plan/request-changes` | 🔀     | Use `POST /milestone-plans/request-changes`                                                 |
| `GET /milestones/{id}` (detail)                      | ✅      | `GET /milestones/{milestoneId}`                                                     |
| `POST /milestones/{id}/start`                        | —      | **Not needed** — covered by `fund-next`                                             |
| `POST /milestones/{id}/approve`                      | 🔀     | Use `POST /milestones/{id}/approve-release`                                         |
| `POST /milestones/{id}/request-changes`              | 🔀     | Use `POST /milestones/{id}/request-work-changes`                                    |
| `GET/PUT /teams/{teamId}/payout-splits`              | ✅      | See **§7** — `PayoutSplitsController`                                               |
| `GET/PUT /projects/{id}/payout-splits`               | ✅      | See **§7** — applied on release when configured                                     |


---



## 3. Still missing for MVP (no disputes, no full lock)

Money model is **settled**: progressive `fund-next` only. Gaps below are UI / polish / later features.


| Expected capability                          | Notes                                             | Status             |
| -------------------------------------------- | ------------------------------------------------- | ------------------ |
| Wire **Request work changes** UI             | API: `POST /milestones/{id}/request-work-changes` | ⚠️ FE gap          |
| Upload/list files with `milestoneId` from UI | backend accepts `milestoneId`; UI doesn’t send it | ⚠️ FE gap          |
| Pending hold → Available                     | ledger Pending then Available                     | ❌ Optional / later |

Backend done: milestone detail · payout splits · ProjectEvents · wallet profile notify.  
~~Full project escrow lock~~ — **cancelled**.

### Phase 2 — Disputes (out of MVP)


| Capability                                      | Suggested endpoint                      | Status     |
| ----------------------------------------------- | --------------------------------------- | ---------- |
| Dispute milestone slice                         | `POST /milestones/{id}/dispute`         | ⏸️ Phase 2 |
| Resolve dispute → release or refund             | `POST /milestones/{id}/resolve-dispute` | ⏸️ Phase 2 |
| Refund to client / `ReleaseStatus.Refunded`     | ledger + escrow update                  | ⏸️ Phase 2 |
| Whether dispute freezes one MS vs whole project | product policy                          | ⏸️ Phase 2 |


**Also deferred (not Phase-2 disputes, still later):**

- Client pre-draft milestones on project create
- Licensed escrow / real PSP gateway

---



## 4. Quick matrix — MVP client = Request changes **or** Agree


| Action                                | Endpoint                                     | Backend | Frontend | Scope                         |
| ------------------------------------- | -------------------------------------------- | ------- | -------- | ----------------------------- |
| Propose plan                          | `POST /milestone-plans`                      | ✅       | ✅ Chat   | MVP                           |
| List plans                            | `GET …/milestone-plans` (+ `/latest`)        | ✅       | ✅        | MVP                           |
| **Request plan changes**              | `POST /milestone-plans/request-changes`      | ✅       | ✅        | MVP                           |
| **Accept / Agree plan**               | `POST /milestone-plans/{id}/accept`          | ✅       | ✅        | MVP                           |
| **Fund next MS** (= fund + start)     | `POST …/milestones/fund-next`                | ✅       | ✅        | MVP                           |
| List project MS                       | `GET …/milestones`                           | ✅       | ✅        | MVP                           |
| My milestones                         | `GET /milestones/mine`                       | ✅       | ✅        | MVP                           |
| Submit work                           | `POST /milestones/{id}/submit`               | ✅       | ✅        | MVP                           |
| **Approve & release** (agree on work) | `POST /milestones/{id}/approve-release`      | ✅       | ✅        | MVP                           |
| **Request work changes**              | `POST /milestones/{id}/request-work-changes` | ✅       | ⚠️ No UI | MVP                           |
| Escrow summary                        | `GET …/escrow`                               | ✅       | ✅        | MVP                           |
| Milestone detail                      | `GET /milestones/{id}`                       | ✅       | ❌        | MVP (BE done)                 |
| Separate `start`                      | —                                            | —       | —        | **Not used** (fund covers it) |
| Full budget escrow lock               | —                                            | —       | —        | **Cancelled**                 |
| Payout splits API                     | see §7                                       | ✅       | ❌        | MVP (BE done; FE later)       |
| **Dispute / Refund**                  | `…/dispute` · resolve                        | —       | —        | ⏸️ **Phase 2**                |


---



## 5. Happy path (MVP)

```
Discussion started
  → POST /milestone-plans                         (Developer)
  → POST /milestone-plans/request-changes          (Client)  ─┐ loop until agreed
  → POST /milestone-plans/{id}/accept              (Client Agree = Hire)
  → POST …/milestones/fund-next                   (Client funds MS #1 AND starts work)
  → work + optional tasks under milestone
  → POST /milestones/{id}/submit                   (Developer)
  → Client chooses ONE:
       • POST /milestones/{id}/approve-release     (Agree → release)
       • POST /milestones/{id}/request-work-changes (Request changes → FE still missing)
  → fund-next (MS #2) → submit → (request changes | approve) … (repeat per milestone)
  → Auto-release worker after 14 days if client silent

No full-project lock. No separate start. Phase 2: disputes only.
```

---



## 6. Files to update when implementing gaps

- `FreeGency.Api/Controllers/V1/MilestonesController.cs`
- `FreeGency.Application/Features/Milestones/**`
- `docs/API-ENDPOINTS.md` §13–14 (still lists old paths)
- Frontend: `project-milestones.component.*` (wire **Request work changes**), `my-milestones.component.`*, `project-files.component.*`

---



## 7. Payout splits API — **implemented**

### Endpoints

| Method | Path | Who | Purpose |
| ------ | ---- | --- | ------- |
| `GET` | `/teams/{teamId}/payout-splits` | Team Owner / Leader | Read **default** team splits |
| `PUT` | `/teams/{teamId}/payout-splits` | Team Owner / Leader (Developer mode) | Replace defaults (**Percent only**) |
| `GET` | `/projects/{projectId}/payout-splits` | Team Owner / Leader | Project override, else fallback to team defaults |
| `PUT` | `/projects/{projectId}/payout-splits` | Team Owner / Leader (Developer mode) | Replace project override (Percent or Fixed vs budget) |

Body for `PUT`:

```json
{
  "splitType": "Percent",
  "items": [
    { "userId": "…", "value": 40 },
    { "userId": "…", "value": 60 }
  ]
}
```

### On milestone release

```
Approve / auto-release (team project)
  → project override splits, else team defaults
  → valid? credit each member User wallet + TeamSplit ledger
  → else 100% Team wallet + EscrowRelease (legacy path)
Solo assignee → EscrowRelease to user wallet (unchanged)
```

### Status checklist

| Piece | Status |
| ----- | ------ |
| Entity + EF + repo | ✅ |
| `PayoutSplitsController` + `PayoutSplitService` | ✅ |
| Apply splits inside `approve-release` / auto-release | ✅ |
| Frontend (team settings + project override UI) | ❌ |
| Solo assignee path | ✅ |

---

## 8. Profile scoping — ClientProfile vs DeveloperProfile

> Several milestone-related requests need to bind the **correct profile**, not just `UserId`.  
> Same account can have both profiles; inbox / chat / sender stamp must match the **role of the action**.

### Intended mapping

| Action / concern | Must use | Why |
| ---------------- | -------- | --- |
| Own the project / fund / accept plan / approve work / request (plan or work) changes | **Client** side: auth as project owner + notify/chat as **ClientProfile** | Client role |
| Propose / revise plan / submit milestone | **Developer** side: auth as assignee/leader + notify/chat as **DeveloperProfile** | Developer role |
| Notifications to client (plan proposed, MS submitted, …) | `ClientProfileId` | Shows in client inbox only |
| Notifications to assignee (plan changes, funded, released, …) | `DeveloperProfileId` | Shows in developer inbox only |
| Chat room members (Project room after hire) | ClientProfile + DeveloperProfile(s) | Already done on accept |
| Money (wallet / escrow / `ClientId` / `AssignedUserId`) | **UserId / TeamId** (account level) | OK — wallets are not per-profile |

`Project.ClientId` stays FK → **User.Id** (ownership). Profile ids are for **identity in chat + notifications + active-mode gates**.

### What is already OK

| Place | Behavior |
| ----- | -------- |
| `ProposePlanAsync` notify | → `ClientProfileId` |
| `RequestPlanChangesAsync` notify | → `DeveloperProfileId` (proposer) |
| `SubmitMilestoneAsync` notify | → `ClientProfileId` |
| `FundNext` / `ApproveAndRelease` / `RequestWorkChanges` notify | → `DeveloperProfileId` |
| Accept plan → Project chat members | ClientProfile + DeveloperProfile(s) |
| Notification inbox query | filtered by **active** profile |

### What needs fixing

| Issue | Where | Fix | Status |
| ----- | ----- | --- | ------ |
| **No ActiveProfileMode check** on role actions | Client/Developer milestone commands | Require active mode = Client / Developer | ✅ Done |
| Chat sender stamped with **wrong** profile | Propose / request-changes messages | Stamp by action role (`ResolveSenderProfilesForModeAsync`) | ✅ Done |
| Milestone / plan / escrow **GET** open | GetByProjectId / plans / escrow | Participant check (client / assignee / discussion) | ✅ Done |
| **ProjectFiles** client-only | `ProjectFileService` | Client + hired worker; developer Deliverable; profile gates | ✅ Done |
| Upload `milestoneId` not wired from FE | Files UI | Send `milestoneId` when attaching to a MS | ⚠️ FE still |
| Wallet top-up notify via `UserId` | `WalletService` | Prefer profile-scoped notify | ✅ Done |
| Auto-release notify for teams | AutoRelease worker | Notify team leaders’ DeveloperProfile | ✅ Done |
| ProjectEvents on milestone lifecycle | MilestonePlanCommands | Record EventType on plan/fund/submit/approve/… | ✅ Done |

### Request checklist (when touching these endpoints)

```
Client-only actions  → gate ActiveProfileMode = Client
                      → notify DeveloperProfile (counterpart)
                      → chat SenderClientProfileId set, Developer null

Developer-only actions → gate ActiveProfileMode = Developer
                        → notify ClientProfile
                        → chat SenderDeveloperProfileId set, Client null

Reads (milestones/plans/escrow/files) → participant check (client OR assignee/team)
Money moves → keep UserId/Team wallets (no profile wallet)
```

### Related files

- `Features/Milestones/Commands/MilestonePlanCommands.cs` — auth + notify + chat stamp  
- `Features/Milestones/Queries/MilestoneService.cs` — GET list / mine  
- `Features/Escrow/**` — GET escrow  
- `Features/ProjectFiles/**` — client + hired worker  
- `Features/PayoutSplits/**` — team/project splits + release apply  
- `ICurrentUserService` — UserId only; profiles via `IUserRepository.GetActiveProfileAsync`  
