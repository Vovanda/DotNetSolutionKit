FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sawking.Avox.KnowlegeService.API/Sawking.Avox.KnowlegeService.API.csproj", "Sawking.Avox.KnowlegeService.API/"]
RUN dotnet restore "Sawking.Avox.KnowlegeService.API/Sawking.Avox.KnowlegeService.API.csproj"
COPY . .
WORKDIR "/src/Sawking.Avox.KnowlegeService.API"
RUN dotnet build "Sawking.Avox.KnowlegeService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sawking.Avox.KnowlegeService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sawking.Avox.KnowlegeService.API.dll"]
