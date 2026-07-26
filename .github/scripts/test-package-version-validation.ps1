$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$validator = Join-Path $PSScriptRoot "validate-package-version.ps1"
$testRoot = Join-Path ([System.IO.Path]::GetTempPath()) "Megaraz.ResultPattern-version-validation-$([guid]::NewGuid())"
$powerShellExecutable = (Get-Process -Id $PID).Path

function New-TestFixture {
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [Parameter(Mandatory)]
        [string] $ProjectVersion,

        [Parameter(Mandatory)]
        [string] $PackageVersion
    )

    $fixture = Join-Path $testRoot $Name
    $packageDirectory = Join-Path $fixture "artifacts"
    $packageContent = Join-Path $fixture "package"
    New-Item -ItemType Directory -Path $packageDirectory, $packageContent | Out-Null

    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Version>$ProjectVersion</Version>
  </PropertyGroup>
</Project>
"@ | Set-Content -LiteralPath (Join-Path $fixture "Package.csproj")

    @"
<?xml version="1.0"?>
<package>
  <metadata>
    <id>Megaraz.ResultPattern</id>
    <version>$PackageVersion</version>
  </metadata>
</package>
"@ | Set-Content -LiteralPath (Join-Path $packageContent "Megaraz.ResultPattern.nuspec")

    $zipPath = Join-Path $packageDirectory "Megaraz.ResultPattern.$PackageVersion.zip"
    $packagePath = Join-Path $packageDirectory "Megaraz.ResultPattern.$PackageVersion.nupkg"
    Compress-Archive -Path (Join-Path $packageContent "*") -DestinationPath $zipPath
    Move-Item -LiteralPath $zipPath -Destination $packagePath

    return $fixture
}

function Invoke-ValidationCase {
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [Parameter(Mandatory)]
        [string] $Tag,

        [Parameter(Mandatory)]
        [string] $ProjectVersion,

        [Parameter(Mandatory)]
        [string] $PackageVersion,

        [Parameter(Mandatory)]
        [bool] $ShouldSucceed
    )

    $fixture = New-TestFixture $Name $ProjectVersion $PackageVersion
    $arguments = @(
        "-NoProfile",
        "-File", $validator,
        "-Tag", $Tag,
        "-ProjectPath", (Join-Path $fixture "Package.csproj"),
        "-PackageDirectory", (Join-Path $fixture "artifacts")
    )

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        & $powerShellExecutable @arguments *> $null
        $succeeded = $LASTEXITCODE -eq 0
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($succeeded -ne $ShouldSucceed) {
        throw "Validation case '$Name' expected success '$ShouldSucceed' but got '$succeeded'."
    }

    Write-Host "Passed validation case: $Name"
}

try {
    Invoke-ValidationCase "matching versions" "v0.2.2" "0.2.2" "0.2.2" $true
    Invoke-ValidationCase "malformed tag" "v0.2" "0.2.2" "0.2.2" $false
    Invoke-ValidationCase "project mismatch" "v0.2.2" "0.2.1" "0.2.2" $false
    Invoke-ValidationCase "package mismatch" "v0.2.2" "0.2.2" "0.2.1" $false
}
finally {
    if (Test-Path -LiteralPath $testRoot) {
        Remove-Item -LiteralPath $testRoot -Recurse -Force
    }
}
