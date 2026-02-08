# Style Guide: IDE User Settings

> Conventions unique to this project.

## .csproj.user Files
User-specific project settings (like active debug profile) are stored in `.csproj.user` files:
```xml
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ActiveDebugProfile>https</ActiveDebugProfile>
  </PropertyGroup>
</Project>
```

These files are typically gitignored and contain per-developer preferences.
