# Define source and destinations
$sourceImage = "ActuatorApp/Assets/Products/Controlbox/TC12.png"
$destinations = @(
    "ActuatorApp/Assets/Products/PowerSupply/TP5.png",
    "ActuatorApp/Assets/Products/PowerSupply/TP7.png",
    "ActuatorApp/Assets/Products/Accessories/TYC.png"
)

# Check source
if (-not (Test-Path $sourceImage)) {
    Write-Host "Source image $sourceImage not found. Using generic placeholder."
    # Create a dummy image if source doesn't exist (unlikely)
    # But TC12.png should exist based on products.json
}

foreach ($dest in $destinations) {
    if (-not (Test-Path $dest)) {
        Write-Host "Creating placeholder for $dest"
        Copy-Item -Path $sourceImage -Destination $dest
    } else {
        Write-Host "Image $dest already exists."
    }
}
