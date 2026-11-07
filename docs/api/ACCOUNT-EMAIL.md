# Account email delivery

Registration creates an unconfirmed account. Atlas sends a confirmation email; the user opens the link, selects **Confirm email**, then signs in with their password. No console or administrator action is needed. Sign-in also offers confirmation resend and password recovery.

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

Run **Atlas Azure Terraform plan**, review it, then run the existing **Atlas Azure Terraform apply** workflow with the same installation/application selection. Deploy the matching application code through **AtlasDeploy**. No manual Azure app settings are needed. Later application deployments retain the provisioned settings; mailbox changes use Terraform plan/apply again.

Keep the application password only in GitHub Secrets. Terraform marks it sensitive, but the protected remote state and encrypted plan still contain it, following the existing infrastructure secret handling. Neither the SMTP password nor tokens are Terraform outputs.

Missing required mailbox settings cause an enabled Terraform plan to fail before apply. Email defaults to disabled until configured; disabling it and applying removes the SMTP settings. In Development, no transport retains the private JSON mail files; Production refuses registration/recovery while mail is unavailable.

## Verify delivery

1. Configure the GitHub environment, apply Terraform, and deploy this branch to demo.
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
