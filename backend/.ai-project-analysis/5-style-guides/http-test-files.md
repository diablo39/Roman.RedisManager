# Style Guide: HTTP Test Files

> Conventions unique to this project.

## .http File Format
The project uses Visual Studio / REST Client `.http` files for manual API testing.

## Variable Declarations
Host address is defined as a variable at the top of the file:
```
@Roman.RedisManager.Web_HostAddress = https://localhost:7244
```

Variable naming: `{ProjectName}_HostAddress` using the HTTPS profile URL from `launchSettings.json`.

## Request Format
```
GET {{Roman.RedisManager.Web_HostAddress}}/api/redisservers

###
```

Requests are separated by `###`.
