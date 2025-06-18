#Requires -Modules @{ModuleName='CloudEvents.Sdk';ModuleVersion='1.0.0'}

param($Request)

Add-Type -Path "$pwd\Google.Events.Protobuf.dll"
Add-Type -Path "$pwd\Google.Protobuf.dll" 

$cloudEvent = ConvertFrom-HttpMessage `
    -Headers $Request.Headers `
    -Body $Request.Body `
    -DataTypeName 'Google.Events.Protobuf.Cloud.Storage.V1.StorageObjectData'

$data = $cloudEvent.Data

Write-Host "Storage object information:"
Write-Host "  Name: $($data.Name)"
Write-Host "  Bucket: $($data.Bucket)"
Write-Host "  Size: $($data.Size)"
Write-Host "  Content type: $($data.ContentType)"

Write-Host "CloudEvent information:"
Write-Host "  ID: $($cloudEvent.Id)"
Write-Host "  Source: $($cloudEvent.Source)"
Write-Host "  Type: $($cloudEvent.Type)"
Write-Host "  Subject: $($cloudEvent.Subject)"
Write-Host "  DataSchema: $($cloudEvent.DataSchema)"
Write-Host "  DataContentType: $($cloudEvent.DataContentType)"
Write-Host "  Time: $($cloudEvent.Time.ToUniversalTime().ToString('yyyy-MM-dd''T''HH:mm:ss.ff''Z'''))"
Write-Host "  SpecVersion: $($cloudEvent.SpecVersion)"
Write-Host "  Data: $($cloudEvent.Data)"
