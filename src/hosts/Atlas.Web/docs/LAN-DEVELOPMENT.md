# Phone testing over local HTTPS

Normal desktop development is unchanged. LAN authentication is opt-in. The API remains at https://localhost:7240; the phone connects only to Vite.

## Windows setup (PowerShell)

Install [mkcert](https://github.com/FiloSottile/mkcert#installation), then run:

```powershell
mkcert -install
ipconfig
```

Find the active Wi-Fi/Ethernet IPv4 address. Substitute it below. Store certificates outside the repository:

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
Environment variables are read from the shell, not automatically from .env.local.
The configured port is strict so the printed origin remains correct.

## Trust on the phone

Run `mkcert -CAROOT` to locate the CA folder. Transfer **only rootCA.pem** to your phone using a trusted file-transfer method. If needed, copy it with a .crt extension. Never transfer rootCA-key.pem or atlas-key.pem, and never commit certificates or keys.

- iPhone/iPad: install the downloaded certificate profile in Settings (General → VPN & Device Management), then enable full trust under General → About → Certificate Trust Settings. See [Apple's instructions](https://support.apple.com/en-us/102390).
- Android: install it as a CA certificate through Settings → Security/Privacy → Encryption & credentials → Install a certificate → CA certificate. Names vary by device. This setup targets browser testing, not native Android apps.

Connect both devices to the same network. Open **https://192.168.1.25:5173** using your actual address. There should be no certificate warning. Do not use the old HTTP URL for signed-in testing.

Check node loading, vote totals/reactions, login, vote/change/undo, logout, and signed-out state after reload. Allow TCP 5173 on your computer's private-network firewall; avoid guest Wi-Fi client isolation.

## Reuse and troubleshooting

The phone trusts this local CA, not every development server automatically. Reuse the same mkcert CA to issue certificates for other projects. The same server certificate can also serve different ports/projects on the same listed IP/hostname. New names/IPs require a new server certificate, but no new phone trust installation while the CA remains the same. Certificates expire; renewed server certificates from the same trusted CA do not normally require reinstalling the CA. A new CA, phone reset or removed trust requires installation again.

If your PC address changes, regenerate the server certificate for the new IP and update ATLAS_LAN_ORIGIN. A DHCP reservation can keep the address stable.

- Connection refused: verify address, port, firewall and Wi-Fi.
- Certificate warning: verify the certificate's IP and phone CA trust.
- Session bridge 403: use the exact configured HTTPS origin, including port.
- API unavailable: start Atlas.Api and check its normal database/certificate setup.

Stop LAN mode by stopping Vite and removing ATLAS_LAN_ORIGIN, ATLAS_LAN_CERT and ATLAS_LAN_KEY from the shell environment. Remove the CA from your phone when no longer needed. This CA trust is powerful and is for your own development devices only.
