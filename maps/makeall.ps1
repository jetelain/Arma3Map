Remove-Item $PSScriptRoot/all.js
Get-Content -Path $PSScriptRoot/*.js  -Encoding UTF8 | Set-Content $PSScriptRoot/all.js -Encoding UTF8

$js = Get-Content $PSScriptRoot/all.js
$js = $js -replace 'Arma3Map\.Maps\.([a-z0-9_]+) *= *{', '$1:{'
$js = $js -replace 'Arma3Map\.Maps\.([a-z0-9_]+) =', ''
$js = $js -replace 'CRS\: MGRS_CRS\([0-9\.]+, [0-9\.]+, [0-9\.]+\),', ''
$js = $js -replace '};', '},'
$js[0] = '{'+$js[0];
$js[$js.Length-1] = '}}';

$js | Set-Content $PSScriptRoot/all.json -Encoding UTF8

$json = $js | ConvertFrom-Json
$json | ConvertTo-Json -Depth 3 -Compress | Set-Content $PSScriptRoot/all.json -Encoding UTF8