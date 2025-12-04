# Create Battery directory if not exists
$batteryDir = "ActuatorApp/Assets/Products/Battery"
if (-not (Test-Path $batteryDir)) {
    New-Item -ItemType Directory -Force -Path $batteryDir | Out-Null
    Write-Host "Created directory: $batteryDir"
}

# Move TBB images from Controlbox to Battery
$controlboxDir = "ActuatorApp/Assets/Products/Controlbox"
$tbbImages = Get-ChildItem -Path $controlboxDir -Filter "TBB*.png"

foreach ($img in $tbbImages) {
    $dest = Join-Path $batteryDir $img.Name
    Move-Item -Path $img.FullName -Destination $dest -Force
    Write-Host "Moved $($img.Name) to Battery folder"
}
