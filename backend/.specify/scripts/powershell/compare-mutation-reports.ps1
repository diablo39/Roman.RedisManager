#!/usr/bin/env pwsh
#
# Comparison helper for mutation-report JSON files. Similar logic is represented
# by C# helpers/tests under
# `tests/Roman.RedisManager.Tests/Infrastructure/Quality`; the two
# implementations are intentionally duplicated so that unit tests can validate
# the same behaviour. See the README in that directory for an explanation.
#
[CmdletBinding()]
param(
    [string]$CurrentReportPath,
    [string]$PreviousReportPath,
    [string]$OutputPath = "tests/Roman.RedisManager.Tests/StrykerOutput/mutation-comparison.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-MutationScore {
    param([psobject]$Report)

    $thresholdsProperty = $Report.PSObject.Properties["thresholds"]
    if ($null -ne $thresholdsProperty -and $null -ne $thresholdsProperty.Value.PSObject.Properties["mutationScore"]) {
        return [decimal]$thresholdsProperty.Value.mutationScore
    }

    $metricsProperty = $Report.PSObject.Properties["metrics"]
    if ($null -ne $metricsProperty -and $null -ne $metricsProperty.Value.PSObject.Properties["mutationScore"]) {
        return [decimal]$metricsProperty.Value.mutationScore
    }

    if ($null -eq $Report.files) {
        throw "Unable to determine mutation score from report payload."
    }

    $killed = 0
    $tested = 0
    foreach ($file in $Report.files.PSObject.Properties) {
        foreach ($mutant in $file.Value.mutants) {
            if ($mutant.status -eq "Killed") {
                $killed++
                $tested++
                continue
            }

            if ($mutant.status -eq "Survived" -or $mutant.status -eq "NoCoverage" -or $mutant.status -eq "Timeout") {
                $tested++
            }
        }
    }

    if ($tested -eq 0) {
        return [decimal]0
    }

    return [Math]::Round(($killed / [decimal]$tested) * 100, 2)
}

if (-not $CurrentReportPath) {
    throw "CurrentReportPath is required."
}

if (-not (Test-Path $CurrentReportPath -PathType Leaf)) {
    throw "Current report not found: $CurrentReportPath"
}

New-Item -ItemType Directory -Path (Split-Path -Parent $OutputPath) -Force | Out-Null

$current = Get-Content -Raw -Path $CurrentReportPath | ConvertFrom-Json
$currentScore = Get-MutationScore -Report $current
$timestamp = [DateTimeOffset]::UtcNow.ToString("u")

if (-not $PreviousReportPath -or -not (Test-Path $PreviousReportPath -PathType Leaf)) {
    @(
        "# Mutation Comparison",
        "",
        "Generated: $timestamp",
        "",
        "- Current mutation score: $currentScore",
        "- Previous baseline: not available (first run or missing baseline)"
    ) | Set-Content -Path $OutputPath -Encoding utf8

    Write-Output "Comparison written to $OutputPath"
    exit 0
}

$previous = Get-Content -Raw -Path $PreviousReportPath | ConvertFrom-Json
$previousScore = Get-MutationScore -Report $previous
$delta = [Math]::Round(($currentScore - $previousScore), 2)
$status = if ($delta -gt 0) { "improved" } elseif ($delta -lt 0) { "regressed" } else { "stable" }

@(
    "# Mutation Comparison",
    "",
    "Generated: $timestamp",
    "",
    "- Current mutation score: $currentScore",
    "- Previous mutation score: $previousScore",
    "- Delta: $delta",
    "- Status: $status"
) | Set-Content -Path $OutputPath -Encoding utf8

Write-Output "Comparison written to $OutputPath"
