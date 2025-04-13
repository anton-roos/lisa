# =====================================
# Stage 1: Build
# =====================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /src

# Copy solution and project files
COPY Lisa.sln ./
COPY Lisa/*.csproj ./Lisa/
COPY Lisa.Test/*.csproj ./Lisa.Test/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY Lisa/ ./Lisa/
COPY Lisa.Test/ ./Lisa.Test/

# Build the application
WORKDIR /src/Lisa
RUN dotnet build -c Release -o /app/build

# =====================================
# Stage 2: Publish
# =====================================
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# =====================================
# Stage 3: Final Runtime
# =====================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final

WORKDIR /app
EXPOSE 80

# (Optional) Set environment for production
# ENV ASPNETCORE_ENVIRONMENT=Production

# Copy published app
COPY --from=publish /app/publish .

# Entry point
ENTRYPOINT ["dotnet", "Lisa.dll"]
