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

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Lisa.dll"]
