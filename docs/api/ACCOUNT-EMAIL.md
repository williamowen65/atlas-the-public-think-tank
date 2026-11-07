# Account email delivery

Registration creates an unconfirmed account. Atlas sends a confirmation email; the user opens the link, selects **Confirm email**, then signs in with their password. No console or administrator action is needed. Sign-in also offers confirmation resend and password recovery.

## Connect your existing mail provider

Use an authenticated SMTP service supporting STARTTLS (normally port 587). The sender is a verified mailbox/address from your provider. The Azure app name does not create an email address. If your provider only supports an HTTP API or implicit TLS on port 465, add the appropriate adapter before enabling delivery.

Set these separately for demo and live using App Service configuration/secrets (double underscores map to configuration sections):

| Setting | Value |
| --- | --- |
| `Atlas__AccountMail__Transport` | `Smtp` |
| `Atlas__AccountMail__Host` | Provider SMTP host |
| `Atlas__AccountMail__Port` | `587`, or provider STARTTLS port |
| `Atlas__AccountMail__Username` | Provider SMTP username |
| `Atlas__AccountMail__Password` | Provider SMTP credential; secret |
| `Atlas__AccountMail__From` | Verified sender, for example `atlas@thepublicthinktank.com` |
| `Atlas__AccountMail__PublicOrigin` | Site HTTPS origin only, without a path; demo must use the demo origin |

Complete the provider's sender/domain verification and DNS requirements first. Never commit credentials. In local development use .NET user secrets for the API project to configure the same keys with colons instead of double underscores.

With no transport selected, Development retains the private JSON files in `Atlas:DevelopmentMailDirectory`. Production refuses registration/recovery until email delivery is configured. Explicit invalid SMTP configuration never falls back to files.

## Verify delivery

1. Deploy to demo and configure its provider settings.
2. Register an address you control. Check inbox/spam, sender, and that the link points to demo.
3. Confirm the email through the website and sign in.
4. Request a password reset. Open the email, choose a valid new password, and sign in again.
5. Exercise confirmation resend, an invalid token, and a provider failure. A registration whose email failed may already exist: use resend confirmation after restoring delivery.

Links carry Identity tokens in the URL fragment. The page removes the fragment after reading it; tokens are sent only in the CSRF-protected POST body. Confirmation requires an explicit button click, preventing link-preview fetches from confirming an account. Password resets do not automatically sign in. Identity owns expiry and token validation.

Delivery is synchronous, without a background retry queue. Provider acceptance does not guarantee inbox delivery. Check provider delivery reports for bounces. Preserve the deployment's Identity Data Protection keys across restarts so pending links remain usable. Do not record email bodies, tokens, credentials, or provider responses in logs.

Provider credentials and actual inbox delivery have not yet been validated. Backend checks require .NET 10; frontend checks use `npm run check`.

## Namecheap Private Email

For **Private Email** (not cPanel hosting email), use `mail.privateemail.com`, port `587` with STARTTLS, your full mailbox address as Username and From, and the mailbox's application password as Password. Do not use your Namecheap account login password. Set PublicOrigin to the demo site's HTTPS origin for demo and the live site's origin for live. Both apps may use the same sender mailbox.

See [Namecheap's SMTP settings](https://www.namecheap.com/support/knowledgebase/article.aspx/1179/2175/general-private-email-configuration-for-mail-clients-and-mobile-devices/). Domain registration or forwarding alone does not provide an authenticated sending mailbox. Confirm the email product and mailbox before configuring the app.

In Private Email settings, enable SMTP under **Connect third-party apps** and generate an application password for Atlas. Namecheap’s current instructions specify an application password for mail clients.
