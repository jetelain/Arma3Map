Remove-Item $PSScriptRoot/all.js
Get-Content -Path $PSScriptRoot/*.js| Set-Content $PSScriptRoot/all.js