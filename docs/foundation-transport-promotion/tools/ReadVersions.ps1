param([Parameter(Mandatory = $true)][string]$Directory)
$ErrorActionPreference = 'Stop'
$rows = foreach ($file in Get-ChildItem -LiteralPath $Directory -Filter '*.dll') {
    $identity = [System.Reflection.AssemblyName]::GetAssemblyName($file.FullName)
    [pscustomobject]@{
        name = $identity.Name
        assemblyVersion = $identity.Version.ToString()
        fileVersion = $file.VersionInfo.FileVersion
    }
}
$rows | Sort-Object name | ConvertTo-Json
