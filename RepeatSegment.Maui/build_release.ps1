$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$dotnet = "C:\Program Files\dotnet\dotnet.exe"
$csproj = "C:\ProjectsCSharp\RepeatSegment.Android\RepeatSegment.Maui\RepeatSegment.Maui.csproj"
$androidSdk = "$env:LOCALAPPDATA\Android\Sdk"
$javaSdk = "C:\Program Files\Android\Android Studio\jbr"

Write-Host "Building Release..."
Write-Host "dotnet: $dotnet"
Write-Host "csproj: $csproj"
Write-Host "AndroidSdk: $androidSdk"
Write-Host "JavaSdk: $javaSdk"

& $dotnet build $csproj -c Release -f net9.0-android `
    -p:AndroidSdkDirectory="$androidSdk" `
    -p:JavaSdkDirectory="$javaSdk" `
    2>&1 | Out-String -Width 4096 | Write-Host
