#!/usr/bin/env pwsh
#
# This script orchestrates mutation testing by invoking dotnet-stryker. It
# implements much of the same argument-building and validation behaviour that
# is also modelled by C# helper classes in
# `tests/Roman.RedisManager.Tests/Infrastructure/Quality`. Those helpers exist
# so the logic can be verified with unit tests; the script does *not* load or
# call into the test project. See
# `tests/Roman.RedisManager.Tests/Infrastructure/Quality/README.md` for details.
#
[CmdletBinding()]
param(
    [string]$SolutionPath = "Roman.RedisManager.slnx",
    [string]$TestProjectPath = "tests/Roman.RedisManager.Tests/Roman.RedisManager.Tests.csproj",
    [string]$MutableProjectPath = "src/Roman.RedisManager.Application/Roman.RedisManager.Application.csproj",
    [string]$ConfigurationPath = "tests/Roman.RedisManager.Tests/stryker-config.json",
    [string]$OutputDirectory = "tests/Roman.RedisManager.Tests/StrykerOutput",
    [switch]$WithBaseline,
    [switch]$SkipToolRestore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendRoot = Resolve-Path (Join-Path $scriptRoot "../../..")
$backendRootPath = $backendRoot.Path

Push-Location $backendRootPath
try {
    if (-not $SkipToolRestore) {
        dotnet tool restore
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet tool restore failed."
        }
    }

    if (-not (Test-Path $ConfigurationPath -PathType Leaf)) {
        throw "Stryker configuration file not found: $ConfigurationPath"
    }

    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

    $args = @(
        "dotnet-stryker",
        "--solution", $SolutionPath,
        "--test-project", $TestProjectPath,
        "--project", $MutableProjectPath,
        "--config-file", $ConfigurationPath,
        "--reporter", "html",
        "--reporter", "json",
        "--output", $OutputDirectory
    )

    if ($WithBaseline) {
        $args += @("--with-baseline")
    }

    dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "Mutation run failed."
    }

    $jsonReports = Get-ChildItem -Path $OutputDirectory -Filter "mutation-report.json" -Recurse -ErrorAction SilentlyContinue
    $htmlReports = Get-ChildItem -Path $OutputDirectory -Filter "mutation-report.html" -Recurse -ErrorAction SilentlyContinue

    if (-not $jsonReports) {
        throw "Expected JSON report not found in $OutputDirectory."
    }

    if (-not $htmlReports) {
        throw "Expected HTML report not found in $OutputDirectory."
    }

    Write-Output "Mutation run completed successfully."
    Write-Output "JSON report: $($jsonReports[0].FullName)"
    Write-Output "HTML report: $($htmlReports[0].FullName)"
}
finally {
    Pop-Location
}
