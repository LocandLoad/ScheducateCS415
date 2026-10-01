# Scheducate Design Classes — v1
## User (entity)
- id: string
- email: string
- userName: string
- passwordHash: string

Relationships:
- User "1" --> "*" Schedule
- User "1" --> "*" UserGroup
- User "1" --> "*" GroupMember

## Schedule (entity)
- id: int
- name: string
- userId: string
- availability: byte[42]

Relationship: User "1" --> "*" Schedule

### Design decisions
- **userId is stored directly on Schedule.**
Each schedule belongs to one user, so a join table is unnecessary.

- **availability uses a fixed 42-byte array.**
The array represents 336 half-hour time slots across seven days.

## UserGroup (entity)
- id: int
- name: string
- ownerId: string

Relationships:
- User "1" --> "*" UserGroup
- UserGroup "1" --> "*" GroupMember


## GroupMember (entity)
- id: int
- userId: string
- groupId: int
- sharedScheduleId: int | null

Relationship: GroupMember "*" --> "1" Schedule

### Design decisions
- **GroupMember is a join entity between User and UserGroup.** 
It allows users to belong to multiple groups.
- **sharedScheduleId is optional.** 
Members only share a schedule when they explicitly choose one.

## GroupInvitation (entity)
- id: int
- groupId: int
- token: string
- createdAt: DateTime

Relationship: UserGroup "1" --> "*" GroupInvitation

### Design decisions
- **token uniquely identifies an invitation.** 
Invitations are removed after being accepted.