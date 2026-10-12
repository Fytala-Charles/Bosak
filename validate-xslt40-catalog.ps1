param(
    [string]$SuiteRoot = (Join-Path $PSScriptRoot '.upstream/xslt40-test'),
    [string[]]$Files = @('catalog.xml')
)
$ErrorActionPreference = 'Stop'
$suitePath = (Resolve-Path -LiteralPath $SuiteRoot).Path
$schemas = New-Object System.Xml.Schema.XmlSchemaSet
$schemas.XmlResolver = New-Object System.Xml.XmlUrlResolver
[void]$schemas.Add('http://www.w3.org/2012/10/xslt-test-catalog', (Join-Path $suitePath 'admin/catalog-schema.xsd'))
$schemas.Compile()
foreach ($file in $Files) {
    $path = if ([IO.Path]::IsPathRooted($file)) { $file } else { Join-Path $suitePath $file }
    $settings = New-Object System.Xml.XmlReaderSettings
    $settings.Schemas = $schemas
    $settings.ValidationType = [System.Xml.ValidationType]::Schema
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.add_ValidationEventHandler({ param($sender, $event) throw $event.Exception })
    $reader = [System.Xml.XmlReader]::Create($path, $settings)
    try { while ($reader.Read()) {} } finally { $reader.Dispose() }
    Write-Output "Valid catalog XML: $path"
}
