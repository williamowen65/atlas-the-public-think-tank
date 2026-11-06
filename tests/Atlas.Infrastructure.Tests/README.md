# Atlas infrastructure tests

These checks run without provisioning Azure. Terraform and operational scripts live
in [infrastructure](../../infrastructure/README.md).

## What is tested

| Area | Checks |
| --- | --- |
| Terraform policy | Private SQL, TLS, VNet integration, identities and free-offer settings |
| Admin tools | Packaging, target checks, bootstrap confirmation and entry point |
| Target selection | Branch rules, profiles, names, state keys and application defaults |
| Website readiness | Pending deployment versus strict verify, placeholder rejection and bounded requests |
| Plan guards | Variant removal, deletion/replacement and secret-safe diagnostics |
| Cleanup | Rehearsal scope, exact confirmation, destroy-only plans and temporary overrides |

Script tests use stand-in CLI commands rather than Azure or SQL.
With Terraform on PATH, a local built-in-resource fixture also exercises lifecycle
protection and the cleanup override.

## Run locally

From the repository root:

```powershell
python -m pip install "python-hcl2==8.1.4" "PyYAML==6.0.3"
python -m unittest discover -s tests/Atlas.Infrastructure.Tests -p "test_*.py" -v
```

These are Python `unittest` tests, not a .NET test project. They do not run through
`dotnet test` or Visual Studio Test Explorer.

## What CI adds

[Terraform validation](../../.github/workflows/AtlasInfrastructure.yml) also runs
formatting, provider initialization without remote state, Terraform validation,
admin compilation and Trivy checks.

Those checks are separate from the Python assertions.
A passing offline run does not prove Azure deployment or runtime behavior.
For the Azure-connected preview, use [Deployment overview](../../infrastructure/DEPLOYMENT.md).
