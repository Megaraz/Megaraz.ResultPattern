param(
    [Parameter(Mandatory)]
    [string] $PackageDirectory,

    [Parameter(Mandatory)]
    [string] $RepositoryCommit
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$expectedTfms = @("net8.0", "net9.0", "net10.0")
$expectedPackageCount = 1
$expectedSymbolPackageCount = 1

$packages = @(Get-ChildItem -LiteralPath $PackageDirectory -Filter "*.nupkg" -File | Where-Object Name -NotLike "*.snupkg")
$symbolPackages = @(Get-ChildItem -LiteralPath $PackageDirectory -Filter "*.snupkg" -File)

if ($packages.Count -ne $expectedPackageCount) {
    throw "Expected $expectedPackageCount primary package in '$PackageDirectory', but found $($packages.Count)."
}

if ($symbolPackages.Count -ne $expectedSymbolPackageCount) {
    throw "Expected $expectedSymbolPackageCount symbol package in '$PackageDirectory', but found $($symbolPackages.Count)."
}

function Get-ArchiveEntries {
    param([Parameter(Mandatory)][string] $Path)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        return @($archive.Entries | ForEach-Object FullName)
    }
    finally {
        $archive.Dispose()
    }
}

function Get-Nuspec {
    param([Parameter(Mandatory)][string] $Path)

    $archive = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entries = @($archive.Entries | Where-Object FullName -Like "*.nuspec")
        if ($entries.Count -ne 1) {
            throw "Expected exactly one .nuspec in '$Path', but found $($entries.Count)."
        }

        $reader = [System.IO.StreamReader]::new($entries[0].Open())
        try {
            [xml] $nuspec = $reader.ReadToEnd()
            return $nuspec
        }
        finally {
            $reader.Dispose()
        }
    }
    finally {
        $archive.Dispose()
    }
}

$packageEntries = Get-ArchiveEntries $packages[0].FullName
foreach ($tfm in $expectedTfms) {
    foreach ($extension in @("dll", "xml")) {
        $entry = "lib/$tfm/Megaraz.ResultPattern.$extension"
        if ($packageEntries -notcontains $entry) {
            throw "Primary package is missing '$entry'."
        }
    }
}

if ($packageEntries -notcontains "README.md") {
    throw "Primary package is missing README.md."
}

if ($packageEntries | Where-Object { $_ -match '(^|/)(\.git|\.github|bin|obj)/' }) {
    throw "Primary package contains an unintended repository or build-output file."
}

[xml] $nuspec = Get-Nuspec $packages[0].FullName
$metadata = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']")
if ($null -eq $metadata) {
    throw "Primary package .nuspec has no metadata element."
}

$license = [string] $metadata.SelectSingleNode("*[local-name()='license']").InnerText
if ($license -ne "MIT") {
    throw "Primary package license metadata must be MIT, but was '$license'."
}

$commit = [string] $metadata.SelectSingleNode("*[local-name()='repository']").GetAttribute("commit")
if (-not $commit.Equals($RepositoryCommit, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Primary package repository commit '$commit' does not match '$RepositoryCommit'."
}

$symbolEntries = Get-ArchiveEntries $symbolPackages[0].FullName
foreach ($tfm in $expectedTfms) {
    $entry = "lib/$tfm/Megaraz.ResultPattern.pdb"
    if ($symbolEntries -notcontains $entry) {
        throw "Symbol package is missing portable PDB '$entry'."
    }
}

Write-Host "Validated primary and symbol package contents for $($packages[0].Name)."
