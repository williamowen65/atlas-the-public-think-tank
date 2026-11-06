# Atlas infrastructure tests

Infrastructure checks live here with the other Atlas test suites. The deployed Terraform modules and operational scripts remain in `infrastructure/`.

## Run locally

From the repository root:

```powershell
python -m pip install "python-hcl2==8.1.4" "PyYAML==6.0.3"
python -m unittest discover -s tests/Atlas.Infrastructure.Tests -p "test_*.py" -v
```

The admin workflow tests exercise packaging, publication target checks, bootstrap confirmation, and the bootstrap entrypoint using stand-in CLI commands without contacting Azure or SQL.

Deployment-target tests also verify branch restrictions, disjoint rehearsal naming/state, app-role checks and tracked profile resolution, custom rehearsal names, application-set defaults, shared-credential routing and workflow output wiring. Terraform JSON tests cover removal of existing variants, deletion/replacement plans and secret-safe diagnostics.

The policy tests inspect Terraform HCL declarations for Atlas-specific rules (private SQL endpoint, TLS, VNet integration, managed identities, free-offer settings). They are **Python unittest tests**, not a .NET/MSTest `.csproj`; they run in GitHub Actions through [AtlasInfrastructure.yml](../../.github/workflows/AtlasInfrastructure.yml), not through `dotnet test` or Visual Studio Test Explorer.

The CI workflow also runs `terraform fmt -check`, `terraform init -backend=false`, `terraform validate`, and Trivy checks. Those are separate from the Python assertions. None of these offline checks applies infrastructure or verifies Azure runtime behavior. For an authenticated preview, see the [deployment runbook](../../infrastructure/DEPLOYMENT.md).
