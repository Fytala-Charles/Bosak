param([switch]$NoBuild)
$ErrorActionPreference = 'Stop'
$fixtureRoot = Join-Path $PSScriptRoot 'tests/Bosak.Xslt.Conformance/Fixtures/Xslt40Adapter'
$catalog = Join-Path $fixtureRoot 'catalog.xml'
$outputRoot = Join-Path $PSScriptRoot '.artifacts/xslt40/controls'
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$runArgs = @('run','--project',(Join-Path $PSScriptRoot 'tests/Bosak.Xslt.Conformance'),'-c','Release')
if ($NoBuild) { $runArgs += '--no-build' }
function Invoke-Control([string]$Name, [string[]]$Tail, [int]$ExpectedExit, [string]$ExpectedSummary) {
    $logPath = Join-Path $outputRoot "$Name.log"
    & dotnet @runArgs -- @Tail 2>&1 | Set-Content -LiteralPath $logPath
    $code = $LASTEXITCODE
    $log = Get-Content -LiteralPath $logPath -Raw
    if ($code -ne $ExpectedExit -or $log -notmatch $ExpectedSummary) { throw "Adapter control $Name failed; see $logPath" }
    Write-Output "PASS adapter control: $Name"
}
Invoke-Control 'positive-and-exact-error' @('--xslt40',$catalog,'xslt40-adapter-controls') 0 '2 passed, 0 failed, 0 skipped'
Invoke-Control 'legacy-profile-skips40' @($catalog,'xslt40-adapter-controls') 0 '0 passed, 0 failed, 2 skipped'
Invoke-Control 'zero-selection-is-failure' @('--xslt40',$catalog,'no-such-set') 1 'Total:\s+0'
[xml]$negativeSet = Get-Content -LiteralPath (Join-Path $fixtureRoot '_controls-test-set.xml') -Raw
$manager = New-Object System.Xml.XmlNamespaceManager($negativeSet.NameTable)
$manager.AddNamespace('c','http://www.w3.org/2012/10/xslt-test-catalog')
$negativeSet.SelectSingleNode('//c:error',$manager).SetAttribute('code','XTDE3340')
foreach ($stylesheet in $negativeSet.SelectNodes('//c:stylesheet',$manager)) {
    $stylesheet.SetAttribute('file', (Join-Path $fixtureRoot $stylesheet.GetAttribute('file')))
}
$negativeSet.Save((Join-Path $outputRoot '_negative-test-set.xml'))
[xml]$negativeCatalog = Get-Content -LiteralPath $catalog -Raw
$negativeCatalog.DocumentElement.FirstChild.SetAttribute('file','_negative-test-set.xml')
$negativeCatalog.Save((Join-Path $outputRoot 'catalog.xml'))
Invoke-Control 'wrong-code-alias-is-failure' @('--xslt40',(Join-Path $outputRoot 'catalog.xml'),'xslt40-adapter-controls') 1 '1 passed, 1 failed, 0 skipped'

# Exercise native catalog version lists without changing the original controls.
function Invoke-SpecControl([string]$Name, [string]$Value, [string]$Profile, [int]$ExpectedExit, [string]$Summary) {
    $controlRoot = Join-Path $outputRoot $Name
    New-Item -ItemType Directory -Path $controlRoot -Force | Out-Null
    [xml]$set = Get-Content -LiteralPath (Join-Path $fixtureRoot '_controls-test-set.xml') -Raw
    $ns = New-Object System.Xml.XmlNamespaceManager($set.NameTable)
    $ns.AddNamespace('c','http://www.w3.org/2012/10/xslt-test-catalog')
    foreach ($spec in $set.SelectNodes('//c:spec',$ns)) { $spec.SetAttribute('value',$Value) }
    foreach ($sheet in $set.SelectNodes('//c:stylesheet',$ns)) {
        $sheet.SetAttribute('file',(Join-Path $fixtureRoot $sheet.GetAttribute('file')))
    }
    $set.Save((Join-Path $controlRoot '_controls-test-set.xml'))
    Copy-Item -LiteralPath $catalog -Destination (Join-Path $controlRoot 'catalog.xml')
    $tail = @((Join-Path $controlRoot 'catalog.xml'),'xslt40-adapter-controls')
    if ($Profile) { $tail = @($Profile) + $tail }
    Invoke-Control $Name $tail $ExpectedExit $Summary
}
Invoke-SpecControl 'old-version-list-skips40' 'XSLT20 XSLT30' '--xslt40' 1 '0 passed, 0 failed, 2 skipped'
Invoke-SpecControl 'version-list-includes40' 'XSLT30 XSLT40' '--xslt40' 0 '2 passed, 0 failed, 0 skipped'
Invoke-SpecControl 'version-list-includes30' 'XSLT20 XSLT30' '' 0 '2 passed, 0 failed, 0 skipped'
Invoke-SpecControl 'version-list-xml-whitespace-plus' " XSLT20`nXSLT30+`t" '--xslt40' 0 '2 passed, 0 failed, 0 skipped'
