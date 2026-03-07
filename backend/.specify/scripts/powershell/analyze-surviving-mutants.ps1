#!/usr/bin/env pwsh
#
# Analyze script for surviving/no-coverage mutants. A parallel C# version of
# the classification logic lives in the test project under
# `tests/Roman.RedisManager.Tests/Infrastructure/Quality` to enable unit testing.
# The script itself does not load those types; the duplication exists solely
# for maintainability and ease of testing.
#
[CmdletBinding()]
param(
    [string]$MutationReportPath,
    [string]$OutputMarkdownPath = "tests/Roman.RedisManager.Tests/StrykerOutput/mutation-findings.md",
    [string]$OutputJsonPath = "tests/Roman.RedisManager.Tests/StrykerOutput/mutation-findings.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not $MutationReportPath) {
    throw "MutationReportPath is required."
}

if (-not (Test-Path $MutationReportPath -PathType Leaf)) {
    throw "Mutation report not found: $MutationReportPath"
}

$report = Get-Content -Raw -Path $MutationReportPath | ConvertFrom-Json
$findings = New-Object System.Collections.Generic.List[object]

foreach ($file in $report.files.PSObject.Properties) {
    $filePath = $file.Name
    $mutants = $file.Value.mutants
    if ($null -eq $mutants) {
        continue
    }

    foreach ($mutant in $mutants) {
        if ($mutant.status -ne "Survived" -and $mutant.status -ne "NoCoverage") {
            continue
        }

        $category = if ($mutant.status -eq "Survived") { "SurvivedMutant" } else { "NoCoverage" }
        $severity = if ($mutant.status -eq "Survived") { "High" } else { "Medium" }
        $line = if ($null -ne $mutant.location -and $null -ne $mutant.location.start) { $mutant.location.start.line } else { 0 }
        $description = if ($null -ne $mutant.PSObject.Properties["description"]) { $mutant.description } else { "$($mutant.mutatorName): $($mutant.replacement)" }

        $findings.Add([pscustomobject]@{
            findingId = "mutant-$($mutant.id)"
            category = $category
            severity = $severity
            location = "${filePath}:$line"
            mutantStatus = $mutant.status
            description = $description
            recommendation = "Strengthen assertions for the behavior around $filePath line $line."
        })
    }
}

New-Item -ItemType Directory -Path (Split-Path -Parent $OutputJsonPath) -Force | Out-Null
$findings | ConvertTo-Json -Depth 6 | Set-Content -Path $OutputJsonPath -Encoding utf8

$lines = @("# Mutation Findings", "")
if ($findings.Count -eq 0) {
    $lines += "No surviving or no-coverage mutants found."
}
else {
    foreach ($finding in $findings) {
        $lines += "- [$($finding.severity)] $($finding.category) at $($finding.location): $($finding.recommendation)"
    }
}

$lines | Set-Content -Path $OutputMarkdownPath -Encoding utf8

Write-Output "Findings markdown: $OutputMarkdownPath"
Write-Output "Findings json: $OutputJsonPath"
