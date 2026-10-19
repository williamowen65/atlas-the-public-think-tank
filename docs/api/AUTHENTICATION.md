# Cookies, CSRF and accounts

[Back to the API starting guide](README.md).

## Browser authentication

A successful password login issues an eight-hour, non-sliding HttpOnly, Secure,
SameSite=Lax `__Host-Atlas.Session` cookie. The server validates the account and
active profile; protected policies check current persisted roles. JavaScript does
not need to store the session in localStorage. The API returns 401/403 instead of
redirecting to an HTML login page.

Before any POST/PUT/DELETE, including login, obtain `/api/auth/csrf`. Keep the
antiforgery cookie and send the returned token in `X-CSRF-TOKEN`. Fetch a fresh
token after login/logout because the token is bound to the authenticated identity.
The CSRF cookie is Secure, HttpOnly and SameSite=Strict. SameSite is defense in
depth; it does not replace token validation. The SPA can read the JSON token.

Sign-in requires confirmed email; registration creates an unconfirmed account.
For local development only, configure
`Atlas:DevelopmentMailDirectory` in user secrets to a private directory outside
the checkout; registration writes the confirmation payload there. Submit it to
`/api/auth/confirm-email`. These are account-bearing secrets; restrict access and
delete them after use. Tokens are not returned in JSON or application logs.
Without development delivery configured, registration fails with 503 before
creating an account. Production email delivery/recovery is not silently simulated.
Two-factor enrollment/challenges remain PTT-127; accounts requiring 2FA are denied
rather than bypassing the second factor.

