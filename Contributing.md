# Install .NET 9.0 SDK
 - https://dotnet.microsoft.com/en-us/download/dotnet/9.0
 - run `dotnet --version` to test if the CLI is working.

# Install C# Dev Kit VSCode Extension
 `https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit`

# Install Postgress SQL
Change the following connection string to work with your local database in appsettings.json
```Json
"ConnectionStrings": {
    "Lisa": "Host=localhost;Port=5432;Database=Lisa;Username=postgres;Password=admin;",
    "Hangfire": "Host=localhost;Port=5432;Database=Hangfire;Username=postgres;Password=admin;"
  }
```

# Instsall EF Core Tools
 1. dotnet tool install --global dotnet-ef
 2. change directory to Lisa where the Lisa.csproj is located
 3. run `dotnet ef database update` to update the database and apply migrations.
   

# Standards & Architecture
 - All Code tries to adhere to SOLID principles
 - Written in C# 12 and .NET 9