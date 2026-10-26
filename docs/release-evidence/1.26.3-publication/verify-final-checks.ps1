$ErrorActionPreference = 'Stop'
$releasePublication = 'D:/Development/Bosak/.guard-tmp/release1263-publication'
dotnet run --project "$releasePublication/content-hash/Hash.csproj" -c Release -- "$releasePublication/public-packages" "$releasePublication/nuget-content-hashes.json" *> "$releasePublication/content-hash.log"
if ($LASTEXITCODE) { throw 'Content hash verification failed' }
python D:/Development/Bosak/.guard-tmp/verify-release1263-public.py
exit $LASTEXITCODE
