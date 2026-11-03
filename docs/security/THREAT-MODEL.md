# docs/security/THREAT-MODEL.md

## Attack Surface

- POST /Identity/Account/Register — unauthenticated, accepts email, username, and password through ASP.NET Core Identity.
- POST /Identity/Account/Login — unauthenticated, accepts credentials and establishes an authentication cookie.
- POST /Identity/Account/Logout — authenticated, terminates the current session.
- GET /Schedules — authenticated, returns schedules belonging to the current user.
- GET /Schedules/Create — authenticated, displays the schedule creation form.
- POST /Schedules/Create — authenticated, accepts schedule name and availability data.
- GET /Schedules/Delete/{id} — authenticated, retrieves a schedule for deletion after verifying ownership.
- POST /Schedules/Delete/{id} — authenticated, deletes an owned schedule and removes its shared schedule references.
- GET /UserGroups — authenticated, returns groups owned by or joined by the current user.
- POST /UserGroups/Create — authenticated, accepts a group name and assigns ownership to the current user.
- GET /UserGroups/Details/{id} — authenticated, returns group information, member information, and shared availability; restricted to owners and members.
- POST /UserGroups/AddSchedule — authenticated, allows a group member to share or remove one of their own schedules.
- GET /UserGroups/Invite/{id} — authenticated, creates an invitation token and invitation URL for a group owned by the current user.
- GET /UserGroups/Join/{token} — explicitly allows anonymous access; retrieves an invitation and redirects unauthenticated users to login.
- POST /UserGroups/AcceptInvitation — authenticated, accepts an invitation using its token and creates a group membership.
- POST /UserGroups/Join/{id} — authenticated, allows a group owner to join their own group.
- ASP.NET Core Identity authentication cookies — establish authenticated sessions and authorize access to protected controllers.
- Database — stores user credentials, email addresses, schedules, availability information, group memberships, and invitation tokens.
- Demo account credentials — hardcoded in `DbInitializer.cs` and seeded automatically during application startup.

## STRIDE Analysis, with Risk Rating

| Component | Threat | Category | Likelihood | Impact | Risk Rating | Mitigation |
|---|---|---|---|---|---|---|
| DbInitializer.cs | An attacker discovers hardcoded demo credentials in the public repository and uses them to authenticate as a demo user. Demo accounts are created automatically at startup. | Spoofing | High | High | Critical | Remove hardcoded credentials from production builds. Disable demo account seeding in production and use environment-specific configuration. Rotate any exposed credentials. |
| POST /Identity/Account/Login | Unlimited login attempts allow attackers to perform brute-force or credential-stuffing attacks. | Spoofing / Denial of Service | Medium | High | High | Configure ASP.NET Core Identity account lockout, implement rate limiting, and monitor repeated authentication failures. |
| GET /UserGroups/Invite/{id} | Creating invitation tokens through a GET request allows unintended invitation creation through cross-site requests or automated link access. | Tampering / Elevation of Privilege | Medium | Medium | Medium | Change invitation creation to POST, require antiforgery validation, and verify group ownership before generating invitations. |
| GroupInvitation | Invitation tokens do not have an expiration mechanism. An unused invitation remains valid until accepted or its associated group is deleted. | Elevation of Privilege / Spoofing | Medium | High | High | Implement invitation expiration, token revocation, and validation of invitation age before accepting membership. |
| POST /Schedules/Create | An authenticated user can submit malformed or excessively large availability data because the controller does not explicitly validate the availability array's expected length or contents. | Tampering / Denial of Service | Medium | Medium | Medium | Validate availability length (42 bytes), permitted values, and request size on the server before saving. |
| GET /UserGroups/Join/{token} | Invitation lookup responses disclose whether an invitation exists and include the supplied token in error messages. | Information Disclosure | Medium | Low | Low | Return generic error messages without echoing invitation tokens or exposing internal lookup details. |
| GET /UserGroups/Details/{id} | Group information and member email addresses are displayed to all authorized group members. This may expose personal information beyond what is necessary for scheduling. | Information Disclosure | Medium | Medium | Medium | Minimize displayed personal information and expose email addresses only when required by the application's intended functionality. |
| POST /UserGroups/AcceptInvitation | An authenticated user possessing a valid invitation token can join the associated group. A leaked or forwarded invitation can therefore be used by an unintended recipient. | Spoofing / Elevation of Privilege | Medium | Medium | Medium | Use short-lived, revocable, single-use invitations and consider binding invitations to intended recipient accounts. |

## Security (docs/security/THREAT-MODEL.md, appended)

### Hardening Log

- 2026: Identified hardcoded demo credentials and unconditional demo account seeding as a critical authentication vulnerability.
- 2026: Identified state-changing invitation creation through a GET endpoint; recommend POST with antiforgery validation.
- 2026: Identified missing invitation expiration and revocation mechanisms.
- 2026: Identified the absence of explicit application-level login rate limiting.
- 2026: Identified missing server-side validation of schedule availability data.
- 2026: Reviewed authorization boundaries for schedules and groups. Ownership is derived from the authenticated user, and group details are restricted to owners and members.
- 2026: Reviewed Razor view rendering. Default Razor output encoding provides protection against ordinary HTML injection; no confirmed stored-XSS vulnerability was identified during this review.

### Security Measurement Baseline

- Vulnerabilities identified this pass: 5 primary security findings requiring remediation.
- Additional security concerns: 3 (invitation error disclosure, group member email exposure, and invitation recipient verification).
- Critical findings: 1 (hardcoded demo credentials and unconditional demo account seeding).
- Security test coverage: 0 of 5 primary findings have dedicated automated security regression tests.
- Authentication: ASP.NET Core Identity with cookie-based authentication.
- Authorization: Controller-level `[Authorize]` protection, supplemented by ownership and membership checks.
- CSRF protection: Explicit `[ValidateAntiForgeryToken]` attributes are present on POST actions. Invitation creation currently uses GET despite modifying database state.
- Input validation: ASP.NET Core model binding and Identity password validation are present, but additional validation is needed for availability data and invitation tokens.
- Maturity note: Scheducate's security practices are currently at an initial, primarily reactive stage. Authentication and authorization controls are implemented, but several security weaknesses remain, particularly around demo credentials, invitation management, and request validation. This baseline documents the current state rather than claiming that the application is fully hardened.