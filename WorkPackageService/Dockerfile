FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY WorkPackageService/WorkPackageService.csproj WorkPackageService/
RUN dotnet restore WorkPackageService/WorkPackageService.csproj
COPY WorkPackageService/ WorkPackageService/
WORKDIR /src/WorkPackageService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WorkPackageService.dll"]
