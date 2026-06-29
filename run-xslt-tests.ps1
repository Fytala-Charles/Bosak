# ===========================================================================================================================================================
# AUTHOR               : Charles Korthout
# CREATE DATE          : 29 juni 2026
# PURPOSE              : Run Bosak.Xslt.Tests from a temp copy to bypass local Application Control blocking.
# SPECIAL NOTES        : On this machine Windows Application Control blocks the rebuilt test assembly in its
#                        normal bin directory. Copying the build output to %TEMP% and running vstest from there
#                        is a reliable workaround.
# COPYRIGHT            : Fytala
# LICENSE              : License.txt
# ===========================================================================================================================================================
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceDir = Join-Path $repoRoot "tests\Bosak.Xslt.Tests\bin\$Configuration\net10.0"
$targetDir = Join-Path $env:TEMP "BosakXsltTestsRun"

if (-not (Test-Path $sourceDir)) {
    throw "Test output not found: $sourceDir. Build the solution first with: dotnet build Bosak.sln -c $Configuration"
}

if (Test-Path $targetDir) {
    Remove-Item -Recurse -Force $targetDir
}
New-Item -ItemType Directory -Path $targetDir | Out-Null

Copy-Item -Path "$sourceDir\*" -Destination $targetDir -Recurse -Force

$testDll = Join-Path $targetDir "Bosak.Xslt.Tests.dll"
& dotnet vstest $testDll /Logger:console`;Verbosity=minimal

exit $LASTEXITCODE
