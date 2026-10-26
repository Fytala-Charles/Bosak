$ErrorActionPreference='Continue'
$releaseEvidence='D:/Development/Bosak/.guard-tmp/release1261-candidate'
$env:BOSAK_RELEASE_RECEIPT_DIR=$releaseEvidence
foreach($kind in @('broad','focused')) {
  $env:NUGET_PACKAGES="$releaseEvidence/$kind-cache"
  $consumerProject="$releaseEvidence/$kind"
  dotnet restore $consumerProject --configfile "$consumerProject/NuGet.Config" --no-http-cache *> "$releaseEvidence/$kind-restore.log"
  if($LASTEXITCODE){throw "$kind restore failed"}
  if($kind -eq 'broad') {
    dotnet run --project $consumerProject -c Release --no-restore *> "$releaseEvidence/broad-run.log"
  } else {
    dotnet test $consumerProject -c Release --no-restore --logger 'trx;LogFileName=focused.trx' --results-directory "$releaseEvidence/focused-results" *> "$releaseEvidence/focused-tests.log"
  }
  if($LASTEXITCODE){throw "$kind checks failed"}
  dotnet run --project "$releaseEvidence/metadata/Metadata.csproj" -c Release -- "$consumerProject/bin/Release/net10.0" "$releaseEvidence/$kind-metadata.json" *> "$releaseEvidence/$kind-metadata.log"
  if($LASTEXITCODE){throw "$kind metadata failed"}
  Push-Location $consumerProject
  try {
    dotnet list package --no-restore --vulnerable --include-transitive --format json --source https://api.nuget.org/v3/index.json > "$releaseEvidence/$kind-audit.json"
    if($LASTEXITCODE){throw "$kind vulnerability audit failed"}
  } finally { Pop-Location }
}
