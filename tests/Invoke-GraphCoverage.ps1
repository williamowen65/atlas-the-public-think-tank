# Run from any directory: powershell -File tests/Invoke-GraphCoverage.ps1
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
Push-Location $repoRoot
try {
    $runName = 'graph-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
    $runDirectory = Join-Path $repoRoot "artifacts/test-reports/$runName"
    $resultsDirectory = Join-Path $runDirectory 'results'
    $reportDirectory = Join-Path $runDirectory 'html'
    New-Item -ItemType Directory -Force -Path $resultsDirectory | Out-Null
    @(
        'Scope: Graph tests; coverage includes Atlas.Graph and Atlas.Contracts only.',
        'Not an application-wide or requirements coverage report.',
        "Created: $(Get-Date -Format o)",
        "Commit: $(git rev-parse HEAD)",
        'Working tree status:',
        (git status --short),
        '.NET environment:',
        (dotnet --info)
    ) | Set-Content (Join-Path $runDirectory 'run-info.txt')

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'Tool restore failed.' }

    dotnet test tests/Atlas.Graph.Tests/Atlas.Graph.Tests.csproj --configuration Release --settings tests/graph-coverage.runsettings --collect:"XPlat Code Coverage" --logger "trx;LogFileName=graph.trx" --results-directory $resultsDirectory
    $testExitCode = $LASTEXITCODE
    $coverageFiles = @(Get-ChildItem $resultsDirectory -Filter coverage.cobertura.xml -Recurse)
    $trxFiles = @(Get-ChildItem $resultsDirectory -Filter '*.trx' -Recurse)
    if ($trxFiles.Count -eq 0) { throw "No TRX test results produced. Inspect test output. Artifacts: $runDirectory" }
    if ($coverageFiles.Count -eq 0) { throw "No coverage collected. This is not zero coverage. Inspect collector/test output. Artifacts: $runDirectory" }

    $reportsArgument = '-reports:' + (($coverageFiles | ForEach-Object { $_.FullName }) -join ';')
    dotnet tool run reportgenerator $reportsArgument "-targetdir:$reportDirectory" "-reporttypes:Html;Cobertura;TextSummary"
    if ($LASTEXITCODE -ne 0) { throw 'Coverage report generation failed.' }

    [xml]$trx = Get-Content $trxFiles[0].FullName -Raw
    $counters = $trx.TestRun.ResultSummary.Counters
    Write-Host "Tests: total=$($counters.total), passed=$($counters.passed), failed=$($counters.failed), notExecuted=$($counters.notExecuted)"
    Write-Host "Coverage report: $(Join-Path $reportDirectory 'index.html')"
    Write-Host "Test results: $($trxFiles[0].FullName)"
    Write-Host "Run metadata: $(Join-Path $runDirectory 'run-info.txt')"
    # Preserve failures even when a useful report could still be generated.
    if ($testExitCode -ne 0) { exit $testExitCode }
}
finally {
    Pop-Location
}
