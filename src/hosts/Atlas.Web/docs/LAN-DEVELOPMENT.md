# Phone testing over local HTTPS

Normal desktop development is unchanged. LAN authentication is opt-in. The API remains at https://localhost:7240; the phone connects only to Vite.

## Why this exists and what it exposes

Use a real phone to review responsive styles and signed-in controls. Public nodes use the anonymous proxy, while voting, reactions and session checks currently use the session proxy. Its default loopback restriction blocks LAN requests. HTTPS is also required for Atlas's Secure authentication/CSRF cookies.

LAN mode makes Vite reachable by other devices wherever the machine's firewall permits. The whole development server is exposed, including development source modules and the allowlisted API bridge—not just the page screenshot. Use a trusted private network and development data/accounts; do not port-forward it or use it as production hosting.

The exact Host/:authority check prevents unintended destination names; it does not authenticate the device connecting to the allowed address. Another device on the reachable network can browse public data and attempt login. The backend still enforces identity, authorization and CSRF. Existing same-origin write checks and Atlas-only cookie filtering remain enabled. No certificate keys or database connection strings are intentionally sent to the browser, but frontend source is visible by design; never place secrets there.

Only Vite is newly exposed: the API target remains loopback. HTTPS encrypts traffic and authenticates the server; it does not limit access to your particular phone. The local CA can sign other certificates, so keep its private key private and install its trust only on devices you control.

This is a configuration/guard review, not a complete security audit or an assurance that the dev server has no vulnerabilities.

## Save settings once, then use the normal LAN command

After generating your certificate below, run these once from src/hosts/Atlas.Web:

```powershell
Copy-Item .env.lan.example .env.lan.local
notepad .env.lan.local
```

Set ATLAS_LAN_ORIGIN to your PC's Wi-Fi address, for example:
```dotenv
ATLAS_LAN_ORIGIN=https://192.168.50.55:5173
```

The LAN launcher defaults certificate paths to %LOCALAPPDATA%/Atlas/dev-certs/atlas.pem and atlas-key.pem. Set ATLAS_LAN_CERT and ATLAS_LAN_KEY in the same file if your files are elsewhere; use forward slashes in Windows paths. The local file is ignored by Git; keep certificates and private keys outside the repository.

From any new terminal, run just `npm run dev:lan:api`. It loads this file before starting API and Vite. Explicit shell environment values take precedence, so remove stale shell overrides if edits to the file appear ineffective. Ordinary dev/build commands do not load this LAN-specific file.

Missing origin or certificate files now stop the LAN launcher with guidance instead of falling back to HTTP. Existing Vite validation still requires a private HTTPS origin. This does not generate certificates or install phone trust. If your Wi-Fi IP changes, update the saved origin and regenerate the server certificate to include the new IP; the phone can continue trusting the same mkcert CA.

## Windows setup (PowerShell)

Install [mkcert](https://github.com/FiloSottile/mkcert#installation). For Chocolatey, open PowerShell **as Administrator** and run `choco install mkcert -y`. A non-elevated install can report lock-file access errors or access denied under C:\\ProgramData\\chocolatey. Retry elevated before deleting any lock file; another installation may hold it.

Return to regular PowerShell under your normal Windows account for certificate generation and running the app. Install the local CA (approve any trust/elevation prompt), then identify your address:

```powershell
mkcert -install
ipconfig
```

Find the active Wi-Fi/Ethernet IPv4 address with a default gateway. In the reported setup this was 192.168.50.55, not the other adapter's 192.168.56.1. Substitute it below. Store certificates outside the repository:

```powershell
$atlasCertDir = Join-Path $env:LOCALAPPDATA "Atlas\dev-certs"
New-Item -ItemType Directory -Force $atlasCertDir
mkcert -cert-file "$atlasCertDir\atlas.pem" -key-file "$atlasCertDir\atlas-key.pem" localhost 127.0.0.1 ::1 192.168.1.25
$env:ATLAS_LAN_ORIGIN = "https://192.168.1.25:5173"
$env:ATLAS_LAN_CERT = "$atlasCertDir\atlas.pem"
$env:ATLAS_LAN_KEY = "$atlasCertDir\atlas-key.pem"
```

From src/hosts/Atlas.Web:

```powershell
npm run dev:lan:api
```

If the API is already running, use `npm run dev -- --host 0.0.0.0` in the configured shell instead.
The LAN launcher reads .env.lan.local; .env.local is not used for these server settings.
The configured port is strict so the printed origin remains correct.

Shell environment variables last only for this PowerShell session. Save settings in .env.lan.local as described above to reuse them in new terminals. Both `Test-Path $env:ATLAS_LAN_CERT` and `Test-Path $env:ATLAS_LAN_KEY` should return True. Vite's startup output must show HTTPS.

`dev:lan:api` launches the API and Vite together; Vite's configuration reads these variables. The command alone does not generate certificates or configure phone trust.

## Trust on the phone

Run `mkcert -CAROOT` to locate the CA folder. Transfer **only rootCA.pem** to your phone using a trusted file-transfer method. If needed, copy it with a .crt extension. Never transfer rootCA-key.pem or atlas-key.pem, and never commit certificates or keys.

- iPhone/iPad: install the downloaded certificate profile in Settings (General → VPN & Device Management), then enable full trust under General → About → Certificate Trust Settings. See [Apple's instructions](https://support.apple.com/en-us/102390).
- Android: install it as a CA certificate through Settings → Security/Privacy → Encryption & credentials → Install a certificate → CA certificate. Names vary by device. This setup targets browser testing, not native Android apps.

### Android credential-store distinction

On the user's Android device, Settings used **Credential Storage**, **View Security Certificates / Display Trusted CA Certificates**, **User Certificates / View and Modify Stored Credentials**, and **Install from Phone Storage**. The first attempt appeared only under user credentials and Chrome still showed a certificate warning. Installing it as a trusted CA made it appear in the trusted CA list.

Use Install from Phone Storage and select **CA certificate** where offered. A label such as “Atlas Local Development” is just a friendly name; naming it does not establish the correct trust type. Verify it appears in trusted CA certificates (the User tab/category), rather than assuming any entry under User Certificates is sufficient. Menu names vary: if no CA choice exists, consult instructions for that phone model. Do not use Clear Credentials, which can remove unrelated credentials.

Download the public root from your own PC (email/file transfer is possible); do not substitute atlas.pem or any private key. Chrome should open the page without an unsafe-connection bypass.

Connect both devices to the same network. Open **https://192.168.1.25:5173** using your actual address. There should be no certificate warning. Do not use the old HTTP URL for signed-in testing.

Check node loading, vote totals/reactions, login, vote/change/undo, logout, and signed-out state after reload. Allow TCP 5173 on your computer's private-network firewall; avoid guest Wi-Fi client isolation.

## Reuse and troubleshooting

The phone trusts this local CA, not every development server automatically. Reuse the same mkcert CA to issue certificates for other projects. The same server certificate can also serve different ports/projects on the same listed IP/hostname. New names/IPs require a new server certificate, but no new phone trust installation while the CA remains the same. Certificates expire; renewed server certificates from the same trusted CA do not normally require reinstalling the CA. A new CA, phone reset or removed trust requires installation again.

If your PC address changes, regenerate the server certificate for the new IP and update ATLAS_LAN_ORIGIN. A DHCP reservation can keep the address stable.

- Connection refused: verify address, port, firewall and Wi-Fi.
- Certificate warning: verify the certificate's IP and phone CA trust.
- Session bridge 403: use the exact configured HTTPS origin, including port. The response diagnostics show the received host, configured host and HTTPS setting.
- HTTP/2 may omit Host and supply :authority instead. This caused the confirmed 403 even though the URL and TLS configuration were correct. The guard now resolves either header and uses the result for both host validation and same-origin writes. The regression test captures the production middleware through a mock Vite configureServer hook.
- Nodes loading while votes/reactions/session fail indicates different proxy paths, not necessarily a database problem. Inspect the failed request's Response text before changing certificates.
- If HTTPS works without warnings on the PC but warns on Android, check the phone's trusted CA store. If both devices get the guard's 403, certificate trust is a separate issue.
- API unavailable: start Atlas.Api and check its normal database/certificate setup.

Stop LAN mode by stopping Vite and removing ATLAS_LAN_ORIGIN, ATLAS_LAN_CERT and ATLAS_LAN_KEY from the shell environment. Remove the CA from your phone when no longer needed. This CA trust is powerful and is for your own development devices only.
