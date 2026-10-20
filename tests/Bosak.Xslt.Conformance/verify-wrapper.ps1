param([string]$OutputDirectory = '')
$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repositoryRoot ('.artifacts/wrapper-controls/' + [guid]::NewGuid().ToString('N')) }
$suite = Join-Path $OutputDirectory 'suite with spaces'
[IO.Directory]::CreateDirectory($suite) | Out-Null
$catalogNamespace = 'http://www.w3.org/2012/10/xslt-test-catalog'
[IO.File]::WriteAllText((Join-Path $suite 'catalog.xml'), "<catalog xmlns='$catalogNamespace'><test-set name='controls' file='tests.xml'/></catalog>")
[IO.File]::WriteAllText((Join-Path $suite 'tests.xml'), @"
<test-set xmlns="$catalogNamespace" name="controls">
  <test-case name="pass"><test><stylesheet file="pass.xsl"/></test><result><assert-xml>&lt;out/&gt;</assert-xml></result></test-case>
  <test-case name="fail"><test><stylesheet file="pass.xsl"/></test><result><error code="XTSE0010"/></result></test-case>
  <test-case name="slow"><test><stylesheet file="slow.xsl"/></test><result><assert-xml>&lt;out/&gt;</assert-xml></result></test-case>
</test-set>
"@)
[IO.File]::WriteAllText((Join-Path $suite 'pass.xsl'), '<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0"><xsl:template name="xsl:initial-template"><out/></xsl:template></xsl:stylesheet>')
[IO.File]::WriteAllText((Join-Path $suite 'slow.xsl'), @'
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0">
<xsl:template name="xsl:initial-template"><out><xsl:iterate select="1 to 1000000">
<xsl:param name="total" select="0"/><xsl:on-completion select="$total"/>
<xsl:next-iteration><xsl:with-param name="total" select="$total + ."/></xsl:next-iteration>
</xsl:iterate></out></xsl:template></xsl:stylesheet>
'@)
& git -C $suite init --quiet
if ($LASTEXITCODE -ne 0) { throw 'Fixture git init failed.' }
& git -C $suite add catalog.xml tests.xml pass.xsl slow.xsl
if ($LASTEXITCODE -ne 0) { throw 'Fixture git add failed.' }
& git -C $suite -c user.name='Bosak test control' -c user.email='test@example.invalid' commit --quiet -m 'Local wrapper controls; not an upstream contribution'
if ($LASTEXITCODE -ne 0) { throw 'Fixture git commit failed.' }
function Assert-Wrapper([string]$Case, [int]$Timeout, [int]$ExpectedExit, [string]$Status, [bool]$Success) {
    $output = @(& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $repositoryRoot 'run-xslt40-tests.ps1') -SuiteRoot $suite -TestSet controls -TestCase $Case -ProcessorProfile Xslt40Draft -NoBuild -TimeoutSeconds $Timeout 2>&1)
    $actualExit = $LASTEXITCODE
    $output | Set-Content -LiteralPath (Join-Path $OutputDirectory "$Case.log")
    if ($actualExit -ne $ExpectedExit) { throw "$Case expected wrapper exit $ExpectedExit, got $actualExit" }
    $receiptLine = @($output | Where-Object { "$_" -like 'Receipt: *' })
    if ($receiptLine.Count -ne 1) { throw "$Case did not retain a receipt." }
    $receiptPath = "$($receiptLine[0])".Substring(9)
    $receipt = Get-Content -LiteralPath $receiptPath -Raw | ConvertFrom-Json
    if ($receipt.status -ne $Status -or $receipt.success -ne $Success) { throw "$Case incorrect receipt status/success: $receiptPath" }
    if ($Case -eq 'slow' -and $receipt.runnerExitCode -ne 124) { throw 'Timeout must record exit 124.' }
    if (-not (Test-Path -LiteralPath $receipt.log)) { throw 'Missing retained stdout.' }
    Write-Output "PASS wrapper-$Case ($receiptPath)"
}
Assert-Wrapper pass 30 0 Completed $true
Assert-Wrapper fail 30 1 Completed $false
Assert-Wrapper no-such-case 30 1 Completed $false
Assert-Wrapper slow 1 1 TimedOut $false
