FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /src
COPY Lisa/*.csproj ./Lisa/

WORKDIR /src/Lisa
RUN dotnet restore

COPY Lisa/. ./

RUN dotnet build -c Release -o /app/build

FROM build AS publish

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final

WORKDIR /app
EXPOSE 80

RUN apt-get update && apt-get install -y curl

# Ensure the publish directory exists
COPY --from=publish /app/publish /app/publish

# List the files to verify
RUN ls -al /app/publish

ENTRYPOINT ["dotnet", "Lisa.dll"]
