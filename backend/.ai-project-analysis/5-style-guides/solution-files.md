# Style Guide: Solution Files

> Conventions unique to this project.

## .slnx Format
The solution uses the new XML-based `.slnx` format (not the legacy `.sln` text format).

## Solution Folder Structure
Projects are organized into solution folders mirroring the disk layout:
- `/src/` — All source projects.
- `/tests/` — All test projects.
- `/Solution Items/` — Loose files like `.github/copilot-instructions.md` and `README.md`.

## Project Organization
```xml
<Solution>
  <Folder Name="/Solution Items/">
    <File Path=".github/copilot-instructions.md" />
    <File Path="README.md" />
  </Folder>
  <Folder Name="/src/">
    <Project Path="src/Roman.RedisManager.Application/..." />
    <Project Path="src/Roman.RedisManager.Domain/..." />
    <Project Path="src/Roman.RedisManager.Extensions/..." />
    <Project Path="src/Roman.RedisManager.Infrastructure/..." />
    <Project Path="src/Roman.RedisManager.Web/..." />
  </Folder>
  <Folder Name="/tests/">
    <Project Path="tests/Roman.RedisManager.Tests/..." />
  </Folder>
</Solution>
```
