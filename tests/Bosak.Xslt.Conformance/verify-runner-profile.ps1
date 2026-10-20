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
Assert-Runner 'filtered-accounting' @('--xslt40','--processor-profile','xslt40-draft',$catalogPath,'processor-profile-controls','profile-common') 0 @(1,0,4)
$filtered = Get-Content -LiteralPath (Join-Path $OutputDirectory 'filtered-accounting.log') -Raw
foreach ($line in @('Unselected: 4','Selected: 1','Selected skipped: 0','Missing test sets: 0')) {
    if ($filtered -notmatch ('(?m)^' + [regex]::Escape($line) + '\s*$')) { throw "Missing accounting: $line" }
}
$missingCatalog = Join-Path $OutputDirectory 'missing-catalog.xml'
$fixtureDirectory = [Security.SecurityElement]::Escape((Split-Path $catalogPath))
[IO.File]::WriteAllText($missingCatalog, @"
<catalog xmlns="http://www.w3.org/2012/10/xslt-test-catalog">
  <test-set name="valid" file="$fixtureDirectory/_profile-test-set.xml"/>
  <test-set name="missing" file="does-not-exist.xml"/>
</catalog>
"@)
Assert-Runner 'missing-selected-set' @('--xslt40','--processor-profile','xslt40-draft',$missingCatalog) 1 @(5,0,0)
Assert-Runner 'missing-unselected-set' @('--xslt40','--processor-profile','xslt40-draft',$missingCatalog,'valid') 0 @(5,0,0)
$dependencySet = Join-Path $OutputDirectory 'dependency-set.xml'
$dependencyCatalog = Join-Path $OutputDirectory 'dependency-catalog.xml'
$dependencyDocument = [xml](Get-Content -LiteralPath (Join-Path (Split-Path $catalogPath) '_profile-test-set.xml') -Raw)
$dependency = $dependencyDocument.CreateElement('dependencies', 'http://www.w3.org/2012/10/xslt-test-catalog')
$feature = $dependencyDocument.CreateElement('feature', 'http://www.w3.org/2012/10/xslt-test-catalog')
$feature.SetAttribute('value', 'dynamic-evaluation')
$null = $dependency.AppendChild($feature)
$null = $dependencyDocument.DocumentElement.PrependChild($dependency)
$dependencyDocument.Save($dependencySet)
[IO.File]::WriteAllText($dependencyCatalog, '<catalog xmlns="http://www.w3.org/2012/10/xslt-test-catalog"><test-set name="dependency" file="dependency-set.xml"/></catalog>')
Assert-Runner 'filtered-set-dependency' @('--xslt40','--processor-profile','xslt40-draft',$dependencyCatalog,'dependency','profile-common') 1 @(0,0,5)
$dependencyLog = Get-Content -LiteralPath (Join-Path $OutputDirectory 'filtered-set-dependency.log') -Raw
foreach ($line in @('Unselected: 4','Selected: 1','Selected skipped: 1')) {
    if ($dependencyLog -notmatch ('(?m)^' + [regex]::Escape($line) + '\s*$')) { throw "Incorrect dependency accounting: $line" }
}
$structuredCatalog = Join-Path $PSScriptRoot 'Fixtures/StructuredErrors/catalog.xml'
Assert-Runner 'structured-errors-legacy' @('--xslt40','--processor-profile','legacy',$structuredCatalog) 1 @(3,4,0)
Assert-Runner 'structured-errors-draft' @('--xslt40','--processor-profile','xslt40-draft',$structuredCatalog) 1 @(3,4,0)
foreach ($lane in @('legacy','draft')) {
    $structuredLog = Get-Content -LiteralPath (Join-Path $OutputDirectory "structured-errors-$lane.log") -Raw
    foreach ($case in @('default-match','custom-match','empty-match')) {
        if ($structuredLog -notmatch ('(?m)^\s+PASS ' + [regex]::Escape($case) + '\s*$')) { throw "Missing structured match: $lane/$case" }
    }
    foreach ($case in @('wrong-local','wrong-namespace','standard-lookalike','message-spoof')) {
        if ($structuredLog -notmatch ('(?m)^\s+FAIL ' + [regex]::Escape($case) + ':')) { throw "Missing structured rejection: $lane/$case" }
    }
}
Write-Output "Verified $script:verified runner profile controls including exact structured error controls."
# The last negative control intentionally exits 1. CI dot-sources this script
# and otherwise propagates that stale native status despite every assertion passing.
exit 0
