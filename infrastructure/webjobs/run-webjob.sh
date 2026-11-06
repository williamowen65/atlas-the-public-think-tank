#!/usr/bin/env bash
# Entry point for the single App_Data/jobs/triggered/atlas-bootstrap job.
# The GitHub workflow copies this file as run.sh because Azure WebJobs
# recognizes that exact filename as its explicit entry point.
# Schema and demo seeding are explicit SSH commands in tools/atlas-admin.
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

: "${ATLAS_BOOTSTRAP_SQL_USERNAME:?First-time bootstrap not enabled in Terraform}"
: "${ATLAS_BOOTSTRAP_SQL_PASSWORD:?First-time bootstrap not enabled in Terraform}"
exec dotnet ./Atlas.Admin.dll --bootstrap-cloud-identities
