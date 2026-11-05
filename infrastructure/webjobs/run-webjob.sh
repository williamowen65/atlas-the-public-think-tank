#!/usr/bin/env bash
# Source for the entry point packaged under each App_Data/jobs/triggered/atlas-*/.
# The GitHub workflow copies this file as run.sh because Azure WebJobs
# recognizes that exact filename as its explicit entry point.
# The package records the operation because Kudu can run from a temporary path.
set -euo pipefail
cd "$(dirname "$0")"

: "${ATLAS_DEPLOYMENT_VARIANT:?Missing deployment variant}"
: "${ATLAS_EXPECTED_DATABASE:?Missing expected database}"
: "${ATLAS_SQL_CONNECTION_STRING:?Missing managed-identity SQL connection}"
case "$ATLAS_DEPLOYMENT_VARIANT" in
    live|demo) ;;
    *) echo "Invalid deployment variant" >&2; exit 1 ;;
esac

if [[ "$ATLAS_EXPECTED_DATABASE" != *-"$ATLAS_DEPLOYMENT_VARIANT" ]] ||
   [[ "$ATLAS_SQL_CONNECTION_STRING" != *"Database=$ATLAS_EXPECTED_DATABASE;"* ]] ||
   [[ "$ATLAS_SQL_CONNECTION_STRING" != *"Authentication=Active Directory Managed Identity;"* ]]; then
    echo "Database or identity configuration mismatch; refusing to execute." >&2
    exit 1
fi

IFS= read -r job < ./job-name
case "$job" in
    atlas-diagnose)
        : "${ATLAS_BOOTSTRAP_SQL_USERNAME:?Diagnostic requires temporary bootstrap credentials}"
        : "${ATLAS_BOOTSTRAP_SQL_PASSWORD:?Diagnostic requires temporary bootstrap credentials}"
        exec dotnet ./Atlas.Admin.dll --diagnose-cloud-connection
        ;;
    atlas-bootstrap)
        : "${ATLAS_BOOTSTRAP_SQL_USERNAME:?First-time bootstrap not enabled in Terraform}"
        : "${ATLAS_BOOTSTRAP_SQL_PASSWORD:?First-time bootstrap not enabled in Terraform}"
        exec dotnet ./Atlas.Admin.dll --bootstrap-cloud-identities
        ;;
    atlas-schema)
        exec dotnet ./Atlas.Admin.dll --setup-database
        ;;
    atlas-seed-demo)
        if [[ "$ATLAS_DEPLOYMENT_VARIANT" != "demo" ]]; then
            echo "Seeding is forbidden for live." >&2
            exit 1
        fi
        : "${ATLAS_DEMO_PASSWORD:?Demo seed password is required}"
        exec dotnet ./Atlas.Admin.dll --seed-demo
        ;;
    *)
        echo "Unknown WebJob; refusing to run." >&2
        exit 1
        ;;
esac
