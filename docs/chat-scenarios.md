# FreeGency — Chat scenarios

Reference for team / proposal / project messaging.  
UI behaviour demo: `FreeGency-Frontend/freegency/public/demo/proposal-milestone-flow.html`

## MessageType

| Value | Meaning |
|-------|---------|
| `Text` | Normal chat text |
| `Attachment` | File in chat (`FileUrl` / `FileName`) |
| `System` | Automatic event (no sender profile) |
| `MilestonePlan` | Plan card in chat (`PlanVersionId` → `MilestonePlanVersion`) |

## RoomType / ChatRoomStatus

| RoomType | When |
|----------|------|
| `Proposal` | Client ↔ applicant negotiation (includes plan cards) |
| `Project` | After Hire (Accept Milestone Plan) |
| `TeamGroup` | Leader-created internal group |
| `TeamMain` | Default team room — created automatically when the team is created (owner/leader is the first member; new members join on accept) |

| Status | When |
|--------|------|
| `Active` | Live room |
| `Archived` | Proposal room after Hire only (history kept, no new negotiation sends) |

### Proposal identity

| Field | Solo | Team |
|-------|------|------|
| `ApplicantType` | User | Team |
| `TeamId` | null | team |
| `UserId` | freelancer | **leader who submitted** (negotiation speaker) |

No separate `SubmittedByUserId` — `UserId` covers the speaker in both cases.

### Chat participant identity (profile-scoped)

Chat membership and message senders use **ClientProfile** or **DeveloperProfile** (XOR), not `UserId`.

| Role in room | Profile used |
|--------------|--------------|
| Client | `ClientProfile` of `project.ClientId` |
| Freelancer / Team Leader | `DeveloperProfile` of that user |

Inbox / membership / send authorization resolve the caller's **active profile** via `User.ActiveProfileMode`:

- Client mode → match `ChatRoomMember.ClientProfileId` / `Message.SenderClientProfileId`
- Developer mode → match `ChatRoomMember.DeveloperProfileId` / `Message.SenderDeveloperProfileId`

`ChatRoom.CreatedByUserId` remains account-level audit only.

---

## Scenario A — Solo freelancer proposal

1. Freelancer submits → `UserId` = freelancer.
2. Client **Start Discussion** → `RoomType.Proposal` with Client (`ClientProfile`) + Freelancer (`DeveloperProfile`) (`CanSend` both true).
3. System message: discussion started / negotiate plan next.
4. Freelancer **Propose Plan** → `MessageType.MilestonePlan` in same room (sender = active DeveloperProfile).
5. Client **Request Changes** or **Accept Plan** → Hire.
6. On Accept: Proposal room → `Archived`; new `RoomType.Project` (Client + Freelancer profiles) + System message.

---

## Scenario B — Team proposal (multiple leaders)

1. Leader A submits for team → `TeamId` = team, `UserId` = Leader A (speaker).
2. Client Start Discussion → Proposal room:
   - Client (`ClientProfile`, `CanSend` true)
   - **All** Team Leaders (`DeveloperProfile`, full history)
   - Only Leader A has `CanSend` true among leaders
3. Team Management → Messages: every leader sees **Project title · Client**, with `canSend` for self (when active as Developer).
4. Only Leader A may send Text/Attachment and propose Milestone Plans.
5. Other leaders: read-only on that negotiation room.
6. Accept Plan → archive Proposal room; Project room with Client + **all** leaders (`CanSend` true). Members added later with free-text `RoleLabel` (as DeveloperProfile).

---

## Scenario C — Milestone plan inside chat

| Action | Chat message |
|--------|----------------|
| Propose plan vN | `MilestonePlan` + `PlanVersionId` (developer profile sender) |
| Request changes | `Text` from client profile with comment |
| Accept plan | `System` then archive + Project room System opener |

---

## Scenario D — Team Management vs Personal Messages

| Surface | Who | Content |
|---------|-----|---------|
| **Team → Management → Messages** | Any Team Leader (Developer mode) | Proposal rooms (active + archived) + TeamGroups |
| **Personal Messages** | Active profile inbox | Rooms where that Client/Developer profile is a member |

Regular members do **not** see client negotiation chats.

---

## Scenario E — Create Team Group

Leader creates a private group from **Team → Messages → Create group** (`POST /teams/{id}/chat-groups`) → `RoomType.TeamGroup` with selected DeveloperProfile members. Appears in members’ personal Messages and Team Management inbox.

`TeamMain` (“General”) is created automatically when the team is created (and backfilled for older teams).

---

## Scenario F — After Hire: add project workers

Project room starts Client + leaders; leader adds members with `RoleLabel` badge (DeveloperProfile).

---

## Scenario G — Leader change mid-project

New leader joins Project rooms; old leaves voluntarily.  
Negotiation speaker transfer (change who has `CanSend` / which proposal `UserId` owns the speaker): follow-up if needed.

---

## Scenario H — Archive rules

Archive Proposal chat **only** when Milestone Plan is accepted (Hire).

---

## Key entity fields (Chat DTOs deferred)

- `ProjectProposal.UserId` — solo applicant **or** team submitter/speaker
- `ChatRoom.Status`, `ArchivedAt`, `SourceProposalRoomId`
- `ChatRoomMember.ClientProfileId` XOR `DeveloperProfileId`, `CanSend`, `RoleLabel`
- `Message.SenderClientProfileId` XOR `SenderDeveloperProfileId`, `MessageType`, `PlanVersionId`
- Message DTOs expose `SenderId` (profile id) + `SenderProfileType` (`Client` | `Developer`)

## Hooks wired

- `StartDiscussion` — profile members + System message
- `ProposePlan` / `RequestPlanChanges` / `AcceptPlan` — chat messages + archive + Project room
