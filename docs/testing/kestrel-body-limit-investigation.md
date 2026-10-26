# Kestrel body-limit test investigation

Tracking: [PTT-152](https://thepublicthinktank.atlassian.net/browse/PTT-152).
[Evidence notebook](https://thepublicthinktank.atlassian.net/wiki/spaces/PTT/pages/8355841).
[PR #370](https://github.com/williamowen65/atlas-the-public-think-tank/pull/370).

## Observed failure

The original HTTPS version of
`HttpAbuseTests.Kestrel_rejects_known_length_and_chunked_oversized_bodies_and_accepts_the_boundary`
failed on Windows at the initial CSRF GET with an SSL unexpected-EOF exception.
The user confirmed a loopback HTTPS address, HasPrivateKey=true, and that the
client certificate-validation callback was never reached. The exact server-side
TLS cause remains unconfirmed.

Original baseline: `d8c7817d443836c0b2e44a78350d2a4794369932`.
Linux [Test API job](https://github.com/williamowen65/atlas-the-public-think-tank/actions/runs/40219393410/job/121697294630)
records this method passing (252 ms). Windows used SDK 10.0.402 and installed
.NET 10 runtime 10.0.13; inspected CI used SDK 10.0.403 and runtime 10.0.14.
This investigation branch originally started at main
`3cb1016deed7332de780d866e7769e00a04f3c54`.

## Historical HTTP experiment (superseded)

At the user's request, the intermediate test used real loopback Kestrel over HTTP to
isolate body-size enforcement from certificate/TLS setup. This is a test-design
change, not a demonstrated fix for Windows TLS.

Only this derived factory changes:
- HTTPS redirection uses the middleware's -1 no-port sentinel.
- The antiforgery cookie uses a test-only name without the __Host- prefix and
  permits HTTP. Real cookie/token validation stays enabled.
- The client does not follow redirects, so accidental redirects fail visibly.

A small tokenless POST must return 400 with the CSRF error. With the paired
cookie and request token, all original size checks remain:
- 65,537-byte known-length body returns 413 with a safe error body.
- 65,537-byte unknown-length/chunked body returns 413 with a safe error body.
- Exactly 65,536 bytes returns 204.

Production settings and shared factories remain unchanged. Existing redirect,
HSTS, secure-cookie and CSRF tests remain in place; their in-memory hosting does
not prove a real TLS handshake. This test no longer provides that handshake
coverage either. HTTPS framing and HTTP headers do not consume the body limit;
the body itself is measured in bytes, not character count.

## Verification

From repository root on `PTT-152-test-consistency`, record SHA, working-tree
changes and dotnet --info, then run:

```powershell
dotnet test tests/Atlas.Api.Tests/Atlas.Api.Tests.csproj --configuration Release --filter "FullyQualifiedName~Kestrel_rejects_known_length_and_chunked_oversized_bodies_and_accepts_the_boundary" --logger "console;verbosity=detailed"
```

Then run the full API project and compare Windows/Linux results at the same
commit. Record passes, failures and SQL-dependent skips separately.

Status of the HTTP experiment: implementation/source reviewed. This editing environment has no dotnet
executable; compilation, CI and Windows execution remain pending. Do not mark
the discrepancy resolved until the updated test executes successfully in both.

## Resume HTTPS diagnosis

The user chose to return to HTTPS and collect the server's error before deciding
on a certificate fix. The test now restores the original generated certificate,
fingerprint validation, real HTTPS listener, secure CSRF setup and original
three body-size assertions. HTTP-only cookie/redirection overrides are removed.

Only this test's derived host enables Debug logging for
Microsoft.AspNetCore.Server.Kestrel (including its Https category) and console
output with timestamps. No production logging settings change. This is a
diagnostic change, not a confirmed certificate fix.

After pulling the branch, run the focused command above. In Visual Studio,
open the failed test's Output/Standard Output in Test Explorer. For a command
line run, the detailed console logger above should surface captured output.
Copy the Kestrel handshake failure entry and its full inner exception, together
with the client exception and SHA. Look for HTTPS connection/handshake errors;
the exact wording depends on the runtime. No server-side cause has yet been
captured, so do not infer one from a missing callback alone.

The error may have been filtered out at the previous Information log level;
there is no guarantee it was previously saved elsewhere. If this run still
omits the server exception, the next step is an explicitly captured test logger
or .NET networking trace. Avoid posting any private-key material.

No test execution was possible in the editing environment (dotnet unavailable).
Windows and CI validation of this diagnostic revision remain pending.
