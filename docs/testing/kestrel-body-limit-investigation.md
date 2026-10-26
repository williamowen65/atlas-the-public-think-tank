# Kestrel body-limit test investigation

Tracking: [PTT-152](https://thepublicthinktank.atlassian.net/browse/PTT-152).
[Evidence notebook](https://thepublicthinktank.atlassian.net/wiki/spaces/PTT/pages/8355841).

## Scope

Diagnose only `HttpAbuseTests.Kestrel_rejects_known_length_and_chunked_oversized_bodies_and_accepts_the_boundary` in `tests/Atlas.Api.Tests/HttpAbuseTests.cs`. This draft starts with an investigation record, not a claimed fix.

Branch starts at main commit `3cb1016deed7332de780d866e7769e00a04f3c54`. Original Windows/CI comparison used `d8c7817d443836c0b2e44a78350d2a4794369932`; do not present different commits as a controlled comparison.

## Reported failure and known evidence

Windows reports HttpRequestException: “The SSL connection could not be established,” with inner IOException: “Received an unexpected EOF or 0 bytes from the transport stream.” The reported source line is the initial GET of /api/auth/csrf.

This test is discovered and starts; the reported problem is an HTTPS transport failure before body-size assertions, not evidence that the test is omitted.

At the original baseline, Linux [Test API job 121697294630](https://github.com/williamowen65/atlas-the-public-think-tank/actions/runs/40219393410/job/121697294630) explicitly records this method passing (252 ms).

Captured Windows environment: Windows 10.0.26300 win-x64, SDK 10.0.402, host and installed ASP.NET/Core runtime 10.0.13. Inspected Linux job: Ubuntu 26.04.1, SDK 10.0.403, runtime installation 10.0.14. Actual runtime selected during each reproduction must be recorded.

## Source review

The method creates an RSA key and self-signed certificate, starts a real loopback Kestrel HTTPS listener, and obtains its assigned address. A test-only client accepts only that certificate's fingerprint. It first obtains a CSRF token, then checks:

- 65,537-byte known-length body returns 413 with a safe error body.
- 65,537-byte unknown-length/chunked body returns 413 with a safe error body.
- Exactly 65,536 bytes returns 204.

Client certificate acceptance cannot establish that the server successfully used its private key. Certificate/private-key handling by the server on Windows is a hypothesis to investigate, not a confirmed cause. Other possibilities include TLS configuration, connection lifecycle, and runtime differences. The generated certificate means trusting the user's development certificate is not an established remedy.

## First reproduction

From repository root on this branch, record `git rev-parse HEAD`, `git status --short` and `dotnet --info`, then run:

```powershell
dotnet test tests/Atlas.Api.Tests/Atlas.Api.Tests.csproj --configuration Release --filter "FullyQualifiedName~Kestrel_rejects_known_length_and_chunked_oversized_bodies_and_accepts_the_boundary" --logger "console;verbosity=detailed"
```

Record exact command, SHA, selected runtime, test count, complete exception chain and failing phase. Repeat on Linux at the same SHA. If isolated execution passes, compare with the full API suite before changing dependencies.

## Diagnostic work and completion criteria

1. Identify the failing phase: server startup, initial TLS/CSRF GET, known-length rejection, chunked rejection, or boundary acceptance.
2. Capture bounded test-only Kestrel/HTTPS diagnostics around the failure. Preserve the original exception; do not log private keys or secrets.
3. Test one evidence-backed hypothesis at a time, recording before/after commit, environment and result.
4. If a fix is identified, preserve HTTPS, certificate fingerprint validation and all three body-limit assertions.
5. Verify the focused test and full API suite on Windows and Linux; distinguish SQL-dependent skips from executed tests.
6. Document the confirmed cause, fix, successful comparisons and unresolved limits before marking the draft ready.

Do not replace HTTPS with HTTP, accept all certificates, skip Windows, or add retries solely to obtain a pass.

## Status

Initial source review complete. No new reproduction or root-cause confirmation yet. No executable test or production behavior changed in this initial commit.
