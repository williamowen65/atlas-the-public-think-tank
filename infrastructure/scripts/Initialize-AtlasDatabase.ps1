# Local/private-network operator helper only. It does NOT connect to Azure\n# SQL Private Link from a standard developer workstation or GitHub runner.\n# For protected Azure operations, invoke the Atlas.Admin WebJob via GitHub.\n# Atlas database initialization is intentionally NOT an azurerm resource.
# This is version-controlled operational code: migrations and Full-Text setup
# must be managed by the application, not as Terraform one-off provisioners.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('live','demo')]
    [string] $Variant,

    [Parameter(Mandatory = $true)]
    [string] $ExpectedDatabase,

    [switch] $SeedDemo
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($env:ATLAS_SQL_CONNECTION_STRING)) {
    throw 'ATLAS_SQL_CONNECTION_STRING must be set privately before database initialization.'
}

# Explicitly verify the target DB to avoid seeding live through a stale environment.
$parsed = [System.Data.Common.DbConnectionStringBuilder]::new()
$parsed.ConnectionString = $env:ATLAS_SQL_CONNECTION_STRING
$database = if ($parsed.ContainsKey('Initial Catalog')) {
    [string]$parsed['Initial Catalog']
} elseif ($parsed.ContainsKey('Database')) {
    [string]$parsed['Database']
} else { '' }

if ($database -cne $ExpectedDatabase -or $database -cnotlike "*-$Variant") {
    throw "Database mismatch: expected '$ExpectedDatabase' with suffix '-$Variant', got '$database'. Nothing executed."
}
if ($Variant -eq 'live' -and $SeedDemo) {
    throw 'Refusing to seed the live database. Use Variant=demo only.'
}
if ($SeedDemo -and [string]::IsNullOrWhiteSpace($env:ATLAS_DEMO_PASSWORD)) {
    throw 'Public demo seeding requires ATLAS_DEMO_PASSWORD from a secret source, not the known local fixture password.'
}

$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Push-Location $root
try {
    Write-Host "Applying checked-in EF migrations and SQL Full-Text setup to '$Variant' ($database)."
    dotnet run --project src/hosts/Atlas.Console -- --setup-database
    if ($LASTEXITCODE -ne 0) { throw 'Database setup failed; deployment halted.' }

    if ($SeedDemo) {
        Write-Host 'Seeding demo once, only if the database is initially empty.'
        dotnet run --project src/hosts/Atlas.Console -- --seed-demo
        if ($LASTEXITCODE -ne 0) { throw 'Demo seeding failed or refused because the database was not empty.' }
    }
    Write-Host 'Database initialization finished. Verify Full-Text population/queries before marking ready.'
} finally {
    Pop-Location
}
