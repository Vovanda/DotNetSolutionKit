# --- RUNTIME IMAGE ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=1000
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 443

# --- BUILD IMAGE ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files (order matters for caching)
COPY ["NamespaceRoot.ProductName.ServiceNameOrCustom.API/NamespaceRoot.ProductName.ServiceNameOrCustom.API.csproj", "NamespaceRoot.ProductName.ServiceNameOrCustom.API/"]
COPY ["NamespaceRoot.ProductName.ServiceNameOrCustom.Application/NamespaceRoot.ProductName.ServiceNameOrCustom.Application.csproj", "NamespaceRoot.ProductName.ServiceNameOrCustom.Application/"]
COPY ["NamespaceRoot.ProductName.ServiceNameOrCustom/NamspaceRoot.ProductName.ServiceNameOrCustom.csproj", "NamespaceRoot.ProductName.ServiceNameOrCustom/"]
COPY ["NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure/NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.csproj", "NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure/"]

# Common layer projects
COPY ["NamespaceRoot.ProductName.Common/NamspaceRoot.ProductName.Common.csproj", "NamespaceRoot.ProductName.Common/"]
COPY ["NamespaceRoot.ProductName.Common.Application/NamespaceRoot.ProductName.Common.Application.csproj", "NamespaceRoot.ProductName.Common.Application/"]
COPY ["NamespaceRoot.ProductName.Common.Contracts/NamespaceRoot.ProductName.Common.Contracts.csproj", "NamespaceRoot.ProductName.Common.Contracts/"]
COPY ["NamespaceRoot.ProductName.Common.Infrastructure/NamespaceRoot.ProductName.Common.Infrastructure.csproj", "NamespaceRoot.ProductName.Common.Infrastructure/"]
COPY ["NamespaceRoot.ProductName.Common.Web/NamespaceRoot.ProductName.Common.Web.csproj", "NamespaceRoot.ProductName.Common.Web/"]

RUN dotnet restore "NamespaceRoot.ProductName.ServiceNameOrCustom.API/NamespaceRoot.ProductName.ServiceNameOrCustom.API.csproj"

COPY . .
WORKDIR "/src/NamespaceRoot.ProductName.ServiceNameOrCustom.API"

RUN dotnet build "NamespaceRoot.ProductName.ServiceNameOrCustom.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# --- PUBLISH IMAGE ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "NamespaceRoot.ProductName.ServiceNameOrCustom.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# --- FINAL IMAGE ---
FROM base AS final
ARG APP_UID=1000
WORKDIR /app

# Create https directory with proper permissions
USER root
RUN mkdir -p /app/https && chown -R $APP_UID:$APP_UID /app/https
USER $APP_UID

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NamespaceRoot.ProductName.ServiceNameOrCustom.API.dll"]