param(
    [Parameter(Mandatory=$true, position=0)][string]$version
)
$ErrorActionPreference = "Stop"

function Validate-Version {
    param(
        [string]$version
    )
    if ($version -match '^v\d+\.\d+\.\d+$') {
        return $true
    } else {
        return $false
    }
}

function Update-SolutionInfo {
    param(
        [string]$version,
        [string]$scriptDir
    )
    $solutionInfoPath = Join-Path -Path $scriptDir -ChildPath "SolutionInfo.cs"
    $content = Get-Content $solutionInfoPath
    $content = $content -replace 'AssemblyVersion\(".*"\)', "AssemblyVersion(`"$version`")"
    $content = $content -replace 'AssemblyFileVersion\(".*"\)', "AssemblyFileVersion(`"$version`")"
    Set-Content -Path $solutionInfoPath -Value $content
}

function Update-CargoToml {
    param(
        [string]$version,
        [string]$scriptDir
    )
	$cargoTomlPath = Join-Path -Path $scriptDir -ChildPath "Cargo.toml"
    $content = Get-Content $cargoTomlPath
    
    $inPackageSection = $false
    $newContent = $content | ForEach-Object {
        if ($_ -match '^\[package\]') {
            $inPackageSection = $true
        }
        if ($inPackageSection -and $_ -match '^version = ".*"$') {
            $_ = "version = `"$version`""
            $inPackageSection = $false
        }
        $_
    }
    Set-Content -Path $cargoTomlPath -Value $newContent
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

if (Validate-Version -version $version) {
    $versionNumber = $version -replace '^v', ''
    Update-SolutionInfo -version $versionNumber -scriptDir $scriptDir
    Update-CargoToml -version $versionNumber -scriptDir (Join-Path $scriptDir pm_affinityservice)
    Write-Host "Versions updated to $versionNumber"
} else {
    throw "Invalid version format. Please use format v1.2.3"
}
