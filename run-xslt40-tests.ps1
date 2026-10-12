param(
    [string]$SuiteRoot = (Join-Path $PSScriptRoot '.upstream/xslt40-test'),
    [string]$TestSet = 'select',
    [string]$TestCase = '',
    [switch]$SchemaAware,
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
$suitePath = (Resolve-Path -LiteralPath $SuiteRoot).Path
$catalogPath = Join-Path $suitePath 'catalog.xml'
if (-not (Test-Path -LiteralPath $catalogPath)) { throw "Missing upstream catalog: $catalogPath" }
if ([string]::IsNullOrWhiteSpace($TestSet)) { throw 'Select a test set explicitly; this wrapper does not launch an unbounded full sweep.' }
$suiteRevision = (& git -C $suitePath rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'SuiteRoot must be an independent Git checkout.' }
$suiteStatus = @(& git -C $suitePath status --porcelain)
if ($LASTEXITCODE -ne 0) { throw 'Unable to inspect suite status.' }
$bosakRevision = (& git -C $PSScriptRoot rev-parse HEAD).Trim()
$bosakStatus = @(& git -C $PSScriptRoot status --porcelain)
$outputRoot = Join-Path $PSScriptRoot '.artifacts/xslt40'
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$runId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
$logPath = Join-Path $outputRoot "$runId.log"
$receiptPath = Join-Path $outputRoot "$runId.json"
$runnerArgs = @('run', '--project', (Join-Path $PSScriptRoot 'tests/Bosak.Xslt.Conformance'), '-c', 'Release')
if ($NoBuild) { $runnerArgs += '--no-build' }
$runnerArgs += @('--', '--xslt40')
if ($SchemaAware) { $runnerArgs += '--schema-aware' }
$runnerArgs += @($catalogPath, $TestSet)
if ($TestCase) { $runnerArgs += $TestCase }
& dotnet @runnerArgs 2>&1 | Tee-Object -FilePath $logPath
$runnerExit = $LASTEXITCODE
$log = Get-Content -LiteralPath $logPath -Raw
$counts = @{}
foreach ($label in @('Total','Passed','Failed','Skipped')) {
    $match = [regex]::Match($log, "(?m)^" + $label + ":\s+(\d+)\s*$")
    if ($match.Success) { $counts[$label] = [int]$match.Groups[1].Value }
}
$validSummary = $counts.Count -eq 4
$success = $runnerExit -eq 0 -and $validSummary -and $counts.Failed -eq 0 -and ($counts.Passed + $counts.Failed) -gt 0
$receipt = [ordered]@{
    timestampUtc = [DateTime]::UtcNow.ToString('o')
    upstream = 'https://github.com/qt4cg/xslt40-test'
    suiteRoot = $suitePath
    suiteRevision = $suiteRevision
    suiteStatus = $suiteStatus
    bosakRevision = $bosakRevision
    bosakStatus = $bosakStatus
    profile = 'XSLT40 catalog dependencies; stylesheet versions retained; exact error codes'
    schemaAware = [bool]$SchemaAware
    testSet = $TestSet
    testCase = $TestCase
    runnerExitCode = $runnerExit
    counts = $counts
    success = $success
    scope = 'Selected tests only. Skips and partial adapter/engine coverage are not conformance passes.'
    log = $logPath
}
$receipt | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $receiptPath -Encoding utf8
Write-Output "Receipt: $receiptPath"
if (-not $success) { exit 1 }
