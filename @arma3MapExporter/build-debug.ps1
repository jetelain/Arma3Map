.\hemtt.exe build

$OneDriveMods = "$Env:OneDrive\Documents\1erGTD\Mods"
if ( Test-Path "$OneDriveMods" -PathType Container ) {
    Write-Output "Copy to OneDrive"
	Copy-Item "$PSScriptRoot\*.cpp" -Destination "$OneDriveMods\@arma3MapExporter\"
	Copy-Item "$PSScriptRoot\*.dll" -Destination "$OneDriveMods\@arma3MapExporter\"
	Copy-Item "$PSScriptRoot\addons\*.pbo" -Destination "$OneDriveMods\@arma3MapExporter\addons\"
}