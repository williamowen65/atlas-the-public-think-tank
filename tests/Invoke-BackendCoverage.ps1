$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
Push-Location $repoRoot
try {
    $runName = 'backend-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
    $runDirectory = Join-Path $repoRoot "artifacts/test-reports/$runName"
    $reportDirectory = Join-Path $runDirectory 'html'
    New-Item -ItemType Directory -Force -Path $runDirectory | Out-Null
    $sqlConfigured = -not [string]::IsNullOrWhiteSpace($env:ATLAS_SQL_TEST_CONNECTION_STRING)
    @(
        'Scope: rewrite backend tests; merged Atlas.* application coverage; excludes test assemblies and obj files.',
        'Assemblies absent from collection are listed separately and do not enter the measured percentage.',
        "Created: $(Get-Date -Format o)", "Commit: $(git rev-parse HEAD)",
        "SQL test environment variable configured: $sqlConfigured",
        'Working tree status:', (git status --short), '.NET environment:', (dotnet --info)
    ) | Set-Content (Join-Path $runDirectory 'run-info.txt')
    if (-not $sqlConfigured) { Write-Warning 'SQL prerequisites not configured in the environment. Review skipped SQL tests; this may be a partial baseline.' }
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'Tool restore failed.' }
    $selection = Get-Content tests/Atlas.Rewrite.Tests.slnf -Raw | ConvertFrom-Json
    $failed = $false
    $rows = @()
    foreach ($relativeProject in $selection.solution.projects) {
        $project = $relativeProject.Replace('\', '/')
        $name = [IO.Path]::GetFileNameWithoutExtension($project)
        $results = Join-Path $runDirectory "results/$name"
        dotnet test $project --configuration Release --settings tests/backend-coverage.runsettings --collect:"XPlat Code Coverage" --logger "trx;LogFileName=$name.trx" --results-directory $results
        $code = $LASTEXITCODE
        if ($code -ne 0) { $failed = $true }
        $trxPath = Join-Path $results "$name.trx"
        $coverage = @(Get-ChildItem $results -Filter coverage.cobertura.xml -Recurse -ErrorAction SilentlyContinue)
        if (-not (Test-Path $trxPath) -or $coverage.Count -eq 0) { $failed = $true }
        if (Test-Path $trxPath) {
            [xml]$trx = Get-Content $trxPath -Raw
            $counts = $trx.TestRun.ResultSummary.Counters
            $rows += [pscustomobject]@{ Project=$name; ExitCode=$code; Total=$counts.total; Executed=$counts.executed; Passed=$counts.passed; Failed=$counts.failed; NotExecuted=$counts.notExecuted; CoverageFiles=$coverage.Count }
        } else {
            $rows += [pscustomobject]@{ Project=$name; ExitCode=$code; Total='missing'; Executed='missing'; Passed='missing'; Failed='missing'; NotExecuted='missing'; CoverageFiles=$coverage.Count }
        }
    }
    $rows | Export-Csv (Join-Path $runDirectory 'test-summary.csv') -NoTypeInformation
    $coverageFiles = @(Get-ChildItem (Join-Path $runDirectory 'results') -Filter coverage.cobertura.xml -Recurse)
    if ($coverageFiles.Count -eq 0) { throw "No coverage collected. Inspect test-summary.csv and logs in $runDirectory" }
    $reportsArgument = '-reports:' + (($coverageFiles | ForEach-Object { $_.FullName }) -join ';')
    dotnet tool run reportgenerator $reportsArgument "-targetdir:$reportDirectory" "-reporttypes:Html;Cobertura;TextSummary"
    if ($LASTEXITCODE -ne 0) { throw 'Report generation failed.' }
    [xml]$merged = Get-Content (Join-Path $reportDirectory 'Cobertura.xml') -Raw
    $measured = @($merged.coverage.packages.package | ForEach-Object { $_.name })
    $inventory = Get-ChildItem src -Filter 'Atlas.*.csproj' -Recurse | ForEach-Object {
        $name = $_.BaseName
        [pscustomobject]@{ Assembly=$name; IncludedInReport=($measured -contains $name); Note=$(if ($measured -contains $name) { 'Measured; inspect line/branch coverage' } else { 'UNMEASURED: not included in aggregate denominator; review missing tests or source-less assembly' }) }
    }
    $inventory | Export-Csv (Join-Path $runDirectory 'assembly-inventory.csv') -NoTypeInformation
    $rows | Format-Table -AutoSize
    Write-Host "Coverage report: $(Join-Path $reportDirectory 'index.html')"
    Write-Host "Per-project results and inventory: $runDirectory"
    if ($failed) { Write-Error 'One or more test projects failed or did not produce required evidence. The report is partial.' }
}
finally { Pop-Location }
