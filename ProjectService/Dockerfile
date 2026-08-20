FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ProjectService/ProjectService.csproj", "ProjectService/"]
RUN dotnet restore "ProjectService/ProjectService.csproj"

COPY ProjectService/ ProjectService/
WORKDIR /src/ProjectService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProjectService.dll"]
