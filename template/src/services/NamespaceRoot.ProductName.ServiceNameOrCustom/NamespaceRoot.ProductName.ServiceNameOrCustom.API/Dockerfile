FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NamespaceRoot.ProductName.ServiceNameOrCustom.API/NamespaceRoot.ProductName.ServiceNameOrCustom.API.csproj", "NamespaceRoot.ProductName.ServiceNameOrCustom.API/"]
RUN dotnet restore "NamespaceRoot.ProductName.ServiceNameOrCustom.API/NamespaceRoot.ProductName.ServiceNameOrCustom.API.csproj"
COPY . .
WORKDIR "/src/NamespaceRoot.ProductName.ServiceNameOrCustom.API"
RUN dotnet build "NamespaceRoot.ProductName.ServiceNameOrCustom.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NamespaceRoot.ProductName.ServiceNameOrCustom.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NamespaceRoot.ProductName.ServiceNameOrCustom.API.dll"]
