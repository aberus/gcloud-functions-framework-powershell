#Requires -Modules @{ModuleName='CloudEvents.Sdk';ModuleVersion='0.3.3'}

param($Request)

$cloudEvent = ConvertFrom-HttpMessage `
    -Headers $Request.Headers `
    -Body $Request.Body

Write-Host "CloudEvent information:"
Write-Host ("ID: {0}" -f $cloudEvent.Id)
Write-Host ("Source: {0}" -f $cloudEvent.Source)
Write-Host ("Type: {0}" -f $cloudEvent.Type)
Write-Host ("Subject: {0}" -f $cloudEvent.Subject)
Write-Host ("DataSchema: {0}" -f $cloudEvent.DataSchema)
Write-Host ("DataContentType: {0}" -f $cloudEvent.DataContentType)
Write-Host ("Time: {0}" -f ($cloudEvent.Time.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")))
Write-Host ("SpecVersion: {0}" -f $cloudEvent.SpecVersion)
Write-Host ("Data: {0}" -f $cloudEvent.Data)