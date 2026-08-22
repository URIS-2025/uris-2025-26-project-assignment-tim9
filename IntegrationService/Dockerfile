FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY IntegrationService/IntegrationService.csproj IntegrationService/
RUN dotnet restore IntegrationService/IntegrationService.csproj
COPY IntegrationService/ IntegrationService/
WORKDIR /src/IntegrationService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "IntegrationService.dll"]
