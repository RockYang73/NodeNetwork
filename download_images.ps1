$urls = @{
    "TP5" = "https://www.timotion.com/uploads/images/products/power-supplies/TP5P-Series/TP5P-Series-1.png"
    "TP7" = "https://www.timotion.com/uploads/images/products/power-supplies/TP7-Series/TP7-Series-1.png"
    "TYC" = "https://www.timotion.com/uploads/images/products/accessories/TYC-Series/TYC-Series-1.png"
}

$fallbackUrls = @{
    "TP5" = "https://www.timotion.com/uploads/images/products/power-supplies/TP5P-Series/TP5P-Series-1.webp"
    "TP7" = "https://www.timotion.com/uploads/images/products/power-supplies/TP7-Series/TP7-Series-1.webp"
    "TYC" = "https://www.timotion.com/uploads/images/products/accessories/TYC-Series/TYC-Series-1.webp"
}

$destDirs = @{
    "TP5" = "ActuatorApp/Assets/Products/PowerSupply"
    "TP7" = "ActuatorApp/Assets/Products/PowerSupply"
    "TYC" = "ActuatorApp/Assets/Products/Accessories"
}

foreach ($key in $urls.Keys) {
    $dir = $destDirs[$key]
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    
    $destPath = Join-Path $dir "$key.png"
    $url = $urls[$key]
    
    Write-Host "Trying to download $key from $url..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $destPath -ErrorAction Stop -UserAgent "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
        Write-Host "Success (PNG)."
    }
    catch {
        Write-Host "PNG failed, trying WebP..."
        $fallbackUrl = $fallbackUrls[$key]
        $destPathWebp = Join-Path $dir "$key.webp"
        try {
            Invoke-WebRequest -Uri $fallbackUrl -OutFile $destPathWebp -ErrorAction Stop -UserAgent "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
            Write-Host "Success (WebP)."
        }
        catch {
            Write-Host "Failed to download $key."
            Write-Error $_
        }
    }
}
