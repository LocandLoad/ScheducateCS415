# Analysis Model

## Model Overview

~~~mermaid
graph TD
UC["Use Cases"] --> Scenario["Scenario-Based Model (who does what, when)"]
UC --> Class["Class-Based Model (what data and entities exist)"]
UC --> Functional["Functional Model (how data flows and transforms)"]
UC --> Behavioral["Behavioral Model (what states an entity can be in)"]
~~~

---

## Scenario-Based Model
### Use Case Diagram

~~~mermaid
graph TD

    Visitor --> UC01["Register Account"]
    Visitor --> UC02["Log In"]

    User --> UC03["Edit Schedule"]
    User --> UC04["View Groups"]
~~~

---

## Class-Based Model

### Domain Class Diagram

~~~mermaid
classDiagram
    class User {
        +id
        +email
        +userName
        +passwordHash
    }

    class Schedule {
        +id
        +name
        +userId
        +availability
    }

    class UserGroup {
        +id
        +name
        +ownerId
    }

    class GroupMember {
        +id
        +userId
        +groupId
        +sharedScheduleId
    }

    class GroupInvitation {
        +id
    }

    User "1" --> "*" Schedule : owns
    User "1" --> "*" UserGroup : owns
    User "1" --> "*" GroupMember : has memberships
    UserGroup "1" --> "*" GroupMember : contains
    GroupMember "1" --> "0..1" Schedule : shares
    UserGroup "1" --> "*" GroupInvitation : has
~~~

---


## Functional Model

### Schedule Data Flow

~~~mermaid
flowchart LR
    User((User)) -->|"schedule information"| Validate["Validate Schedule"]
    Validate -->|"schedule record"| ScheduleStore[(Schedule Store)]
    ScheduleStore -->|"saved availability"| User
~~~

### Group Availability Data Flow

~~~mermaid
flowchart LR
    User((User)) -->|"group request"| Group["Select Group"]
    Group --> Members["Retrieve Group Members"]
    Members --> Schedules["Retrieve Shared Schedules"]
    Schedules --> Availability["Read Availability"]
    Availability --> Calculate["Calculate Group Availability"]
    Calculate --> Result["Available Time Periods"]
    Result --> User
~~~

### Group Invitation Data Flow

~~~mermaid
flowchart LR
    Owner((Group Owner)) -->|"invitation"| Invitation["Create Group Invitation"]
    Invitation --> InvitationStore[(Invitation Store)]
    InvitationStore -->|"pending invitation"| User((Invited User))
    User -->|"accept / decline"| Response["Process Invitation"]
    Response --> Membership["Create Group Membership"]
~~~

---

## Behavioral Model

### Schedule State Diagram

~~~mermaid
stateDiagram-v2
    [*] --> Empty: user creates schedule
    Empty --> Configured: user sets availability
    Configured --> Configured: user edits availability
    Configured --> Configured: user saves changes
    Configured --> [*]: schedule deleted
~~~

### Group Membership State Diagram

~~~mermaid
stateDiagram-v2
    [*] --> Invited: owner sends invitation
    Invited --> Member: user accepts invitation
    Invited --> [*]: user declines invitation
    Member --> Member: user updates shared schedule
    Member --> [*]: user leaves group
~~~

### Group State Diagram

~~~mermaid
stateDiagram-v2
    [*] --> Created: owner creates group
    Created --> Active: members join
    Active --> Active: members join or leave
    Active --> Active: schedules are shared
    Active --> Active: availability is recalculated
    Active --> [*]: owner deletes group
~~~