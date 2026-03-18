#!/bin/bash
# .claude/hooks/mutation-testing-reminder.sh
#
# PostToolUse hook — fires after Edit or Write operations.
# Detects when test files (*Tests.cs) are created or modified and injects
# a reminder into Claude's context that mutation testing must be executed
# before the work is considered complete.
#
# This hook is advisory (exit 0) — it does not block the operation.
# It returns structured JSON so the reminder appears as context to Claude,
# not just as a user-facing message.
#
# No external dependencies (no jq) — uses pure bash for portability.

INPUT=$(cat)

# Extract file_path from JSON using bash pattern matching (no jq dependency).
# Handles both forward-slash and backslash-escaped paths in JSON.
FILE_PATH=""
if [[ "$INPUT" =~ \"file_path\"[[:space:]]*:[[:space:]]*\"([^\"]+)\" ]]; then
  FILE_PATH="${BASH_REMATCH[1]}"
fi

# Normalise Windows backslashes and escaped backslashes to forward slashes
FILE_PATH="${FILE_PATH//\\\\/\/}"
FILE_PATH="${FILE_PATH//\\//}"

# Check if the file is a C# test file inside a tests directory
if [[ "$FILE_PATH" == */tests/*Tests.cs ]] || [[ "$FILE_PATH" == */Tests/*Tests.cs ]]; then
  # Return structured output that injects context into Claude's reasoning
  cat <<'HOOK_JSON'
{
  "hookSpecificOutput": {
    "hookEventName": "PostToolUse",
    "additionalContext": "MUTATION TESTING REQUIRED: A test file was just modified. You MUST execute the mutation testing loop (run -> analyze -> improve -> rerun) before considering this work complete. Run dotnet-stryker scoped to the project under test, analyze surviving mutants, improve assertions if actionable survivors exist, and rerun until threshold is met. See AGENTS.md 'Mutation Testing Quality Gate' for the full checklist."
  }
}
HOOK_JSON
  exit 0
fi

exit 0
