# Account email delivery

Registration creates an unconfirmed account. Atlas sends a confirmation email; opening the link automatically confirms the account. The page then displays the sign-in form with the verified email prefilled; the user must enter their password. No console or administrator action is needed. Sign-in also offers confirmation resend and password recovery.

## Connect your existing mail provider

Use an authenticated SMTP service supporting STARTTLS (normally port 587). The sender is a verified mailbox/address from your provider. The Azure app name does not create an email address. If your provider only supports an HTTP API or implicit TLS on port 465, add the appropriate adapter before enabling delivery.

## Configure once through GitHub and Terraform

In repository **Settings → Environments → atlas-infra-apply**, add:

| Kind | Name | Value |
| --- | --- | --- |
| Variable | `ATLAS_MAIL_ENABLED` | `true` when ready to send |
| Variable | `ATLAS_MAIL_ADDRESS` | Your full Namecheap mailbox address, used for login and sender |
| Secret | `ATLAS_MAIL_PASSWORD` | The Namecheap application password |
| Variable (optional) | `ATLAS_MAIL_HOST` | Defaults to `mail.privateemail.com` |
| Variable (optional) | `ATLAS_MAIL_PORT` | Defaults to `587` (STARTTLS) |
| Variable (optional) | `ATLAS_MAIL_PUBLIC_ORIGINS` | JSON map for custom domains, e.g. `{"live":"https://thepublicthinktank.com"}` |

Demo and live share these mailbox settings. Terraform derives a separate HTTPS App Service origin for each app, so demo links go to demo and live links go to live. Only set the optional origins map when using custom site domains. Rehearsal installations also inherit the shared mailbox when email is enabled.

For branch testing, run **Atlas Azure Terraform plan** from `PTT-174-account-email` with installation **main** and applications **demo**. Review the plan, then run **Atlas Azure Terraform apply** from the same branch with the same selections and SQL bootstrap **off**. This explicitly targets only the demo web app, retains live in Terraform state, and rejects changes to live/shared resources. After merge, run the normal **both** plan/apply from main to configure live too. Deploy the matching application code through **AtlasDeploy**. No manual Azure app settings are needed. Later application deployments retain the provisioned settings; mailbox changes use Terraform plan/apply again.

Keep the application password only in GitHub Secrets. Terraform marks it sensitive, but the protected remote state and encrypted plan still contain it, following the existing infrastructure secret handling. Neither the SMTP password nor tokens are Terraform outputs.

Missing required mailbox settings cause an enabled Terraform plan to fail before apply. Email defaults to disabled until configured; disabling it and applying removes the SMTP settings. In Development, no transport retains the private JSON mail files; Production refuses registration/recovery while mail is unavailable.

## Verify delivery

1. Configure the GitHub environment, apply Terraform, and deploy this branch to demo.
2. Register an address you control. Check inbox/spam, sender, and that the link points to demo.
3. Open the email link and verify automatic confirmation, then sign in with the prefilled email and your password.
4. Request a password reset. Open the email, choose a valid new password, and sign in again.
5. Exercise confirmation resend, an invalid token, and a provider failure. A registration whose email failed may already exist: use resend confirmation after restoring delivery.

Links carry Identity tokens in the URL fragment. The page removes the fragment after reading it; tokens are sent only in the CSRF-protected POST body. Confirmation posts automatically when the page opens. The endpoint accepts only active, unlocked, previously unconfirmed accounts, rotates the security stamp without issuing any session cookie, and rejects repeat confirmation. Mail scanners that execute JavaScript may consume a link; users can still sign in with their password if it was already confirmed. Password resets do not automatically sign in. Email confirmation also requires a separate password sign-in; confirmation alone never creates a session. Identity owns expiry and token validation.

Delivery is synchronous, without a background retry queue. Provider acceptance does not guarantee inbox delivery. Check provider delivery reports for bounces. Preserve the deployment's Identity Data Protection keys across restarts so pending links remain usable. Do not record email bodies, tokens, credentials, or provider responses in logs.

Provider credentials and actual inbox delivery have not yet been validated. Backend checks require .NET 10; frontend checks use `npm run check`.

## Namecheap Private Email

For **Private Email** (not cPanel hosting email), use `mail.privateemail.com`, port `587` with STARTTLS, your full mailbox address as Username and From, and the mailbox's application password as Password. Do not use your Namecheap account login password. Set PublicOrigin to the demo site's HTTPS origin for demo and the live site's origin for live. Both apps may use the same sender mailbox.

See [Namecheap's SMTP settings](https://www.namecheap.com/support/knowledgebase/article.aspx/1179/2175/general-private-email-configuration-for-mail-clients-and-mobile-devices/). Domain registration or forwarding alone does not provide an authenticated sending mailbox. Confirm the email product and mailbox before configuring the app.

In Private Email settings, enable SMTP under **Connect third-party apps** and generate an application password for Atlas. Namecheap’s current instructions specify an application password for mail clients.

## Faster local testing with real email

By default, localhost Development writes private JSON mail files and sends no real email. To send through Namecheap locally, explicitly configure SMTP in the API project's .NET user secrets. GitHub Environment variables are not automatically available on your computer.

In Visual Studio, right-click **Atlas.Api → Manage User Secrets** and merge this section into your existing secrets (keep your SQL connection settings):

```json
{
  "Atlas": {
    "AccountMail": {
      "Transport": "Smtp",
      "Host": "mail.privateemail.com",
      "Port": "587",
      "Username": "YOUR_FULL_MAILBOX_ADDRESS",
      "From": "YOUR_FULL_MAILBOX_ADDRESS",
      "Password": "YOUR_NAMECHEAP_APP_PASSWORD",
      "PublicOrigin": "http://localhost:5173"
    }
  }
}
```

Use the actual URL and port printed by Vite for PublicOrigin: this is the frontend URL, not the API port. HTTPS origins work too when local certificates are configured. HTTP is accepted only for loopback origins in Development; Production still requires HTTPS.

Run the local API and frontend using `npm run dev:api` from `src/hosts/Atlas.Web`. Use the local database (initialized with Identity roles), then register from the local frontend with an address you control. A real email is sent to that address, and the link returns to the configured localhost frontend. Keep the dev servers running while you open it **on the same computer**. On a phone, localhost refers to the phone; use the separate LAN HTTPS development setup and configure that frontend HTTPS origin if testing across devices.

Use a separate Namecheap application password labeled Atlas local so it can be revoked independently. Local signup must use a local database, not the deployed demo/live database. To return to file-only mail, remove the AccountMail section or its Transport setting from local secrets. Do not commit or share the secrets file.

## Test account recovery locally

1. Start API/Vite with the local SMTP settings above and a local database.
2. Register an address you control and confirm it. Recovery emails are sent only for confirmed, active accounts.
3. On Sign in, choose **Forgot your password?**, enter that address, and select **Send reset email**. The response stays generic for account privacy.
4. Open the newest reset email on a device that can reach PublicOrigin. Enter the new password twice; all requirements should be checked before submitting.
5. After success, choose **Sign in**. Verify the old password fails and the new password works.
6. Reusing the reset token should fail. Request a fresh reset email if needed. A request timeout does not prove token expiry; check whether sign-in already works before repeating the reset.

No admin/console action is required for recovery. SMTP provider credentials are separate from the account password being reset.
