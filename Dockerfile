# Use the SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

# Set the working directory inside the container for the build stage
WORKDIR /src

# Copy the csproj file(s) and restore any dependencies (via dotnet restore)
COPY Lisa/*.csproj ./Lisa/

# Set the correct working directory for the Lisa project
WORKDIR /src/Lisa

# Run dotnet restore to restore dependencies
RUN dotnet restore

# Copy the entire application into the container (after restoring dependencies)
COPY Lisa/. ./

# Build the application in release mode
RUN dotnet build -c Release -o /app/build

# Check if Lisa.dll exists in the build directory
RUN ls -al /app/build

# Publish the application
FROM build AS publish

RUN dotnet publish -c Release -o /app/publish


# Use the ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final

# Set the working directory to the folder where your app is published
WORKDIR /app/publish

EXPOSE 80

# Copy the published output directly into the current working directory
COPY --from=publish /app/publish .

# List the files to verify the publish directory contents (for debugging)
RUN ls -al

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "Lisa.dll"]