param(
    [string]$Configuration = 'Release',
    [string]$OutputDirectory = ''
)
$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repositoryRoot '.artifacts/runner-profile-smoke' }
[IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null
$assemblyPath = Join-Path $PSScriptRoot "bin/$Configuration/net10.0/Bosak.Xslt.Conformance.dll"
if (-not (Test-Path -LiteralPath $assemblyPath)) { throw 'Build the conformance project before running these controls.' }
$catalogPath = Join-Path $PSScriptRoot 'Fixtures/ProcessorProfile/catalog.xml'
$adapterCatalogPath = Join-Path $PSScriptRoot 'Fixtures/Xslt40Adapter/catalog.xml'
$script:verified = 0
function Assert-Runner {
    param([string]$Name, [string[]]$RunnerArguments, [int]$ExpectedExit, [int[]]$Counts = @(), [string]$Profile = '')
    $outputLines = @(& dotnet $assemblyPath @RunnerArguments 2>&1)
    $actualExit = $LASTEXITCODE
    $outputText = $outputLines -join [Environment]::NewLine
    [IO.File]::WriteAllText((Join-Path $OutputDirectory ($Name + '.log')), $outputText)
    if ($actualExit -ne $ExpectedExit) { throw "$Name expected exit $ExpectedExit, got $actualExit. See its retained log." }
    if ($Counts.Count -gt 0) {
        $labels = @('Passed', 'Failed', 'Skipped')
        for ($index = 0; $index -lt 3; $index++) {
            if ($outputText -notmatch ('(?m)^' + $labels[$index] + ':[ ]+' + $Counts[$index] + '(?![0-9])')) { throw "$Name has unexpected $($labels[$index]) count." }
        }
    }
    if ($Profile -and $outputText -notmatch ('Processor profile: ' + [regex]::Escape($Profile))) { throw "$Name omitted the processor profile." }
    $script:verified++
    Write-Output "PASS $Name"
}
Assert-Runner 'default-legacy' @('--xslt40', $catalogPath) 1 @(2,3,0)
Assert-Runner 'explicit-legacy' @('--processor-profile','legacy','--xslt40',$catalogPath) 1 @(2,3,0) 'Legacy'
Assert-Runner 'draft' @('--xslt40','--processor-profile','xslt40-draft',$catalogPath) 0 @(5,0,0) 'Xslt40Draft'
Assert-Runner 'draft-independent-catalog' @('--processor-profile','xslt40-draft',$catalogPath) 0 @(4,0,1) 'Xslt40Draft'
Assert-Runner 'draft-empty-selection' @('--processor-profile','xslt40-draft',$catalogPath,'processor-profile-controls','no-such-case') 1 @(0,0,5) 'Xslt40Draft'
Assert-Runner 'invalid-profile' @('--processor-profile','invalid',$catalogPath) 2
Assert-Runner 'missing-profile' @('--processor-profile') 2
Assert-Runner 'duplicate-profile' @('--processor-profile','legacy','--processor-profile','xslt40-draft',$catalogPath) 2
Assert-Runner 'adapter-legacy' @('--xslt40',$adapterCatalogPath) 0 @(2,0,0)
Assert-Runner 'adapter-draft' @('--xslt40','--processor-profile','xslt40-draft',$adapterCatalogPath) 0 @(2,0,0) 'Xslt40Draft'
Write-Output "Verified $script:verified runner profile controls."
