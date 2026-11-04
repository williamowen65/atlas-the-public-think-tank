"""Fail a manual GitHub deployment unless the newly triggered Azure WebJob succeeded.

Uses the Azure CLI's existing OIDC session to query management-plane history.
Never reads SQL connection strings, Kudu publishing profiles or application logs.
"""
import json
import os
import subprocess
import sys
import time
from datetime import datetime, timedelta, timezone


def required(name: str) -> str:
    value = os.environ.get(name)
    if not value:
        raise SystemExit(f"Missing {name}")
    return value


def instant(value: str) -> datetime:
    return datetime.fromisoformat(value.replace("Z", "+00:00"))


def main() -> int:
    job = required("ATLAS_ADMIN_JOB")
    site = required("ATLAS_ADMIN_SITE")
    group = required("ATLAS_ADMIN_RG")
    subscription = required("ATLAS_ADMIN_SUB")
    started = instant(required("ATLAS_TRIGGER_STARTED")) - timedelta(seconds=5)
    url = (
        f"https://management.azure.com/subscriptions/{subscription}/resourceGroups/{group}"
        f"/providers/Microsoft.Web/sites/{site}/triggeredwebjobs/{job}/history"
        "?api-version=2026-03-15"
    )
    for attempt in range(36):
        result = subprocess.run(
            ["az", "rest", "--method", "get", "--url", url, "--output", "json"],
            check=True, capture_output=True, text=True,
        )
        data = json.loads(result.stdout)
        runs = [
            run
            for item in data.get("value", [])
            for run in item.get("properties", {}).get("runs", [])
            if run.get("start_time")
            and instant(run["start_time"]) >= started
            and run.get("web_job_name", job) == job
        ]
        if runs:
            latest = max(runs, key=lambda row: row["start_time"])
            status = latest.get("status")
            print(f"Azure job {job}: {status}", flush=True)
            if status == "Success":
                return 0
            if status in {"Failed", "Error", "Aborted"}:
                print("Administrative job did not succeed. Inspect the Azure WebJob execution history.", file=sys.stderr)
                return 1
        elif attempt % 3 == 0:
            print(f"Waiting for the current {job} invocation to appear in Azure history.", flush=True)
        time.sleep(10)
    print("Timed out waiting for job success; do not proceed with application deployment.", file=sys.stderr)
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
