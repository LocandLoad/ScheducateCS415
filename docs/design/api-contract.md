# Scheducate API Contract — v1

## Schedules

### GET /Schedules
Success:
    200 — Schedule list

### POST /Schedules/Create
Request:
    { Name: string, Availability: byte[] }
Success:
    302 — Redirects to /Schedules
Errors:
    400 — Invalid schedule data

### POST /Schedules/Delete/:id
Success:
    302 — Redirects to /Schedules
Errors:
    404 — Schedule not found
---

## Groups

### GET /UserGroups
Success:
    200 — Group list

### POST /UserGroups/Create
Request:
    { Name: string }
Success:
    302 — Redirects to /UserGroups

### GET /UserGroups/Details/:id

Requires authentication.
Success:
    200 — Group details
Errors:
    403 — User is not a group member or owner
    404 — Group not found

### POST /UserGroups/AddSchedule
Request:
    { groupId: number, scheduleId: number | null }
Success:
    302 — Redirects to group details
---

## Invitations

### GET /UserGroups/Invite/:id
Success:
    200 — Invitation URL
Errors:
    404 — Group not found

### GET /UserGroups/Join/:token
Success:
    200 — Invitation page
Behavior:
    Unauthenticated users are redirected to login.

### POST /UserGroups/AcceptInvitation
Request:
    { token: string }
Success:
    302 — Redirects to group details
Errors:
    404 — Invitation not found