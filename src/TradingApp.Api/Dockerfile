# ============ Build ============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["src/TradingApp.Api/TradingApp.Api.csproj", "TradingApp.Api/"]
COPY ["src/TradingApp.Application/TradingApp.Application.csproj", "TradingApp.Application/"]
COPY ["src/TradingApp.Domain/TradingApp.Domain.csproj", "TradingApp.Domain/"]
COPY ["src/TradingApp.Infrastructure/TradingApp.Infrastructure.csproj", "TradingApp.Infrastructure/"]

# Restore
RUN dotnet restore "TradingApp.Api/TradingApp.Api.csproj"

# Copy source
COPY src/ .

# Build & publish
WORKDIR /src/TradingApp.Api
RUN dotnet publish "TradingApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============ Runtime ============
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "TradingApp.Api.dll"]