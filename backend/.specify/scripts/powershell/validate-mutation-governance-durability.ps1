#!/usr/bin/env pwsh
#
# Governance durability check. Related tests for this logic are located in the
# Quality helpers directory; see README.md there for the full rationale.
#
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$backendRoot = Resolve-Path (Join-Path $PSScriptRoot "../../..")
$repoRoot = Resolve-Path (Join-Path $backendRoot "..")
$requiredPhrase = "run -> analyze -> improve -> rerun"

$targets = @(
    (Join-Path $backendRoot ".github/copilot-instructions.md"),
    (Join-Path $backendRoot ".specify/memory/constitution.md"),
    (Join-Path $backendRoot ".specify/templates/spec-template.md"),
    (Join-Path $backendRoot ".specify/templates/plan-template.md"),
    (Join-Path $backendRoot ".specify/templates/tasks-template.md"),
    (Join-Path $backendRoot ".github/prompts/speckit.implement.prompt.md"),
    (Join-Path $backendRoot ".github/prompts/speckit.tasks.prompt.md"),
    (Join-Path $repoRoot ".specify/templates/agent-file-template.md")
)

$missing = New-Object System.Collections.Generic.List[string]
foreach ($target in $targets) {
    if (-not (Test-Path $target -PathType Leaf)) {
        $missing.Add("Missing file: $target")
        continue
    }

    $content = Get-Content -Raw -Path $target
    if ($content -notlike "*$requiredPhrase*") {
        $missing.Add("Missing required phrase in: $target")
    }
}

if ($missing.Count -gt 0) {
    $missing | ForEach-Object { Write-Error $_ }
    throw "Mutation governance durability validation failed."
}

Write-Output "Mutation governance durability validation passed."
