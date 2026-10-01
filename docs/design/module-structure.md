# Scheducate Server Module Structure — v1

## Controllers (boundary classes)
- Receive HTTP requests, handle authorization, interact with the database, and return views or redirects.
- **HomeController** — public pages.
- **SchedulesController** — schedule creation and management.
- **UserGroupsController** — groups, memberships, invitations, and shared schedules.

## Models

- **User** — authenticated user account.
- **Schedule** — user's availability schedule.
- **UserGroup** — group and owner information.
- **GroupMember** — group membership and shared schedule.
- **GroupInvitation** — group invitation tokens.

## Data Access
- **ApplicationDbContext** — Entity Framework Core database context.
- Handles persistence for users, schedules, groups, memberships, and invitations.
- ASP.NET Identity handles user authentication and password management.

## Dependency flow
Controllers -> ApplicationDbContext -> Database
Controllers -> ASP.NET Identity -> User Authentication
Models -> ApplicationDbContext