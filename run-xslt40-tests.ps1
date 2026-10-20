param(
    [string]$SuiteRoot = (Join-Path $PSScriptRoot '.upstream/xslt40-test'),
    [string]$TestSet = 'select',
    [string]$TestCase = '',
    [ValidateSet('Legacy', 'Xslt40Draft')]
    [string]$ProcessorProfile = 'Legacy',
    [switch]$SchemaAware,
    [ValidateRange(1, 86400)]
    [int]$TimeoutSeconds = 300,
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
$suitePath = (Resolve-Path -LiteralPath $SuiteRoot).Path
$catalogPath = Join-Path $suitePath 'catalog.xml'
if (-not (Test-Path -LiteralPath $catalogPath)) { throw "Missing upstream catalog: $catalogPath" }
if ([string]::IsNullOrWhiteSpace($TestSet)) { throw 'Select a test set explicitly; this wrapper does not launch an unbounded full sweep.' }
$suiteRevision = (& git -C $suitePath rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'SuiteRoot must be an independent Git checkout.' }
$suiteTop = (& git -C $suitePath rev-parse --show-toplevel).Trim()
if ([IO.Path]::GetFullPath($suiteTop).TrimEnd('/','\') -ne $suitePath.TrimEnd('/','\')) { throw 'SuiteRoot must be the root of an independent Git checkout.' }
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
$processorProfileArgument = if ($ProcessorProfile -eq 'Xslt40Draft') { 'xslt40-draft' } else { 'legacy' }
$runnerArgs += @('--', '--xslt40', '--processor-profile', $processorProfileArgument)
if ($SchemaAware) { $runnerArgs += '--schema-aware' }
$runnerArgs += @($catalogPath, $TestSet)
if ($TestCase) { $runnerArgs += $TestCase }
$receipt = [ordered]@{
    timestampUtc = [DateTime]::UtcNow.ToString('o')
    upstream = 'https://github.com/qt4cg/xslt40-test'
    suiteRoot = $suitePath
    suiteRevision = $suiteRevision
    suiteStatus = $suiteStatus
    bosakRevision = $bosakRevision
    bosakStatus = $bosakStatus
    profile = 'XSLT40 catalog dependencies; stylesheet versions retained; exact error codes'
    catalogProfile = 'Xslt40'
    processorProfile = if ($ProcessorProfile -eq 'Xslt40Draft') { 'Xslt40Draft' } else { 'Legacy' }
    runnerArguments = $runnerArgs
    schemaAware = [bool]$SchemaAware
    testSet = $TestSet
    testCase = $TestCase
    timeoutSeconds = $TimeoutSeconds
    status = 'Running'
    runnerExitCode = $null
    counts = @{}
    success = $false
    scope = 'Selected tests only. Skips and partial adapter/engine coverage are not conformance passes.'
    log = $logPath
}
function Save-Receipt {
    $temporaryReceipt = "$receiptPath.tmp"
    $receipt | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $temporaryReceipt -Encoding utf8
    Move-Item -LiteralPath $temporaryReceipt -Destination $receiptPath -Force
}
# Start-Process joins ArgumentList into a Windows command line. Quote each argument
# using the CommandLineToArgvW escaping rules, including trailing backslashes.
function Quote-ProcessArgument([string]$Value) {
    '"' + [regex]::Replace([regex]::Replace($Value, '(\\*)"', '$1$1\"'), '(\\+)$', '$1$1') + '"'
}
$workingDirectory = Join-Path $outputRoot $runId
[IO.Directory]::CreateDirectory($workingDirectory) | Out-Null
$errorPath = "$logPath.stderr"
$receipt['stderrLog'] = $errorPath
$receipt['workingDirectory'] = $workingDirectory
$process = $null
$windowsHost = [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT
Save-Receipt
try {
    $quotedArgs = ($runnerArgs | ForEach-Object { Quote-ProcessArgument $_ }) -join ' '
    $startOptions = @{ FilePath = 'dotnet'; ArgumentList = $quotedArgs; WorkingDirectory = $workingDirectory; RedirectStandardOutput = $logPath; RedirectStandardError = $errorPath; PassThru = $true }
    if ($windowsHost) { $startOptions['WindowStyle'] = 'Hidden' }
    $process = Start-Process @startOptions
    # Acquire the handle before waiting: Windows PowerShell otherwise loses the
    # exit code when Start-Process's event handler reaps a short-lived process.
    $null = $process.Handle
    $receipt['processId'] = $process.Id
    Save-Receipt
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        $receipt.status = 'TimedOut'
        $receipt.runnerExitCode = 124
        Save-Receipt
        if ($windowsHost) {
            & taskkill /PID $process.Id /T /F > "$logPath.cleanup" 2>&1
        } else {
            $process.Kill($true)
        }
        if (-not $process.WaitForExit(5000)) { throw 'Timed out process did not terminate after process-tree cleanup.' }
    } else {
        $process.WaitForExit()
        $receipt.runnerExitCode = $process.ExitCode
        $receipt.status = 'Completed'
    }
} catch {
    if ($receipt.status -ne 'TimedOut') { $receipt.status = 'InterruptedOrFailed' }
    $receipt['exception'] = $_.Exception.Message
} finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
    $log = if (Test-Path -LiteralPath $logPath) { Get-Content -LiteralPath $logPath -Raw } else { '' }
    $log = [string]$log
    foreach ($label in @('Total','Passed','Failed','Skipped','Unselected','Selected','Selected skipped','Missing test sets')) {
        $match = [regex]::Match($log, '(?m)^' + [regex]::Escape($label) + ':\s+(\d+)\s*$')
        if ($match.Success) { $receipt.counts[$label] = [int]$match.Groups[1].Value }
    }
    $c = $receipt.counts
    $validSummary = @('Total','Passed','Failed','Skipped','Unselected','Selected','Selected skipped','Missing test sets' | Where-Object { -not $c.ContainsKey($_) }).Count -eq 0
    $receipt.success = $receipt.status -eq 'Completed' -and $receipt.runnerExitCode -eq 0 -and $validSummary -and $c.Failed -eq 0 -and ($c.Passed + $c.Failed) -gt 0 -and $c['Missing test sets'] -eq 0 -and $c.Total -eq ($c.Passed + $c.Failed + $c.Skipped) -and $c.Selected -eq ($c.Total - $c.Unselected) -and $c['Selected skipped'] -eq ($c.Skipped - $c.Unselected)
    $receipt['finishedUtc'] = [DateTime]::UtcNow.ToString('o')
    Save-Receipt
}
Write-Output "Receipt: $receiptPath"
if (-not $receipt.success) { exit 1 }
