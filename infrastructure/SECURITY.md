# Atlas Azure infrastructure security

**Status:** Version-controlled configuration and CI checks, **not a verified production deployment**. Applies to PTT-169. Read [DEPLOYMENT.md](DEPLOYMENT.md) for setup and [PROVISIONED-RESOURCES.md](PROVISIONED-RESOURCES.md) for resource inventory.

## Current security baseline

| Control | Code location | What it does | Limitation |
| --- | --- | --- | --- |
| Web HTTPS-only | `terraform/modules/webapp/main.tf` | `https_only = true` and minimum TLS 1.2 | Application authorization and session/CSRF checks must also pass |
| Legacy publishing closed | Web App module | Disables FTP/FTPS and FTP basic authentication | GitHub deployment OIDC/RBAC still requires Entra and GitHub setup |
| Restricted SQL connectivity | `terraform/modules/sql/main.tf` + `sql_allowed_ipv4` input | The Azure SQL public endpoint has **zero** allowed IPs initially. Named rules admit exact individual IPv4 addresses, excluding the special Azure-wide `0.0.0.0` rule | Azure App Service outbound IPs can be shared with other tenants. Firewall reachability is not authentication |
| Encrypted SQL transport | SQL module | Sets minimum SQL TLS 1.2 | Connection strings must also require encryption and validate server certificates |
| Data isolation | Terraform `live`/`demo` variants | Separate Azure SQL databases and Web Apps; one shared App Service plan | Runtime logins must have per-database access only; the UI must label and constrain demo behavior |
| Accidental data loss guard | SQL module | `prevent_destroy = true` protects each DB in ordinary Terraform changes | Does not protect against deletion outside Terraform, privileged SQL writes, or missing backups |
| Managed identities | Web App module | Each Web App has its own system-assigned identity | The identity currently has no SQL permissions or configured Entra database authentication |
| SQL admin secret handling | Terraform variables and private backend | SQL admin password and runtime connection strings are marked sensitive | Sensitive Terraform inputs can still be present in state. Protect backend access and prefer managed identity/Key Vault later |
| Deployment access | `.github/workflows/AtlasDeploy.yml` | Manual workflow, with environment-specific short-lived GitHub OIDC credentials intended for Azure | Federated credentials, narrow Azure role grants and environment approval policy need to be provisioned before use |
| Safe telemetry | `Atlas.Api` and Azure observability module | Category-based app diagnostics and metrics via Azure Monitor; no raw SQL auto-instrumentation | Validate cloud ingestion, privacy and cost; durable audit outbox export remains incomplete |

**Public vs private:** The Atlas *website* needs a public HTTPS endpoint for users. The *database* does not need broad public access; SQL presently uses a public endpoint **with a restrictive firewall** to remain compatible with low-cost App Service. If future hosting/network tiers support it economically, use VNet integration and a SQL private endpoint for stronger network isolation.

**Important:** Allowlisting the App Service's possible outbound IPs is a practical first step, but those addresses may be shared. Only an authenticated, least-privilege SQL identity grants database access. SQL initialization and production SQL accounts must be designed so the web application is never running as the database administrator.

## Terraform CI security checks

[AtlasInfrastructure.yml](../.github/workflows/AtlasInfrastructure.yml) runs on pushes that change infrastructure files. It does **not** deploy to Azure and needs no Azure credentials.

| Check | Enforcement | Purpose |
| --- | --- | --- |
| `terraform fmt` / `init -backend=false` / `validate` | Blocking | Formatting and configuration validity |
| `infrastructure/tests/test_security_baseline.py` | **Blocking** | Parse Terraform HCL and verify HTTPS, FTP disabled, TLS minimum, system-assigned identity, exact-IP SQL allowlist, no wildcard Azure rule, SQL free-offer safety defaults, and DB deletion protection |
| Trivy Terraform misconfiguration scan | **Advisory initially** | Report high/critical broader best-practice issues without making existing intentional tradeoffs silently disappear |
| Trivy infrastructure secret scan | **Blocking** | Detect high/critical exposed secrets accidentally committed to the Terraform directory |

The baseline tests use `python-hcl2==8.1.4` and do not access Azure. Trivy uses `aquasecurity/trivy-action@v0.36.0`, with readable findings in the GitHub Actions job output. A **passing baseline test** only proves the checked-in controls still have their expected values; it does **not** prove an Azure resource is secure or that no other Terraform settings can weaken it.

### Follow-up security gates

1. Review Trivy's first findings. Confirm actual risks and Azure/free-tier constraints, and resolve them; then make the relevant HIGH/CRITICAL checks blocking. Never suppress a finding without a documented reason.
2. Add an authenticated **manual `terraform plan`** against remote state, with protected identity and no secrets or state stored in GitHub artifacts. Review it before any manually approved apply.
3. Verify **deployed configuration and drift** once Azure is running; static CI cannot see Portal-side changes or runtime permissions.
4. Provision and verify least-privilege per-variant SQL identities and OIDC access. Prefer managed-identity database authentication over storing permanent SQL connection passwords in Terraform state.
5. Add runtime security checks after deployment: HTTPS redirect, authentication/session/CSRF, API authorization, direct-link routing, audit-outbox delivery, SQL FTS readiness, and zero cross-database access.
6. Check backup/restore, telemetry retention/budget, and alerts. Revisit private SQL networking once its cost and F1 compatibility are known.

## How to review locally

From the repository root, with Python and Terraform installed:

```powershell
python -m pip install "python-hcl2==8.1.4"
python -m unittest discover -s infrastructure/tests -p "test_*.py" -v
Push-Location infrastructure/terraform
terraform fmt -check -recursive
terraform init -backend=false
terraform validate
Pop-Location
```

Trivy's full findings appear in the GitHub Actions workflow. Static scans and tests are safe to execute without provisioning resources.
