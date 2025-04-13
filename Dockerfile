# Use the SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

# Set the working directory inside the container for the build stage
WORKDIR /src

# Copy the csproj file(s) and restore any dependencies (via dotnet restore)
COPY Lisa/*.csproj ./Lisa/

# Set the correct working directory for the Lisa project
WORKDIR /src/Lisa

# Verify the SDK is installed and print the .NET SDK version
RUN dotnet --version

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

# Check if Lisa.dll exists in the publish directory
RUN ls -al /app/publish

# Use the ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final

# Set the working directory inside the container for the runtime stage
WORKDIR /app

# Expose port 80 for the app
EXPOSE 80

# Install curl (optional, based on your needs)
RUN apt-get update && apt-get install -y curl

# Copy the publish directory from the build stage
COPY --from=publish /app/publish /app/publish

# List the files to verify the publish directory contents (for debugging)
RUN ls -al /app/publish

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "/app/publish/Lisa.dll"]
