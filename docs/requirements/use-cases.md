# Use Cases

## US-01: Register Account

### Overview

- **Primary Actor:** Visitor
- **Preconditions:** The visitor is not authenticated.
- **Postconditions:** A new account exists for the submitted email address and is ready for email verification.

### Main Success Scenario

1. The visitor submits an email address and password.
2. The system validates the email format and password strength.
3. The system confirms that no account already exists for the email address.
4. The system creates the account with the password stored as a secure hash.
5. The system initiates the email-verification process.
6. The system tells the visitor to verify the account before logging in

### Extensions

- **2a.** The email format is invalid: system rejects the request and asks for a valid email address.
- **2b.** The password does not meet the password-strength requirements: system rejects the request and explains the requirements.
- **3a.** An account already exists for the submitted email: system returns a generic registration failure message that does not reveal whether the email is already registered.
- **5a.** Verification-message delivery fails: the account remains unverified and the user can request another verification message later.

---

## US-02: Log In

### Overview

- **Primary Actor:** Registered user
- **Preconditions:** The user has a registered, verified account.
- **Postconditions:** The user holds a valid access token and refresh token. The subsequent requests are authenticated.

### Main Success Scenario

1. The user submits an email and password.
2. The system verifies the credentials against the stored password hash.
3. The system issues a new access token and refresh token pair.
4. The client stores the refresh token securely and attaches the access token to subsequent requests.

### Extensions

- **2a.** Credentials do not match: system returns a generic "invalid email or password" error, not revealing *which* field was wrong, to avoid account enumeration security issue.
- **2b.** Account exists but is not yet verified: system rejects login and offers to resend a verification message.
- **3a.** If refresh token issuance fails: system does not issue an access token either. This helps to mitigate no partial authentication state.

---

## US-03: Edit Schedule

### Overview

- **Primary Actor:** An authenticated user
- **Preconditions:** The user is authenticated.
- **Postconditions:** Schedule is updated and viewable to both the user and others in the relevant user's groups

### Main Success Scenario

1. The User edits a new schedule with most recent schedule prefilled in.
2. The User requests to publish the schedule.
3. The system transitions the schedue to the new one, overriding the previous one.
4. The schedule becomes visible in the user's groups and on the user's schedule page.

### Extensions

- **1a.** User navigates away mid-change: The previous schedule is retained.
- **2a.** User is not the schedule's owner (e.g., a stale session referencing another user's schedule): system rejects with an authorization error.

---

## US-04: View Groups

### Overview

- **Primary Actor:** An authenticated user
- **Preconditions:** The user must be a member of a group.
- **Postconditions:** The user sees all the blocks of time in which all members are available.
### Main Success Scenario

1. The user navigates to the group page.
2. The user clicks on the relevant group.
3. The user sees availible time matchups between group members.

### Extensions

- **2a.** No groups exist yet: system shows an explicit empty state, not a blank screen.
- **2b.** A requested page is beyond the available data: system returns an empty page, not an error.